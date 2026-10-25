using System;
using System.Collections.Generic;

using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;

using ECommons.DalamudServices;

using FFXIVClientStructs.FFXIV.Client.Game.Group;

using KirboToolbox.Core.Extensions;

namespace KirboToolbox.Core.Trackers;

internal static class ObjectTracker
{
    private const float MaxDistance = 90f;

    public static IReadOnlyList<IGameObject> AllGameObjects => GameObjects;
    public static IReadOnlyList<IBattleChara> AllBattleCharas => BattleCharas;
    private static readonly List<IGameObject> GameObjects = new();
    private static readonly List<IBattleChara> BattleCharas = new();

    // Rebuilt every frame so Relation checks are cheap lookups instead of per-object native calls.
    private static readonly HashSet<uint> PartyIds = new();
    private static bool inAlliance;

    public static bool IsInParty(uint entityId) => PartyIds.Contains(entityId);

    public static unsafe bool IsInAlliance(uint entityId) =>
        inAlliance && GroupManager.Instance()->MainGroup.IsEntityIdInAlliance(entityId);

    public static void Enable() => Svc.Framework.Update += OnUpdate;
    public static void Disable() => Svc.Framework.Update -= OnUpdate;

    private static unsafe void OnUpdate(IFramework framework)
    {
        GameObjects.Clear();
        BattleCharas.Clear();
        PartyIds.Clear();

        // Party comes from Dalamud's party list (it also covers cross-world parties).
        foreach (var member in Svc.Party)
        {
            PartyIds.Add(member.EntityId);
        }

        // Alliance has no Dalamud service, so ask the game. The flag lets us skip the call outside alliances.
        var groups = GroupManager.Instance();
        inAlliance = groups != null && groups->MainGroup.IsAlliance;

        if (Svc.Objects.LocalPlayer == null)
        {
            return;
        }

        foreach (var obj in Svc.Objects)
        {
            if (obj.Address == IntPtr.Zero)
            {
                continue;
            }

            if (obj.DistanceToPlayer() >= MaxDistance)
            {
                continue;
            }

            GameObjects.Add(obj);

            if (obj is IBattleChara chara && chara.Name.TextValue.Length > 0)
            {
                BattleCharas.Add(chara);
            }
        }
    }

    // Needs to check for isenemy
    public static int NumberOfHostilesInRange
    {
        get
        {
            int count = 0;
            foreach (IBattleChara o in AllBattleCharas)
            {
                if (o.DistanceToPlayer() < 30)
                {
                    count++;
                }
            }
            return count;
        }
    }
}
