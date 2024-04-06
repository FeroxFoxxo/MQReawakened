using Server.Reawakened.Entities.Components.GameObjects.InterObjs.Abstractions;
using Server.Reawakened.Players;

namespace Server.Reawakened.Entities.Components.GameObjects.InterObjs;
public class InterObjStatusComp : BaseInterObjStatusComp<InterObjStatus>
{
    public override void NotifyCollision(NotifyCollision_SyncEvent notifyCollisionEvent, Player player) { }
}
