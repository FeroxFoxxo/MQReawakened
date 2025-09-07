using Server.Base.Core.Abstractions;
using Server.Colliders.Abstractions;
using Server.Colliders.DTOs;
using Server.Reawakened.Rooms.Services;

namespace Server.Colliders.Services;

public class ColliderSnapshotProvider : IService, IColliderSnapshotProvider
{
    private readonly WorldHandler _world;
    public ColliderSnapshotProvider(WorldHandler world) => _world = world;
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
