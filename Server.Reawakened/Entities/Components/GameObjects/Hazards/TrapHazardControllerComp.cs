using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.AbstractComponents;
using Server.Reawakened.Players;

namespace Server.Reawakened.Entities.Components;

public class TrapHazardControllerComp : BaseHazardControllerComp<TrapHazardController>
{
    public override void NotifyCollision(NotifyCollision_SyncEvent notifyCollisionEvent, Player player)
    {
        if (notifyCollisionEvent.Colliding)
            Logger.LogInformation("{characterName} collided with Trap Hazard ({Id}).", player.CharacterName, Id);
    }
}
