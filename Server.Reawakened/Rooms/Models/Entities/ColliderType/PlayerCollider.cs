using Server.Reawakened.Entities.Enums;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities.Colliders;

namespace Server.Reawakened.Rooms.Models.Entities.ColliderType;
public class PlayerCollider(Player player) : BaseCollider(player.TempData.GameObjectId, player.TempData.Position, 1, 1, player.GetPlayersPlaneString(), player.Room, ColliderClass.Player)
{
    public Player Player => player;

    public override void SendCollisionEvent(BaseCollider received)
    {
        if (received.Type is ColliderClass.Player or ColliderClass.Attack)
            return;

        if (received is AIProjectileCollider aiProjectileCollider &&
            received.Type == ColliderClass.AiAttack)
        {
            Room.SendSyncEvent(new StatusEffect_SyncEvent(player.GameObjectId, Room.Time, (int)aiProjectileCollider.Effect,
            0, 1, true, aiProjectileCollider.OwnderId, false));

            var damage = aiProjectileCollider.Damage - player.Character.Data.CalculateDefense(aiProjectileCollider.Effect, aiProjectileCollider.ItemCatalog);

            player.ApplyCharacterDamage(damage, 1, aiProjectileCollider.TimerThread);

            player.TemporaryInvincibility(aiProjectileCollider.TimerThread, 1);

            Room.Colliders.Remove(aiProjectileCollider.PrjId);
        }

        if (received is HazardEffectCollider hazard)
            hazard.ApplyEffectBasedOffHazardType(hazard.Id, player);
    }

    public override string[] IsColliding(bool isAttack)
    {
        var roomList = Room.Colliders.Values.ToList();
        List<string> collidedWith = [];

        foreach (var collider in roomList)
        {
            if (CheckCollision(collider) &&
                collider.Type != ColliderClass.Player && collider.Type != ColliderClass.Attack)
            {
                collidedWith.Add(collider.Id);
                collider.SendCollisionEvent(this);
            }
        }

        return [.. collidedWith];
    }
}
