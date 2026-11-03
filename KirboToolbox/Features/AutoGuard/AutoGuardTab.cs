using System;
using System.Numerics;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;

using KirboToolbox.Configs;

namespace KirboToolbox.Features.AutoGuard;

/// <summary>The "Auto Guard" tab: settings, status and counters.</summary>
internal sealed class AutoGuardTab
{
    private readonly Plugin plugin;

    public AutoGuardTab(Plugin plugin) => this.plugin = plugin;

    private Configuration Config => plugin.Configuration;
    private AutoGuardService Service => plugin.AutoGuard;

    public void Draw()
    {
        using var child = ImRaii.Child("AutoGuardChild", Vector2.Zero, true);
        if (!child.Success)
        {
            return;
        }

        var enabled = Config.AutoGuardEnabled;
        if (ImGui.Checkbox("Enable Auto-Guard on Marksman's Spite", ref enabled))
        {
            Config.AutoGuardEnabled = enabled;
            Config.Save();
        }

        var alert = Config.AutoGuardAlertOnCooldown;
        if (ImGui.Checkbox("Alert in chat if Guard is not available", ref alert))
        {
            Config.AutoGuardAlertOnCooldown = alert;
            Config.Save();
        }

        ImGui.Separator();
        ImGui.TextUnformatted("Status");

        if (Config.AutoGuardEnabled)
        {
            ImGui.TextColored(ImGuiColors.ParsedGreen, "ACTIVE");
        }
        else
        {
            ImGui.TextColored(ImGuiColors.DalamudRed, "DISABLED");
        }

        if (Service.IsGuardReady)
        {
            ImGui.TextColored(ImGuiColors.ParsedGreen, "Guard: AVAILABLE");
        }
        else if (Service.GuardCooldownRemaining > 0.1f)
        {
            ImGui.TextColored(ImGuiColors.DalamudOrange, $"Guard: on cooldown ({Service.GuardCooldownRemaining:F1}s)");
        }
        else
        {
            ImGui.TextColored(ImGuiColors.DalamudRed, "Guard: NOT AVAILABLE (not in PvP, wrong state, ...)");
        }

        var age = (DateTime.Now - Service.LastEventTime).TotalSeconds;
        if (Service.LastEventTime != DateTime.MinValue && age < 10)
        {
            var color = Service.LastEvent.Contains("ACTIVATED") ? ImGuiColors.ParsedGreen : ImGuiColors.DalamudRed;
            ImGui.TextColored(color, $"Last: {Service.LastEvent}");
        }

        ImGui.Separator();
        ImGui.TextUnformatted("Action IDs");
        ImGui.BulletText($"Marksman's Spite: {(Service.SpiteActionId != 0 ? Service.SpiteActionId.ToString() : "NOT FOUND")}");
        ImGui.BulletText($"Guard: {(Service.GuardActionId != 0 ? Service.GuardActionId.ToString() : "NOT FOUND")}");

        ImGui.Separator();
        ImGui.TextUnformatted("Stats");
        ImGui.BulletText($"Times Spite detected: {Config.AutoGuardTimesDetected}");
        ImGui.BulletText($"Times Guard activated: {Config.AutoGuardTimesGuarded}");
    }
}
