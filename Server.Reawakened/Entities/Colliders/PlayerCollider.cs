using A2m.Server;
using Server.Reawakened.Entities.Colliders.Abstractions;
using Server.Reawakened.Entities.Colliders.Enums;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Planes;

namespace Server.Reawakened.Entities.Colliders;

public class PlayerCollider(Player player) : BaseCollider
{
    public Player Player => player;
    public override Vector3Model Position => Player.TempData.Position;
    public override Room Room => player.Room;
    public override string Id => player.TempData.GameObjectId;
    public override RectModel BoundingBox => new (-0.5f, 0, 1, 1);
    public override string Plane => player.GetPlayersPlaneString();
    public override ColliderType Type => ColliderType.Player;

    public override void SendCollisionEvent(BaseCollider received)
    {
        if (received.Type is ColliderType.Player or ColliderType.Attack)
            return;

        if (received is AIProjectileCollider aiProjectileCollider &&
            received.Type == ColliderType.AiAttack)
        {
            var damage = aiProjectileCollider.Damage - player.Character.CalculateDefense
                (aiProjectileCollider.Effect, aiProjectileCollider.ItemCatalog);

            player.ApplyCharacterDamage(damage, aiProjectileCollider.Id, 1, aiProjectileCollider.ServerRConfig, aiProjectileCollider.TimerThread);
            player.TemporaryInvincibility(aiProjectileCollider.TimerThread, aiProjectileCollider.ServerRConfig, 1);

            Room.RemoveCollider(aiProjectileCollider.Id);
        }

        if (received is HazardEffectCollider hazard)
            hazard.ApplyEffectBasedOffHazardType(hazard.Id, player);

        if (received is StomperZoneCollider stomper)
        {
            if (!player.TempData.Invincible)
            {
                if (stomper.Hazard)
                    player.ApplyDamageByPercent(0.1, stomper.Id, 1, stomper.ServerRConfig, stomper.TimerThread);

                Room.SendSyncEvent(new StatusEffect_SyncEvent(player.GameObjectId, Room.Time, (int)ItemEffectType.StompDamage, 0, 1, true, stomper.Id, false));
                player.TemporaryInvincibility(stomper.TimerThread, stomper.ServerRConfig, 1);
            }
        }
    }

    public override string[] RunCollisionDetection(bool isAttack)
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
