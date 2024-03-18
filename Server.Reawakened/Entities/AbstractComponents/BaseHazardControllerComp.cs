using Server.Reawakened.Players;
using Server.Reawakened.Rooms.Models.Entities;
using A2m.Server;
using Server.Reawakened.Players.Extensions;
using Server.Base.Timers.Services;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Rooms.Models.Entities.ColliderType;
using Server.Base.Timers.Extensions;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Configs;

namespace Server.Reawakened.Entities.AbstractComponents;

public abstract class BaseHazardControllerComp<T> : Component<T> where T : HazardController
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

    public Player EffectedPlayer = null;

    public string HazardId;
    public ItemEffectType EffectType = ItemEffectType.Unknown;
    public Base.Timers.Timer PoisonTimer = null;
    public bool IsActive = true;
    public bool TimedHazard = false;

    public int Damage;

    public ServerRConfig ServerRConfig { get; set; }
    public TimerThread TimerThread { get; set; }
    public ILogger<BaseHazardControllerComp<HazardController>> Logger { get; set; }

    public override object[] GetInitData(Player player) => [0];

    public override void InitializeComponent()
    {
        Enum.TryParse(HurtEffect, true, out EffectType);

        //Activate timed hazards.
        if (ActiveDuration > 0 && DeactivationDuration > 0)
        {
            TimedHazard = true;
            TimerThread.DelayCall(ActivateHazard, null, TimeSpan.Zero, TimeSpan.Zero, 1);
        }

        //Check if colliders should be created.
        CanCreateColliderCheck();
    }

    public void CanCreateColliderCheck()
    {
        //Prevents enemies and hazards sharing same collider Ids.
        if (!Room.Enemies.ContainsKey(Id))
        {
            //Hazards with the LinearPlatform component already have colliders and do not need a new one created. They have NoEffect.
            if (HurtEffect != ServerRConfig.NoEffect 
                //Many Toxic Clouds seem to have no components. So we use prefab name. (Seek Moss Temple for example)
                || PrefabName.Contains(ServerRConfig.ToxicCloud))
                TimerThread.DelayCall(ColliderCreationDelay, null, TimeSpan.FromSeconds(3), TimeSpan.Zero, 1);
        }
    }

    //Creates hazard colliders after enemy colliders are created to prevent duplicated collider ID bugs.
    public void ColliderCreationDelay(object _)
    {
        HazardId = Id;
        var hazardCollider = new HazardEffectCollider(HazardId, Position, Rectangle, ParentPlane, Room);
        Room.Colliders.TryAdd(HazardId, hazardCollider);
    }

    public void DeactivateHazard(object _)
    {
        IsActive = false;

        TimerThread.DelayCall(ActivateHazard, null,
                TimeSpan.FromSeconds(DeactivationDuration), TimeSpan.Zero, 1);
    }

    public void ActivateHazard(object _)
    {
        IsActive = true;

        TimerThread.DelayCall(DeactivateHazard, null,
                TimeSpan.FromSeconds(ActiveDuration), TimeSpan.Zero, 1);
    }

    public void ApplyHazardEffect(Player player)
    {
        if (player == null || player.TempData.Invincible ||
            TimedHazard && !IsActive || HitOnlyVisible && player.TempData.Invisible)
            return;

        EffectedPlayer = player;

        Enum.TryParse(HurtEffect, true, out ItemEffectType effectType);

        Damage = (int)Math.Ceiling(EffectedPlayer.Character.Data.MaxLife * HealthRatioDamage);

        //For toxic purple cloud hazards with no components
        if (PrefabName.Contains(ServerRConfig.ToxicCloud))
            effectType = ItemEffectType.PoisonDamage;

        switch (effectType)
        {
            case ItemEffectType.SlowStatusEffect:
                ApplySlowEffect(null);
                break;

            case ItemEffectType.BluntDamage:
                Room.SendSyncEvent(new StatusEffect_SyncEvent(EffectedPlayer.GameObjectId, Room.Time,
                (int)ItemEffectType.BluntDamage, 1, 1, true, HazardId, false));

                EffectedPlayer.ApplyCharacterDamage(Room, Damage, DamageDelay, TimerThread);
                break;

            case ItemEffectType.PoisonDamage:
                TimerThread.DelayCall(ApplyPoisonEffect, null,
                    TimeSpan.FromSeconds(InitialDamageDelay), TimeSpan.FromSeconds(DamageDelay), 1);
                break;

            default:
                Room.SendSyncEvent(new StatusEffect_SyncEvent(EffectedPlayer.GameObjectId, Room.Time,
                (int)ItemEffectType.FireDamage, 1, 1, true, HazardId, false));

                EffectedPlayer.ApplyCharacterDamage(Room, Damage, DamageDelay, TimerThread);

                EffectedPlayer.TemporaryInvincibility(TimerThread, 1);
                break;
        }

        Logger.LogInformation("Applied {statusEffect} to {characterName}", EffectType, EffectedPlayer.CharacterName);
    }

    public void ApplyPoisonEffect(object _)
    {
        //Checks collision after InitialDamageDelay
        if (!Room.Colliders[HazardId].CheckCollision(new PlayerCollider(EffectedPlayer)))
            return;

        EffectedPlayer.StartPoisonDamage(HazardId, Damage, (int)HurtLength, TimerThread);
    }

    public void ApplySlowEffect(object _) =>
        EffectedPlayer.ApplySlowEffect(HazardId, Damage);

    public void DisableHazardEffects(Player player)
    {
        switch (EffectType)
        {
            case ItemEffectType.SlowStatusEffect:
                player.NullifySlowStatusEffect(HazardId);
                break;
            case ItemEffectType.PoisonDamage:
                //Poison damage FX don't stop animating after exiting poison colliders.
                Room.SendSyncEvent(new FX_SyncEvent(player.GameObjectId, Room.Time,
                    player.GameObjectId, player.TempData.Position.X, player.TempData.Position.Y, FX_SyncEvent.FXState.Stop));
                break;
        }
    }
}
