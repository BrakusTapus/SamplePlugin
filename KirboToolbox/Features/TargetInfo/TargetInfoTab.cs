using System;
using System.Linq;
using System.Numerics;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Interface.Utility.Raii;

using ECommons.DalamudServices;
using ECommons.ImGuiMethods;

using FFXIVClientStructs.FFXIV.Client.Game.Object;

using KirboToolbox.Core.Trackers;
using KirboToolbox.Helpers.UI;

namespace KirboToolbox.Features.TargetInfo;

/// <summary>
/// The "Target Info" tab: shows the nameplate data of your current target (left)
/// and of every tracked nameplate (right).
/// </summary>
internal sealed unsafe class TargetInfoTab
{
    public void Draw()
    {
        DrawToolbar();
        ImGui.Separator();

        var available = ImGui.GetContentRegionAvail();

        using (var left = ImRaii.Child("targetinfo_left", new Vector2(available.X / 2, available.Y - 10), true, ImGuiWindowFlags.NoScrollbar))
        {
            if (left.Success)
            {
                DrawCurrentTarget();
            }
        }

        ImGui.SameLine();

        using (var right = ImRaii.Child("targetinfo_right", new Vector2((available.X / 2) - 10, available.Y - 10), true, ImGuiWindowFlags.NoScrollbar))
        {
            if (right.Success)
            {
                DrawAllNamePlates();
            }
        }
    }

    private static void DrawToolbar()
    {
        if (ImGui.Button("Redraw Nameplate"))
        {
            Svc.NamePlates.RequestRedraw();
        }

        ImGuiEx.Tooltip("Send a redraw request for nameplates");
        ImGui.SameLine();

        if (ImGui.Button("Clear List"))
        {
            NamePlateTracker.ClearList();
        }

        ImGuiEx.Tooltip("Clears the listed nameplates");
        ImGui.SameLine();
        ImGui.TextUnformatted("AllNamePlates: " + NamePlateTracker.AllNamePlates.Count);
    }

    private static void DrawCurrentTarget()
    {
        var targetId = Svc.Targets.Target?.GameObjectId ?? 0;
        var entry = NamePlateTracker.AllNamePlates.FirstOrDefault(n => n.GameObjectId == targetId);
        if (entry == null)
        {
            ImGui.TextUnformatted("Direct: NOT FOUND");
            return;
        }

        ImGui.TextUnformatted($"Direct: IconID = {entry.NameIconId}");
        ImGui.TextUnformatted($"1 = {entry.Name}");
        ImGui.TextUnformatted($"2 = {entry.IsBoss}");
        ImGui.TextUnformatted($"3 = {entry.GameObjectId}");
    }

    private static void DrawAllNamePlates()
    {
        foreach (var entry in NamePlateTracker.AllNamePlates)
        {
            if (!entry.IsTargetable)
            {
                continue;
            }

            DrawEntry(entry);

            // Add vertical spacing between entries
            ImGui.Dummy(new Vector2(0, 10));
        }
    }

    private static void DrawEntry(NamePlateEntry entry)
    {
        const ImGuiTableFlags flags = ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersInner | ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders;

        using var table = ImRaii.Table($"NamePlateTable_{entry.GameObjectId}", 2, flags);
        if (!table.Success)
        {
            return;
        }

        ImGui.TableSetupColumn("Label", ImGuiTableColumnFlags.WidthFixed, 150f);
        ImGui.TableSetupColumn("Value", ImGuiTableColumnFlags.WidthFixed, 200f);

        // Name (with the nameplate icon next to it, if there is one)
        ImGui.TableNextRow();
        ImGui.TableSetColumnIndex(0);
        ImGui.TextUnformatted("Name");
        ImGui.TableSetColumnIndex(1);
        ImGui.TextUnformatted(entry.Name);
        DrawNameIcon(entry);

        DrawRow("GameObjectId:", entry.GameObjectId.ToString());
        DrawRow("Boss from icon:", entry.IsBoss.ToString());
        DrawRow("Targetable:", entry.IsTargetable.ToString());

        // Hitbox radius (editable)
        ImGui.TableNextRow();
        ImGui.TableSetColumnIndex(0);
        ImGui.TextUnformatted("Hitbox Radius:");
        ImGui.TableSetColumnIndex(1);
        DrawHitboxSlider(entry);
    }

    private static void DrawRow(string label, string value)
    {
        ImGui.TableNextRow();
        ImGui.TableSetColumnIndex(0);
        ImGui.TextUnformatted(label);
        ImGui.TableSetColumnIndex(1);
        ImGui.TextUnformatted(value);
    }

    private static void DrawNameIcon(NamePlateEntry entry)
    {
        IDalamudTextureWrap? icon = ImGuiExt.GetGameIconTexture((uint)entry.NameIconId)?.GetWrapOrDefault();
        if (icon == null)
        {
            return;
        }

        ImGui.SameLine();
        ImGui.Image(icon.Handle, new Vector2(22, 22));

        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.TextUnformatted($"NameIconId: {entry.NameIconId}");
            ImGui.EndTooltip();
        }
    }

    private static void DrawHitboxSlider(NamePlateEntry entry)
    {
        var chara = entry.BattleChara;
        if (chara == null || chara.Address == IntPtr.Zero)
        {
            ImGui.TextUnformatted("N/A (No BattleChara)");
            return;
        }

        var gameObject = (GameObject*)chara.Address;
        ImGui.SetNextItemWidth(100);
        ImGui.SliderFloat($"##HitboxRadiusSlider{chara.Name}{chara.EntityId}", ref gameObject->HitboxRadius, 0f, 100f);
    }
}
