using A2m.Server;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Rooms.Extensions;
using UnityEngine;
using PetDefines;
using Server.Reawakened.XMLs.Bundles.Base;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Entities.Enemies.EnemyTypes.Abstractions;
using TimerCallback = Server.Base.Timers.Timer.TimerCallback;

namespace Server.Reawakened.Players.Models.Pets;

public class PetAbilityModel
{
    public PetAbilityParams PetAbilityParams { get; set; }
    public float AbilityCooldown { get; set; }
    public bool DefenceBoostActive { get; set; }
    public bool DefensiveBarrierActive { get; set; }

    public PetAbilityModel(PetAbilityParams petAbilityParams, float abilityCooldown, bool defenceBoostActive, bool defensiveBarrierActive)
    {
        PetAbilityParams = petAbilityParams;
        AbilityCooldown = abilityCooldown;
        DefenceBoostActive = defenceBoostActive;
        DefensiveBarrierActive = defensiveBarrierActive;
    }

    public void SendAbility(Player player, PetModel pet, ItemCatalog itemCatalog, TimerThread timerThread)
    {
        pet.UseEnergy(player);
        AbilityCooldown = player.Room.Time + PetAbilityParams.CooldownTime;
        pet.InCoopJumpState = false;
        pet.InCoopSwitchState = false;

        player.Room.SendSyncEvent(new PetState_SyncEvent(player.GameObjectId, player.Room.Time,
            PetInformation.StateSyncType.Ability, GetAbilitySyncParams(player)));
        player.Room.SendSyncEvent(new StatusEffect_SyncEvent(player.GameObjectId, player.Room.Time,
            (int)ItemEffectType.AbilityPower, 0, (int)PetAbilityParams.Duration, true,
            itemCatalog.GetItemFromId(int.Parse(pet.PetId)).PrefabName, true));

        //Sends method of ability type after a short delay.
        timerThread.DelayCall(StartAbilityType(), player, TimeSpan.FromSeconds(PetAbilityParams.InitialDelayBeforeUse),
            TimeSpan.FromSeconds(PetAbilityParams.Frequency), PetAbilityParams.HitCount);
    }

    public string GetAbilitySyncParams(Player player) =>
        IsAttackAbility() ? GetClosestEnemy(GetDetectedEnemies(player)).Id : player.GameObjectId;

    public TimerCallback StartAbilityType() => PetAbilityParams.AbilityType switch
    {
        PetAbilityType.Invalid => null,
        PetAbilityType.Heal => PetHealPlayer,
        PetAbilityType.Damage or PetAbilityType.DamageOverTime or
        PetAbilityType.DamageOverTimeFromAbove => AttackEnemy,
        PetAbilityType.DamageZone => AttackEnemiesInZone,
        PetAbilityType.Defence => ActivateDefence,
        PetAbilityType.DefensiveBarrier => ActivateDefensiveBarrier,
        PetAbilityType.Unknown => null,
        _ => null,
    };

    public void PetHealPlayer(object player)
    {
        var petOwner = (Player)player;
        petOwner.PetHeal((int)Math.Ceiling(petOwner.Character.Data.MaxLife * PetAbilityParams.ApplyOnHealthRatio));
    }

    public void AttackEnemy(object player)
    {
        var petOwner = (Player)player;
        GetClosestEnemy(GetDetectedEnemies(petOwner))?.PetDamage(petOwner);
    }

    public void AttackEnemiesInZone(object player)
    {
        var petOwner = (Player)player;

        foreach (var enemyDetected in GetDetectedEnemies(petOwner).Values.Where
                    (enemy => EnemyInDamageZone(enemy, GetCurrentPetPos(petOwner))))
            enemyDetected.PetDamage(petOwner);
    }

    public void ActivateDefence(object player) => DefenceBoostActive = !DefenceBoostActive;

    public void ActivateDefensiveBarrier(object player) => DefensiveBarrierActive = !DefensiveBarrierActive;

    public Dictionary<int, BaseEnemy> GetDetectedEnemies(Player player)
    {
        var enemies = new Dictionary<int, BaseEnemy>();
        var petPosition = GetCurrentPetPos(player);

        foreach (var enemy in player.Room.GetEnemies()
            .Where(enemy => enemy.ParentPlane == player.GetPlayersPlaneString() &&
            EnemyInDetectionZone(enemy, GetCurrentPetPos(player))))
        {
            var distanceFromPlayer = (int)Math.Sqrt(
                Math.Pow(petPosition.x - enemy.Position.x, 2) +
                Math.Pow(petPosition.y - enemy.Position.y, 2));

            enemies.TryAdd(distanceFromPlayer, enemy);
        }

        return enemies;
    }

    private BaseEnemy GetClosestEnemy(Dictionary<int, BaseEnemy> detectedEnemies)
    {
        if (detectedEnemies.Count <= 0)
            return null;

        var closestDistance = detectedEnemies.Keys.Min();
        return detectedEnemies[closestDistance];
    }

    private bool EnemyInDetectionZone(BaseEnemy enemy, Vector3 petPosition) =>
        petPosition.x + PetAbilityParams.DetectionZone.x >= enemy.Position.x &&
        petPosition.y + PetAbilityParams.DetectionZone.y >= enemy.Position.y &&
        petPosition.x - PetAbilityParams.DetectionZoneOffset.x <= enemy.Position.x &&
        petPosition.y - PetAbilityParams.DetectionZoneOffset.y <= enemy.Position.y;

    private bool EnemyInDamageZone(BaseEnemy enemy, Vector3 petPosition) =>
       petPosition.x + PetAbilityParams.DamageArea.x >= enemy.Position.x &&
        petPosition.y + PetAbilityParams.DamageArea.y >= enemy.Position.y &&
        petPosition.x - PetAbilityParams.DamageAreaOffset.x <= enemy.Position.x &&
        petPosition.y - PetAbilityParams.DamageAreaOffset.y <= enemy.Position.y;

    public bool IsAttackAbility() => PetAbilityParams.AbilityType is
            PetAbilityType.Damage or
            PetAbilityType.DamageZone or
            PetAbilityType.DamageOverTime or
            PetAbilityType.DamageOverTimeFromAbove;

    public Vector3 GetCurrentPetPos(Player player) =>
        new()
        {
            x = player.TempData.Position.x,
            y = player.TempData.Position.y,
            z = player.TempData.Position.z
        };
}
