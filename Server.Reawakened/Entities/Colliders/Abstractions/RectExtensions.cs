using UnityEngine;

namespace Server.Reawakened.Entities.Colliders.Abstractions;

internal static class RectExtensions
{
    public static bool Overlaps(this Rect rA, Rect rB) =>
        rA.x < rB.x + rB.width
        && rA.x + rA.width > rB.x
        && rA.y < rB.y + rB.height
        && rA.y + rA.height > rB.y;
}
