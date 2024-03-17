using Server.Reawakened.Entities.AbstractComponents;
using Server.Reawakened.Entities.Components;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms.Models.Planes;

namespace Server.Reawakened.Rooms.Models.Entities.ColliderType;
public class HazardEffectCollider(string hazardId, Vector3Model position, RectModel rect, string plane, Room room) : BaseCollider(hazardId, AdjustPosition(position, rect), rect.Width, rect.Height, plane, room, "hazard")
{
    public override void SendCollisionEvent(BaseCollider received)
    {
        if (received is not PlayerCollider playerCollider)
            return;

        ApplyEffectBasedOffHazardType(hazardId, playerCollider.Player);
    }

    public override void SendNonCollisionEvent(BaseCollider received)
    {
        if (received is not PlayerCollider playerCollider)
            return;

        DisableEffectBasedOffHazardType(hazardId, playerCollider.Player);
    }

    public void ApplyEffectBasedOffHazardType(string hazardId, Player player)
    {
        Room.GetEntityFromId<BaseHazardControllerComp<HazardController>>(hazardId)?.ApplyHazardEffect(player);
        Room.GetEntityFromId<BaseHazardControllerComp<TrapHazardController>>(hazardId)?.ApplyHazardEffect(player);
        Room.GetEntityFromId<DroppingsControllerComp>(hazardId)?.FreezePlayer(player);
    }

    public void DisableEffectBasedOffHazardType(string hazardId, Player player)
    {
        Room.GetEntityFromId<BaseHazardControllerComp<HazardController>>(hazardId)?.DisableHazardEffects(player);
        Room.GetEntityFromId<BaseHazardControllerComp<TrapHazardController>>(hazardId)?.DisableHazardEffects(player);
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
