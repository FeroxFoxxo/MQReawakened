using A2m.Server;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms.Extensions;

namespace Server.Reawakened.Rooms.Models.Entities.ColliderType;
public class PlayerCollider(Player player) : BaseCollider(player.TempData.GameObjectId, player.TempData.Position, 1, 1, player.GetPlayersPlaneString(), player.Room, "player")
{
    public Player Player = player;
    public override void SendCollisionEvent(BaseCollider received)
    {
        if (received is AIProjectileCollider aIProjectileCollider &&
            received.ColliderType != "player" && received.ColliderType != "attack")
        {
            Room.SendSyncEvent(new StatusEffect_SyncEvent(player.GameObjectId, Room.Time, (int)ItemEffectType.BluntDamage,
            0, 1, true, aIProjectileCollider.OwnderId, false));

            player.ApplyDamageByObject(Room, int.Parse(aIProjectileCollider.OwnderId), aIProjectileCollider.TimerThread);

            Room.Colliders.Remove(aIProjectileCollider.PrjId);
        }
    }

    public override string[] IsColliding(bool isAttack)
    {
        var roomList = Room.Colliders.Values.ToList();
        List<string> collidedWith = [];

        foreach (var collider in roomList)
        {
            if (CheckCollision(collider) &&
                collider.ColliderType != "player" && collider.ColliderType != "attack")
            {
                collidedWith.Add(collider.Id);
                collider.SendCollisionEvent(this);
            }
        }

        return [.. collidedWith];
    }
}
