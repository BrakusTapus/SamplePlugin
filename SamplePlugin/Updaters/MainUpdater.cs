using System;
using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;
using ECommons.DalamudServices;
using SamplePlugin.Helpers;

namespace SamplePlugin.Updaters;

internal static class MainUpdater
{
    private const float MaxDistance = 90f;

    public static IReadOnlyList<IGameObject> AllGameObjects => _allGameObjects;
    public static IReadOnlyList<IBattleChara> AllBattleCharas => _allBattleCharas;
    private static readonly List<IGameObject> _allGameObjects = new();
    private static readonly List<IBattleChara> _allBattleCharas = new();

    public static void Enable()  => Svc.Framework.Update += OnUpdate;
    public static void Disable() => Svc.Framework.Update -= OnUpdate;

    private static void OnUpdate(IFramework framework)
    {
        _allGameObjects.Clear();
        _allBattleCharas.Clear();

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
