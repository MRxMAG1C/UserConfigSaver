using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;

namespace UserConfigSaver;

public class PluginConfig : BasePluginConfig
{
    [JsonPropertyName("DatabaseHost")] public string DatabaseHost { get; set; } = "45wg9j.h.filess.io";
    [JsonPropertyName("DatabasePort")] public int DatabasePort { get; set; } = 3306;
    [JsonPropertyName("DatabaseName")] public string DatabaseName { get; set; } = "userconfigsaver_halfwhere";
    [JsonPropertyName("DatabaseUser")] public string DatabaseUser { get; set; } = "userconfigsaver_halfwhere";
    [JsonPropertyName("DatabasePassword")] public string DatabasePassword { get; set; } = "[PASSWORD]";
}