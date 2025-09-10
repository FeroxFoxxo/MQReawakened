using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Server.Colliders.DTOs;
using Server.Colliders.Services;
using Web.Razor.Hubs;

namespace Web.Razor.Services;

public class ColliderPushService(ColliderSnapshotProvider _snapshots, IHubContext<ColliderHub> _hub, ILogger<ColliderPushService> _logger, InMemoryRoomVersionTracker _versions, InMemoryColliderSubscriptionTracker _subs) : BackgroundService
{
    private readonly int _baseIntervalMs = Math.Clamp(int.TryParse(Environment.GetEnvironmentVariable("COLLIDER_BASE_INTERVAL_MS"), out var envBase) ? envBase : 250, 20, 5000);
    private int _currentIntervalMs = Math.Clamp(int.TryParse(Environment.GetEnvironmentVariable("COLLIDER_BASE_INTERVAL_MS"), out var envStart) ? envStart : 250, 20, 5000);
    private const int MaxIdleInterval = 5000;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        RoomCollidersDto[] last = [];
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var anySubs = _subs.HasAnySubscribers();
                if (!anySubs)
                {
                    _currentIntervalMs = Math.Min(MaxIdleInterval, (int)(_currentIntervalMs * 1.5));
                    await Task.Delay(_currentIntervalMs, stoppingToken);
                    continue;
                }
                else if (_currentIntervalMs > _baseIntervalMs)
                { _currentIntervalMs = _baseIntervalMs; }
                var current = _snapshots.GetSnapshots();
                if (last.Length > 0)
                {
                    var newRooms = current.Where(c => !last.Any(l => l.LevelId == c.LevelId && l.RoomInstanceId == c.RoomInstanceId));
                    foreach (var nr in newRooms)
                    {
                        await _hub.Clients.All.SendAsync("roomAdded", new
                        {
                            levelId = nr.LevelId,
                            roomInstanceId = nr.RoomInstanceId,
                            name = nr.Name
                        }, stoppingToken);
                    }
                }
                var totalChanges = 0;
                foreach (var room in current)
                {
                    var group = $"room:{room.LevelId}:{room.RoomInstanceId}";
                    var prev = last.FirstOrDefault(r => r.LevelId == room.LevelId && r.RoomInstanceId == room.RoomInstanceId);

                    if (!_subs.HasSubscribers(room.LevelId, room.RoomInstanceId))
                    {
                        continue;
                    }

                    if (prev == null)
                    {
                        var ver = _versions.Increment(room.LevelId, room.RoomInstanceId);
                        var bbox = ColliderPushServiceBounds.Calc(room.Colliders);
                        await _hub.Clients.Group(group).SendAsync("collidersReset", new
                        {
                            levelId = room.LevelId,
                            roomInstanceId = room.RoomInstanceId,
                            version = ver,
                            colliders = room.Colliders,
                            stats = new { added = room.Colliders.Length, removed = 0, updated = 0 },
                            bbox
                        }, stoppingToken);
                        continue;
                    }

                    var prevMap = prev.Colliders.GroupBy(c => c.Id).ToDictionary(g => g.Key, g => g.Last());
                    var currMap = room.Colliders.GroupBy(c => c.Id).ToDictionary(g => g.Key, g => g.Last());

                    var added = currMap.Keys.Except(prevMap.Keys).Select(id => currMap[id]).ToArray();
                    var removed = prevMap.Keys.Except(currMap.Keys).ToArray();
                    var updated = new List<object>();

                    foreach (var id in currMap.Keys.Intersect(prevMap.Keys))
                    {
                        var a = currMap[id];
                        var b = prevMap[id];
                        if (a.Active != b.Active || a.X != b.X || a.Y != b.Y || a.Width != b.Width || a.Height != b.Height || a.Invisible != b.Invisible || a.Type != b.Type || a.Plane != b.Plane)
                        {
                            updated.Add(new
                            {
                                id = a.Id,
                                a.Type,
                                a.Plane,
                                a.Active,
                                a.Invisible,
                                x = a.X,
                                y = a.Y,
                                width = a.Width,
                                height = a.Height
                            });
                        }
                    }

                    if (added.Length > 0 || removed.Length > 0 || updated.Count > 0)
                    {
                        var ver = _versions.Increment(room.LevelId, room.RoomInstanceId);
                        totalChanges += added.Length + removed.Length + updated.Count;
                        var bbox = ColliderPushServiceBounds.Calc(room.Colliders);
                        await _hub.Clients.Group(group).SendAsync("collidersDiff", new
                        {
                            levelId = room.LevelId,
                            roomInstanceId = room.RoomInstanceId,
                            version = ver,
                            added = added.Select(a => new { a.Id, a.Type, a.Plane, a.Active, a.Invisible, x = a.X, y = a.Y, width = a.Width, height = a.Height }).ToArray(),
                            removed,
                            updated,
                            stats = new { added = added.Length, removed = removed.Length, updated = updated.Count },
                            bbox
                        }, stoppingToken);
                    }
                }

                if (last.Length > 0)
                {
                    var closedRooms = last.Where(l => !current.Any(c => c.LevelId == l.LevelId && c.RoomInstanceId == l.RoomInstanceId));
                    foreach (var cr in closedRooms)
                    {
                        await _hub.Clients.All.SendAsync("roomRemoved", new
                        {
                            levelId = cr.LevelId,
                            roomInstanceId = cr.RoomInstanceId
                        }, stoppingToken);
                    }
                }

                last = current;

                _currentIntervalMs = totalChanges switch
                {
                    0 => Math.Min(1500, (int)(_currentIntervalMs * 1.15)),
                    > 50 => Math.Max(50, _currentIntervalMs / 2),
                    _ => _baseIntervalMs
                };
            }
            catch (Exception ex)
            { _logger.LogError(ex, "Collider push failure"); }

            await Task.Delay(_currentIntervalMs, stoppingToken);
        }
    }
}

internal static class ColliderPushServiceBounds
{
    public static object Calc(ColliderDto[] colliders)
    {
        if (colliders.Length == 0)
            return new { minX = 0f, minY = 0f, maxX = 0f, maxY = 0f, width = 0f, height = 0f };
        var minX = colliders.Min(c => c.X);
        var minY = colliders.Min(c => c.Y);
        var maxX = colliders.Max(c => c.X + c.Width);
        var maxY = colliders.Max(c => c.Y + c.Height);
        return new { minX, minY, maxX, maxY, width = maxX - minX, height = maxY - minY };
    }
}
