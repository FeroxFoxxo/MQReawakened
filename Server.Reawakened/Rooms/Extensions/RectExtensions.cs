using Server.Reawakened.Rooms.Models.Planes;
using UnityEngine;

namespace Server.Reawakened.Rooms.Extensions;

public static class RectExtensions
{
    public static RectModel GetSurroundingRect(this IReadOnlyList<Vector2> polygon)
    {
        if (polygon.Count <= 0)
            return default;

        var vector = polygon[0];
        var x = vector.x;
        var vector2 = polygon[0];
        var x2 = vector2.x;
        var vector3 = polygon[0];
        var y = vector3.y;
        var vector4 = polygon[0];
        var y2 = vector4.y;

        for (var i = 1; i < polygon.Count; i++)
        {
            var vector5 = polygon[i];
            if (vector5.x < x)
            {
                var vector6 = polygon[i];
                x = vector6.x;
            }

            var vector7 = polygon[i];
            if (vector7.x > x2)
            {
                var vector8 = polygon[i];
                x2 = vector8.x;
            }

            var vector9 = polygon[i];
            if (vector9.y < y)
            {
                var vector10 = polygon[i];
                y = vector10.y;
            }

            var vector11 = polygon[i];
            // ReSharper disable once InvertIf
            if (vector11.y > y2)
            {
                var vector12 = polygon[i];
                y2 = vector12.y;
            }
        }

        return new RectModel(x, y, x2 - x, y2 - y);
    }
}
