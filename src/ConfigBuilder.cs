using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using System.Collections.Generic;

namespace UserConfigSaver;

public static class ConfigBuilder
{
    public static List<(int cat, string key, string val)> Build(CCSPlayerController player)
    {
        var list = new List<(int, string, string)>();

        void Add(int cat, string cvar) =>
            list.Add((cat, cvar, NativeAPI.GetClientConvarValue(player.Slot, cvar)));

        /*                                                         UNCOMMENT IF A FIX FOR CLIENT-SIDED CVARS IS FOUND
        Add(Categories.Hud, "hud_safezonex");
        Add(Categories.Hud, "hud_scaling");
        Add(Categories.Hud, "hud_scalefactor");
        Add(Categories.Hud, "cl_draw_only_deathnotices");
        Add(Categories.Hud, "cl_showpos");
        Add(Categories.Hud, "cl_hud_color");
        Add(Categories.Hud, "cl_hud_background_alpha");
        */

        Add(Categories.Viewmodel, "viewmodel_offset_x");
        Add(Categories.Viewmodel, "viewmodel_offset_y");
        Add(Categories.Viewmodel, "viewmodel_offset_z");
        Add(Categories.Viewmodel, "viewmodel_fov");
        Add(Categories.Viewmodel, "viewmodel_presetpos");

        var crosshairCode = player.CrosshairCodes;
        if (!string.IsNullOrEmpty(crosshairCode))
            list.Add((Categories.Crosshair, "crosshair_code", crosshairCode));

        Add(Categories.Mouse, "sensitivity");
        /*                                                          UNCOMMENT IF A FIX FOR CLIENT-SIDED CVARS IS FOUND
        Add(Categories.Mouse, "m_rawinput");
        Add(Categories.Mouse, "m_mouseaccel1");
        Add(Categories.Mouse, "zoom_sensitivity_ratio_mouse");
        */
        return list;
    }
}