using System;
using System.Numerics;

using Dalamud.Hooking;

using ECommons.DalamudServices;

using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;

namespace KirboToolbox.Features.AutoGuard;

/// <summary>
/// Hooks the game's "action effect received" function and raises an event
/// when Marksman's Spite is aimed at the local player.
/// </summary>
internal sealed unsafe class SpiteDetector : IDisposable
{
    private delegate void ReceiveActionEffectDelegate(
        uint casterEntityId,
        Character* casterPtr,
        Vector3* targetPos,
        ActionEffectHandler.Header* header,
        ActionEffectHandler.TargetEffects* effects,
        GameObjectId* targetEntityIds);

    private readonly Hook<ReceiveActionEffectDelegate> hook;
    private readonly uint spiteActionId;

    /// <summary>Raised when Marksman's Spite targets the local player. Argument = caster's EntityId.</summary>
    public event Action<uint>? SpiteTargetedLocalPlayer;

    public SpiteDetector(uint spiteActionId)
    {
        this.spiteActionId = spiteActionId;

        var address = (nint)ActionEffectHandler.Addresses.Receive.Value;
        hook = Svc.Hook.HookFromAddress<ReceiveActionEffectDelegate>(address, OnReceiveActionEffect);
        hook.Enable();

        Svc.Log.Info("SpiteDetector: ReceiveActionEffect hook enabled.");
    }

    private void OnReceiveActionEffect(
        uint casterEntityId,
        Character* casterPtr,
        Vector3* targetPos,
        ActionEffectHandler.Header* header,
        ActionEffectHandler.TargetEffects* effects,
        GameObjectId* targetEntityIds)
    {
        try
        {
            if (spiteActionId != 0 && header != null && header->ActionId == spiteActionId)
            {
                var localPlayer = Svc.Objects.LocalPlayer;
                if (localPlayer != null)
                {
                    var myEntityId = localPlayer.EntityId;
                    for (var i = 0; i < header->NumTargets; i++)
                    {
                        if (targetEntityIds[i].ObjectId == myEntityId)
                        {
                            Svc.Log.Info($"SpiteDetector: Marksman's Spite is targeting us! Caster={casterEntityId}");
                            SpiteTargetedLocalPlayer?.Invoke(casterEntityId);
                            break;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Never let an exception escape from a hook, or the game can crash.
            Svc.Log.Error(ex, "SpiteDetector: error in hook.");
        }

        // ALWAYS call the original, otherwise the game stops processing actions.
        hook.Original(casterEntityId, casterPtr, targetPos, header, effects, targetEntityIds);
    }

    public void Dispose()
    {
        hook.Dispose();
        Svc.Log.Info("SpiteDetector: hook disposed.");
    }
}
