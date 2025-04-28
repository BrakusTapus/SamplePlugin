using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Gui.NamePlate;
using ECommons.DalamudServices;
using ECommons.GameFunctions;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Graphics;
using FFXIVClientStructs.FFXIV.Common.Component.BGCollision;
using SamplePlugin.DalamudServices;
using SamplePlugin.Updaters;

namespace SamplePlugin.Helpers;
internal static class ObjectHelper
{
    /// <summary>
    /// Checks if the nameplate associated with this handler has a NameIconId of 61710, 61711, or 61712.
    /// Only returns true if the associated object is a BattleChara.
    /// </summary>
    /// <param name="handler">The nameplate update handler.</param>
    /// <returns>True if the NameIconId is one of the specified IDs, otherwise false.</returns>
    public static bool IsBossFromNamePlateIconId(this INamePlateUpdateHandler handler)
    {
        if (handler.BattleChara == null)
            return false;

        return handler.NameIconId is 61710 or 61711 or 61712;
    }

    /// <summary>
    /// The distance from <paramref name="obj"/> to the player
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static float DistanceToPlayer(this IGameObject? obj)
    {
        var localPlayer = Svc.ClientState.LocalPlayer;
        if (obj == null) return float.MaxValue;
        if (localPlayer == null) return float.MaxValue;
        if (obj is not IBattleChara b) return float.MaxValue;

        var distance = Vector3.Distance(localPlayer.Position, obj.Position) - (localPlayer.HitboxRadius + obj.HitboxRadius);
        return distance;
    }

    /// <summary>
    /// Determines if the player can see the specified game object.
    /// </summary>
    /// <param name="obj">The game object to check visibility for.</param>
    /// <returns>
    /// <c>true</c> if the player can see the specified game object; otherwise, <c>false</c>.
    /// </returns>
    internal static unsafe bool CanSee(this IGameObject obj)
    {
        if (obj == null) return false;
        if (Player.Object == null) return false;
        if (obj.Struct() == null) return false;

        const uint specificEnemyId = 3830; // Bioculture Node in Aetherial Chemical Research Facility
        if (obj.GameObjectId == specificEnemyId)
        {
            return true;
        }

        var point = Player.Object.Position + Vector3.UnitY * Player.GameObject->Height;
        var tarPt = obj.Position + Vector3.UnitY * obj.Struct()->Height;
        var direction = tarPt - point;

        int* unknown = stackalloc int[] { 0x4000, 0, 0x4000, 0 };

        RaycastHit hit;
        var ray = new Ray(point, direction);

        return !FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance()->BGCollisionModule
            ->RaycastMaterialFilter(&hit, &point, &direction, direction.Length(), 1, unknown);
    }

    /// <summary>
    /// 
    /// </summary>
    public static int NumberOfHostilesInRange
    {
        get
        {
            int count = 0;
            foreach (var o in MainUpdater.AllTargets)
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
