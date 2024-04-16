using A2m.Server;
using Microsoft.Extensions.Logging;
using PetDefines;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Configs;
using Server.Reawakened.Entities.Enemies.EnemyTypes.Abstractions;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.Bundles.Base;
using UnityEngine;

namespace Protocols.External._Z__PetHandler;
public class UpdatePetMode : ExternalProtocol
{
    public override string ProtocolName => "Zm";

    public PetAbilities PetAbilities { get; set; }

    public TimerThread TimerThread { get; set; }
    public ItemRConfig ItemRConfig { get; set; }
    public ItemCatalog ItemCatalog { get; set; }
    public ILogger<UpdatePetMode> Logger { get; set; }

    public override void Run(string[] message)
    {
        var petId = Player.Character.Data.PetItemId;
        var pet = Player.Character.Pets[petId];
        var petAbilityParams = PetAbilities.PetAbilityData[petId];

        var syncParams = Player.GameObjectId;

        var closestDistance = 0;
        Dictionary<int, BaseEnemy> closestEnemies = null;
        BaseEnemy closestEnemy = null;

        if (pet.InCoopJumpState || pet.InCoopSwitchState ||
            pet.AbilityCooldown > Player.Room.Time && pet.AbilityCooldown != 0)
        {
            Logger.LogInformation("{characterName}'s pet is already in a coop state or waiting for ability cooldown to finish.", Player.CharacterName);
            return;
        }

        if (pet.UseEnergy(Player, ItemRConfig) <= 0)
        {
            Player.SendXt("Zo", Player.UserId);
            return;
        }

        if (IsAttackAbility(petAbilityParams))
        {
            closestEnemies = ClosestEnemiesToAttack(petAbilityParams);

            if (closestEnemies.Count > 0)
            {
                closestDistance = closestEnemies.Keys.Min();
                closestEnemy = closestEnemies[closestDistance];
            }
        }

        pet.AbilityCooldown = Player.Room.Time + petAbilityParams.CooldownTime;

        switch (petAbilityParams.AbilityType)
        {
            //case PetAbilityType.Invalid:
            //    break;

            case PetAbilityType.Heal:
                TimerThread.DelayCall(PetHealPlayer, petAbilityParams, TimeSpan.FromSeconds(petAbilityParams.InitialDelayBeforeUse),
                    TimeSpan.FromSeconds(petAbilityParams.Frequency), petAbilityParams.UseCount);
                break;

            case PetAbilityType.DamageZone:
                foreach (var enemyToAttack in closestEnemies.Values)
                {
                    Player.Room.SendSyncEvent(new DamageZone_SyncEvent(enemyToAttack.Id, Player.Room.Time,
                        enemyToAttack.Position.x, enemyToAttack.Position.y, enemyToAttack.Position.z, 5f, 5f, 5f,
                        Player.Room.CreateProjectileId(), ItemCatalog.GetItemFromId(petId).PrefabName));

                    TimerThread.DelayCall(AttackClosestEnemy, enemyToAttack, TimeSpan.FromSeconds(petAbilityParams.InitialDelayBeforeUse),
                        TimeSpan.FromSeconds(petAbilityParams.Frequency), petAbilityParams.HitCount);
                }
                break;

            case PetAbilityType.Damage:
            case PetAbilityType.DamageOverTime:
            case PetAbilityType.DamageOverTimeFromAbove:
                TimerThread.DelayCall(AttackClosestEnemy, closestEnemy, TimeSpan.FromSeconds(petAbilityParams.InitialDelayBeforeUse),
                    TimeSpan.FromSeconds(petAbilityParams.Frequency), petAbilityParams.HitCount);
                syncParams = closestEnemy.Id;
                break;

            case PetAbilityType.Defence:
                Player.Room.SendSyncEvent(new StatusEffect_SyncEvent(Player.GameObjectId, Player.Room.Time,
                    (int)ItemEffectType.IncreaseAllResist, (int)Math.Ceiling(Player.Character.Data.MaxLife * petAbilityParams.DefensiveBonusRatio),
                    (int)petAbilityParams.Duration, true, ItemCatalog.GetItemFromId(petId).PrefabName, true));

                Player.TempData.PetDefenseBoost = true;
                TimerThread.DelayCall(DisablePetDefenseBoost, Player, TimeSpan.FromSeconds(petAbilityParams.Duration),
                    TimeSpan.FromSeconds(petAbilityParams.Frequency), petAbilityParams.HitCount);
                break;

            case PetAbilityType.DefensiveBarrier:
                Player.Room.SendSyncEvent(new StatusEffect_SyncEvent(Player.GameObjectId, Player.Room.Time,
                 (int)ItemEffectType.IncreaseAllResist, (int)Math.Ceiling(Player.Character.Data.MaxLife * petAbilityParams.DefensiveBonusRatio),
                 (int)petAbilityParams.Duration, true, ItemCatalog.GetItemFromId(petId).PrefabName, true));

                Player.TempData.PetDefensiveBarrier = true;
                TimerThread.DelayCall(DisablePetDefensiveBarrier, Player, TimeSpan.FromSeconds(petAbilityParams.Duration), TimeSpan.Zero, petAbilityParams.HitCount);
                break;

                //case PetAbilityType.Unknown:
                //    break;
        }

        Player.Room.SendSyncEvent(new PetState_SyncEvent(Player.GameObjectId, Player.Room.Time,
            PetInformation.StateSyncType.Ability, syncParams));

        pet.AbilityCooldown = Player.Room.Time + petAbilityParams.CooldownTime;
        Player.SendXt("Zm", Player.UserId, true);
    }

    private void DisablePetDefensiveBarrier(object _) => Player.TempData.PetDefensiveBarrier = false;

    private void DisablePetDefenseBoost(object _) => Player.TempData.PetDefenseBoost = false;

    private void PetHealPlayer(object petAbilityParams)
    {
        var abilityParams = (PetAbilityParams)petAbilityParams;

        Player.PetHeal((int)Math.Ceiling(Player.Character.Data.MaxLife * abilityParams.ApplyOnHealthRatio));
    }

    private void AttackClosestEnemy(object enemy)
    {
        var closestEnemy = (BaseEnemy)enemy;
        closestEnemy.PetDamage(Player);
    }

    private Dictionary<int, BaseEnemy> ClosestEnemiesToAttack(PetAbilityParams petAbilityParams)
    {
        var enemies = new Dictionary<int, BaseEnemy>();
        var petPosition = Player.TempData.Position;

        foreach (var enemy in Player.Room.GetEnemies()
            .Where(enemy => enemy.ParentPlane == Player.GetPlayersPlaneString() &&
            EnemyInRange(petAbilityParams, enemy, petPosition)))
        {
            var distanceFromPlayer = (int)Math.Sqrt(
                Math.Pow(Player.TempData.Position.x - enemy.Position.x, 2) +
                Math.Pow(Player.TempData.Position.y - enemy.Position.y, 2));

            enemies.TryAdd(distanceFromPlayer, enemy);
        }

        return enemies;
    }

    private bool EnemyInRange(PetAbilityParams petAbilityParams, BaseEnemy enemy, Vector3 petPosition) =>
        petPosition.x - petAbilityParams.DetectionZone.x <= enemy.Position.x &&
        petPosition.y - petAbilityParams.DetectionZone.y <= enemy.Position.y &&
        petPosition.x + petAbilityParams.DetectionZoneOffset.x >= enemy.Position.x &&
        petPosition.y + petAbilityParams.DetectionZoneOffset.y >= enemy.Position.y;

    private bool IsAttackAbility(PetAbilityParams petAbilityParams) =>
        petAbilityParams.AbilityType is PetAbilityType.Damage or
            PetAbilityType.DamageZone or
            PetAbilityType.DamageOverTime or
            PetAbilityType.DamageOverTimeFromAbove;
}
