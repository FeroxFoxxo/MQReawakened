using Microsoft.Extensions.Logging;
using Server.Reawakened.XMLs.Abstractions.Enums;
using Server.Reawakened.XMLs.Abstractions.Interfaces;
using Server.Reawakened.XMLs.Data.Enemy.Abstractions;
using Server.Reawakened.XMLs.Data.Enemy.Enums;
using Server.Reawakened.XMLs.Data.Enemy.Models;
using Server.Reawakened.XMLs.Data.Enemy.States;
using Server.Reawakened.XMLs.Data.LootRewards.Enums;
using System.Xml;
using ActingState = Server.Reawakened.XMLs.Data.Enemy.States.ActingState;

namespace Server.Reawakened.XMLs.Bundles.Internal;

public class InternalEnemyData : InternalXml
{
    public override string BundleName => "InternalEnemyData";
    public override BundlePriority Priority => BundlePriority.Low;

    public ILogger<InternalEnemyData> Logger { get; set; }

    public Dictionary<string, EnemyModel> EnemyInfoCatalog;

    public override void InitializeVariables() => EnemyInfoCatalog = [];

    public override void ReadDescription(XmlDocument xmlDocument)
    {
        foreach (XmlNode enemyXml in xmlDocument.ChildNodes)
        {
            if (enemyXml.Name != "InternalEnemyData") continue;

            foreach (XmlNode aiTypeElement in enemyXml.ChildNodes)
            {
                if (aiTypeElement.Name != "AiType") continue;

                var aiType = AiType.Unknown;

                foreach (XmlAttribute aiTypeData in aiTypeElement.Attributes)
                    switch (aiTypeData.Name)
                    {
                        case "name":
                            aiType = Enum.Parse<AiType>(aiTypeData.Value);
                            break;
                    }

                foreach (XmlNode enemyCategoryElement in aiTypeElement.ChildNodes)
                {
                    if (enemyCategoryElement.Name != "EnemyCategory") continue;

                    var enemyCategory = EnemyCategory.Unknown;

                    foreach (XmlAttribute categoryData in enemyCategoryElement.Attributes)
                        switch (categoryData.Name)
                        {
                            case "name":
                                enemyCategory = Enum.Parse<EnemyCategory>(categoryData.Value);
                                break;
                        }

                    foreach (XmlNode enemy in enemyCategoryElement.ChildNodes)
                    {
                        if (enemy.Name != "Enemy") continue;

                        var prefabName = string.Empty;

                        foreach (XmlAttribute enemyData in enemy.Attributes)
                            switch (enemyData.Name)
                            {
                                case "name":
                                    prefabName = enemyData.Value;
                                    break;
                            }

                        var enemyModel = new EnemyModel()
                        {
                            AiType = aiType,
                            EnemyCategory = enemyCategory
                        };

                        foreach (XmlNode data in enemy.ChildNodes)
                            switch (data.Name)
                            {
                                case "Behaviors":
                                    var behaviors = new Dictionary<StateType, BaseState>();

                                    foreach (XmlNode behavior in data.ChildNodes)
                                    {
                                        BaseState state = null;
                                        var stateType = Enum.Parse<StateType>(behavior.Name);
                                        var resources = new List<EnemyResourceModel>();

                                        foreach (XmlNode enemyResource in behavior.ChildNodes)
                                        {
                                            var resourceType = string.Empty;
                                            var resourceName = string.Empty;

                                            foreach (XmlAttribute enemyResourceData in enemyResource.Attributes)
                                                if (enemyResourceData.Name.Equals("type"))
                                                    resourceType = enemyResourceData.Value;
                                                else if (enemyResourceData.Name.Equals("name"))
                                                    resourceName = enemyResourceData.Value;

                                            if (!string.IsNullOrEmpty(resourceType))
                                                resources.Add(new EnemyResourceModel(resourceType, resourceName));
                                        }

                                        switch (stateType)
                                        {
                                            case StateType.Patrol:
                                                var patrolSpeed = 0f;
                                                var patrolSmoothMove = true;
                                                var patrolEndPathWaitTime = 0f;

                                                foreach (XmlAttribute behaviorData in behavior.Attributes)
                                                    switch (behaviorData.Name)
                                                    {
                                                        case "speed":
                                                            patrolSpeed = float.Parse(behaviorData.Value);
                                                            break;
                                                        case "smoothMove":
                                                            patrolSmoothMove = bool.Parse(behaviorData.Value);
                                                            break;
                                                        case "endPathWaitTime":
                                                            patrolEndPathWaitTime = float.Parse(behaviorData.Value);
                                                            break;
                                                    }

                                                var patrolProperties = new PatrolProperties(patrolSpeed, patrolSmoothMove, patrolEndPathWaitTime, 0, 0, 0, 0);
                                                state = new PatrolState(patrolProperties, resources);
                                                break;
                                            case StateType.Aggro:
                                                var aggroSpeed = 0f;
                                                var aggroMoveBeyondTargetDistance = 0f;
                                                var aggroStayOnPatrolPath = true;
                                                var aggroAttackBeyondPatrolLine = 0f;
                                                var aggroUseAttackBeyondPatrolLine = true;
                                                var aggroDetectionRangeUpY = 0f;
                                                var aggroDetectionRangeDownY = 0f;

                                                foreach (XmlAttribute behaviorData in behavior.Attributes)
                                                    switch (behaviorData.Name)
                                                    {
                                                        case "speed":
                                                            aggroSpeed = float.Parse(behaviorData.Value);
                                                            break;
                                                        case "moveBeyondTargetDistance":
                                                            aggroMoveBeyondTargetDistance = float.Parse(behaviorData.Value);
                                                            break;
                                                        case "stayOnPatrolPath":
                                                            aggroStayOnPatrolPath = bool.Parse(behaviorData.Value);
                                                            break;
                                                        case "attackBeyondPatrolLine":
                                                            aggroAttackBeyondPatrolLine = float.Parse(behaviorData.Value);
                                                            break;
                                                        case "useAttackBeyondPatrolLine":
                                                            aggroUseAttackBeyondPatrolLine = bool.Parse(behaviorData.Value);
                                                            break;
                                                        case "detectionRangeUpY":
                                                            aggroDetectionRangeUpY = float.Parse(behaviorData.Value);
                                                            break;
                                                        case "detectionRangeDownY":
                                                            aggroDetectionRangeDownY = float.Parse(behaviorData.Value);
                                                            break;
                                                    }

                                                var aggroProperties = new AggroProperties(
                                                    aggroSpeed, aggroMoveBeyondTargetDistance, aggroStayOnPatrolPath, aggroAttackBeyondPatrolLine,
                                                    aggroUseAttackBeyondPatrolLine, aggroDetectionRangeUpY, aggroDetectionRangeDownY
                                                );

                                                state = new AggroState(aggroProperties, resources);
                                                break;
                                            case StateType.LookAround:
                                                var lookAroundTime = 0f;
                                                var lookAroundStartDirection = 0;
                                                var lookAroundForceDirection = 0;
                                                var lookAroundInitialProgressRatio = 0f;
                                                var lookAroundSnapOnGround = true;

                                                foreach (XmlAttribute behaviorData in behavior.Attributes)
                                                    switch (behaviorData.Name)
                                                    {
                                                        case "lookTime":
                                                            lookAroundTime = float.Parse(behaviorData.Value);
                                                            break;
                                                        case "startDirection":
                                                            lookAroundStartDirection = int.Parse(behaviorData.Value);
                                                            break;
                                                        case "forceDirection":
                                                            lookAroundForceDirection = int.Parse(behaviorData.Value);
                                                            break;
                                                        case "initialProgressRatio":
                                                            lookAroundInitialProgressRatio = float.Parse(behaviorData.Value);
                                                            break;
                                                        case "snapOnGround":
                                                            lookAroundSnapOnGround = bool.Parse(behaviorData.Value);
                                                            break;
                                                    }

                                                var lookAroundProperties = new LookAroundProperties(
                                                    lookAroundTime, lookAroundStartDirection, lookAroundForceDirection, lookAroundInitialProgressRatio, lookAroundSnapOnGround
                                                );

                                                state = new LookAroundState(lookAroundProperties, resources);
                                                break;
                                            case StateType.ComeBack:
                                                var comeBackSpeed = 1f;

                                                foreach (XmlAttribute behaviorData in behavior.Attributes)
                                                    switch (behaviorData.Name)
                                                    {
                                                        case "speed":
                                                            comeBackSpeed = float.Parse(behaviorData.Value);
                                                            break;
                                                    }

                                                var comeBackProperties = new ComeBackProperties(comeBackSpeed);
                                                state = new ComeBackState(comeBackProperties, resources);
                                                break;
                                            case StateType.Shooting:
                                                var shootingNbBulletsPerRound = 1;
                                                var shootingFireSpreadAngle = 0f;
                                                var shootingDelayBetweenBullet = 0f;
                                                var shootingDelayShootAnim = 0f;
                                                var shootingNbFireRounds = 1;
                                                var shootingDelayBetweenFireRound = 0f;
                                                var shootingStartCoolDownTime = 0f;
                                                var shootingEndCoolDownTime = 0f;
                                                var shootingProjectileSpeed = 0f;
                                                var shootingFireSpreadClockwise = true;
                                                var shootingFireSpreadStartAngle = 0f;

                                                foreach (XmlAttribute behaviorData in behavior.Attributes)
                                                    switch (behaviorData.Name)
                                                    {
                                                        case "nbBulletsPerRound":
                                                            shootingNbBulletsPerRound = int.Parse(behaviorData.Value);
                                                            break;
                                                        case "fireSpreadAngle":
                                                            shootingFireSpreadAngle = float.Parse(behaviorData.Value);
                                                            break;
                                                        case "delayBetweenBullet":
                                                            shootingDelayBetweenBullet = float.Parse(behaviorData.Value);
                                                            break;
                                                        case "delayShoot_Anim":
                                                            shootingDelayShootAnim = float.Parse(behaviorData.Value);
                                                            break;
                                                        case "delayBetweenFireRound":
                                                            shootingDelayBetweenFireRound = float.Parse(behaviorData.Value);
                                                            break;
                                                        case "nbFireRounds":
                                                            shootingNbFireRounds = int.Parse(behaviorData.Value);
                                                            break;
                                                        case "startCoolDownTime":
                                                            shootingStartCoolDownTime = float.Parse(behaviorData.Value);
                                                            break;
                                                        case "endCoolDownTime":
                                                            shootingEndCoolDownTime = float.Parse(behaviorData.Value);
                                                            break;
                                                        case "projectileSpeed":
                                                            shootingProjectileSpeed = float.Parse(behaviorData.Value);
                                                            break;
                                                        case "fireSpreadClockwise":
                                                            shootingFireSpreadClockwise = bool.Parse(behaviorData.Value);
                                                            break;
                                                        case "fireSpreadStartAngle":
                                                            shootingFireSpreadStartAngle = float.Parse(behaviorData.Value);
                                                            break;
                                                    }

                                                var shootingProperties = new ShootingProperties(
                                                    shootingNbBulletsPerRound, shootingFireSpreadAngle, shootingDelayBetweenBullet, shootingDelayShootAnim,
                                                    shootingNbFireRounds, shootingDelayBetweenFireRound, shootingStartCoolDownTime, shootingEndCoolDownTime,
                                                    shootingProjectileSpeed, shootingFireSpreadClockwise, shootingFireSpreadStartAngle
                                                );

                                                state = new ShootingState(shootingProperties, resources);
                                                break;
                                            case StateType.Bomber:
                                                var bomberInTime = 0f;
                                                var bomberLoopTime = 0f;
                                                var bomberBombRadius = 0f;

                                                foreach (XmlAttribute behaviorData in behavior.Attributes)
                                                    switch (behaviorData.Name)
                                                    {
                                                        case "inTime":
                                                            bomberInTime = float.Parse(behaviorData.Value);
                                                            break;
                                                        case "loopTime":
                                                            bomberLoopTime = float.Parse(behaviorData.Value);
                                                            break;
                                                        case "bombRadius":
                                                            bomberBombRadius = float.Parse(behaviorData.Value);
                                                            break;
                                                    }

                                                var bomberProperties = new BomberProperties(bomberInTime, bomberLoopTime, bomberBombRadius);
                                                state = new BomberState(bomberProperties, resources);
                                                break;
                                            case StateType.Grenadier:
                                                var grenadierInTime = 0f;
                                                var grenadierLoopTime = 0f;
                                                var grenadierOutTime = 0f;
                                                var grenadierIsTracking = false;
                                                var grenadierProjCount = 1;
                                                var grenadierProjSpeed = 1f;
                                                var grenadierMaxHeight = 1f;

                                                foreach (XmlAttribute behaviorData in behavior.Attributes)
                                                    switch (behaviorData.Name)
                                                    {
                                                        case "inTime":
                                                            grenadierInTime = float.Parse(behaviorData.Value);
                                                            break;
                                                        case "loopTime":
                                                            grenadierLoopTime = float.Parse(behaviorData.Value);
                                                            break;
                                                        case "outTime":
                                                            grenadierOutTime = float.Parse(behaviorData.Value);
                                                            break;
                                                        case "isTracking":
                                                            grenadierIsTracking = bool.Parse(behaviorData.Value);
                                                            break;
                                                        case "projCount":
                                                            grenadierProjCount = int.Parse(behaviorData.Value);
                                                            break;
                                                        case "projSpeed":
                                                            grenadierProjSpeed = float.Parse(behaviorData.Value);
                                                            break;
                                                        case "maxHeight":
                                                            grenadierMaxHeight = float.Parse(behaviorData.Value);
                                                            break;
                                                    }

                                                var grenadierProperties = new GrenadierProperties(
                                                    grenadierInTime, grenadierLoopTime, grenadierOutTime, grenadierIsTracking,
                                                    grenadierProjCount, grenadierProjSpeed, grenadierMaxHeight
                                                );

                                                state = new GrenadierState(grenadierProperties, resources);
                                                break;
                                            case StateType.Stomper:
                                                var stomperAttackTime = 0f;
                                                var stomperImpactTime = 0f;
                                                var stomperDamageDistance = 0f;
                                                var stomperDamageOffset = 0f;

                                                foreach (XmlAttribute behaviorData in behavior.Attributes)
                                                    switch (behaviorData.Name)
                                                    {
                                                        case "attackTime":
                                                            stomperAttackTime = float.Parse(behaviorData.Value);
                                                            break;
                                                        case "impactTime":
                                                            stomperImpactTime = float.Parse(behaviorData.Value);
                                                            break;
                                                        case "damageDistance":
                                                            stomperDamageDistance = float.Parse(behaviorData.Value);
                                                            break;
                                                        case "damageOffset":
                                                            stomperDamageOffset = float.Parse(behaviorData.Value);
                                                            break;
                                                    }

                                                var stomperProperties = new StomperProperties(stomperAttackTime, stomperImpactTime, stomperDamageDistance, stomperDamageOffset);
                                                state = new StomperState(stomperProperties, resources);
                                                break;
                                            case StateType.Stinger:
                                                var stingerSpeedForward = 0f;
                                                var stingerSpeedBackward = 0f;
                                                var stingerInDurationForward = 0f;
                                                var stingerAttackDuration = 0f;
                                                var stingerDamageAttackTimeOffset = 0f;
                                                var stingerInDurationBackward = 0f;
                                                var stingerDamageDistance = 0f;

                                                foreach (XmlAttribute behaviorData in behavior.Attributes)
                                                    switch (behaviorData.Name)
                                                    {
                                                        case "speedForward":
                                                            stingerSpeedForward = float.Parse(behaviorData.Value);
                                                            break;
                                                        case "speedBackward":
                                                            stingerSpeedBackward = float.Parse(behaviorData.Value);
                                                            break;
                                                        case "inDurationForward":
                                                            stingerInDurationForward = float.Parse(behaviorData.Value);
                                                            break;
                                                        case "attackDuration":
                                                            stingerAttackDuration = float.Parse(behaviorData.Value);
                                                            break;
                                                        case "damageAttackTimeOffset":
                                                            stingerDamageAttackTimeOffset = float.Parse(behaviorData.Value);
                                                            break;
                                                        case "inDurationBackward":
                                                            stingerInDurationBackward = float.Parse(behaviorData.Value);
                                                            break;
                                                        case "damageDistance":
                                                            stingerDamageDistance = float.Parse(behaviorData.Value);
                                                            break;
                                                    }

                                                var stingerProperties = new StingerProperties(
                                                    stingerSpeedForward, stingerSpeedBackward, stingerInDurationForward, stingerAttackDuration,
                                                    stingerDamageAttackTimeOffset, stingerInDurationBackward, stingerDamageDistance
                                                );

                                                state = new StingerState(stingerProperties, resources);
                                                break;
                                            case StateType.Spike:
                                                var spikeSpeedForward = 0f;
                                                var spikeSpeedBackward = 0f;
                                                var spikeInDurationForward = 0f;
                                                var spikeAttackDuration = 0f;
                                                var spikeAttackShootTime = 0f;
                                                var spikeInDurationBackward = 0f;
                                                var spikeSpreadAngle = 0f;
                                                var spikeDetectionRadius = 0f;
                                                var spikeTargettedProjectileCount = 0;
                                                var spikeRandomProjectileCount = 0;
                                                var spikeProjectileSpeed = 0f;

                                                foreach (XmlAttribute behaviorData in behavior.Attributes)
                                                    switch (behaviorData.Name)
                                                    {
                                                        case "speedForward":
                                                            spikeSpeedForward = float.Parse(behaviorData.Value);
                                                            break;
                                                        case "speedBackward":
                                                            spikeSpeedBackward = float.Parse(behaviorData.Value);
                                                            break;
                                                        case "inDurationForward":
                                                            spikeInDurationForward = float.Parse(behaviorData.Value);
                                                            break;
                                                        case "attackDuration":
                                                            spikeAttackDuration = float.Parse(behaviorData.Value);
                                                            break;
                                                        case "attackShootTime":
                                                            spikeAttackShootTime = float.Parse(behaviorData.Value);
                                                            break;
                                                        case "inDurationBackward":
                                                            spikeInDurationBackward = float.Parse(behaviorData.Value);
                                                            break;
                                                        case "spreadAngle":
                                                            spikeSpreadAngle = float.Parse(behaviorData.Value);
                                                            break;
                                                        case "detectionRadius":
                                                            spikeDetectionRadius = float.Parse(behaviorData.Value);
                                                            break;
                                                        case "targettedProjectileCount":
                                                            spikeTargettedProjectileCount = int.Parse(behaviorData.Value);
                                                            break;
                                                        case "randomProjectileCount":
                                                            spikeRandomProjectileCount = int.Parse(behaviorData.Value);
                                                            break;
                                                        case "projectileSpeed":
                                                            spikeProjectileSpeed = float.Parse(behaviorData.Value);
                                                            break;
                                                    }

                                                var spikeProperties = new SpikeProperties(
                                                    spikeSpeedForward, spikeSpeedBackward, spikeInDurationForward,
                                                    spikeAttackDuration, spikeAttackShootTime, spikeInDurationBackward, spikeSpreadAngle, spikeDetectionRadius,
                                                    spikeTargettedProjectileCount, spikeRandomProjectileCount, spikeProjectileSpeed
                                                );

                                                state = new SpikeState(spikeProperties, resources);
                                                break;
                                            case StateType.Projectile:
                                                var projectilePrefabName = string.Empty;
                                                var projectileSpeed = 0f;
                                                var projectileAngle = 0f;
                                                var projectileCntPerRound = 0;
                                                var projectileDelayPerShot = 0f;
                                                var projectileDelayBetweenFireRounds = 0f;
                                                var projectileGravity = 0f;
                                                var projectileIsTracking = false;

                                                foreach (XmlAttribute behaviorData in behavior.Attributes)
                                                    switch (behaviorData.Name)
                                                    {
                                                        case "projectilePrefabName":
                                                            projectilePrefabName = behaviorData.Value;
                                                            break;
                                                        case "projectileSpeed":
                                                            projectileSpeed = float.Parse(behaviorData.Value);
                                                            break;
                                                        case "projectileAngle":
                                                            projectileAngle = float.Parse(behaviorData.Value);
                                                            break;
                                                        case "projectileCntPerRound":
                                                            projectileCntPerRound = int.Parse(behaviorData.Value);
                                                            break;
                                                        case "delayPerShot":
                                                            projectileDelayPerShot = float.Parse(behaviorData.Value);
                                                            break;
                                                        case "delayBetweenFireRounds":
                                                            projectileDelayBetweenFireRounds = float.Parse(behaviorData.Value);
                                                            break;
                                                        case "gravity":
                                                            projectileGravity = float.Parse(behaviorData.Value);
                                                            break;
                                                        case "isTracking":
                                                            projectileIsTracking = bool.Parse(behaviorData.Value);
                                                            break;
                                                    }

                                                var projectileProperties = new ProjectileProperties(
                                                    projectilePrefabName, projectileSpeed, projectileAngle, projectileCntPerRound, projectileDelayPerShot, projectileDelayBetweenFireRounds,
                                                    projectileGravity, projectileIsTracking
                                                );

                                                state = new ProjectileState(projectileProperties, resources);
                                                break;
                                            case StateType.GoTo:
                                                state = new GoToState(resources);
                                                break;
                                            case StateType.Acting:
                                                var actingSnapOnGround = false;

                                                foreach (XmlAttribute behaviorData in behavior.Attributes)
                                                    switch (behaviorData.Name)
                                                    {
                                                        case "snapOnGround":
                                                            actingSnapOnGround = bool.Parse(behaviorData.Value);
                                                            break;
                                                    }

                                                var actingProperties = new ActingProperties(actingSnapOnGround);

                                                state = new ActingState(actingProperties, resources);
                                                break;
                                            default:
                                                Logger.LogError("Unimplemented state for: {State} ({EnemyName})", stateType, prefabName);
                                                break;
                                        }

                                        behaviors.Add(stateType, state);
                                    }

                                    enemyModel.BehaviorData = behaviors;
                                    break;
                                case "LootTable":
                                    var lootTable = new List<EnemyDropModel>();

                                    foreach (XmlNode dynamicDrop in data.ChildNodes)
                                    {
                                        var dropType = DynamicDropType.Unknown;
                                        var dropId = 0;
                                        var dropChance = 0f;
                                        var dropMinLevel = 1;
                                        var dropMaxLevel = 65;

                                        foreach (XmlAttribute dynamicDropAttributes in dynamicDrop.Attributes)
                                            switch (dynamicDropAttributes.Name)
                                            {
                                                case "type":
                                                    switch (dynamicDropAttributes.Value)
                                                    {
                                                        case "item":
                                                            dropType = DynamicDropType.Item;
                                                            break;
                                                        case "randomArmor":
                                                            dropType = DynamicDropType.RandomArmor;
                                                            break;
                                                        case "randomIngredient":
                                                            dropType = DynamicDropType.RandomIngredient;
                                                            break;
                                                    }
                                                    break;
                                                case "id":
                                                    dropId = int.Parse(dynamicDropAttributes.Value);
                                                    break;
                                                case "chance":
                                                    dropChance = float.Parse(dynamicDropAttributes.Value);
                                                    break;
                                                case "minLevel":
                                                    dropMinLevel = int.Parse(dynamicDropAttributes.Value);
                                                    break;
                                                case "maxLevel":
                                                    dropMaxLevel = int.Parse(dynamicDropAttributes.Value);
                                                    break;
                                            }

                                        lootTable.Add(new EnemyDropModel(dropType, dropId, dropChance, dropMinLevel, dropMaxLevel));
                                    }

                                    enemyModel.EnemyLootTable = lootTable;
                                    break;
                                default:
                                    Logger.LogError("Unknown enemy data type for: {DataType} ({EnemyName}", data.Name, prefabName);
                                    break;
                            }

                        enemyModel.EnsureValidData(prefabName, Logger);

                        EnemyInfoCatalog.Add(prefabName, enemyModel);
                    }
                }
            }
        }
    }
}
