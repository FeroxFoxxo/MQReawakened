using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Configs;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Entities.Colliders;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.XMLs.Bundles.Base;
using UnityEngine;

namespace Server.Reawakened.Entities.Components.GameObjects.Hazards.Abstractions;

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

    public ItemEffectType EffectType = ItemEffectType.Unknown;
    public bool IsActive = true;
    public bool TimedHazard = false;

    public int Damage;
    private IEnemyController _enemyController;
    private string _id;

    public TimerThread TimerThread { get; set; }
    public ItemRConfig ItemRConfig { get; set; }
    public ServerRConfig ServerRConfig { get; set; }
    public WorldStatistics WorldStatistics { get; set; }
    public ItemCatalog ItemCatalog { get; set; }
    public ILogger<BaseHazardControllerComp<HazardController>> Logger { get; set; }

    public override object[] GetInitData(Player player) => [0];

    public override void InitializeComponent()
    {
        SetId(Id);

        //Activate timed hazards.
        if (ActiveDuration > 0 && DeactivationDuration > 0)
        {
            TimedHazard = true;
            TimerThread.DelayCall(ActivateHazard, null, TimeSpan.Zero, TimeSpan.Zero, 1);
        }

        switch (HurtEffect)
        {
            case "NoEffect":
                // Temporary mapping for NoEffect
                EffectType = ItemEffectType.Unknown_70;
                return;
            case "StandardDamage":
                EffectType = ItemEffectType.BluntDamage;
                break;
            default:
                //Many Toxic Clouds seem to have no components, so we find the object with PrefabName to create its colliders. (Seek Moss Temple for example)
                if (PrefabName.Contains("ToxicCloud"))
                    EffectType = ItemEffectType.PoisonDamage;
                else if (!Enum.TryParse(HurtEffect, true, out EffectType))
                    Logger.LogError("Could not find effect type of: '{effect}' for '{prefabName}' ({id})!", HurtEffect, PrefabName, Id);

                break;
        }

        //Prevents enemies and hazards sharing same collider Ids.
        if (_enemyController == null)
        {
            //Prevents spider webs from inaccurately adjusting collider positioning.
            if (EffectType == ItemEffectType.SlowStatusEffect)
                Rectangle.X = 0;

            var box = new Rect(Rectangle.X, Rectangle.Y, Rectangle.Width, Rectangle.Height);
            var position = new Vector3(Position.X, Position.Y, Position.Z);

            Room.AddCollider(new HazardEffectCollider(_id, position, box, ParentPlane, Room, Logger));
        }
    }

    public void SetId(string id)
    {
        _id = id;
        _enemyController = Room.GetEnemyFromId(id);
    }

    public void DeactivateHazard(object _)
    {
        IsActive = false;

        TimerThread.DelayCall(ActivateHazard, null, TimeSpan.FromSeconds(DeactivationDuration), TimeSpan.Zero, 1);
    }

    public void ActivateHazard(object _)
    {
        IsActive = true;

        TimerThread.DelayCall(DeactivateHazard, null, TimeSpan.FromSeconds(ActiveDuration), TimeSpan.Zero, 1);
    }

    //Standard Hazards
    public override void NotifyCollision(NotifyCollision_SyncEvent notifyCollisionEvent, Player player)
    {
        if (!notifyCollisionEvent.Colliding || player.TempData.Invincible)
            return;

        if (Room.ContainsEnemy(Id))
        {
            if (player.TempData.PetDefensiveBarrier)
                Room.GetEnemy(Id).PetDamage(player);

            else
                ApplyHazardEffect(player);
        }
        else
        {
            player.ApplyDamageByPercent(HealthRatioDamage, Id, HurtLength, ServerRConfig, TimerThread);
            Room.SendSyncEvent(new StatusEffect_SyncEvent(player.GameObjectId, Room.Time,
          (int)ItemEffectType.BluntDamage, 1, (int)HurtLength, true, _id, false));
        }
    }

    public void ApplyHazardEffect(Player player)
    {
        if (player == null || player.TempData.Invincible ||
            TimedHazard && !IsActive || HitOnlyVisible && player.TempData.Invisible ||
            EffectType == ItemEffectType.SlowStatusEffect && player.TempData.IsSlowed ||
            player.HasNullifyEffect(ItemCatalog))
            return;

        Damage = (int)Math.Ceiling(player.Character.Data.MaxLife * HealthRatioDamage);

        Logger.LogTrace("Applying {statusEffect} to {characterName} from {prefabName}", EffectType, player.CharacterName, PrefabName);

        switch (EffectType)
        {
            // Temporary mapping for no effect.
            case ItemEffectType.Unknown_70:
                return;

            case ItemEffectType.SlowStatusEffect:
                ApplySlowEffect(player);
                break;

            case ItemEffectType.BluntDamage:
                var enemy = Room.GetEnemy(Id);
                var damage = (float)WorldStatistics.GetValue(ItemEffectType.AbilityPower, WorldStatisticsGroup.Enemy, enemy.Level) -
                             player.Character.Data.CalculateDefense(EffectType, ItemCatalog);

                player.ApplyCharacterDamage((int)damage, Id, DamageDelay, ServerRConfig, TimerThread);
                break;

            case ItemEffectType.PoisonDamage:
                TimerThread.DelayCall(ApplyPoisonEffect, player,
                    TimeSpan.FromSeconds(InitialDamageDelay), TimeSpan.FromSeconds(DamageDelay), 1);
                break;

            default:
                Logger.LogInformation("Unknown status effect {statusEffect} from {prefabName}", HurtEffect, PrefabName);

                //Water breathing.
                if (HurtLength < 0)
                {
                    if (IsActive)
                    {
                        ApplyWaterBreathing(player);
                        IsActive = false;
                    }
                    return;
                }

                Room.SendSyncEvent(new StatusEffect_SyncEvent(player.GameObjectId, Room.Time, (int)ItemEffectType.BluntDamage, 1, 1, true, _id, false));

                player.ApplyCharacterDamage(Damage, Id, DamageDelay, ServerRConfig, TimerThread);

                break;
        }
    }

    public void ApplyPoisonEffect(object playerData)
    {
        if (playerData == null)
            return;

        if (playerData is not Player player)
            return;

        var collider = Room.GetColliderById(_id);

        if (collider != null)
            if (!collider.CheckCollision(new PlayerCollider(player)))
                return;

        player.StartPoisonDamage(_id, Damage, (int)HurtLength, ServerRConfig, TimerThread);
    }

    public void ApplyWaterBreathing(object playerData)
    {
        if (playerData == null || playerData is not Player player)
            return;

        Room.SendSyncEvent(new StatusEffect_SyncEvent(player.GameObjectId, Room.Time,
                    (int)ItemEffectType.WaterBreathing, 1, 1, true, _id, false));

        player.StartUnderwater(player.Character.Data.MaxLife / 10, TimerThread, ServerRConfig, ItemRConfig, Logger);

        TimerThread.DelayCall(RestartTimerDelay, null, TimeSpan.FromSeconds(1), TimeSpan.Zero, 1);
        Logger.LogInformation("Reset underwater timer for {characterName}", player.CharacterName);
    }

    public void RestartTimerDelay(object data) => IsActive = true;

    private void ApplySlowEffect(Player player)
    {
        player.ApplySlowEffect(_id, Damage);
        //IsSlowed reduces slow status effect log spam.  
        player.TempData.IsSlowed = true;
        TimerThread.DelayCall(DisableIsSlowed, player, TimeSpan.FromSeconds(0.75), TimeSpan.Zero, 1);
    }

    private void DisableIsSlowed(object player)
    {
        var slowedPlayer = (Player)player;
        slowedPlayer.TempData.IsSlowed = false;
    }
