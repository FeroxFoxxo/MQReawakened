
using Server.Colliders.DTOs;

namespace Server.Colliders.Abstractions;
public interface IColliderSnapshotProvider
{
    RoomCollidersDto[] GetSnapshots();
}
