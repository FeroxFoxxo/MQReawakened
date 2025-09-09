using Server.Base.Core.Abstractions;
using Server.Colliders.DTOs;
using Server.Reawakened.Rooms.Services;

namespace Server.Colliders.Services;

public class ColliderSnapshotProvider(WorldHandler _world) : IService
{
    public void Initialize() { }

    public RoomCollidersDto[] GetSnapshots() => [.. _world.GetOpenRooms().Select(room =>
        {
            var colliders = room.GetColliders().Select(c => new ColliderDto(
                c.Id,
                c.Type.ToString(),
                c.Plane,
                c.Active,
                c.IsInvisible,
                c.Position.x,
                c.Position.y,
                c.BoundingBox.width,
                c.BoundingBox.height)).ToArray();
            
            var instance = int.Parse(room.ToString().Split('_').Last());
            return new RoomCollidersDto(room.LevelInfo.LevelId, instance, room.LevelInfo.Name, colliders);
        })];
}
