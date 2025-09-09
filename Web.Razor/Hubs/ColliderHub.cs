using Microsoft.AspNetCore.SignalR;
using Server.Colliders.DTOs;
using Server.Colliders.Services;

namespace Web.Razor.Hubs;

public class ColliderHub : Hub
{
    private readonly ColliderSnapshotProvider _snapshots;
    private readonly InMemoryColliderSubscriptionTracker _subs;

    public ColliderHub(ColliderSnapshotProvider snapshots, InMemoryColliderSubscriptionTracker subs)
    { _snapshots = snapshots; _subs = subs; }

    public static async Task<string> Ping() => await Task.FromResult("pong");

    public async Task<RoomCollidersDto[]> GetRooms() => await Task.FromResult(_snapshots.GetSnapshots());

    public async Task<RoomCollidersDto> SubscribeRoom(int levelId, int roomInstanceId)
    {
        var room = _snapshots.GetSnapshots().FirstOrDefault(r => r.LevelId == levelId && r.RoomInstanceId == roomInstanceId);
        if (room == null) return null;
        await Groups.AddToGroupAsync(Context.ConnectionId, $"room:{room.LevelId}:{room.RoomInstanceId}");
        _subs.Subscribe(Context.ConnectionId, room.LevelId, room.RoomInstanceId);
        await Clients.Group($"room:{room.LevelId}:{room.RoomInstanceId}").SendAsync("roomSubscriberCount", new {
            levelId = room.LevelId,
            roomInstanceId = room.RoomInstanceId,
            count = _subs.GetRoomSubscriberCount(room.LevelId, room.RoomInstanceId)
        });
        return room;
    }

    public async Task UnsubscribeRoom(int levelId, int roomInstanceId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"room:{levelId}:{roomInstanceId}");
        _subs.Unsubscribe(Context.ConnectionId, levelId, roomInstanceId);
        await Clients.Group($"room:{levelId}:{roomInstanceId}").SendAsync("roomSubscriberCount", new {
            levelId,
            roomInstanceId,
            count = _subs.GetRoomSubscriberCount(levelId, roomInstanceId)
        });
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var affected = _subs.RemoveAll(Context.ConnectionId);
        foreach (var (levelId, roomInstanceId) in affected)
        {
            await Clients.Group($"room:{levelId}:{roomInstanceId}").SendAsync("roomSubscriberCount", new {
                levelId,
                roomInstanceId,
                count = _subs.GetRoomSubscriberCount(levelId, roomInstanceId)
            });
        }
        await base.OnDisconnectedAsync(exception);
    }

    public Task<int> GetSubscriberCount(int levelId, int roomInstanceId) =>
        Task.FromResult(_subs.GetRoomSubscriberCount(levelId, roomInstanceId));
}
