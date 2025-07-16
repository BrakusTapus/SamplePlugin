using Dalamud.Configuration;
using Dalamud.Plugin;
using ECommons.DalamudServices;
using SamplePlugin.DalamudServices;
using System;

namespace SamplePlugin.Configs;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public bool IsConfigWindowMovable { get; set; } = true;
    public bool DisplayPlayerInfoTab { get; set; } = true;
    public bool DisplayTargetInfoTab { get; set; } = true;
    public bool DisplayHighLightInfoTab { get; set; } = true;
    public bool EnableHighLightOverlay { get; set; } = false;
    public bool HighlightPlayer { get; set; } = false;
    public bool HighlightAllGameObjects { get; set; } = false;

    // the below exist just to make saving less cumbersome
    public void Save()
    {
        Svc.PluginInterface.SavePluginConfig(this);
        Svc.Log.Debug($"Saved plugin config.");
    }
}
