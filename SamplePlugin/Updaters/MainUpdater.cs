//using System.Collections.Generic;
using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;
using ECommons.DalamudServices;

namespace SamplePlugin.Updaters;

internal static class MainUpdater
{
    public static IReadOnlyList<IBattleChara> AllTargets => _allTargets;
    private static readonly List<IBattleChara> _allTargets = new();

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
    }

    internal static void UpdateTargets()
    {
        _allTargets.Clear();
        foreach (var obj in Svc.Objects)
        {
            if (obj is IBattleChara battleChara && obj.Name.ToString() != string.Empty)
            {
                _allTargets.Add(battleChara);
            }
        }
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

//    public static List<IBattleChara> AllTargets { get; set; } = [];
//    internal static void UpdateTargets()
//    {
//        AllTargets = GetAllTargets();
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
