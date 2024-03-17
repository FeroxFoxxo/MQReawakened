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

    public Player EffectedPlayer;
    public string HazardId;
    public ItemEffectType EffectType;
    public bool IsActive = true;
    public bool TimedHazard = false;
    public int Damage;

    public ServerRConfig ServerRConfig { get; set; }
    public TimerThread TimerThread { get; set; }
    public ILogger<BaseHazardControllerComp<HazardController>> Logger { get; set; }

    public override object[] GetInitData(Player player) => [0];

    public override void Update()
    {
        foreach (var player in Room.Players?.Values)
        {
            var playerCollider = new PlayerCollider(player);
            playerCollider.IsColliding(false);
        }
    }

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
            HazardId = Id;

            //Hazards with the LinearPlatform component already have colliders and do not need a new one created.
            if (HurtEffect != ServerRConfig.NoEffect)
                TimerThread.DelayCall(ColliderCreationDelay, null, TimeSpan.FromSeconds(3), TimeSpan.Zero, 1);
        }
    }

    //Creates hazard colliders after enemy colliders are created to prevent duplicated collider ID bugs.
    public void ColliderCreationDelay(object _)
    {
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

        Damage = (int)Math.Ceiling(player.Character.Data.MaxLife * HealthRatioDamage);

        switch (effectType)
        {
            case ItemEffectType.SlowStatusEffect:
                ApplySlowEffect(null);
                break;

            case ItemEffectType.BluntDamage:
                player.Room.SendSyncEvent(new StatusEffect_SyncEvent(player.GameObjectId, player.Room.Time,
                (int)ItemEffectType.BluntDamage, 1, 1, true, HazardId, false));

                player.ApplyCharacterDamage(Room, Damage, TimerThread);
                break;

            case ItemEffectType.PoisonDamage:
                player.TempData.IsPoisoned = true;
                var ticksTillDeath = (int)Math.Ceiling((double)player.Character.Data.MaxLife / Damage);

                TimerThread.DelayCall(StartPoisonEffect, null,
                    TimeSpan.FromSeconds(InitialDamageDelay), TimeSpan.FromSeconds(DamageDelay), ticksTillDeath);
                break;

            default:
                player.Room.SendSyncEvent(new StatusEffect_SyncEvent(player.GameObjectId, player.Room.Time,
                (int)ItemEffectType.FireDamage, 1, 1, true, HazardId, false));

                player.ApplyCharacterDamage(Room, Damage, TimerThread);

                player.TemporaryInvincibility(TimerThread, 1);
                break;
        }

        Logger.LogInformation("Applied {statusEffect} to {characterName}", EffectType, player.CharacterName);
    }

    public void StartPoisonEffect(object _) =>
        EffectedPlayer.StartPoisonDamage(HazardId, Damage, (int)HurtLength, TimerThread);

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
                player.TempData.IsPoisoned = false;
                break;
        }
    }
}
