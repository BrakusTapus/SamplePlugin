using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Windowing;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using SamplePlugin.Configs;
using SamplePlugin.Data;
using SamplePlugin.Helpers;
using SamplePlugin.Helpers.UI;
using SamplePlugin.Updaters;

namespace SamplePlugin.UI;

internal sealed class TargetHighlight : Window
{
    private const float IconSize = 22f;
    private const float Padding = 4f;

    // Environmental objects often have a tiny (or zero) hitbox; without a floor their box collapses to a sliver.
    private const float MinRadius = 0.4f;
    private const float MinHeight = 1.0f;

    private readonly Configuration config;

    // Active categories for THIS frame, in catalog order (= priority order). Reused to avoid allocations.
    private readonly List<(HighlightCategory Category, CategorySettings Settings)> rules = new();

    public TargetHighlight(Plugin plugin)
        : base(nameof(TargetHighlight),
               ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoInputs |
               ImGuiWindowFlags.NoNav | ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoFocusOnAppearing)
    {
        // The window is just a host for Draw(); we paint on the background draw list,
        // which already covers the whole screen, so the window itself can be tiny.
        Size = new Vector2(1, 1);
        SizeCondition = ImGuiCond.Always;
        Position = Vector2.Zero;
        PositionCondition = ImGuiCond.Always;
        RespectCloseHotkey = false;
        config = plugin.Configuration;
    }

    public override void Draw()
    {
        var player = Svc.Objects.LocalPlayer;
        if (player == null || Svc.Condition[ConditionFlag.BetweenAreas] || !config.EnableHighLightOverlay)
            return;

        RefreshRules();
        if (rules.Count == 0 && !config.HighlightPlayer)
            return;

        var drawList = ImGui.GetBackgroundDrawList();
        var rainbow = GetGradientColor(); // compute once per frame, not once per object
        Vector4 Pick(Vector4 normal) => config.UseGradientColor ? rainbow : normal;

        if (config.HighlightPlayer)
            DrawBox(drawList, player, Pick(ImGuiColors.DalamudViolet), player.GetJobRole() != JobRole.None);

        if (rules.Count == 0)
            return;

        foreach (var obj in MainUpdater.AllGameObjects)
        {
            if (obj.Address == IntPtr.Zero || obj.GameObjectId == player.GameObjectId)
                continue;

            // Layer 1: classify once...
            var info = ObjectInfo.From(obj);

            // ...Layer 3: first enabled category that matches wins.
            foreach (var (category, settings) in rules)
            {
                if ((settings.Relations & info.Relation) == 0 || !category.Matches(info))
                    continue;

                // Untargetable or unnamed objects are usually invisible helpers; only draw them if the rule opts in.
                if (!settings.IncludeHidden && IsHidden(obj))
                    continue;

                DrawBox(drawList, obj, Pick(settings.Color), info.Role != JobRole.None);
                break;
            }
        }
    }

    private static bool IsHidden(IGameObject obj) =>
        (obj is IBattleChara && !obj.IsTargetable) || obj.Name.TextValue.Length == 0;

    private void RefreshRules()
    {
        rules.Clear();
        foreach (var category in HighlightCatalog.All)
        {
            if (config.Highlights.TryGetValue(category.Id, out var settings) && settings.Relations != Relation.None)
                rules.Add((category, settings));
        }
    }

    /// <summary>
    /// Projects the 8 corners of the object's world-space bounding box to the screen and
    /// returns the 2D rectangle that contains them. No camera/distance maths needed.
    /// </summary>
    private static unsafe bool TryGetScreenRect(IGameObject obj, out Vector2 min, out Vector2 max)
    {
        min = new Vector2(float.MaxValue);
        max = new Vector2(float.MinValue);

        var pos = obj.Position;
        var r = MathF.Max(obj.HitboxRadius, MinRadius);
        var h = MathF.Max(((GameObject*)obj.Address)->Height, MinHeight) + 0.85f;

        for (var i = 0; i < 8; i++)
        {
            var corner = pos + new Vector3(
                (i & 1) == 0 ? -r : r,
                (i & 2) == 0 ? 0f : h,
                (i & 4) == 0 ? -r : r);

            if (!Svc.GameGui.WorldToScreen(corner, out var screen))
                return false;

            min = Vector2.Min(min, screen);
            max = Vector2.Max(max, screen);
        }

        return true;
    }

    /// <summary>
    /// Single draw routine for every object type: a rectangle, plus a job icon for characters that have a job.
    /// </summary>
    private static void DrawBox(ImDrawListPtr drawList, IGameObject obj, Vector4 color, bool showJobIcon)
    {
        if (obj.Address == IntPtr.Zero || !TryGetScreenRect(obj, out var min, out var max))
            return;

        drawList.AddRect(min, max, ImGui.GetColorU32(color), 5f, ImDrawFlags.RoundCornersAll, 3f);

        if (!showJobIcon || obj is not IBattleChara chara)
            return;

        var icon = ImGuiExt.GetGameIconTexture(chara.ClassJob.RowId + 62100).GetWrapOrDefault();
        if (icon is null)
            return;

        var iconMin = new Vector2((min.X + max.X) / 2f - IconSize / 2f, min.Y - IconSize - Padding);
        drawList.AddImage(icon.Handle, iconMin, iconMin + new Vector2(IconSize));
    }

    /// <summary>Smooth rainbow that cycles once per second.</summary>
    private static Vector4 GetGradientColor()
    {
        var t = (float)ImGui.GetTime() % 1f;
        return new Vector4(
            (MathF.Sin(2 * MathF.PI * t) + 1) / 2,
            (MathF.Sin(2 * MathF.PI * (t + 1f / 3f)) + 1) / 2,
            (MathF.Sin(2 * MathF.PI * (t + 2f / 3f)) + 1) / 2,
            1f);
    }

    public void Dispose()
    {
    }
}
