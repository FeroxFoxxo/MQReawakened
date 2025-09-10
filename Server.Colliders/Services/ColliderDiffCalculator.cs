using Server.Colliders.DTOs;

namespace Server.Colliders.Services;

public static class ColliderDiffCalculator
{
    public static ColliderDiffResult Calculate(RoomCollidersDto previous, RoomCollidersDto current)
    {
        var prevMap = previous.Colliders.GroupBy(c => c.Id).ToDictionary(g => g.Key, g => g.Last());
        var currMap = current.Colliders.GroupBy(c => c.Id).ToDictionary(g => g.Key, g => g.Last());
        var added = currMap.Keys.Except(prevMap.Keys).Select(id => currMap[id]).ToArray();
        var removed = prevMap.Keys.Except(currMap.Keys).ToArray();
        var updated = currMap.Keys.Intersect(prevMap.Keys)
            .Select(id => new { A = currMap[id], B = prevMap[id] })
            .Where(x => x.A.Active != x.B.Active || x.A.Invisible != x.B.Invisible || x.A.X != x.B.X || x.A.Y != x.B.Y || x.A.Width != x.B.Width || x.A.Height != x.B.Height || x.A.Type != x.B.Type || x.A.Plane != x.B.Plane)
            .Select(x => x.A)
            .ToArray();

        var bounds = CalcBounds(current.Colliders);
        var stats = new ColliderStatsDto(added.Length, removed.Length, updated.Length);
        return new ColliderDiffResult(current.LevelId, current.RoomInstanceId, 0, added, removed, updated, bounds, stats);
    }

    private static ColliderBoundsDto CalcBounds(ColliderDto[] colliders)
    {
        if (colliders.Length == 0) return new ColliderBoundsDto(0,0,0,0,0,0);
        var minX = colliders.Min(c => c.X);
        var minY = colliders.Min(c => c.Y);
        var maxX = colliders.Max(c => c.X + c.Width);
        var maxY = colliders.Max(c => c.Y + c.Height);
        return new ColliderBoundsDto(minX, minY, maxX, maxY, maxX - minX, maxY - minY);
    }
}
