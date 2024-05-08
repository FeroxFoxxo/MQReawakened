using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.Colliders.Abstractions;
using Server.Reawakened.Entities.Colliders.Enums;
using Server.Reawakened.Entities.Components.GameObjects.Hazards;
using Server.Reawakened.Entities.Components.GameObjects.Hazards.Abstractions;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms;
using UnityEngine;

namespace Server.Reawakened.Entities.Colliders;
public class HazardEffectCollider(string hazardId, Vector3 position, Rect box, string plane,
    Room room, ILogger<BaseHazardControllerComp<HazardController>> logger) :
    BaseCollider(hazardId, position, box, plane, room, ColliderType.Hazard)
{
    public override void SendCollisionEvent(BaseCollider received)
    {
        if (received is not PlayerCollider playerCollider)
            return;

        ApplyEffectBasedOffHazardType(Id, playerCollider.Player);

        if (!playerCollider.Player.TempData.CollidingHazards.Contains(Id))
        {
            logger.LogInformation("{characterName} collided with hazard ({Id}).", playerCollider.Player.CharacterName, Id);
            playerCollider.Player.TempData.CollidingHazards.Add(Id);
        }
    }

    public void ApplyEffectBasedOffHazardType(string hazardId, Player player)
    {
        Room.GetEntityFromId<BaseHazardControllerComp<HazardController>>(hazardId)?.ApplyHazardEffect(player);
        Room.GetEntityFromId<BaseHazardControllerComp<TrapHazardController>>(hazardId)?.ApplyHazardEffect(player);
        Room.GetEntityFromId<DroppingsControllerComp>(hazardId)?.FreezePlayer(player);
    }
}
