using A2m.Server;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.XMLs.Bundles;

namespace Server.Reawakened.Rooms.Models.Entities.ColliderType;
public class PlayerCollider(Player player) : BaseCollider(player.TempData.GameObjectId, player.TempData.Position, 1, 1, player.GetPlayersPlaneString(), player.Room, "player")
{
    public override void SendCollisionEvent(BaseCollider received)
    {
        if (received is AIProjectileCollider aIProjectileCollider &&
            received.ColliderType != "player" && received.ColliderType != "attack")
        {
            Room.SendSyncEvent(new StatusEffect_SyncEvent(player.GameObjectId, Room.Time, (int)ItemEffectType.BluntDamage,
            0, 1, true, aIProjectileCollider.OwnderId, false));

            var damage = received.Damage - player.Character.Data.CalculateDefense(received.Effect, received.ItemCatalog);

            player.ApplyCharacterDamage(Room, damage);

            player.SetTemporaryInvincibility(aIProjectileCollider.TimerThread, 1.3);

            Room.Colliders.Remove(aIProjectileCollider.PrjId);
        }
    }
}
