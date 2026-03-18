using CounterStrikeSharp.API.Core;

namespace UserConfigSaver;

public static class ConfigBuilder
{
    public static List<PlayerSetting> Build(CCSPlayerController player)
    {
        var list = new List<PlayerSetting>();

        void Add(CsSettingCategory category, string cvar) =>
            list.Add(new PlayerSetting(category, cvar, NativeAPI.GetClientConvarValue(player.Slot, cvar)));

        Add(CsSettingCategory.Viewmodel, "viewmodel_offset_x");
        Add(CsSettingCategory.Viewmodel, "viewmodel_offset_y");
        Add(CsSettingCategory.Viewmodel, "viewmodel_offset_z");
        Add(CsSettingCategory.Viewmodel, "viewmodel_fov");

        var crosshairCode = player.CrosshairCodes;
        if (!string.IsNullOrEmpty(crosshairCode))
            list.Add(new PlayerSetting(CsSettingCategory.Crosshair, "crosshair_code", crosshairCode));

        Add(CsSettingCategory.Mouse, "sensitivity");
        return list;
    }
}
