using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Windowing;
using ECommons.DalamudServices;
using ECommons.ExcelServices;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using SamplePlugin.Configs;
using SamplePlugin.Helpers;
using SamplePlugin.Helpers.UI;
using SamplePlugin.Updaters;

namespace SamplePlugin.UI;

internal sealed class TargetHighlight : Window
{
    // One row per role. Adding a role = adding one line here + its config flags.
    private sealed record RoleStyle(
        Job[] Jobs,
        Func<Configuration, bool> Enabled,
        Func<Configuration, bool> EnemyOnly,
        Vector4 Color,
        ObjectHighlightColor Outline);

    private static readonly RoleStyle[] Roles =
    {
        new(new[] { Job.DRK, Job.GNB, Job.WAR, Job.PLD },
            c => c.HighlightAllBattleCharasTanks, c => c.HighlightEnemyTanksOnly,
            ImGuiColors.ParsedBlue, ObjectHighlightColor.Blue),

        new(new[] { Job.WHM, Job.SCH, Job.AST, Job.SGE },
            c => c.HighlightAllBattleCharasHealers, c => c.HighlightEnemyHealersOnly,
            ImGuiColors.ParsedGreen, ObjectHighlightColor.Green),

        new(new[] { Job.MNK, Job.DRG, Job.NIN, Job.SAM, Job.RPR, Job.VPR },
            c => c.HighlightAllBattleCharasDPSMelee, c => c.HighlightEnemyDPSMeleeOnly,
            ImGuiColors.DPSRed, ObjectHighlightColor.Red),

        new(new[] { Job.BRD, Job.MCH, Job.DNC },
            c => c.HighlightAllBattleCharasDPSRanged, c => c.HighlightEnemyDPSRangedOnly,
            ImGuiColors.ParsedOrange, ObjectHighlightColor.Orange),

        new(new[] { Job.BLM, Job.SMN, Job.RDM, Job.PCT },
            c => c.HighlightAllBattleCharasDPSCaster, c => c.HighlightEnemyDPSCasterOnly,
            ImGuiColors.ParsedPurple, ObjectHighlightColor.Magenta),
    };

    private const float IconSize = 22f;
    private const float Padding = 4f;

    private readonly Configuration config;

    // Which objects we put a game-side outline on last frame / this frame,
    // so we only ever clear outlines that WE added.
    private readonly HashSet<ulong> outlinedLastFrame = new();
    private readonly HashSet<ulong> outlinedThisFrame = new();

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

    public override unsafe void Draw()
    {
        var player = Svc.Objects.LocalPlayer;
        if (player == null || Svc.Condition[ConditionFlag.BetweenAreas] || !config.EnableHighLightOverlay)
        {
            FlushOutlines();
            return;
        }

        var drawList = ImGui.GetBackgroundDrawList();
        var rainbow = GetGradientColor(); // compute once per frame, not once per object

        Vector4 Pick(Vector4 normal) => config.UseGradientColor ? rainbow : normal;

        if (config.HighlightPlayer)
        {
            DrawBox(drawList, player, Pick(ImGuiColors.DalamudViolet));
        }

        foreach (var chara in MainUpdater.AllGameObjects.OfType<IBattleChara>())
        {
            if (!chara.IsTargetable || chara.Address == IntPtr.Zero || chara.GameObjectId == player.GameObjectId)
                continue;

            var role = Roles.FirstOrDefault(r => chara.IsJobs(r.Jobs));
            if (role == null)
                continue; // not a combat job

            if (role.Enabled(config))
            {
                if (role.EnemyOnly(config) && !chara.IsEnemy())
                    continue;

                ((GameObject*)chara.Address)->Highlight(role.Outline, true);
                outlinedThisFrame.Add(chara.GameObjectId);
                DrawBox(drawList, chara, Pick(role.Color));
            }
            else if (config.HighlightAllBattleCharas)
            {
                DrawBox(drawList, chara, Pick(ImGuiColors.DalamudWhite));
            }
        }

        FlushOutlines();
    }

    /// <summary>
    /// Clears the game-side outline on anything we outlined last frame but not this frame.
    /// </summary>
    private unsafe void FlushOutlines()
    {
        foreach (var id in outlinedLastFrame)
        {
            if (outlinedThisFrame.Contains(id)) continue;
            var obj = Svc.Objects.SearchById(id);
            if (obj != null && obj.Address != IntPtr.Zero)
                ((GameObject*)obj.Address)->Highlight(ObjectHighlightColor.None, false);
        }

        outlinedLastFrame.Clear();
        outlinedLastFrame.UnionWith(outlinedThisFrame);
        outlinedThisFrame.Clear();
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
        var r = obj.HitboxRadius;
        var h = ((GameObject*)obj.Address)->Height + 0.85f;

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
    /// Single draw routine for every object type: rectangle, plus a job icon for battle charas.
    /// </summary>
    private static void DrawBox(ImDrawListPtr drawList, IGameObject obj, Vector4 color)
    {
        if (obj.Address == IntPtr.Zero || !TryGetScreenRect(obj, out var min, out var max))
            return;

        drawList.AddRect(min, max, ImGui.GetColorU32(color), 5f, ImDrawFlags.RoundCornersAll, 3f);

        if (obj is not IBattleChara chara)
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
        // Remove any outlines we added when the plugin unloads.
        outlinedThisFrame.Clear();
        FlushOutlines();
    }
}
