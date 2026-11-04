using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.Colliders.Abstractions;
using Server.Reawakened.Entities.Colliders.Enums;
using Server.Reawakened.Entities.Components.GameObjects.Hazards;
using Server.Reawakened.Entities.Components.GameObjects.Hazards.Abstractions;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.Rooms.Models.Planes;

namespace Server.Reawakened.Entities.Colliders;

public class HazardEffectCollider(BaseComponent hazardController, ILogger<BaseHazardControllerComp<HazardController>> logger) : BaseCollider
{
    public override Room Room => hazardController.Room;
    public override string Id => hazardController.Id;
    public override Vector3Model Position => hazardController.Position;
    public override RectModel BoundingBox => hazardController.Rectangle;
    public override string Plane => hazardController.ParentPlane;
    public override ColliderType Type => ColliderType.Hazard;

    public override bool CanCollideWithType(BaseCollider collider) =>
        collider.Type switch
        {
            ColliderType.Player => true,
            _ => false
        };
    
    public override string[] RunCollisionDetection() => RunBaseCollisionDetection();
    
    public void ApplyEffectBasedOffHazardType(string hazardId, Player player)
    {
        Room.GetEntityFromId<BaseHazardControllerComp<HazardController>>(hazardId)?.ApplyHazardEffect(player);
        Room.GetEntityFromId<BaseHazardControllerComp<TrapHazardController>>(hazardId)?.ApplyHazardEffect(player);
        Room.GetEntityFromId<DroppingsControllerComp>(hazardId)?.FreezePlayer(player);
    }
}
