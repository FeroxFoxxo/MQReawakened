using Microsoft.AspNetCore.SignalR;
using Server.Colliders.Abstractions;
using Server.Colliders.DTOs;

namespace Web.Razor.Hubs;

public class ColliderHub : Hub
{
    private readonly IColliderSnapshotProvider _snapshots;

    public ColliderHub(IColliderSnapshotProvider snapshots) => _snapshots = snapshots;

    public static async Task<string> Ping() => await Task.FromResult("pong");

    public async Task<RoomCollidersDto[]> GetRooms() => await Task.FromResult(_snapshots.GetSnapshots());

    public async Task<RoomCollidersDto> SubscribeRoom(int levelId, int roomInstanceId)
    {
        var room = _snapshots.GetSnapshots().FirstOrDefault(r => r.LevelId == levelId && r.RoomInstanceId == roomInstanceId);
        if (room == null) return null;
        await Groups.AddToGroupAsync(Context.ConnectionId, $"room:{room.LevelId}:{room.RoomInstanceId}");
        return room;
    }

    public async Task UnsubscribeRoom(int levelId, int roomInstanceId) =>
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"room:{levelId}:{roomInstanceId}");
}
