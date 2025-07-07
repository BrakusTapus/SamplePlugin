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
    public bool SomePropertyToBeSavedAndWithADefault { get; set; } = true;
    public bool DisplayHighLight { get; set; } = true;

    // the below exist just to make saving less cumbersome
    public void Save()
    {
        Svc.PluginInterface.SavePluginConfig(this);
    }
}
