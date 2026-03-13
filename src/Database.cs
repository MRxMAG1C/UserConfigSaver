using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UserConfigSaver;

public class Database
{
    private readonly string _connectionString;

    public Database(PluginConfig config)
    {
        _connectionString = new MySqlConnectionStringBuilder
        {
            Server = config.DatabaseHost,
            Port = (uint)config.DatabasePort,
            Database = config.DatabaseName,
            UserID = config.DatabaseUser,
            Password = config.DatabasePassword,
            ConnectionTimeout = 10,
            SslMode = MySqlSslMode.Preferred
        }.ConnectionString;
    }

    private static async Task SetTimezoneAsync(MySqlConnection conn)
    {
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SET time_zone = '+01:00';";
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task InitAsync()
    {
        try
        {
            Log.Info($"Connecting to database...");
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();
            await SetTimezoneAsync(conn);
            Log.Info("Connected!");

            var statements = new[]
            {
                @"CREATE TABLE IF NOT EXISTS player_cs2_settings (
                    steamid     VARCHAR(20)  NOT NULL PRIMARY KEY,
                    player_name VARCHAR(128) NOT NULL DEFAULT '',
                    created_at  DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    updated_at  DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
                );",
                @"CREATE TABLE IF NOT EXISTS player_cs2_setting_configs (
                    steamid     VARCHAR(20)  NOT NULL,
                    category_id INT          NOT NULL,
                    `key`       VARCHAR(64)  NOT NULL,
                    value       VARCHAR(128) NOT NULL DEFAULT '',
                    updated_at  DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
                    PRIMARY KEY (steamid, category_id, `key`),
                    FOREIGN KEY (steamid) REFERENCES player_cs2_settings(steamid) ON DELETE CASCADE
                );",
                @"CREATE TABLE IF NOT EXISTS setting_categories (
                    id   INT         NOT NULL PRIMARY KEY,
                    name VARCHAR(32) NOT NULL
                );",
                @"INSERT IGNORE INTO setting_categories (id, name) VALUES
                    (1, 'HUD'), (2, 'Viewmodel'), (3, 'Crosshair'), (4, 'Mouse');"
            };

            foreach (var sql in statements)
            {
                await using var cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                await cmd.ExecuteNonQueryAsync();
            }

            Log.Info("Database tables ready.");
        }
        catch (Exception ex)
        {
            Log.Error($"Database init error: {ex.Message}");
        }
    }

    public async Task UpsertPlayerAsync(string steamId, string name, List<(int cat, string key, string val)> configs)
    {
        try
        {
            Log.Info($"Saving settings for {name} ({steamId})...");
            await using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();
            await SetTimezoneAsync(conn);

            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    INSERT INTO player_cs2_settings (steamid, player_name, created_at, updated_at)
                    VALUES (@steamid, @name, NOW(), NOW())
                    ON DUPLICATE KEY UPDATE player_name = VALUES(player_name), updated_at = NOW();";
                cmd.Parameters.AddWithValue("@steamid", steamId);
                cmd.Parameters.AddWithValue("@name", name);
                await cmd.ExecuteNonQueryAsync();
            }

            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    INSERT INTO player_cs2_setting_configs (steamid, category_id, `key`, value, updated_at)
                    VALUES (@steamid, @cat, @key, @val, NOW())
                    ON DUPLICATE KEY UPDATE value = VALUES(value), updated_at = NOW();";

                var pSteam = cmd.Parameters.Add("@steamid", MySqlDbType.VarChar);
                var pCat = cmd.Parameters.Add("@cat", MySqlDbType.Int32);
                var pKey = cmd.Parameters.Add("@key", MySqlDbType.VarChar);
                var pVal = cmd.Parameters.Add("@val", MySqlDbType.VarChar);

                foreach (var (categoryId, key, value) in configs)
                {
                    pSteam.Value = steamId;
                    pCat.Value = categoryId;
                    pKey.Value = key;
                    pVal.Value = value;
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            Log.Info($"Saved settings for {name} ({steamId}).");
        }
        catch (Exception ex)
        {
            Log.Error($"DB error for {steamId}: {ex.Message}");
        }
    }
}