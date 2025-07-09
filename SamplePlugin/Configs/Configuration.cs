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
    public bool DisplayPlayerInfo { get; set; } = true;
    public bool DisplayTargetInfo { get; set; } = true;
    public bool DisplayHighLight { get; set; } = true;
    public bool EnableHighLightOverlay { get; set; } = true;

    // the below exist just to make saving less cumbersome
    public void Save()
    {
        Svc.PluginInterface.SavePluginConfig(this);
    }
}
