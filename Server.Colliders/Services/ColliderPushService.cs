using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Colliders.Abstractions;
using Server.Colliders.DTOs;

namespace Server.Colliders.Services;

public class ColliderPushService(ColliderSnapshotProvider _snapshots,
    InMemoryRoomVersionTracker _versions,
    IColliderUpdatePublisher _publisher,
    InMemoryColliderSubscriptionTracker _subs,
    ILogger<ColliderPushService> _logger) : BackgroundService, IService
{
    private readonly Dictionary<(int, int), RoomCollidersDto> _last = [];

    private static readonly int BaseInterval = Math.Clamp(int.TryParse(Environment.GetEnvironmentVariable("COLLIDER_BASE_INTERVAL_MS"), out var envBase) ? envBase : 250, 20, 5000);
    private int _currentIntervalMs = BaseInterval;
    private const int MaxIdleInterval = 5000;

    public void Initialize() { }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var totalChanges = 0;
            try
            {
                var anySubs = _subs.HasAnySubscribers();
                if (!anySubs)
                {
                    _currentIntervalMs = Math.Min(MaxIdleInterval, (int)(_currentIntervalMs * 1.5));
                    try { await Task.Delay(_currentIntervalMs, stoppingToken); } catch { }
                    continue;
                }
                else if (_currentIntervalMs > BaseInterval)
                {
                    _currentIntervalMs = BaseInterval;
                }
                var current = _snapshots.GetSnapshots();

                foreach (var room in current)
                {
                    var key = (room.LevelId, room.RoomInstanceId);
                    if (!_last.ContainsKey(key))
                    {
                        await _publisher.PublishRoomAddedAsync(room, stoppingToken);
                    }
                }

                foreach (var room in current)
                {
                    var key = (room.LevelId, room.RoomInstanceId);

                    if (!_subs.HasSubscribers(room.LevelId, room.RoomInstanceId))
                    {
                        _last[key] = room;
                        continue;
                    }

                    if (!_last.TryGetValue(key, out var prev))
                    {
                        var version = _versions.Increment(room.LevelId, room.RoomInstanceId);
                        var bounds = CalcBounds(room.Colliders);
                        var stats = new ColliderStatsDto(room.Colliders.Length, 0, 0);
                        await _publisher.PublishResetAsync(room, version, bounds, stats, stoppingToken);
                        _last[key] = room;
                        continue;
                    }

                    var diff = ColliderDiffCalculator.Calculate(prev, room);
                    if (diff.Added.Length == 0 && diff.Removed.Length == 0 && diff.Updated.Length == 0)
                    { _last[key] = room; continue; }
                    var versionWithIncrement = _versions.Increment(room.LevelId, room.RoomInstanceId);
                    diff = diff with { Version = versionWithIncrement, Bounds = CalcBounds(room.Colliders), Stats = new ColliderStatsDto(diff.Added.Length, diff.Removed.Length, diff.Updated.Length) };
                    totalChanges += diff.Added.Length + diff.Removed.Length + diff.Updated.Length;
                    await _publisher.PublishDiffAsync(diff, stoppingToken);
                    _last[key] = room;
                }
                
                var removedKeys = _last.Keys.Where(k => !current.Any(r => r.LevelId == k.Item1 && r.RoomInstanceId == k.Item2)).ToList();
                foreach (var rk in removedKeys)
                {
                    await _publisher.PublishRoomRemovedAsync(rk.Item1, rk.Item2, stoppingToken);
                    _last.Remove(rk);
                }

                _currentIntervalMs = totalChanges == 0
                    ? Math.Min(1500, (int)(_currentIntervalMs * 1.15))
                    : totalChanges > 50 ? Math.Max(50, _currentIntervalMs / 2) : BaseInterval;
            }
            catch (Exception ex)
            { _logger.LogError(ex, "Collider push cycle failure"); }

            try { await Task.Delay(_currentIntervalMs, stoppingToken); } catch { }
        }
    }

    private static ColliderBoundsDto CalcBounds(ColliderDto[] colliders)
    {
        if (colliders.Length == 0) return new ColliderBoundsDto(0, 0, 0, 0, 0, 0);
        var minX = colliders.Min(c => c.X);
        var minY = colliders.Min(c => c.Y);
        var maxX = colliders.Max(c => c.X + c.Width);
        var maxY = colliders.Max(c => c.Y + c.Height);
        return new ColliderBoundsDto(minX, minY, maxX, maxY, maxX - minX, maxY - minY);
    }
}
