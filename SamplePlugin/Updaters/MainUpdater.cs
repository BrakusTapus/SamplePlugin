using System;
using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game.Group;
using SamplePlugin.Helpers;

namespace SamplePlugin.Updaters;

internal static class MainUpdater
{
    private const float MaxDistance = 90f;

    public static IReadOnlyList<IGameObject> AllGameObjects => _allGameObjects;
    public static IReadOnlyList<IBattleChara> AllBattleCharas => _allBattleCharas;
    private static readonly List<IGameObject> _allGameObjects = new();
    private static readonly List<IBattleChara> _allBattleCharas = new();

    // Rebuilt every frame so Relation checks are cheap lookups instead of per-object native calls.
    private static readonly HashSet<uint> _partyIds = new();
    private static bool _inAlliance;

    public static bool IsInParty(uint entityId) => _partyIds.Contains(entityId);

    public static unsafe bool IsInAlliance(uint entityId) =>
        _inAlliance && GroupManager.Instance()->MainGroup.IsEntityIdInAlliance(entityId);

    public static void Enable()  => Svc.Framework.Update += OnUpdate;
    public static void Disable() => Svc.Framework.Update -= OnUpdate;

    private static unsafe void OnUpdate(IFramework framework)
    {
        _allGameObjects.Clear();
        _allBattleCharas.Clear();
        _partyIds.Clear();

        // Party comes from Dalamud's party list (it also covers cross-world parties).
        foreach (var member in Svc.Party)
            _partyIds.Add(member.EntityId);

        // Alliance has no Dalamud service, so ask the game. The flag lets us skip the call outside alliances.
        var groups = GroupManager.Instance();
        _inAlliance = groups != null && groups->MainGroup.IsAlliance;

        if (Svc.Objects.LocalPlayer == null) return;

        foreach (var obj in Svc.Objects)
        {
            if (obj.Address == IntPtr.Zero) continue;
            if (obj.DistanceToPlayer() >= MaxDistance) continue;
            if (obj.Name.TextValue.Length == 0) continue; // last: this is the one that allocates

            _allGameObjects.Add(obj);
            if (obj is IBattleChara chara)
                _allBattleCharas.Add(chara);
        }
    }
}
