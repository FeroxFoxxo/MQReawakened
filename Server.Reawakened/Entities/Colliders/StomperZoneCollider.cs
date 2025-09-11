using Server.Base.Timers.Services;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Entities.Colliders.Abstractions;
using Server.Reawakened.Entities.Colliders.Enums;
using Server.Reawakened.Entities.Components.GameObjects.Stompers;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Models.Planes;

namespace Server.Reawakened.Entities.Colliders;

public class StomperZoneCollider(StomperControllerComp stomperController) : BaseCollider
{
    public bool Hazard => stomperController.Hazard;
    public TimerThread TimerThread => stomperController.TimerThread;
    public ServerRConfig ServerRConfig => stomperController.ServerRConfig;
    public override Vector3Model Position => stomperController.Position;
    public override Room Room => stomperController.Room;
    public override string Id => stomperController.Id;
    public override RectModel BoundingBox => stomperController.Rectangle;
    public override string Plane => stomperController.ParentPlane;
    public override ColliderType Type => ColliderType.Stomper;
    public override bool CanCollideWithType(BaseCollider collider) =>
        collider.Type switch
        {
            ColliderType.Player => true,
            _ => false
        };

    public override string[] RunCollisionDetection() => RunBaseCollisionDetection();
}
