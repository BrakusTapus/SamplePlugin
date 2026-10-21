using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;

namespace KirboToolbox.Helpers;
internal static class Utils
{
    internal static float DistanceBetweenObjects(IGameObject source, IGameObject target)
    {
        return DistanceBetweenObjects(source.Position, target.Position, target.HitboxRadius);
    }
    internal static float DistanceBetweenObjects(Vector3 sourcePos, Vector3 targetPos, float targetHitboxRadius = 0)
    {
        // Only the target's hitbox radius is subtracted. The source's hitbox is deliberately ignored for now.
        var distance = Vector3.Distance(sourcePos, targetPos);
        distance -= targetHitboxRadius;
        return distance;
    }
}
