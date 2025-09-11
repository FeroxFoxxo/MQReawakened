using Server.Reawakened.Entities.Colliders.Abstractions;
using Server.Reawakened.Entities.Colliders.Enums;
using Server.Reawakened.Entities.Enemies.EnemyTypes.Abstractions;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Models.Planes;

namespace Server.Reawakened.Entities.Colliders;
public class EnemyDetectionCollider(BaseEnemy enemy, RectModel box) : BaseCollider(false)
{
    public override Vector3Model Position => enemy.Position;
    public override Room Room => enemy.Room;
    public override string Id => enemy.Id;
    public override RectModel BoundingBox => box;
    public override string Plane => enemy.ParentPlane;
    public override ColliderType Type => ColliderType.EnemyDetection;
}
