using A2m.Server;
using Server.Base.Timers.Extensions;
using Server.Reawakened.Entities.AbstractComponents;
using Server.Reawakened.Entities.Components;
using Server.Reawakened.Entities.Enums;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Planes;

namespace Server.Reawakened.Rooms.Models.Entities.ColliderType;
public class HazardEffectCollider(string hazardId, Vector3Model position, RectModel rect, string plane, Room room) : BaseCollider(hazardId, AdjustPosition(position, rect), rect.Width, rect.Height, plane, room, ColliderClass.Hazard)
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

        if (Room.GetEntityFromId<StomperControllerComp>(hazardId)?.State == Stomper_Movement.StomperState.WaitDown &&
            Room.GetEntityFromId<StomperControllerComp>(hazardId).ApplyStompDamage)
        {
            Room.GetEntityFromId<StomperControllerComp>(hazardId).ApplyStompDamage = false;
            Room.SendSyncEvent(new StatusEffect_SyncEvent(player.GameObjectId, Room.Time,
                (int)ItemEffectType.StompDamage, 1, 1, true, hazardId, false));

            Room.GetEntityFromId<StomperControllerComp>(hazardId).TimerThread.DelayCall(DelayStompDamage, Room.GetEntityFromId<StomperControllerComp>(hazardId),
                TimeSpan.FromSeconds(1), TimeSpan.Zero, 1);
        }
    }

    public void DelayStompDamage(object stompControllerData)
    {
        var stompData = (StomperControllerComp)stompControllerData;
        stompData.ApplyStompDamage = true;
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
