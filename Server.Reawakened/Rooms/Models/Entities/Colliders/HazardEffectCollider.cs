using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.AbstractComponents;
using Server.Reawakened.Entities.Components;
using Server.Reawakened.Entities.Enums;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms.Models.Entities.ColliderType;
using Server.Reawakened.Rooms.Models.Planes;

namespace Server.Reawakened.Rooms.Models.Entities.Colliders;
public class HazardEffectCollider(string hazardId, Vector3Model position, RectModel rect, string plane, Room room, ILogger<BaseHazardControllerComp<HazardController>> logger) : BaseCollider(hazardId, AdjustPosition(position, rect), rect.Width, rect.Height, plane, room, ColliderClass.Hazard)
{
    public override void SendCollisionEvent(BaseCollider received)
    {
        if (received is not PlayerCollider playerCollider)
            return;

        ApplyEffectBasedOffHazardType(hazardId, playerCollider.Player);

        if (!playerCollider.Player.TempData.CollidingHazards.Contains(hazardId))
        {
            logger.LogInformation("{characterName} collided with hazard ({Id}).", playerCollider.Player.CharacterName, hazardId);
            playerCollider.Player.TempData.CollidingHazards.Add(hazardId);
        }
    }

    public void ApplyEffectBasedOffHazardType(string hazardId, Player player)
    {
        Room.GetEntityFromId<BaseHazardControllerComp<HazardController>>(hazardId)?.ApplyHazardEffect(player);
        Room.GetEntityFromId<BaseHazardControllerComp<TrapHazardController>>(hazardId)?.ApplyHazardEffect(player);
        Room.GetEntityFromId<DroppingsControllerComp>(hazardId)?.FreezePlayer(player);
    }

    public static Vector3Model AdjustPosition(Vector3Model originalPosition, RectModel rect)
    {
        var adjustedXPos = rect.X;
        var adjustedYPos = rect.Y;

        return new()
        {
            X = originalPosition.X + adjustedXPos,
            Y = originalPosition.Y + adjustedYPos,
            Z = originalPosition.Z
        };
    }
}
