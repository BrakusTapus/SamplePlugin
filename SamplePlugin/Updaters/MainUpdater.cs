//using System.Collections.Generic;
using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Gui.NamePlate;
using Dalamud.Plugin.Services;
using ECommons.DalamudServices;
using SamplePlugin.Helpers;

namespace SamplePlugin.Updaters;

internal static class MainUpdater
{
    public static IReadOnlyList<IBattleChara> AllBattleCharas => _allBattleCharas;
    private static readonly List<IBattleChara> _allBattleCharas = new();
    public static IReadOnlyList<IGameObject> AllGameObjects => _allGameObjects;
    private static readonly List<IGameObject> _allGameObjects = new();

    public static void Enable()
    {
        Svc.Framework.Update += KirboUpdate;
    }

    public static void Dispose()
    {
        Svc.Framework.Update -= KirboUpdate;
    }

    private static void KirboUpdate(IFramework framework)
    {
        UpdateTargets();
        UpdateGameObjects();
    }

    internal static void UpdateTargets()
    {
        _allBattleCharas.Clear();

        foreach (IGameObject obj in Svc.Objects)
        {
            if (obj is IBattleChara battleChara && obj.Name.ToString() != string.Empty)
            {
                _allBattleCharas.Add(battleChara);
            }
        }

        // Remove entries from _allBattleCharas if:
        // - BattleChara is null
        // - BattleChara is not targetable
        // - Distance to player is >= 45
        _allBattleCharas.RemoveAll(battleChara =>
            battleChara == null ||
            !battleChara.IsTargetable ||
            ObjectHelper.DistanceToPlayer(battleChara) >= 55);
    }

    internal static void UpdateGameObjects()
    {
        _allGameObjects.Clear();
        foreach (IGameObject gameObject in Svc.Objects)
        {
            if (gameObject is IGameObject obj && (gameObject.Name.ToString() != string.Empty && gameObject != null))
            {
                _allGameObjects.Add(obj);
            }
        }

        _allGameObjects.RemoveAll(gameObject =>
            gameObject == null ||
            //!gameObject.IsTargetable ||
            ObjectHelper.DistanceToPlayer(gameObject) >= 55);
    }
}



//using Dalamud.Game.ClientState.Objects.Types;
//using Dalamud.Plugin.Services;
//using ECommons.DalamudServices;

//namespace SamplePlugin.Updaters;

//internal static class MainUpdater
//{
//    public static void Enable()
//    {
//        Svc.Framework.Update += KirboUpdate;
//    }

//    public static void Dispose()
//    {
//        Svc.Framework.Update -= KirboUpdate;
//    }

//    private static void KirboUpdate(IFramework framework)
//    {
//        UpdateTargets();
//    }

//    public static List<IBattleChara> AllBattleCharas { get; set; } = [];
//    internal static void UpdateTargets()
//    {
//        AllBattleCharas = GetAllTargets();
//    }

//    private static List<IBattleChara> GetAllTargets()
//    {
//        List<IBattleChara> allTargets = new List<IBattleChara>();
//        foreach (IGameObject obj in Svc.Objects)
//        {
//            if (obj is IBattleChara battleChara)
//            {
//                allTargets.Add(battleChara);
//            }
//        }
//        return allTargets;
//    }
//}
