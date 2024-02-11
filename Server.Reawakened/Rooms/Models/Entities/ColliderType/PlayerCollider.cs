using Server.Reawakened.Players;

namespace Server.Reawakened.Rooms.Models.Entities.ColliderType;
public class PlayerCollider(Player player) : BaseCollider(player.TempData.GameObjectId, player.TempData.Position, 1, 1, player.TempData.Position.Z > 10 ? "Plane1" : "Plane0", player.Room, "player")
{
    public override void SendCollisionEvent(BaseCollider received)
    {
        if (received is AIProjectileCollider)
        {
            var attack = (AIProjectileCollider)received;
            Console.WriteLine("holy fuck i just got shot");
        }
    }
}
