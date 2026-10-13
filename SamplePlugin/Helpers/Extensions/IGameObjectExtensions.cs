using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Common.Component.BGCollision;
using Lumina.Excel.Sheets;

namespace SamplePlugin.Helpers.Extensions;

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
        if (obj == null) return null;
        var cs = (GameObject*)obj.Address;
        return cs == null || cs->EventHandler == null ? null : cs->EventHandler->Info;
    }
    public static unsafe bool IsInInteractRange(this IGameObject obj) => EventFramework.Instance()->CheckInteractRange((GameObject*)IObjectTable.Get().LocalPlayer!.Address, (GameObject*)obj.Address, 1, false);

}
