using CounterStrikeSharp.API.Core;
using System;
using System.Threading.Tasks;

namespace UserConfigSaver;

public class UserConfigSaverPlugin : BasePlugin, IPluginConfig<PluginConfig>
{
    public override string ModuleName => "UserConfigSaver";
    public override string ModuleVersion => "1.0.0";
    public override string ModuleAuthor => "MRxMAG1C";
    public override string ModuleDescription => "Saves player CS2 configs to MySQL on spawn.";


    public PluginConfig Config { get; set; } = new();
    public void OnConfigParsed(PluginConfig config) => Config = config;

    private Database _db = null!;

    public override void Load(bool hotReload)
    {
        _db = new Database(Config);

        Task.Run(_db.InitAsync).ContinueWith(t =>
        {
            if (t.Exception != null)
            {
                Log.Error($"Init error: {t.Exception.InnerException?.Message}");
            }
                
        }, TaskContinuationOptions.OnlyOnFaulted);

        RegisterEventHandler<EventPlayerConnectFull>(OnPlayerConnectFull);
        RegisterEventHandler<EventPlayerSpawned>(OnPlayerSpawned);

        Log.Info("Plugin loaded.");
    }

    private HookResult OnPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (player == null || !player.IsValid || player.IsBot || player.SteamID == 0)
            return HookResult.Continue;

        Log.Info($"ConnectFull fired for {player.PlayerName}");
        return HookResult.Continue;
    }

    private HookResult OnPlayerSpawned(EventPlayerSpawned @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (player == null || !player.IsValid || player.IsBot || player.SteamID == 0)
            return HookResult.Continue;

        Log.Info($"Spawned fired for {player.PlayerName}");

        var steamId = player.SteamID.ToString();
        var name = player.PlayerName;

        AddTimer(2.0f, () =>
        {
            if (player == null || !player.IsValid) return;

            var configs = ConfigBuilder.Build(player);

            Task.Run(() => _db.UpsertPlayerAsync(steamId, name, configs)).ContinueWith(t =>
            {
                if (t.Exception != null)
                    Log.Error($"Upsert error: {t.Exception.InnerException?.Message}");
            }, TaskContinuationOptions.OnlyOnFaulted);
        });

        return HookResult.Continue;
    }
}