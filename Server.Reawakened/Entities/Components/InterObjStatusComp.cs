using Server.Reawakened.Entities.AbstractComponents;
using Server.Reawakened.Players;

namespace Server.Reawakened.Entities.Components;
public class InterObjStatusComp : BaseInterObjStatusComp<InterObjStatus>
{
    public override void NotifyCollision(NotifyCollision_SyncEvent notifyCollisionEvent, Player player) { }
}
