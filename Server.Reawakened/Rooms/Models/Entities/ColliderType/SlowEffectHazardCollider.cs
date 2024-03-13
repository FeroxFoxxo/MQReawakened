using Server.Reawakened.Entities.Components;
using Server.Reawakened.Rooms.Models.Planes;

namespace Server.Reawakened.Rooms.Models.Entities.ColliderType;
public class SlowEffectHazardCollider(string breakableId, Vector3Model position, float sizeX, float sizeY, string plane, Room room) : BaseCollider(breakableId, position, sizeX, sizeY, plane, room, "breakable")
{
    public override void SendCollisionEvent(BaseCollider received)
    {
        if (received is PlayerCollider playerCollider)
            foreach (var slowEffectHazard in Room.GetEntitiesFromId<HazardControllerComp>(breakableId))
            {
                if (playerCollider.Player.TempData.OnGround)
                    slowEffectHazard.SlowStatusEffect(playerCollider.Id);
                else
                    slowEffectHazard.NullifySlowStatusEffect(playerCollider.Id);
            }
    }
}
