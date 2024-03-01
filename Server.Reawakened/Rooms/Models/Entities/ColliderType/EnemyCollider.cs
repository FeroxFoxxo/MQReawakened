using A2m.Server;
using Server.Reawakened.Rooms.Models.Planes;

namespace Server.Reawakened.Rooms.Models.Entities.ColliderType;
public class EnemyCollider(string id, Vector3Model position, float sizeX, float sizeY, string plane, Room room) : BaseCollider(id, position, sizeX, sizeY, plane, room, "enemy")
{
    public override void SendCollisionEvent(BaseCollider received)
    {
        if (received is AttackCollider attack)
        {
            Room.Enemies.TryGetValue(Id, out var enemy);
            if (enemy != null)
            {
                var damage = enemy.GetDamageType(attack.Damage, attack.DamageType);
                enemy.Damage(damage, attack.Owner);
            }
        }
    }
}
