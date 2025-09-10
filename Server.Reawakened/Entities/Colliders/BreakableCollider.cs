using Server.Reawakened.Entities.Colliders.Abstractions;
using Server.Reawakened.Entities.Colliders.Enums;
using Server.Reawakened.Entities.Components.GameObjects.Breakables;
using Server.Reawakened.Entities.Components.GameObjects.Platforms;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.Rooms.Models.Planes;

namespace Server.Reawakened.Entities.Colliders;
public class BreakableCollider(BaseComponent component, bool enemyHurt) : BaseCollider
{
    public bool EnemyHurt = enemyHurt;

    public override Room Room => component.Room;
    public override string Id => component.Id;
    public override Vector3Model Position => component.Position;
    public override RectModel BoundingBox => component.Rectangle;
    public override string Plane => component.ParentPlane;
    public override ColliderType Type => ColliderType.Breakable;

    public override void SendCollisionEvent(BaseCollider received)
    {
        if (received is AttackCollider attack && !EnemyHurt)
            foreach (var breakable in Room.GetEntitiesFromId<BreakableEventControllerComp>(Id))
                breakable.Damage(attack.Damage, attack.DamageType, attack.Owner);

        if (received is AIProjectileCollider aiprj && EnemyHurt)
            foreach (var breakable in Room.GetEntitiesFromId<BreakableEventControllerComp>(Id))
                breakable.Damage(aiprj.Damage, aiprj.OwnderId);

        if (received is PlayerCollider)
            foreach (var collapsingPlatform in Room.GetEntitiesFromId<CollapsingPlatformComp>(Id))
                collapsingPlatform.Collapse(false);
    }
}
