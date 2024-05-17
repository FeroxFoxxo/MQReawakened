using A2m.Server;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Rooms.Extensions;
using PetDefines;
using Server.Reawakened.Entities.Enemies.EnemyTypes.Abstractions;
using TimerCallback = Server.Base.Timers.Timer.TimerCallback;
using Server.Reawakened.Core.Configs;
using Vector3 = UnityEngine.Vector3;
using Server.Reawakened.Players.Models.Pets;

namespace Server.Reawakened.Players.Extensions;

public static class PetAbilityExtensions
{
    private static void SendDeactivateState(this Player player) =>
        player.Room.SendSyncEvent(
            new PetState_SyncEvent(
                player.GameObjectId, player.Room.Time, PetInformation.StateSyncType.Deactivate, player.GameObjectId
            )
        );

    public static void SendAbility(this Player petOwner, ServerRConfig serverRConfig, TimerThread timerThread)
    {
        if (!petOwner.Character.Pets.TryGetValue(petOwner.GetEquippedPetId(serverRConfig), out var pet))
            return;

        pet.InCoopJumpState = false;
        pet.InCoopSwitchState = false;
        pet.AbilityCooldown = petOwner.Room.Time + pet.AbilityParams.CooldownTime;

        petOwner.Room.SendSyncEvent(new PetState_SyncEvent(petOwner.GameObjectId, petOwner.Room.Time,
            PetInformation.StateSyncType.Ability, petOwner.GetSyncParams(pet.AbilityParams)));
        petOwner.Room.SendSyncEvent(new StatusEffect_SyncEvent(petOwner.GameObjectId, petOwner.Room.Time,
            (int)ItemEffectType.AbilityPower, 0, (int)pet.AbilityParams.Duration, true, petOwner.GameObjectId, true));

        pet.UseEnergy(petOwner);
        //Sends method of ability type after a short delay.
        timerThread.DelayCall(pet.GetAbilityType(), petOwner, TimeSpan.FromSeconds(pet.AbilityParams.InitialDelayBeforeUse),
            TimeSpan.FromSeconds(pet.AbilityParams.Frequency), pet.AbilityParams.HitCount);
    }

    private static TimerCallback GetAbilityType(this PetModel pet) =>
        pet.AbilityParams.AbilityType switch
        {
            PetAbilityType.Heal => PetHealPlayer,
            PetAbilityType.DamageOverTime or PetAbilityType.DamageOverTimeFromAbove or
            PetAbilityType.Damage or PetAbilityType.DamageZone => AttackEnemiesInZone,
            PetAbilityType.Defence => ActivateDefence,
            PetAbilityType.DefensiveBarrier => ActivateDefensiveBarrier,
            _ => null,
        };

    private static string GetSyncParams(this Player petOwner, PetAbilityParams abilityParams) =>
        abilityParams.IsAttackAbility() ? petOwner.GetDetectedEnemies().GetClosestEnemy().Id : petOwner.GameObjectId;

    private static void PetHealPlayer(object player)
    {
        var petOwner = (Player)player;

        if (petOwner == null || !petOwner.Character.Pets.TryGetValue(
            petOwner.GetEquippedPetId(new ServerRConfig()), out var pet))
        {
            petOwner.SendDeactivateState();
            return;
        }

        petOwner.PetHeal((int)Math.Ceiling
        (petOwner.Character.Data.MaxLife * pet.AbilityParams.ApplyOnHealthRatio));
    }

    private static void AttackEnemiesInZone(object player)
    {
        var petOwner = (Player)player;

        if (petOwner == null || !petOwner.Character.Pets.TryGetValue(petOwner.GetEquippedPetId
            (new ServerRConfig()), out var pet) || !pet.AbilityParams.IsAttackAbility())
            return;

        var detectedEnemies = petOwner.GetDetectedEnemies();
        var closestEnemy = detectedEnemies.GetClosestEnemy();

        if (detectedEnemies.Count <= 0 || closestEnemy == null)
        {
            petOwner.SendDeactivateState();
            return;
        }

        if (pet.AbilityParams.AbilityType is PetAbilityType.DamageZone)
            foreach (var enemyDetected in detectedEnemies.Values.Where
            (enemy => pet.AbilityParams.EnemyInDamageZone(enemy, petOwner.GetCurrentPetPos())))
                enemyDetected.PetDamage(petOwner);
        else
            closestEnemy.PetDamage(petOwner);
    }

    private static void ActivateDefence(object player)
    {
        var petOwner = (Player)player;

        if (petOwner == null || !petOwner.Character.Pets.ContainsKey
            (petOwner.GetEquippedPetId(new ServerRConfig())))
        {
            petOwner.SendDeactivateState();
            return;
        }

        petOwner.TempData.PetDefense = !petOwner.TempData.PetDefense;
    }

    private static void ActivateDefensiveBarrier(object player)
    {
        var petOwner = (Player)player;

        if (petOwner == null || !petOwner.Character.Pets.ContainsKey
            (petOwner.GetEquippedPetId(new ServerRConfig())))
        {
            petOwner.SendDeactivateState();
            return;
        }

        petOwner.TempData.PetDefensiveBarrier = !petOwner.TempData.PetDefensiveBarrier;
    }

    public static Dictionary<int, BaseEnemy> GetDetectedEnemies(this Player petOwner)
    {
        if (petOwner == null || !petOwner.Character.Pets.TryGetValue
            (petOwner.GetEquippedPetId(new ServerRConfig()), out var pet))
            return null;

        var enemies = new Dictionary<int, BaseEnemy>();
        var petPosition = petOwner.GetCurrentPetPos();

        foreach (var enemy in petOwner.Room.GetEnemies()
            .Where(enemy => enemy.ParentPlane == petOwner.GetPlayersPlaneString() &&
            pet.AbilityParams.EnemyInDetectionZone(enemy, petPosition)))
        {
            var distanceFromPlayer = (int)Math.Sqrt(
                Math.Pow(petPosition.x - enemy.Position.x, 2) +
                Math.Pow(petPosition.y - enemy.Position.y, 2));

            enemies.TryAdd(distanceFromPlayer, enemy);
        }

        return enemies;
    }

    private static BaseEnemy GetClosestEnemy(this Dictionary<int, BaseEnemy> detectedEnemies)
    {
        if (detectedEnemies.Count <= 0)
            return null;

        var closestDistance = detectedEnemies.Keys.Min();
        return detectedEnemies[closestDistance];
    }

    private static bool EnemyInDetectionZone(this PetAbilityParams abilityParams, BaseEnemy enemy, Vector3 petPosition) =>
        petPosition.x + abilityParams.DetectionZone.x >= enemy.Position.x &&
           petPosition.y + abilityParams.DetectionZone.y >= enemy.Position.y &&
           petPosition.x - abilityParams.DetectionZoneOffset.x <= enemy.Position.x &&
           petPosition.y - abilityParams.DetectionZoneOffset.y <= enemy.Position.y;

    private static bool EnemyInDamageZone(this PetAbilityParams abilityParams, BaseEnemy enemy, Vector3 petPosition) =>
        petPosition.x + abilityParams.DamageArea.x >= enemy.Position.x &&
        petPosition.y + abilityParams.DamageArea.y >= enemy.Position.y &&
        petPosition.x - abilityParams.DamageAreaOffset.x <= enemy.Position.x &&
        petPosition.y - abilityParams.DamageAreaOffset.y <= enemy.Position.y;

    public static bool IsAttackAbility(this PetAbilityParams abilityParams) =>
        abilityParams.AbilityType is
            PetAbilityType.Damage or
            PetAbilityType.DamageZone or
            PetAbilityType.DamageOverTime or
            PetAbilityType.DamageOverTimeFromAbove;

    private static Vector3 GetCurrentPetPos(this Player petOwner) =>
        new()
        {
            x = petOwner.TempData.Position.x,
            y = petOwner.TempData.Position.y,
            z = petOwner.TempData.Position.z
        };
}
