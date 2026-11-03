using System;

using ECommons.DalamudServices;

using FFXIVClientStructs.FFXIV.Client.Game;

namespace KirboToolbox.Features.AutoGuard;

/// <summary>Knows how to check and use the PvP "Guard" action.</summary>
internal sealed unsafe class GuardExecutor // need to add a configurable delay when activating guard when player is targeted by spite
{
    public uint GuardActionId { get; set; }

    /// <summary>True only if Guard can really be used right now (right job/zone, off cooldown, alive...).</summary>
    public bool IsGuardReady()
    {
        if (GuardActionId == 0)
        {
            return false;
        }

        var am = ActionManager.Instance();
        if (am == null)
        {
            return false;
        }

        return am->GetActionStatus(ActionType.Action, GuardActionId) == 0;
    }

    public float GetCooldownRemaining()
    {
        if (GuardActionId == 0)
        {
            return 0f;
        }

        var am = ActionManager.Instance();
        if (am == null)
        {
            return 0f;
        }

        var total = am->GetRecastTime(ActionType.Action, GuardActionId);
        var elapsed = am->GetRecastTimeElapsed(ActionType.Action, GuardActionId);

        return total <= 0 ? 0f : Math.Max(0f, total - elapsed);
    }

    public bool UseGuard()
    {
        if (GuardActionId == 0)
        {
            Svc.Log.Warning("GuardExecutor: Guard action ID not set.");
            return false;
        }

        var am = ActionManager.Instance();
        if (am == null)
        {
            Svc.Log.Warning("GuardExecutor: ActionManager is null.");
            return false;
        }

        if (!am->IsActionOffCooldown(ActionType.Action, GuardActionId))
        {
            Svc.Log.Warning($"GuardExecutor: Guard is on cooldown ({GetCooldownRemaining():F1}s left).");
            return false;
        }

        // 0xE0000000 = "target self"
        var result = am->UseAction(ActionType.Action, GuardActionId, 0xE0000000);
        Svc.Log.Info($"GuardExecutor: UseAction(Guard) returned {result}");
        return result;
    }
}
