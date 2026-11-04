using A2m.Server;
using PetDefines;
using Server.Base.Core.Abstractions;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Entities.Enemies.EnemyTypes.Abstractions;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players.Models.Pets;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Timers;
using UnityEngine;
using TimerCallback = Server.Base.Timers.Timer.TimerCallback;
using Vector3 = UnityEngine.Vector3;

namespace Server.Reawakened.Players.Extensions;

public static class PetAbilityExtensions
{
    private static void SendDeactivateState(this Player player) =>
        player.Room.SendSyncEvent(
            new PetState_SyncEvent(
                player.GameObjectId, player.Room.Time, PetInformation.StateSyncType.Deactivate, player.GameObjectId
            )
        );

    public static void SendAbility(this Player petOwner,
        TimerThread timerThread, ServerRConfig serverRConfig)
    {
        if (!petOwner.Character.Pets.TryGetValue(petOwner.GetItemIdOfEquippedPet(), out var pet))
            return;
        
        pet.InCoopJumpState = false;
        pet.InCoopSwitchState = false;
        pet.AbilityCooldown = petOwner.Room.Time + pet.AbilityParams.CooldownTime;

        if (petOwner.TempData.IsKnockedOut)
            return;

        petOwner.SendXt("Zm", petOwner.UserId, "1");
		
        petOwner.Room.SendSyncEvent(new PetState_SyncEvent(petOwner.GameObjectId, petOwner.Room.Time,
            PetInformation.StateSyncType.Ability, petOwner.GetSyncParams(serverRConfig)));

        pet.UseEnergy(petOwner);

        //Sends method of ability type after a short delay.
        timerThread.RunInterval(pet.GetAbilityType(), new PlayerTimer() { Player = petOwner },
            TimeSpan.FromSeconds(pet.AbilityParams.Frequency), pet.AbilityParams.HitCount,
            TimeSpan.FromSeconds(pet.AbilityParams.InitialDelayBeforeUse)
        );

        timerThread.RunDelayed(DisableAutonomousMode, new PlayerTimer() { Player = petOwner },
            TimeSpan.FromSeconds(pet.AbilityParams.CooldownTime));
    }

    private static void DisableAutonomousMode(ITimerData data)
    {
        if (data is not PlayerTimer timer) return; 

        var player = timer.Player;

        player.SendXt("Zm", player.UserId, false);
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

    private static string GetSyncParams(this Player petOwner, ServerRConfig serverRConfig) =>
        petOwner.Character.Pets.TryGetValue(petOwner.GetItemIdOfEquippedPet(), out var pet)
            ? pet.AbilityParams.IsAttackAbility()
                ? petOwner.TempData.EnemiesInPetAbilityZone.Count > 0
                    ? GetClosestEnemy(petOwner.TempData.EnemiesInPetAbilityZone).Id
                    : string.Empty
                : petOwner.GameObjectId
            : string.Empty;

    private static void PetHealPlayer(ITimerData data)
    {
        if (data is not PlayerTimer timer) return;

        var player = timer.Player;

        if (player.TempData.IsKnockedOut)
            return;

        if (!player.Character.Pets.TryGetValue(player.GetItemIdOfEquippedPet(), out var pet))
        {
            player.SendDeactivateState();
            return;
        }

        var health = GameFlow.StatisticData.GetValue(ItemEffectType.IncreaseHitPoints, WorldStatisticsGroup.Player, player.Character.GlobalLevel);
        
        var ratio = health * pet.AbilityParams.ItemEffectStatRatio;
        var healValue = (int)(Mathf.Ceil(ratio / 10f) * 10f);
        
        player.PetHeal(healValue);
    }

    private static void AttackEnemiesInZone(ITimerData data)
    {
        if (data is not PlayerTimer timer)
            return;

        var player = timer.Player;

        if (player.TempData.IsKnockedOut)
            return;

        if (!player.Character.Pets.TryGetValue(player.GetItemIdOfEquippedPet
            (), out var pet) || !pet.AbilityParams.IsAttackAbility())
            return;

        var closestEnemy = GetClosestEnemy(timer.Player.TempData.EnemiesInPetAbilityZone);

        if (closestEnemy == null)
        {
            player.SendDeactivateState();
            return;
        }

        if (pet.AbilityParams.AbilityType is PetAbilityType.DamageZone)
            foreach (var enemyDetected in timer.Player.TempData.EnemiesInPetAbilityZone.Values.Where
            (enemy => pet.AbilityParams.EnemyInDamageZone(enemy, player.GetCurrentPetPos())))
                enemyDetected.PetDamage(player);
        else
            closestEnemy.PetDamage(player);
    }

    private static void ActivateDefence(ITimerData data)
    {
        if (data is not PlayerTimer timer)
            return;

        var player = timer.Player;

        if (player.TempData.IsKnockedOut)
            return;

        if (!player.Character.Pets.TryGetValue
            (player.GetItemIdOfEquippedPet(), out var pet))
        {
            player.SendDeactivateState();
            return;
        }

        SendDefenseAbilityEffect(player, pet);
        pet.ShieldingPlayer = !pet.ShieldingPlayer;
    }

    private static void ActivateDefensiveBarrier(ITimerData data)
    {
        if (data is not PlayerTimer timer)
            return;

        var player = timer.Player;

        if (player.TempData.IsKnockedOut)
            return;

        if (!player.Character.Pets.TryGetValue
            (player.GetItemIdOfEquippedPet(), out var pet))
        {
            player.SendDeactivateState();
            return;
        }

        SendDefenseAbilityEffect(player, pet);
        pet.GuardingPlayer = !pet.GuardingPlayer;
    }

    private static void SendDefenseAbilityEffect(Player petOwner, PetModel pet)
    {
        if (pet.AbilityParams.AbilityType is PetAbilityType.Defence or PetAbilityType.DefensiveBarrier)
        {
            var damage = GameFlow.StatisticData.GetValue(ItemEffectType.AbilityPower, WorldStatisticsGroup.Enemy, petOwner.Character.GlobalLevel);
            var defense = GameFlow.StatisticData.GetValue(ItemEffectType.Defence, WorldStatisticsGroup.Player, petOwner.Character.GlobalLevel);
            
            var abilityStat = pet.AbilityParams.AbilityType != PetAbilityType.DefensiveBarrier ? pet.AbilityParams.ItemEffectStatRatio : pet.AbilityParams.DefensiveBonusRatio;
            var ratio = damage - defense - abilityStat * damage;
            var defenseValue = (int)(Mathf.Ceil(ratio / 10f) * 10f);
            
            petOwner.Room.SendSyncEvent(new StatusEffect_SyncEvent(petOwner.GameObjectId, petOwner.Room.Time,
                (int)ItemEffectType.Defence, defenseValue, (int)pet.AbilityParams.Duration,
                true, pet.PrefabName, false));
        }
    }

    private static BaseEnemy GetClosestEnemy(this Dictionary<float, BaseEnemy> detectedEnemies)
    {
        if (detectedEnemies.Count <= 0)
            return null;

        var closestDistance = detectedEnemies.Keys.Min();
        return detectedEnemies[closestDistance];
    }

    public static Dictionary<float, BaseEnemy> GetPetDetectedEnemies(this PetAbilityParams abilityParams,
        Player player, ServerRConfig serverRConfig)
    {
        if (player == null || !player.Character.Pets.TryGetValue
            (player.GetItemIdOfEquippedPet(), out var pet) ||
            player.TempData.EnemiesInPetAbilityZone == null)
            return null;

        player.TempData.EnemiesInPetAbilityZone.Clear();

        var petPosition = player.GetCurrentPetPos();
        var detectedEnemies = new Dictionary<float, BaseEnemy>();

        foreach (var enemy in player.Room.GetEnemies().Where
            (enemy => enemy.ParentPlane == player.GetPlayersPlaneString() &&
            abilityParams.EnemyInDetectionZone(enemy, petPosition)))
        {
            var distanceFromPlayer = (float)Math.Sqrt(
                Math.Pow(petPosition.x - enemy.Position.X, 2) +
                Math.Pow(petPosition.y - enemy.Position.Y, 2));

            detectedEnemies.TryAdd(distanceFromPlayer, enemy);
        }

        return player.TempData.EnemiesInPetAbilityZone = detectedEnemies;
    }
    private static bool EnemyInDetectionZone(this PetAbilityParams abilityParams, BaseEnemy enemy, Vector3 petPosition) =>
       petPosition.x + abilityParams.DetectionZone.x >= enemy.Position.X &&
        petPosition.y + abilityParams.DetectionZone.y >= enemy.Position.Y &&
        petPosition.x - abilityParams.DetectionZoneOffset.x <= enemy.Position.X &&
        petPosition.y - abilityParams.DetectionZoneOffset.y <= enemy.Position.Y;

    private static bool EnemyInDamageZone(this PetAbilityParams abilityParams, BaseEnemy enemy, Vector3 petPosition) =>
        petPosition.x + abilityParams.DamageArea.x >= enemy.Position.X &&
        petPosition.y + abilityParams.DamageArea.y >= enemy.Position.Y &&
        petPosition.x - abilityParams.DamageAreaOffset.x <= enemy.Position.X &&
        petPosition.y - abilityParams.DamageAreaOffset.y <= enemy.Position.Y;

    public static bool IsAttackAbility(this PetAbilityParams abilityParams) =>
        abilityParams.AbilityType is
            PetAbilityType.Damage or
            PetAbilityType.DamageZone or
            PetAbilityType.DamageOverTime or
            PetAbilityType.DamageOverTimeFromAbove;

    public static Vector3 GetCurrentPetPos(this Player petOwner) =>
        petOwner.TempData.Position.ToUnityVector3();
}
