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

    public ItemEffectType EffectType;
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
            //Hazards with the LinearPlatform component already have colliders and do not need a new one created.
            if (HurtEffect != ServerRConfig.NoEffect)
                TimerThread.DelayCall(ColliderCreationDelay, null, TimeSpan.FromSeconds(3), TimeSpan.Zero, 1);
        }
    }

    //Creates hazard colliders after enemy colliders are created to prevent duplicated collider ID bugs.
    public void ColliderCreationDelay(object _)
    {
        var hazardCollider = new HazardEffectCollider(_id, Position, Rectangle, ParentPlane, Room);
        Room.Colliders.TryAdd(_id, hazardCollider);
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
                    player.ApplyCharacterDamage(Room, Damage - defense, TimerThread);
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

        Damage = (int)Math.Ceiling(player.Character.Data.MaxLife * HealthRatioDamage);

        switch (EffectType)
        {
            case ItemEffectType.SlowStatusEffect:
                ApplySlowEffect(null);
                break;

            case ItemEffectType.BluntDamage:
                player.Room.SendSyncEvent(new StatusEffect_SyncEvent(player.GameObjectId, player.Room.Time,
                (int)ItemEffectType.BluntDamage, 1, 1, true, _id, false));
            
                player.ApplyCharacterDamage(Room, Damage, TimerThread);
                break;

            case ItemEffectType.PoisonDamage:
                player.TempData.IsPoisoned = true;
                var ticksTillDeath = (int)Math.Ceiling((double)player.Character.Data.MaxLife / Damage);

                TimerThread.DelayCall(StartPoisonEffect, player,
                    TimeSpan.FromSeconds(InitialDamageDelay), TimeSpan.FromSeconds(DamageDelay), ticksTillDeath);
                break;

            default:
                Console.WriteLine(_id);
                player.Room.SendSyncEvent(new StatusEffect_SyncEvent(player.GameObjectId, player.Room.Time,
                (int)EffectType, 1, 1, true, _id, false));

                player.ApplyCharacterDamage(Room, Damage, TimerThread);

                player.TemporaryInvincibility(TimerThread, 1);
                break;
        }

        Logger.LogInformation("Applied {statusEffect} to {characterName}", EffectType, player.CharacterName);
    }

    public void StartPoisonEffect(object playerObject)
    {
        if (playerObject == null)
            return;

        if (playerObject is not Player player)
            return;

        player.StartPoisonDamage(_id, Damage, (int)HurtLength, TimerThread);
    }

    public void ApplySlowEffect(Player player) =>
        player.ApplySlowEffect(_id, Damage);

    public void DisableHazardEffects(Player player)
    {
        switch (EffectType)
        {
            case ItemEffectType.SlowStatusEffect:
                player.NullifySlowStatusEffect(_id);
                break;
            case ItemEffectType.PoisonDamage:
                player.TempData.IsPoisoned = false;
                break;
        }
    }
}
