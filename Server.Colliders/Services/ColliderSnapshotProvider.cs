using Server.Base.Core.Abstractions;
using Server.Colliders.DTOs;
using Server.Reawakened.Rooms.Services;
using Server.Reawakened.Entities.Enemies.Extensions;

namespace Server.Colliders.Services;

public class ColliderSnapshotProvider(WorldHandler _world) : IService
{
    public void Initialize() { }
    public RoomCollidersDto[] GetSnapshots() => [.. _world.GetOpenRooms().Select(room =>
        {
            var colliders = ColliderHelper.BuildCollidersForRoom(room);
            var instance = int.Parse(room.ToString().Split('_').Last());
            return new RoomCollidersDto(room.LevelInfo.LevelId, instance, room.LevelInfo.Name, colliders);
        })];
}
