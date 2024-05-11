using A2m.Server;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Rooms.Extensions;
using UnityEngine;
using PetDefines;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Entities.Enemies.EnemyTypes.Abstractions;
using TimerCallback = Server.Base.Timers.Timer.TimerCallback;
using Server.Reawakened.Core.Configs;
using AssetStudio;
using Microsoft.Extensions.Logging;
using Vector3 = UnityEngine.Vector3;

namespace Server.Reawakened.Players.Models.Pets;

public class PetAbilityExtensions
{
    private void SendDeactivateState(Player player) => player.Room.SendSyncEvent(new PetState_SyncEvent(
        player.GameObjectId, player.Room.Time, PetInformation.StateSyncType.Deactivate, player.GameObjectId));

    public void SendAbility(Player petOwner, ServerRConfig serverRConfig, TimerThread timerThread)
    {
        if (!petOwner.Character.Pets.TryGetValue(petOwner.GetEquippedPetId(serverRConfig), out var pet))
            return;

        pet.InCoopJumpState = false;
        pet.InCoopSwitchState = false;
        pet.AbilityCooldown = petOwner.Room.Time + pet.AbilityParams.CooldownTime;

        petOwner.Room.SendSyncEvent(new PetState_SyncEvent(petOwner.GameObjectId, petOwner.Room.Time,
            PetInformation.StateSyncType.Ability, GetSyncParams(petOwner, pet.AbilityParams)));
        petOwner.Room.SendSyncEvent(new StatusEffect_SyncEvent(petOwner.GameObjectId, petOwner.Room.Time,
            (int)ItemEffectType.AbilityPower, 0, (int)pet.AbilityParams.Duration, true, petOwner.GameObjectId, true));

        pet.UseEnergy(petOwner);
        //Sends method of ability type after a short delay.
        timerThread.DelayCall(GetAbilityType(pet), petOwner, TimeSpan.FromSeconds(pet.AbilityParams.InitialDelayBeforeUse),
            TimeSpan.FromSeconds(pet.AbilityParams.Frequency), pet.AbilityParams.HitCount);
    }

    private TimerCallback GetAbilityType(PetModel pet) =>
        pet.AbilityParams.AbilityType switch
        {
            PetAbilityType.Heal => PetHealPlayer,
            PetAbilityType.DamageOverTime or PetAbilityType.DamageOverTimeFromAbove or
            PetAbilityType.Damage or PetAbilityType.DamageZone => AttackEnemiesInZone,
            PetAbilityType.Defence => ActivateDefence,
            PetAbilityType.DefensiveBarrier => ActivateDefensiveBarrier,
            _ => null,
        };

    private string GetSyncParams(Player petOwner, PetAbilityParams abilityParams) =>
        IsAttackAbility(abilityParams) ? GetClosestEnemy(GetDetectedEnemies(petOwner)).Id : petOwner.GameObjectId;

    private void PetHealPlayer(object player)
    {
        var petOwner = (Player)player;

        if (petOwner == null || !petOwner.Character.Pets.TryGetValue(
            petOwner.GetEquippedPetId(new ServerRConfig()), out var pet))
        {
            SendDeactivateState(petOwner);
            return;
        }


        petOwner.PetHeal((int)Math.Ceiling
        (petOwner.Character.Data.MaxLife * pet.AbilityParams.ApplyOnHealthRatio));
    }

    private void AttackEnemiesInZone(object player)
    {
        var petOwner = (Player)player;

        if (petOwner == null || !petOwner.Character.Pets.TryGetValue(petOwner.GetEquippedPetId
            (new ServerRConfig()), out var pet) || !IsAttackAbility(pet.AbilityParams))
            return;

        var detectedEnemies = GetDetectedEnemies(petOwner);
        var closestEnemy = GetClosestEnemy(detectedEnemies);

        if (detectedEnemies.Count <= 0 || closestEnemy == null)
        {
            SendDeactivateState(petOwner);
            return;
        }

        if (pet.AbilityParams.AbilityType is PetAbilityType.DamageZone)
            foreach (var enemyDetected in detectedEnemies.Values.Where
            (enemy => EnemyInDamageZone(pet.AbilityParams, enemy, GetCurrentPetPos(petOwner))))
                enemyDetected.PetDamage(petOwner);

        else
            closestEnemy.PetDamage(petOwner);
    }

    private void ActivateDefence(object player)
    {
        var petOwner = (Player)player;

        if (petOwner == null || !petOwner.Character.Pets.ContainsKey
            (petOwner.GetEquippedPetId(new ServerRConfig())))
        {
            SendDeactivateState(petOwner);
            return;
        }

        petOwner.TempData.PetDefense = !petOwner.TempData.PetDefense;
    }

    private void ActivateDefensiveBarrier(object player)
    {
        var petOwner = (Player)player;

        if (petOwner == null || !petOwner.Character.Pets.ContainsKey
            (petOwner.GetEquippedPetId(new ServerRConfig())))
        {
            SendDeactivateState(petOwner);
            return;
        }

        petOwner.TempData.PetDefensiveBarrier = !petOwner.TempData.PetDefensiveBarrier;
    }

    public Dictionary<int, BaseEnemy> GetDetectedEnemies(Player petOwner)
    {
        if (petOwner == null || !petOwner.Character.Pets.TryGetValue
            (petOwner.GetEquippedPetId(new ServerRConfig()), out var pet))
            return null;

        var enemies = new Dictionary<int, BaseEnemy>();
        var petPosition = GetCurrentPetPos(petOwner);

        foreach (var enemy in petOwner.Room.GetEnemies()
            .Where(enemy => enemy.ParentPlane == petOwner.GetPlayersPlaneString() &&
            EnemyInDetectionZone(pet.AbilityParams, enemy, petPosition)))
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

    private bool EnemyInDetectionZone(PetAbilityParams abilityParams, BaseEnemy enemy, Vector3 petPosition) =>
        petPosition.x + abilityParams.DetectionZone.x >= enemy.Position.x &&
           petPosition.y + abilityParams.DetectionZone.y >= enemy.Position.y &&
           petPosition.x - abilityParams.DetectionZoneOffset.x <= enemy.Position.x &&
           petPosition.y - abilityParams.DetectionZoneOffset.y <= enemy.Position.y;

    private bool EnemyInDamageZone(PetAbilityParams abilityParams, BaseEnemy enemy, Vector3 petPosition) =>
        petPosition.x + abilityParams.DamageArea.x >= enemy.Position.x &&
        petPosition.y + abilityParams.DamageArea.y >= enemy.Position.y &&
        petPosition.x - abilityParams.DamageAreaOffset.x <= enemy.Position.x &&
        petPosition.y - abilityParams.DamageAreaOffset.y <= enemy.Position.y;

    public bool IsAttackAbility(PetAbilityParams abilityParams) =>
        abilityParams.AbilityType is
            PetAbilityType.Damage or
            PetAbilityType.DamageZone or
            PetAbilityType.DamageOverTime or
            PetAbilityType.DamageOverTimeFromAbove;

    private Vector3 GetCurrentPetPos(Player petOwner) =>
        new()
        {
            x = petOwner.TempData.Position.x,
            y = petOwner.TempData.Position.y,
            z = petOwner.TempData.Position.z
        };
}
