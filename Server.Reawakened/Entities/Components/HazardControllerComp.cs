using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Base.Timers.Services;
using Server.Reawakened.Configs;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;

namespace Server.Reawakened.Entities.Components;

public class HazardControllerComp : Component<HazardController>
{
    public string HurtEffect => ComponentData.HurtEffect;
    public float HurtLength => ComponentData.HurtLenght;
    public float InitialDamageDelay => ComponentData.InitialDamageDelay;
    public float DamageDelay => ComponentData.DamageDelay;
    public bool DeathPlane => ComponentData.DeathPlane;
    public string NullifyingEffect => ComponentData.NullifyingEffect;
    public bool HitOnlyVisible => ComponentData.HitOnlyVisible;
    public float InitialProgressRatio => ComponentData.InitialProgressRatio;
    public float ActiveDuration => ComponentData.ActiveDuration;
    public float DeactivationDuration => ComponentData.DeactivationDuration;
    public float HealthRatioDamage => ComponentData.HealthRatioDamage;
    public int HurtSelfOnDamage => ComponentData.HurtSelfOnDamage;

    public TimerThread TimerThread { get; set; }
    public ServerRConfig ServerRConfig { get; set; }
    public ILogger<HazardControllerComp> Logger { get; set; }

    public override object[] GetInitData(Player player) => [0];

    public override void NotifyCollision(NotifyCollision_SyncEvent notifyCollisionEvent, Player player)
    {
        if (!notifyCollisionEvent.Colliding || player.TempData.Invincible)
            return;

        var character = player.Character;

        Enum.TryParse(HurtEffect, true, out ItemEffectType effectType);

        if (effectType == default)
        {
            var noEffect = new StatusEffect_SyncEvent(player.GameObjectId, Room.Time, (int)ItemEffectType.ArmorPiercingDamage,
            0, 1, true, Entity.GameObject.ObjectInfo.ObjectId, false);

            Room.SendSyncEvent(noEffect);
        }
        else
        {
            var statusEffect = new StatusEffect_SyncEvent(player.GameObjectId, Room.Time, (int)effectType,
                0, 1, true, Entity.GameObject.ObjectInfo.ObjectId, false);

            Room.SendSyncEvent(statusEffect);

            Logger.LogTrace("Triggered status effect for {Character} of {HurtType}", character.Data.CharacterName,
                effectType);
        }

        switch (effectType)
        {
            case ItemEffectType.Unknown:
                SendComponentMethodUnknown("unran-hazards", "Failed Hazard Event", "Hazard Type Switch",
                $"Effect Type: {effectType}");
                break;
            case ItemEffectType.WaterBreathing:
                break;
            default:
                player.ApplyDamageByPercent(Room, .10);
                break;
        }

        player.SetTemporaryInvincibility(TimerThread, 1.3);
    }
}
