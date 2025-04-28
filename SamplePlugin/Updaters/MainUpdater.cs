using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;
using ECommons.DalamudServices;
using SamplePlugin.DalamudServices;

namespace SamplePlugin.Updaters;
internal static class MainUpdater
{
    public static void Enable()
    {
        Svc.Framework.Update += KirboUpdate;
    }


    public static void Dispose()
    {
        Svc.Framework.Update -= KirboUpdate;
    }

    private unsafe static void KirboUpdate(IFramework framework)
    {
        UpdateTargets();
    }


    public static List<IBattleChara> AllTargets { get; set; } = [];
    internal static void UpdateTargets()
    {
        AllTargets = GetAllTargets();
    }

    private static List<IBattleChara> GetAllTargets()
    {
        var allTargets = new List<IBattleChara>();
        foreach (var obj in Svc.Objects)
        {
            if (obj is IBattleChara battleChara)
            {
                allTargets.Add(battleChara);
            }
        }
        return allTargets;
    }
}
