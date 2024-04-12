using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.Components.GameObjects.Hazards.Abstractions;
using Server.Reawakened.Players;

namespace Server.Reawakened.Entities.Components.GameObjects.Hazards;

public class TrapHazardControllerComp : BaseHazardControllerComp<TrapHazardController>
{
    public override void NotifyCollision(NotifyCollision_SyncEvent notifyCollisionEvent, Player player)
    {
        if (notifyCollisionEvent.Colliding)
            Logger.LogInformation("{characterName} collided with Trap Hazard ({Id}).", player.CharacterName, Id);
    }
}
