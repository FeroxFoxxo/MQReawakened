using Microsoft.AspNetCore.SignalR;
using Server.Colliders.Abstractions;
using Server.Colliders.DTOs;
using Web.Razor.Hubs;

namespace Web.Razor.Services;

public class SignalRColliderPublisher(IHubContext<ColliderHub> _hub) : IColliderUpdatePublisher
{
    private static string Group(int levelId,int roomInstanceId) => $"room:{levelId}:{roomInstanceId}";

    public Task PublishResetAsync(RoomCollidersDto snapshot, long version, ColliderBoundsDto bounds, ColliderStatsDto stats, CancellationToken ct) =>
        _hub.Clients.Group(Group(snapshot.LevelId, snapshot.RoomInstanceId)).SendAsync("collidersReset", new {
            levelId = snapshot.LevelId,
            roomInstanceId = snapshot.RoomInstanceId,
            version,
            colliders = snapshot.Colliders,
            stats = new { added = stats.Added, removed = stats.Removed, updated = stats.Updated },
            bbox = new { minX = bounds.MinX, minY = bounds.MinY, maxX = bounds.MaxX, maxY = bounds.MaxY, width = bounds.Width, height = bounds.Height }
        }, ct);

    public Task PublishDiffAsync(ColliderDiffResult diff, CancellationToken ct)
    {
        var added = diff.Added.Select(a => new { a.Id, a.Type, a.Plane, a.Active, a.Invisible, x = a.X, y = a.Y, width = a.Width, height = a.Height }).ToArray();
        var updated = diff.Updated.Select(a => new { id = a.Id, a.Type, a.Plane, a.Active, a.Invisible, x = a.X, y = a.Y, width = a.Width, height = a.Height }).ToArray();
        return _hub.Clients.Group(Group(diff.LevelId, diff.RoomInstanceId)).SendAsync("collidersDiff", new {
            levelId = diff.LevelId,
            roomInstanceId = diff.RoomInstanceId,
            version = diff.Version,
            added,
            removed = diff.Removed,
            updated,
            stats = new { added = diff.Stats.Added, removed = diff.Stats.Removed, updated = diff.Stats.Updated },
            bbox = new { minX = diff.Bounds.MinX, minY = diff.Bounds.MinY, maxX = diff.Bounds.MaxX, maxY = diff.Bounds.MaxY, width = diff.Bounds.Width, height = diff.Bounds.Height }
        }, ct);
    }

    public Task PublishRoomAddedAsync(RoomCollidersDto snapshot, CancellationToken ct) =>
        _hub.Clients.All.SendAsync("roomAdded", new {
            levelId = snapshot.LevelId,
            roomInstanceId = snapshot.RoomInstanceId,
            name = snapshot.Name
        }, ct);

    public Task PublishRoomRemovedAsync(int levelId, int roomInstanceId, CancellationToken ct) =>
        _hub.Clients.All.SendAsync("roomRemoved", new { levelId, roomInstanceId }, ct);
}
