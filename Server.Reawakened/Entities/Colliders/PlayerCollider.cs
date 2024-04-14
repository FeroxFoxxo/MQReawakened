using Server.Reawakened.Entities.Colliders.Abstractions;
using Server.Reawakened.Entities.Colliders.Enums;
using Server.Reawakened.Entities.Enemies.EnemyTypes.Abstractions;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Extensions;
using UnityEngine;

namespace Server.Reawakened.Entities.Colliders;
public class PlayerCollider(Player player) :
    BaseCollider(player.TempData.GameObjectId, player.TempData.CopyPosition(),
        new Vector2(1, 1), player.GetPlayersPlaneString(), player.Room, ColliderType.Player
    )
{
    public Player Player => player;

    public override void SendCollisionEvent(BaseCollider received)
    {
        if (received.Type is ColliderType.Player or ColliderType.Attack)
            return;

        if (received is AIProjectileCollider aiProjectileCollider &&
            received.Type == ColliderType.AiAttack)
        {
            var damage = aiProjectileCollider.Damage - player.Character.Data.CalculateDefense(player, aiProjectileCollider.Effect, aiProjectileCollider.ItemCatalog);

            Room.SendSyncEvent(new StatusEffect_SyncEvent(player.GameObjectId, Room.Time, (int)aiProjectileCollider.Effect,
            0, 1, true, aiProjectileCollider.OwnderId, false));

            player.ApplyCharacterDamage(damage, 1, aiProjectileCollider.TimerThread);
            player.TemporaryInvincibility(aiProjectileCollider.TimerThread, 1);

            Room.RemoveCollider(aiProjectileCollider.PrjId);
        }

        if (received is HazardEffectCollider hazard)
            hazard.ApplyEffectBasedOffHazardType(hazard.Id, player);
    }

    public override string[] IsColliding(bool isAttack)
    {
        var colliders = Room.GetColliders();

        List<string> collidedWith = [];

        foreach (var collider in colliders)
            if (CheckCollision(collider) &&
                collider.Type != ColliderType.Player && collider.Type != ColliderType.Attack)
            {
                collidedWith.Add(collider.Id);
                collider.SendCollisionEvent(this);
            }

        return [.. collidedWith];
    }
}
