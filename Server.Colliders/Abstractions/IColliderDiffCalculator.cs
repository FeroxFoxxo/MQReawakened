
using Server.Colliders.DTOs;

namespace Server.Colliders.Abstractions;
public interface IColliderDiffCalculator
{
    ColliderDiffResult Calculate(RoomCollidersDto previous, RoomCollidersDto current);
}
