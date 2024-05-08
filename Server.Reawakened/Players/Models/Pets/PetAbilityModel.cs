using A2m.Server;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Network.Extensions;
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
    public bool DefenseBoostActive { get; set; }
    public bool DefensiveBarrierActive { get; set; }

    public PetAbilityModel(PetAbilityParams petAbilityParams, float abilityCooldown, bool defenseBoostActive, bool defensiveBarrierActive)
    {
        PetAbilityParams = petAbilityParams;
        AbilityCooldown = abilityCooldown;
        DefenseBoostActive = defenseBoostActive;
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
        player.SendXt("Zm", player.UserId, true);

        //Sends method of ability type after a short delay.
        timerThread.DelayCall(GetAbilityType(), player, TimeSpan.FromSeconds(PetAbilityParams.InitialDelayBeforeUse),
            TimeSpan.FromSeconds(PetAbilityParams.Frequency), PetAbilityParams.HitCount);
    }

    public string GetAbilitySyncParams(Player player) =>
        IsAttackAbility() ? GetClosestEnemy(GetDetectedEnemies(player)).Id : player.GameObjectId;

    public TimerCallback GetAbilityType()
    {
        switch (PetAbilityParams.AbilityType)
        {
            //case PetAbilityType.Invalid:
            //    break;

            case PetAbilityType.Heal:
                return PetHealPlayer;

            case PetAbilityType.Damage:
            case PetAbilityType.DamageOverTime:
            case PetAbilityType.DamageOverTimeFromAbove:
                return AttackEnemy;

            case PetAbilityType.DamageZone:
                return AttackEnemiesInZone;

            case PetAbilityType.Defence:
                DefenseBoostActive = true;
                return DisableDefenseAfterDelay;

            case PetAbilityType.DefensiveBarrier:
                DefensiveBarrierActive = true;
                return DisableDefensiveBarrierAfterDelay;

            //case PetAbilityType.Unknown:
            //    break;

            default: return null;
        }
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

    public void DisableDefensiveBarrierAfterDelay(object _) => DefensiveBarrierActive = false;

    public void DisableDefenseAfterDelay(object _) => DefenseBoostActive = false;
  
    public void PetHealPlayer(object player)
    {
        var petOwner = (Player)player;
        petOwner.PetHeal((int)Math.Ceiling(petOwner.Character.Data.MaxLife * PetAbilityParams.ApplyOnHealthRatio));
    }

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
