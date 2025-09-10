using A2m.Server;
using PetDefines;
using Server.Base.Core.Abstractions;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Entities.Enemies.EnemyTypes.Abstractions;
using Server.Reawakened.Players.Models.Pets;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Timers;
using Server.Reawakened.XMLs.Bundles.Base;
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

    public static void SendAbility(this Player petOwner, ItemCatalog itemCatalog,
        ServerRConfig serverRConfig, TimerThread timerThread, WorldStatistics worldStatistics)
    {
        if (!petOwner.Character.Pets.TryGetValue(petOwner.GetEquippedPetId(serverRConfig), out var pet))
            return;

        pet.InCoopJumpState = false;
        pet.InCoopSwitchState = false;
        pet.AbilityCooldown = petOwner.Room.Time + pet.AbilityParams.CooldownTime;

        if (petOwner.TempData.IsKnockedOut)
            return;

        petOwner.Room.SendSyncEvent(new PetState_SyncEvent(petOwner.GameObjectId, petOwner.Room.Time,
            PetInformation.StateSyncType.Ability, petOwner.GetSyncParams(pet.AbilityParams)));

        if (pet.AbilityParams.AbilityType is PetAbilityType.Defence or PetAbilityType.DefensiveBarrier)
            petOwner.Room.SendSyncEvent(new StatusEffect_SyncEvent(petOwner.GameObjectId, petOwner.Room.Time,
                (int)ItemEffectType.Defence, (int)pet.AbilityParams.DefensiveBonusRatio, (int)pet.AbilityParams.Duration,
                true, itemCatalog.GetItemFromId(int.Parse(pet.PetId)).PrefabName, false));

        pet.UseEnergy(petOwner);
        pet.StartEnergyRegeneration(petOwner, timerThread, worldStatistics);

        //Sends method of ability type after a short delay.
        timerThread.RunInterval(pet.GetAbilityType(), new PlayerTimer() { Player = petOwner },
            TimeSpan.FromSeconds(pet.AbilityParams.Frequency), pet.AbilityParams.HitCount,
            TimeSpan.FromSeconds(pet.AbilityParams.InitialDelayBeforeUse)
        );
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

    private static void PetHealPlayer(ITimerData data)
    {
        if (data is not PlayerTimer timer)
            return;

        var player = timer.Player;

        if (player.TempData.IsKnockedOut)
            return;

        if (!player.Character.Pets.TryGetValue(player.GetEquippedPetId(new ServerRConfig()), out var pet))
        {
            player.SendDeactivateState();
            return;
        }

        player.PetHeal((int)Math.Ceiling
        (player.Character.MaxLife * pet.AbilityParams.ApplyOnHealthRatio));
    }

    private static void AttackEnemiesInZone(ITimerData data)
    {
        if (data is not PlayerTimer timer)
            return;

        var player = timer.Player;

        if (player.TempData.IsKnockedOut)
            return;

        if (!player.Character.Pets.TryGetValue(player.GetEquippedPetId
            (new ServerRConfig()), out var pet) || !pet.AbilityParams.IsAttackAbility())
            return;

        var detectedEnemies = player.GetDetectedEnemies();
        var closestEnemy = detectedEnemies.GetClosestEnemy();

        if (detectedEnemies.Count <= 0 || closestEnemy == null)
        {
            player.SendDeactivateState();
            return;
        }

        if (pet.AbilityParams.AbilityType is PetAbilityType.DamageZone)
            foreach (var enemyDetected in detectedEnemies.Values.Where
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

        if (!player.Character.Pets.ContainsKey
            (player.GetEquippedPetId(new ServerRConfig())))
        {
            player.SendDeactivateState();
            return;
        }

        player.TempData.PetDefense = !player.TempData.PetDefense;
    }

    private static void ActivateDefensiveBarrier(ITimerData data)
    {
        if (data is not PlayerTimer timer)
            return;

        var player = timer.Player;

        if (player.TempData.IsKnockedOut)
            return;

        if (!player.Character.Pets.ContainsKey
            (player.GetEquippedPetId(new ServerRConfig())))
        {
            player.SendDeactivateState();
            return;
        }

        player.TempData.PetDefensiveBarrier = !player.TempData.PetDefensiveBarrier;
    }

    public static Dictionary<int, BaseEnemy> GetDetectedEnemies(this Player player)
    {
        if (player == null)
            return null;

        if (!player.Character.Pets.TryGetValue
            (player.GetEquippedPetId(new ServerRConfig()), out var pet))
            return null;

        var enemies = new Dictionary<int, BaseEnemy>();
        var petPosition = player.GetCurrentPetPos();

        foreach (var enemy in player.Room.GetEnemies()
            .Where(enemy => enemy.ParentPlane == player.GetPlayersPlaneString() &&
            pet.AbilityParams.EnemyInDetectionZone(enemy, petPosition)))
        {
            var distanceFromPlayer = (int)Math.Sqrt(
                Math.Pow(petPosition.x - enemy.Position.X, 2) +
                Math.Pow(petPosition.y - enemy.Position.Y, 2));

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

    private static Vector3 GetCurrentPetPos(this Player petOwner) =>
        petOwner.TempData.Position.ToUnityVector3();
}
