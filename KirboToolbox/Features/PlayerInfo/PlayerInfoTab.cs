using System;
using System.Numerics;

using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin.Services;

using ECommons.DalamudServices;
using ECommons.GameFunctions;

using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Object;

using KirboToolbox.Configs;
using KirboToolbox.Core.Extensions;
using KirboToolbox.Helpers.UI;

using Lumina.Excel.Sheets;

namespace KirboToolbox.Features.PlayerInfo;

/// <summary>
/// The "Player Info" tab: current job, movement speed, hitbox radius and current zone.
/// </summary>
internal sealed unsafe class PlayerInfoTab
{
    private readonly Configuration configuration;

    public PlayerInfoTab(Plugin plugin) => configuration = plugin.Configuration;

    public void Draw()
    {
        using var child = ImRaii.Child("PlayerInfoChild", Vector2.Zero, true);
        if (!child.Success)
        {
            return;
        }

        var playerState = Svc.PlayerState;
        if (playerState == null || !playerState.IsLoaded)
        {
            ImGui.Text("IPlayerState is either null or not loaded!");
            return;
        }

        if (!playerState.ClassJob.IsValid)
        {
            ImGui.Text("Our current job is currently not valid.");
            return;
        }

        var localPlayer = Svc.Objects.LocalPlayer;
        if (localPlayer == null)
        {
            ImGui.Text("Local player is unavailable.");
            return;
        }

        DrawJob(playerState);
        DrawSpeed(localPlayer);
        DrawHitbox(localPlayer, playerState);
        DrawTerritory();
        DrawTargetCheck();
    }

    private static void DrawJob(IPlayerState playerState)
    {
        ImGui.AlignTextToFramePadding();
        ImGui.Text("Current job:");
        ImGui.SameLine(120 * ImGuiHelpers.GlobalScale);
        ImGuiExt.DrawJobIcon(playerState);
        ImGui.SameLine();
        ImGui.Text(playerState.ClassJob.Value.Abbreviation.ToString());
        ImGui.NewLine();
    }

    private void DrawSpeed(IPlayerCharacter localPlayer)
    {
        var speed = configuration.SpeedMultiplier;
        var step = configuration.SpeedStep;

        ImGui.Text("Current movement speed: " + localPlayer.CurrentSpeed.ToString("F3"));
        ImGui.Text("Movement speed multiplier: " + localPlayer.Speed.ToString("F3"));
        ImGui.NewLine();

        ImGui.SetNextItemWidth(150 * ImGuiHelpers.GlobalScale);
        if (ImGui.SliderFloat("##SpeedSlider", ref speed, 0.5f, 5f, "%.2f"))
        {
            ApplySpeed(localPlayer, MathF.Round(speed, 2));
        }

        ImGui.SameLine();
        if (ImGui.Button("-##SpeedDown"))
        {
            ApplySpeed(localPlayer, MathF.Max(0.1f, MathF.Round(speed - step, 2)));
        }

        ImGui.SameLine();
        if (ImGui.Button("+##SpeedUp"))
        {
            ApplySpeed(localPlayer, MathF.Round(speed + step, 2));
        }

        ImGui.SameLine();
        if (ImGui.Button("Reset##SpeedReset"))
        {
            ApplySpeed(localPlayer, 1f);
        }

        ImGui.SetNextItemWidth(80 * ImGuiHelpers.GlobalScale);
        if (ImGui.InputFloat("Step size", ref step, 0f, 0f, "%.2f"))
        {
            configuration.SpeedStep = Math.Clamp(step, 0.01f, 1f);
            configuration.Save();
        }

        ImGui.NewLine();
    }

    /// <summary>One place that sets the speed, remembers it and saves it (the 4 buttons used to repeat this).</summary>
    private void ApplySpeed(IPlayerCharacter localPlayer, float value)
    {
        configuration.SpeedMultiplier = value;
        localPlayer.Speed = value;
        configuration.Save();
    }

    private static void DrawHitbox(IPlayerCharacter localPlayer, IPlayerState playerState)
    {
        if (localPlayer.Address == nint.Zero)
        {
            return;
        }

        var gameObject = (GameObject*)localPlayer.Address;

        ImGui.TextUnformatted($"Hitbox Radius: ({localPlayer.HitboxRadius})");
        ImGui.SliderFloat($"Hitbox Radius###{playerState.CharacterName}{playerState.EntityId}", ref gameObject->HitboxRadius, 0f, 100f);
        ImGui.SameLine();
        if (ImGui.Button("Reset##HitboxRadius"))
        {
            gameObject->HitboxRadius = 0.5f;
        }

        ImGui.NewLine();
    }

    private static void DrawTerritory()
    {
        var territoryId = Svc.ClientState.TerritoryType;
        if (Svc.Data.GetExcelSheet<TerritoryType>().TryGetRow(territoryId, out var territoryRow))
        {
            ImGui.TextUnformatted($"We are currently in ({territoryId}) \"{territoryRow.PlaceName.Value.Name.ExtractText()}\"");
        }
        else
        {
            ImGui.TextUnformatted("Invalid territory.");
        }
    }

    private static void DrawTargetCheck()
    {
        if (Svc.Targets.Target is not IBattleChara target)
        {
            ImGui.Text("target is NULL");
            return;
        }

        var canUse = ActionManager.CanUseActionOnTarget(142, (GameObject*)target.Struct());
        ImGui.Text("can use on target " + canUse);
    }
}
