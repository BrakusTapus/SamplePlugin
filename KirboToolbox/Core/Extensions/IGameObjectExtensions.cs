using System.Numerics;

using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;

using ECommons.DalamudServices;
using ECommons.GameFunctions;
using ECommons.GameHelpers;

using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Common.Component.BGCollision;

namespace KirboToolbox.Core.Extensions;

public static class IGameObjectExtensions
{
    extension(IGameObject obj)
    {
        public unsafe BattleChara* BattleChara => (BattleChara*)obj.Address;
        public unsafe Character* Character => (Character*)obj.Address;

        public unsafe uint NameplateIconId => ((GameObject*)obj.Address)->NamePlateIconId;
        public unsafe bool IsFlying => obj is ICharacter chr && chr.Character->MoveController.MovementState is MovementStateOptions.Flying;
        public bool IsLeveNode => obj is { ObjectKind: Dalamud.Game.ClientState.Objects.Enums.ObjectKind.GatheringPoint, NameplateIconId: 71244 };

        public float DistanceTo() => Vector3.Distance(obj.Position, IObjectTable.Get().LocalPlayer?.Position ?? obj.Position);
        public float DistanceTo(IGameObject other) => Vector3.Distance(obj.Position, other.Position);
        public float DistanceTo(Vector3 position) => Vector3.Distance(obj.Position, position);

    }

    public static bool WithinRange(this IGameObject? obj, Vector3 position, float range) => obj is not null && Vector3.Distance(obj.Position, position) < range;
    public static unsafe bool IsTargetingPlayer(this IGameObject obj) => obj.TargetObjectId == GameObjectManager.Instance()->Objects.IndexSorted[0].Value->GetGameObjectId().ObjectId;
    public static unsafe EventHandlerInfo? EventInfo(this IGameObject obj)
    {
        if (obj == null)
        {
            return null;
        }

        var cs = (GameObject*)obj.Address;
        return cs == null || cs->EventHandler == null ? null : cs->EventHandler->Info;
    }
    public static unsafe bool IsInInteractRange(this IGameObject obj) => EventFramework.Instance()->CheckInteractRange((GameObject*)IObjectTable.Get().LocalPlayer!.Address, (GameObject*)obj.Address, 1, false);

    /// <summary>
    /// Distance from <paramref name="obj"/> to the player, minus both hitbox radii.
    /// Returns float.MaxValue if either one is missing.
    /// </summary>
    public static float DistanceToPlayer(this IGameObject? obj)
    {
        var localPlayer = Svc.Objects.LocalPlayer;
        if (obj == null || obj.Address == nint.Zero || localPlayer == null)
        {
            return float.MaxValue;
        }

        return Vector3.Distance(localPlayer.Position, obj.Position) - (localPlayer.HitboxRadius + obj.HitboxRadius);
    }

    /// <summary>
    /// Line-of-sight check: can the player see this object?
    /// </summary>
    internal static unsafe bool CanSee(this IGameObject obj)
    {
        if (obj == null || Player.Object == null || obj.Struct() == null)
        {
            return false;
        }

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
        return !FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance()->BGCollisionModule
            ->RaycastMaterialFilter(&hit, &point, &direction, direction.Length(), 1, unknown);
    }

    /// <summary>
    /// True if the player can use an attack spell on this object (Blizzard 142, or its PvP version 29653).
    /// </summary>
    internal static unsafe bool IsEnemy(this IGameObject obj)
    {
        if (obj == null)
        {
            return false;
        }

        if (!obj.IsValid())
        {
            return false;
        }

        if (ActionManager.CanUseActionOnTarget(142, obj.Struct()) || ActionManager.CanUseActionOnTarget(29653, obj.Struct())) // (uint)ActionID.BlizzardPvE is 142 | 29653 = pvp variant
        {
            return true;
        }

        return false;
    }
}
