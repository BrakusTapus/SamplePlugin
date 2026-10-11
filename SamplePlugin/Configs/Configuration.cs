using System;
using System.Collections.Generic;
using Dalamud.Configuration;
using Dalamud.Plugin;
using ECommons.DalamudServices;
using SamplePlugin.DalamudServices;
using SamplePlugin.Data;

namespace SamplePlugin.Configs;

[Serializable]
public class Configuration : IPluginConfiguration
{
    private const int CurrentVersion = 1;

    public int Version { get; set; } = 0;

    // Config window
    public bool IsConfigWindowMovable { get; set; } = true;

    // Main window tab bar
    public bool DisplayPlayerInfoTab { get; set; } = true;
    public bool DisplayTargetInfoTab { get; set; } = true;
    public bool DisplayHighLightInfoTab { get; set; } = true;
    public bool ShowInDevMenu { get; set; } = true;

    // Highlight related settings
    public bool EnableHighLightOverlay { get; set; } = false;
    public bool HighlightPlayer { get; set; } = false;
    public bool UseGradientColor { get; set; } = false;
    public bool UseGlowEffect { get; set; } = false;
    public float GlowSize { get; set; } = 8f;
    public int GlowSteps { get; set; } = 8;
    public bool HighlightAllGameObjects { get; set; } = false;
    public bool EnemyOnly { get; set; } = false;

    /// <summary>
    /// What to highlight. Key = HighlightCategory.Id. A category is active if it has an entry here.
    /// </summary>
    public Dictionary<string, CategorySettings> Highlights { get; set; } = new();

    // ---- Legacy highlight flags (Version 0). Only read by the migration below. Safe to delete after a few releases. ----
    [Obsolete("Migrated to Highlights")] public bool HighlightAllBattleCharas { get; set; } = false;
    [Obsolete("Migrated to Highlights")] public bool HighlightAllBattleCharasTanks { get; set; } = false;
    [Obsolete("Migrated to Highlights")] public bool HighlightAllBattleCharasHealers { get; set; } = false;
    [Obsolete("Migrated to Highlights")] public bool HighlightAllBattleCharasDPSMelee { get; set; } = false;
    [Obsolete("Migrated to Highlights")] public bool HighlightAllBattleCharasDPSRanged { get; set; } = false;
    [Obsolete("Migrated to Highlights")] public bool HighlightAllBattleCharasDPSCaster { get; set; } = false;
    [Obsolete("Migrated to Highlights")] public bool HighlightEnemyTanksOnly { get; set; } = false;
    [Obsolete("Migrated to Highlights")] public bool HighlightEnemyHealersOnly { get; set; } = false;
    [Obsolete("Migrated to Highlights")] public bool HighlightEnemyDPSMeleeOnly { get; set; } = false;
    [Obsolete("Migrated to Highlights")] public bool HighlightEnemyDPSRangedOnly { get; set; } = false;
    [Obsolete("Migrated to Highlights")] public bool HighlightEnemyDPSCasterOnly { get; set; } = false;

    /// <summary>Call once right after loading the config.</summary>
    public void MigrateIfNeeded()
    {
        if (Version >= CurrentVersion)
            return;

        MigrateLegacyHighlightFlags();
        Version = CurrentVersion;
        Save();
    }

#pragma warning disable CS0618 // reading the obsolete flags is the whole point of this method
    private void MigrateLegacyHighlightFlags()
    {
        // Old "role on" without "enemy only" meant everyone; with "enemy only" it meant enemies.
        AddFromLegacy("tank",   HighlightAllBattleCharasTanks,    HighlightEnemyTanksOnly);
        AddFromLegacy("healer", HighlightAllBattleCharasHealers,  HighlightEnemyHealersOnly);
        AddFromLegacy("melee",  HighlightAllBattleCharasDPSMelee, HighlightEnemyDPSMeleeOnly);
        AddFromLegacy("ranged", HighlightAllBattleCharasDPSRanged, HighlightEnemyDPSRangedOnly);
        AddFromLegacy("caster", HighlightAllBattleCharasDPSCaster, HighlightEnemyDPSCasterOnly);
        // Old "Highlight All Battlecharas" was the white fallback box for every other combat job.
        AddFromLegacy("combat", HighlightAllBattleCharas, enemyOnly: false);
    }
#pragma warning restore CS0618

    private void AddFromLegacy(string id, bool enabled, bool enemyOnly)
    {
        if (!enabled || Highlights.ContainsKey(id) || !HighlightCatalog.ById.TryGetValue(id, out var category))
            return;

        Highlights[id] = new CategorySettings
        {
            Relations = enemyOnly ? Relation.Enemy : category.Available,
            Color = category.DefaultColor,
        };
    }

    // the below exist just to make saving less cumbersome
    public void Save()
    {
        Svc.PluginInterface.SavePluginConfig(this);
        Svc.Log.Debug($"Saved plugin config.");
    }
}
