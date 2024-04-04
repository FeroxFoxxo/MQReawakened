using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities.Colliders.Abstractions;

namespace Server.Reawakened.Rooms.Models.Entities.Colliders;
public class PlayerCollider(Player player) : BaseCollider(player.TempData.GameObjectId, player.TempData.Position, 1, 1, player.GetPlayersPlaneString(), player.Room, ColliderType.Player)
{
    public Player Player => player;

    public override void SendCollisionEvent(BaseCollider received)
    {
        if (received.Type is ColliderType.Player or ColliderType.Attack)
            return;

        if (received is AIProjectileCollider aiProjectileCollider &&
            received.Type == ColliderType.AiAttack)
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
            if (CheckCollision(collider) &&
                collider.Type != ColliderType.Player && collider.Type != ColliderType.Attack)
            {
                collidedWith.Add(collider.Id);
                collider.SendCollisionEvent(this);
            }

        return [.. collidedWith];
    }
}
