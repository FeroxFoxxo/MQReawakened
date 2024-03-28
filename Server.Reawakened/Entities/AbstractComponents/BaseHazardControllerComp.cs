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
using Server.Reawakened.Entities.Components;
using Server.Reawakened.XMLs.Bundles;

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

    public string HazardId;
    public ItemEffectType EffectType = ItemEffectType.Unknown;
    public bool IsActive = true;
    public bool TimedHazard = false;

    public int Damage;
    private EnemyControllerComp _enemyController;
    private string _id;

    public TimerThread TimerThread { get; set; }
    public ServerRConfig ServerRConfig { get; set; }
    public WorldStatistics WorldStatistics { get; set; }
    public ItemCatalog ItemCatalog { get; set; }
    public ILogger<BaseHazardControllerComp<HazardController>> Logger { get; set; }

    public override object[] GetInitData(Player player) => [0];

    public override void InitializeComponent()
    {
        var controller = Room.GetEntityFromId<EnemyControllerComp>(Id);
        if (controller != null)
            _enemyController = controller;
        _id = Id;

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

    public void SetId(string id)
    {
        _id = id;
        _enemyController = Room.GetEntityFromId<EnemyControllerComp>(id);
    }

    public void CanCreateColliderCheck()
    {
        //Prevents enemies and hazards sharing same collider Ids.
        if (!Room.Enemies.ContainsKey(Id))
        {
            //Hazards which also contain the LinearPlatform component already have colliders and do not need a new one created. They have NoEffect.
            if (HurtEffect != ServerRConfig.NoEffect
                //Many Toxic Clouds seem to have no components, so we find the object with PrefabName to create its colliders. (Seek Moss Temple for example)
                || PrefabName.Contains(ServerRConfig.ToxicCloud))
                TimerThread.DelayCall(ColliderCreationDelay, null, TimeSpan.FromSeconds(3), TimeSpan.Zero, 1);
        }
    }

    //Creates hazard colliders after enemy colliders are created to prevent duplicated collider ID bugs.
    public void ColliderCreationDelay(object _)
    {
        HazardId = Id;
        var hazardCollider = new HazardEffectCollider(HazardId, Position, Rectangle, ParentPlane, Room, Logger);
        Room.Colliders.TryAdd(HazardId, hazardCollider);
    }

    public void DeactivateHazard(object _)
    {
        IsActive = false;

        TimerThread.DelayCall(ActivateHazard, null,
                TimeSpan.FromSeconds(DeactivationDuration), TimeSpan.Zero, 1);
    }

    public override void NotifyCollision(NotifyCollision_SyncEvent notifyCollisionEvent, Player player)
    {
        if (!notifyCollisionEvent.Colliding || player.TempData.Invincible)
            return;

        var character = player.Character;

        Enum.TryParse(HurtEffect, true, out
        ItemEffectType effectType);
        Damage = _enemyController != null
            ? WorldStatistics.GetValue(ItemEffectType.AbilityPower, WorldStatisticsGroup.Enemy, _enemyController.Level)
            : -1;

        if (effectType == default)
        {
            var noEffect = new StatusEffect_SyncEvent(player.GameObjectId, Room.Time, (int)ItemEffectType.BluntDamage,
            0, 1, true, _id, false);

            Room.SendSyncEvent(noEffect);
        }
        else
        {
            var statusEffect = new StatusEffect_SyncEvent(player.GameObjectId, Room.Time, (int)effectType,
                0, 1, true, _id, false);

            Room.SendSyncEvent(statusEffect);

            Logger.LogTrace("Triggered status effect for {Character} of {HurtType}", character.Data.CharacterName,
                effectType);
        }

        var defense = player.Character.Data.CalculateDefense(effectType, ItemCatalog);

        switch (effectType)
        {
            case ItemEffectType.Unknown:
                SendComponentMethodUnknown("unran-hazards", "Failed Hazard Event", "Hazard Type Switch",
                $"Effect Type: {effectType}");
                break;
            case ItemEffectType.WaterBreathing:
                break;
            default:
                if (Damage > 0)
                    player.ApplyCharacterDamage(Room, Damage - defense, DamageDelay, TimerThread);
                else
                    player.ApplyDamageByPercent(Room, .10, TimerThread);
                break;
        }

        player.TemporaryInvincibility(TimerThread, 1);
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

        Enum.TryParse(HurtEffect, true, out ItemEffectType effectType);

        Damage = (int)Math.Ceiling(player.Character.Data.MaxLife * HealthRatioDamage);

        //For toxic purple cloud hazards with no components
        if (PrefabName.Contains(ServerRConfig.ToxicCloud))
            effectType = ItemEffectType.PoisonDamage;

        switch (effectType)
        {
            case ItemEffectType.SlowStatusEffect:
                ApplySlowEffect(player);
                break;

            case ItemEffectType.BluntDamage:
                Room.SendSyncEvent(new StatusEffect_SyncEvent(player.GameObjectId, Room.Time,
                (int)ItemEffectType.BluntDamage, 1, 1, true, HazardId, false));

                player.ApplyCharacterDamage(Room, Damage, DamageDelay, TimerThread);
                break;

            case ItemEffectType.PoisonDamage:
                TimerThread.DelayCall(ApplyPoisonEffect, player,
                    TimeSpan.FromSeconds(InitialDamageDelay), TimeSpan.FromSeconds(DamageDelay), 1);
                break;

            default:
                if (!player.TempData.Invincible)
                    Logger.LogInformation("Applied {statusEffect} to {characterName}", EffectType, player.CharacterName);

                //Used by Flamer and Dragon Statues which emit fire damage.
                Room.SendSyncEvent(new StatusEffect_SyncEvent(player.GameObjectId, Room.Time,
                (int)ItemEffectType.FireDamage, 1, 1, true, _id, false));

                player.ApplyCharacterDamage(Room, Damage, DamageDelay, TimerThread);

                player.TemporaryInvincibility(TimerThread, 1);

                break;
        }
    }

    public void ApplyPoisonEffect(object playerData)
    {
        if (playerData == null) return;

        if (playerData is not Player player)
            return;

        //Checks collision after InitialDamageDelay
        if (!Room.Colliders[HazardId].CheckCollision(new PlayerCollider(player)))
            return;

        player.StartPoisonDamage(HazardId, Damage, (int)HurtLength, TimerThread);
    }

    public void ApplySlowEffect(Player player) =>
        player.ApplySlowEffect(HazardId, Damage);

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
