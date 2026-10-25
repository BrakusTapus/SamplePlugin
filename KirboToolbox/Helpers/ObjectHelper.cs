using System.Numerics;

using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Gui.NamePlate;

using ECommons.DalamudServices;
using ECommons.ExcelServices;
using ECommons.GameFunctions;
using ECommons.GameHelpers;

using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Graphics;
using FFXIVClientStructs.FFXIV.Common.Component.BGCollision;

using KirboToolbox.Core.Extensions;
using KirboToolbox.Core.Game;
using KirboToolbox.Core.Trackers;
using KirboToolbox.Features.Highlight;

namespace KirboToolbox.Helpers;

internal static class ObjectHelper
{
    /// <summary>
    /// 
    /// </summary>
    public static int NumberOfHostilesInRange
    {
        get
        {
            int count = 0;
            foreach (IBattleChara o in ObjectTracker.AllBattleCharas)
            {
                if (o.DistanceToPlayer() < 30)
                {
                    count++;
                }
            }
            return count;
        }
    }

    /// <summary>Exactly one relation per object, checked most-specific first.</summary>
    internal static Relation GetRelation(this IGameObject obj)
    {
        if (obj is IBattleChara)
        {
            if (ObjectTracker.IsInParty(obj.EntityId))
            {
                return Relation.Party;
            }

            if (ObjectTracker.IsInAlliance(obj.EntityId))
            {
                return Relation.Alliance;
            }

            if (obj.IsEnemy())
            {
                return Relation.Enemy;
            }
        }
        return Relation.Others;
    }
}
