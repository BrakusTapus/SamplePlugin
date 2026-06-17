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

    // Config window
    public bool IsConfigWindowMovable { get; set; } = true;

    // Main window tab bar
    public bool DisplayPlayerInfoTab { get; set; } = true;
    public bool DisplayTargetInfoTab { get; set; } = true;
    public bool DisplayHighLightInfoTab { get; set; } = true;

    // Highlight related settings
    public bool EnableHighLightOverlay { get; set; } = false;
    public bool HighlightPlayer { get; set; } = false;
    public bool UseGradientColor { get; set; } = false;
    public bool UseGlowEffect { get; set; } = false;
    public float GlowSize { get; set; } = 8f;
    public int GlowSteps { get; set; } = 8;
    public bool HighlightAllGameObjects { get; set; } = false;
    public bool HighlightAllBattleCharas { get; set; } = false;
    public bool HighlightAllBattleCharasTanks { get; set; } = false;
    public bool HighlightAllBattleCharasHealers { get; set; } = false;
    public bool HighlightAllBattleCharasDPSMelee { get; set; } = false;
    public bool HighlightAllBattleCharasDPSRanged { get; set; } = false;
    public bool HighlightAllBattleCharasDPSCaster { get; set; } = false;


    // the below exist just to make saving less cumbersome
    public void Save()
    {
        Svc.PluginInterface.SavePluginConfig(this);
        Svc.Log.Debug($"Saved plugin config.");
    }
}
