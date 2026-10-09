using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Interface.Colors;

namespace SamplePlugin.Data;

/// <summary>
/// One selectable thing in the highlight tab. The Add menu and the rules table are both generated from
/// <see cref="HighlightCatalog.All"/>, so adding a type of thing to highlight = adding one entry there.
/// </summary>
internal sealed record HighlightCategory(
    string Id,                       // stable save key. Never rename it once released.
    string Label,                    // shown in the UI
    string Group,                    // heading in the Add menu
    Func<ObjectInfo, bool> Matches,  // does this object belong to the category?
    Relation Available,              // which Relation columns make sense (others show a dash)
    Vector4 DefaultColor,
    Relation DefaultRelations = Relation.None) // ticked when first added; None = everything in Available
{
    public Relation InitialRelations => DefaultRelations != Relation.None ? DefaultRelations : Available;
}

internal static class HighlightCatalog
{
    public const string GroupRoles = "Players by role";
    public const string GroupJobTypes = "Players by job type";
    public const string GroupWorld = "NPCs and objects";

    private const Relation FriendlyOnly = Relation.Party | Relation.Alliance | Relation.Others;

    /// <summary>
    /// ORDER MATTERS: when drawing, the first enabled category that matches an object wins.
    /// Put specific entries before generic ones (e.g. "Tanks" before "All combat jobs").
    /// </summary>
    public static readonly HighlightCategory[] All =
    {
        // ---- Players by role -------------------------------------------------------------------------
        Role("tank",   "Tanks",      JobRole.Tank,           ImGuiColors.ParsedBlue),
        Role("healer", "Healers",    JobRole.Healer,         ImGuiColors.ParsedGreen),
        Role("melee",  "Melee DPS",  JobRole.Melee,          ImGuiColors.DPSRed),
        Role("ranged", "Ranged DPS", JobRole.RangedPhysical, ImGuiColors.ParsedOrange),
        Role("caster", "Casters",    JobRole.RangedMagical,  ImGuiColors.ParsedPurple),
        new("combat", "All combat jobs", GroupRoles, i => IsCombat(i.Role), Relation.All, ImGuiColors.DalamudWhite),

        // ---- Players by job type ---------------------------------------------------------------------
        new("crafter",  "Crafters",  GroupJobTypes, i => i.Role == JobRole.DiscipleOfTheHand, FriendlyOnly, new Vector4(0.80f, 0.55f, 0.15f, 1f)),
        new("gatherer", "Gatherers", GroupJobTypes, i => i.Role == JobRole.DiscipleOfTheLand, FriendlyOnly, new Vector4(0.55f, 0.80f, 0.30f, 1f)),

        // ---- NPCs and objects ------------------------------------------------------------------------
        Kind("eventnpc",  "Event NPCs",       ObjectKind.EventNpc,       new Vector4(0.11f, 0.62f, 0.46f, 1f)),
        Kind("battlenpc", "Battle NPCs",      ObjectKind.BattleNpc,      new Vector4(0.85f, 0.35f, 0.19f, 1f), Relation.Enemy | Relation.Others, Relation.Enemy),
        Kind("treasure",  "Treasure chests",  ObjectKind.Treasure,       new Vector4(0.94f, 0.62f, 0.15f, 1f)),
        Kind("aetheryte", "Aetherytes",       ObjectKind.Aetheryte,      new Vector4(0.22f, 0.54f, 0.87f, 1f)),
        Kind("gathpoint", "Gathering points", ObjectKind.GatheringPoint, new Vector4(0.39f, 0.60f, 0.13f, 1f)),
        Kind("eventobj",  "Event objects",    ObjectKind.EventObj,       new Vector4(0.53f, 0.53f, 0.50f, 1f)),
        Kind("minion",    "Minions",          ObjectKind.Companion,      new Vector4(0.83f, 0.33f, 0.49f, 1f)),
    };

    // Declared after All on purpose: static fields initialise in the order they are written.
    public static readonly IReadOnlyDictionary<string, HighlightCategory> ById = All.ToDictionary(c => c.Id);

    private static HighlightCategory Role(string id, string label, JobRole role, Vector4 color) =>
        new(id, label, GroupRoles, i => i.Role == role, Relation.All, color);

    private static HighlightCategory Kind(string id, string label, ObjectKind kind, Vector4 color,
        Relation available = Relation.Others, Relation initial = Relation.None) =>
        new(id, label, GroupWorld, i => i.Kind == kind, available, color, initial);

    private static bool IsCombat(JobRole r) =>
        r is not (JobRole.None or JobRole.DiscipleOfTheLand or JobRole.DiscipleOfTheHand);
}
