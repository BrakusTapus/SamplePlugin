using System;

using Dalamud.Plugin.Services;

using ECommons.DalamudServices;

using KirboToolbox.Configs;

namespace KirboToolbox.Features.AutoGuard;

/// <summary>
/// Resolves the action IDs, listens for Marksman's Spite, and uses Guard.
/// Contains no UI code; <see cref="AutoGuardTab"/> only reads from it.
/// </summary>
internal sealed class AutoGuardService : IDisposable
{
    private const double DedupMs = 500.0;

    private readonly Configuration config;
    private readonly GuardExecutor guardExecutor = new();
    private readonly SpiteDetector? detector;

    // Set by the hook, handled on the next framework update.
    private volatile bool guardRequested;
    private DateTime lastDetection = DateTime.MinValue;

    public uint SpiteActionId { get; private set; }
    public uint GuardActionId => guardExecutor.GuardActionId;

    public string LastEvent { get; private set; } = "None";
    public DateTime LastEventTime { get; private set; } = DateTime.MinValue;

    public bool IsGuardReady => guardExecutor.IsGuardReady();
    public float GuardCooldownRemaining => guardExecutor.GetCooldownRemaining();

    public AutoGuardService(Configuration config)
    {
        this.config = config;

        ResolveActionIds();

        if (SpiteActionId == 0)
        {
            Svc.Log.Warning("AutoGuard: could not resolve Marksman's Spite, detector not started.");
            return;
        }

        detector = new SpiteDetector(SpiteActionId);
        detector.SpiteTargetedLocalPlayer += OnSpiteDetected;
        Svc.Framework.Update += OnUpdate;
    }

    private void ResolveActionIds()
    {
        // NOTE: names are matched in the client language. This works for an English client.
        var sheet = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Action>();

        foreach (var action in sheet)
        {
            if (!action.IsPvP)
            {
                continue;
            }

            var name = action.Name.ToString();

            if (name == "Marksman's Spite" && SpiteActionId == 0)
            {
                SpiteActionId = action.RowId;
                Svc.Log.Info($"AutoGuard: resolved Marksman's Spite ID={action.RowId}");
            }
            else if (name == "Guard" && guardExecutor.GuardActionId == 0)
            {
                guardExecutor.GuardActionId = action.RowId;
                Svc.Log.Info($"AutoGuard: resolved Guard ID={action.RowId}");
            }

            if (SpiteActionId != 0 && guardExecutor.GuardActionId != 0)
            {
                break;
            }
        }

        if (guardExecutor.GuardActionId == 0)
        {
            Svc.Log.Warning("AutoGuard: could not resolve Guard action ID!");
        }
    }

    /// <summary>Called from the packet hook. Keep it tiny: just set a flag.</summary>
    private void OnSpiteDetected(uint casterEntityId)
    {
        var now = DateTime.Now;

        // The game can send more than one packet per cast; ignore repeats.
        if ((now - lastDetection).TotalMilliseconds < DedupMs)
        {
            return;
        }

        lastDetection = now;
        guardRequested = true;
    }

    /// <summary>Runs on the game's main thread, so using actions is safe here.</summary>
    private void OnUpdate(IFramework framework)
    {
        if (!guardRequested)
        {
            return;
        }

        guardRequested = false;

        config.AutoGuardTimesDetected++;
        SetEvent("Marksman's Spite DETECTED!");

        if (!config.AutoGuardEnabled)
        {
            Svc.Log.Info("AutoGuard: Spite detected but Auto-Guard is disabled.");
            config.Save();
            return;
        }

        Svc.Chat.Print("[Kirbo] Marksman's Spite incoming!");

        if (guardExecutor.UseGuard())
        {
            config.AutoGuardTimesGuarded++;
            SetEvent("Guard ACTIVATED!");
            Svc.Chat.Print("[Kirbo] Guard activated!");
        }
        else
        {
            SetEvent("Guard ON COOLDOWN!");
            if (config.AutoGuardAlertOnCooldown)
            {
                Svc.Chat.Print("[Kirbo] Spite incoming but Guard is not available!");
            }
        }

        config.Save();
    }

    private void SetEvent(string message)
    {
        LastEvent = message;
        LastEventTime = DateTime.Now;
    }

    public void Dispose()
    {
        Svc.Framework.Update -= OnUpdate;

        if (detector != null)
        {
            detector.SpiteTargetedLocalPlayer -= OnSpiteDetected;
            detector.Dispose();
        }
    }
}
