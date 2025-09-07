using Server.Colliders.DTOs;

namespace Server.Colliders.Abstractions;

public interface IColliderUpdatePublisher
{
    Task PublishResetAsync(RoomCollidersDto snapshot, long version, ColliderBoundsDto bounds, ColliderStatsDto stats, CancellationToken ct);
    Task PublishDiffAsync(ColliderDiffResult diff, CancellationToken ct);
    Task PublishRoomAddedAsync(RoomCollidersDto snapshot, CancellationToken ct);
    Task PublishRoomRemovedAsync(int levelId, int roomInstanceId, CancellationToken ct);
}
