using Server.Reawakened.Entities.Components;
using Server.Reawakened.Entities.Enums;
using Server.Reawakened.Rooms.Models.Planes;

namespace Server.Reawakened.Rooms.Models.Entities.ColliderType;
public class SlowEffectHazardCollider(string id, Vector3Model position, float sizeX, float sizeY, string plane, Room room) : BaseCollider(id, position, sizeX, sizeY, plane, room, ColliderClass.Hazard)
{
    public override void SendCollisionEvent(BaseCollider received)
    {
        if (received is PlayerCollider playerCollider)
            foreach (var slowEffectHazard in Room.GetEntitiesFromId<HazardControllerComp>(id))
            {
                if (playerCollider.Player.TempData.OnGround)
                    slowEffectHazard.SlowStatusEffect(playerCollider.Id);
                else
                    slowEffectHazard.NullifySlowStatusEffect(playerCollider.Id);
            }
    }
}
