using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Base.Network;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms.Models.Entities;

namespace Server.Reawakened.Entities;

public class HazardControllerEntity : SyncedEntity<HazardController>
{
    public string HurtEffect => EntityData.HurtEffect;
    public float HurtLength => EntityData.HurtLenght;
    public float InitialDamageDelay => EntityData.InitialDamageDelay;
    public float DamageDelay => EntityData.DamageDelay;
    public bool DeathPlane => EntityData.DeathPlane;
    public string NullifyingEffect => EntityData.NullifyingEffect;
    public bool HitOnlyVisible => EntityData.HitOnlyVisible;
    public float InitialProgressRatio => EntityData.InitialProgressRatio;
    public float ActiveDuration => EntityData.ActiveDuration;
    public float DeactivationDuration => EntityData.DeactivationDuration;
    public float HealthRatioDamage => EntityData.HealthRatioDamage;
    public int HurtSelfOnDamage => EntityData.HurtSelfOnDamage;

    public ILogger<HazardControllerEntity> Logger { get; set; }

    public override object[] GetInitData(NetState netState) => new object[] { 0 };

    public override void NotifyCollision(NotifyCollision_SyncEvent notifyCollisionEvent, NetState netState)
    {
        if (HurtEffect == "NoEffect")
            return;

        var player = netState.Get<Player>();
        var character = player.GetCurrentCharacter();

        Enum.TryParse(HurtEffect, true, out ItemEffectType effectType);

        if (effectType == default)
        {
            Logger.LogWarning("No hazard type found for {Type}. Returning...", HurtEffect);
            return;
        }

        var statusEffect = new StatusEffect_SyncEvent(player.GameObjectId.ToString(), Room.Time, (int)effectType,
            0, Convert.ToInt32(HurtLength), true, StoredEntity.GameObject.ObjectInfo.PrefabName, false);

        Room.SendSyncEvent(statusEffect);

        Logger.LogTrace("Triggered status effect for {Character} of {HurtType}", character.Data.CharacterName,
            effectType);

        switch (effectType)
        {
            default:
                SendEntityMethodUnknown("unran-hazards", "Failed Hazard Event", "Hazard Type Switch",
                    $"Effect Type: {effectType}");
                break;
        }
    }
}
