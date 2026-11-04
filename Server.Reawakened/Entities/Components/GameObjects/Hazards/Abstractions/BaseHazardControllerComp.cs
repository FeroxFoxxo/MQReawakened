using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Entities.Colliders;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using Server.Reawakened.Entities.Components.GameObjects.Breakables;
using Server.Reawakened.Entities.Enemies.EnemyTypes.Abstractions;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.Rooms.Models.Timers;
using Server.Reawakened.XMLs.Bundles.Base;
using static Server.Reawakened.Players.Extensions.PlayerStatusEffectExtensions;

namespace Server.Reawakened.Entities.Components.GameObjects.Hazards.Abstractions;

public abstract class BaseHazardControllerComp<T> : Component<T> where T : HazardController
{
    private const float DeathPlaneGracePeriod = 20f;

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
    public ItemEffectType EffectType { get; private set; } = ItemEffectType.Unknown;

    public volatile bool IsActive = true;
    public bool TimedHazard { get; private set; }

    private IEnemyController _enemyController;
    private HazardEffectCollider _collider;
    private float _activationStartTime;

    private int DamageDelayMs => Convert.ToInt32(DamageDelay);
    private int InitialDamageDelayMs => Convert.ToInt32(InitialDamageDelay);

    public TimerThread TimerThread { get; set; }
    public HazardRConfig HazardRConfig { get; set; }
    public ServerRConfig ServerRConfig { get; set; }
    public ItemCatalog ItemCatalog { get; set; }
    public ILogger<BaseHazardControllerComp<HazardController>> Logger { get; set; }

    public override object[] GetInitData(Player player) => [InitialProgressRatio];

    public override void InitializeComponent()
    {
        _activationStartTime = InitialProgressRatio;
        _enemyController = Room.GetEnemyFromId(Id);

        TimedHazard = ActiveDuration > 0 && DeactivationDuration > 0;

        if (!ResolveEffectType())
            return;

        if (_enemyController != null)
            return;

        if (EffectType == ItemEffectType.SlowStatusEffect)
            Rectangle.X = 0;

        _collider = new HazardEffectCollider(this, Logger);

        Logger.LogDebug("Created hazard collider with ID: {Id} for prefab: {PrefabName}", Id, PrefabName);
    }

    private bool ResolveEffectType()
    {
        if (Enum.TryParse<ItemEffectType>(HurtEffect, true, out var parsed))
        {
            EffectType = parsed;
            return true;
        }

        if (PrefabName.Contains("ToxicCloud"))
        {
            EffectType = ItemEffectType.PoisonDamage;
            return true;
        }

        if (PrefabName.Contains("Deathplane"))
        {
            EffectType = ItemEffectType.BluntDamage;
            return true;
        }

        switch (HurtEffect)
        {
            case "NoEffect":
                EffectType = ItemEffectType.Unknown;
                return false;
            case "StandardDamage":
                EffectType = ItemEffectType.BluntDamage;
                return true;
            default:
                EffectType = ItemEffectType.Unknown;
                Logger.LogError("Could not find effect type of: '{Effect}' for '{PrefabName}' ({Id})!",
                    HurtEffect, PrefabName, Id);
                return true;
        }
    }

    public override void Update()
    {
        if (Room == null)
            return;

        base.Update();

        UpdateTimedActivation();

        _collider?.RunCollisionDetection();
    }

    private void UpdateTimedActivation()
    {
        if (!TimedHazard)
            return;

        var elapsed = Room.Time - _activationStartTime;

        if (IsActive && elapsed > ActiveDuration)
        {
            _activationStartTime += ActiveDuration;
            IsActive = false;
        }
        else if (!IsActive && elapsed > DeactivationDuration)
        {
            _activationStartTime += DeactivationDuration;
            IsActive = true;
        }
    }

    public override void NotifyCollision(NotifyCollision_SyncEvent notifyCollisionEvent, Player player)
    {
        if (!notifyCollisionEvent.Colliding && EffectType != ItemEffectType.Unknown)
            return;

        ApplyHazardEffect(player);
    }

    public void ApplyHazardEffect(Player player)
    {
        if (player == null || Room == null || !CanAffect(player))
            return;

        var effectType = EffectType;
        var damage = (int)Math.Ceiling(player.Character.MaxLife * HealthRatioDamage);

        var enemy = Room.GetEnemy(Id);

        if (enemy != null)
        {
            effectType = enemy.EnemyController.EnemyEffectType;
            damage = EnemyDamagePlayer(player, enemy);
        }

        Logger.LogTrace("Applying {StatusEffect} to {CharacterName} from {PrefabName}",
            effectType, player.CharacterName, PrefabName);

        switch (effectType)
        {
            case ItemEffectType.SlowStatusEffect:
                ApplySlowEffect(player);
                break;

            case ItemEffectType.BluntDamage:
            case ItemEffectType.FireDamage:
            case ItemEffectType.AirDamage:
            case ItemEffectType.EarthDamage:
            case ItemEffectType.IceDamage:
            case ItemEffectType.LightningDamage:
                player.ApplyCharacterDamage(damage, effectType, Id, DamageDelayMs);
                break;

            case ItemEffectType.PoisonDamage:
                ApplyPoison(player, damage);
                break;

            case ItemEffectType.WaterBreathing:
                ApplyWaterBreathing(player);
                break;

            case ItemEffectType.SpiderWeb:
                ApplySpiderWeb(player);
                break;

            default:
                Logger.LogInformation("Unknown status effect {StatusEffect} from {PrefabName}",
                    HurtEffect, PrefabName);

                Room.SendSyncEvent(new StatusEffect_SyncEvent(player.GameObjectId, Room.Time,
                    (int)ItemEffectType.BluntDamage, 1, 1, true, Id, false));

                player.ApplyCharacterDamage(damage, effectType, Id, DamageDelayMs);
                break;
        }
    }
    private bool CanAffect(Player player)
    {
        var statusEffects = player.Character.StatusEffects;

        if (statusEffects.HasEffect(ItemEffectType.Invincibility) || player.TempData.IsKnockedOut)
            return false;

        if ((TimedHazard || EffectType == ItemEffectType.WaterBreathing) && !IsActive)
            return false;

        if (HitOnlyVisible && statusEffects.HasEffect(ItemEffectType.Invisibility))
            return false;

        if (DeathPlane && Room.Time < DeathPlaneGracePeriod)
            return false;

        // Reduces slow status effect spam.
        if ((EffectType == ItemEffectType.SlowStatusEffect || !player.TempData.OnGround) &&
            player.TempData.IsSlowed)
            return false;

        if (EffectType == ItemEffectType.SlowStatusEffect && player.HasNullifyEffect(ItemCatalog))
            return false;

        return true;
    }

    public int EnemyDamagePlayer(Player player, BaseEnemy enemy) => enemy.EnemyDamagePlayer(player);

    // POISON
    private void ApplyPoison(Player player, int damage)
    {
        lock (player.TempData)
        {
            if (player.TempData.IsPoisoned)
                return;

            ApplyPoisonDamage(player.GetPoisonEffectData(damage, Id, HazardRConfig,
                TimerThread, InitialDamageDelayMs, DamageDelayMs));
        }
    }

    // SPIDER WEB
    private void ApplySpiderWeb(Player player)
    {
        Room.SendSyncEvent(new StatusEffect_SyncEvent(player.GameObjectId, Room.Time,
            (int)ItemEffectType.SpiderWeb, 1, 1, true, Id, false));

        var breakable = Room.GetEntityFromId<BreakableEventControllerComp>(Id);
        breakable?.Damage(breakable.Damageable.CurrentHealth, ItemEffectType.BluntDamage, player);
    }

    // WATER BREATHING
    public void ApplyWaterBreathing(Player player)
    {
        if (player == null)
            return;

        Room.SendSyncEvent(new StatusEffect_SyncEvent(player.GameObjectId, Room.Time,
            (int)ItemEffectType.WaterBreathing, 1, 1, true, Id, false));

        IsActive = false;

        TimerThread.RunDelayed(ReactivateHazard, this, TimeSpan.FromSeconds(1));

        player.ResetUnderwaterTime();

        Logger.LogDebug("Reset underwater timer for {CharacterName}", player.CharacterName);
    }

    private static void ReactivateHazard(ITimerData data)
    {
        if (data is BaseHazardControllerComp<T> hazard)
            hazard.IsActive = true;
    }

    // SLOW EFFECT
    private void ApplySlowEffect(Player player)
    {
        lock (player.TempData)
        {
            if (player.TempData.IsSlowed)
                return;

            player.TempData.IsSlowed = true;
        }

        player.ApplySlowEffect();

        TimerThread.RunDelayed(DisableSlowEffect, new PlayerTimer { Player = player },
            TimeSpan.FromSeconds(HazardRConfig.SlowEffectInterval));
    }

    private void DisableSlowEffect(ITimerData data)
    {
        if (data is not PlayerTimer { Player: { } player })
            return;

        if (Room == null || _collider == null)
            return;

        if (_collider.CheckCollision(player.GetCollider()))
            player.NullifySlowStatusEffect();
    }
}
