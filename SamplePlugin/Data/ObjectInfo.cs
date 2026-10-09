using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using SamplePlugin.Helpers;

namespace SamplePlugin.Data;

/// <summary>
/// Layer 1 result: everything we need to know about an object, computed ONCE per object per frame.
/// Catalog rules read from this instead of recomputing role / relation themselves.
/// </summary>
internal readonly record struct ObjectInfo(IGameObject Object, ObjectKind Kind, JobRole Role, Relation Relation)
{
    public static ObjectInfo From(IGameObject obj) => new(
        obj,
        obj.ObjectKind,
        obj is IBattleChara chara ? chara.GetJobRole() : JobRole.None,
        obj.GetRelation());
}
