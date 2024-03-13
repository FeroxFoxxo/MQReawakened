using A2m.Server;
using Server.Reawakened.Entities.Enums;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.XMLs.Bundles;

namespace Server.Reawakened.Rooms.Models.Entities.ColliderType;
public class PlayerCollider(Player player) : BaseCollider(player.TempData.GameObjectId, player.TempData.Position, 1, 1, player.GetPlayersPlaneString(), player.Room, ColliderClass.Player)
{
    public Player Player = player;
    public override void SendCollisionEvent(BaseCollider received)
    {
        if (received is AIProjectileCollider aiProjectileCollider &&
            received.Type == ColliderClass.AiAttack)
        {
            Room.SendSyncEvent(new StatusEffect_SyncEvent(player.GameObjectId, Room.Time, (int)aiProjectileCollider.Effect,
            0, 1, true, aiProjectileCollider.OwnderId, false));

            var damage = aiProjectileCollider.Damage - player.Character.Data.CalculateDefense(aiProjectileCollider.Effect, aiProjectileCollider.ItemCatalog);

            player.ApplyCharacterDamage(Room, damage, aiProjectileCollider.TimerThread);

            player.SetTemporaryInvincibility(aiProjectileCollider.TimerThread, 1.3);

            Room.Colliders.Remove(aiProjectileCollider.PrjId);
        }
    }
}
