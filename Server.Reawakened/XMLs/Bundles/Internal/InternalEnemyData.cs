using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.Enums;
using Server.Reawakened.Rooms.Models.Planes;
using Server.Reawakened.XMLs.Abstractions;
using Server.Reawakened.XMLs.Enums;
using Server.Reawakened.XMLs.Models.Enemy.Abstractions;
using Server.Reawakened.XMLs.Models.Enemy.Enums;
using Server.Reawakened.XMLs.Models.Enemy.Models;
using Server.Reawakened.XMLs.Models.Enemy.States;
using System.Xml;

namespace Server.Reawakened.XMLs.BundlesInternal;

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
                {
                    switch (aiTypeData.Name)
                    {
                        case "name":
                            aiType = Enum.Parse<AiType>(aiTypeData.Value);
                            break;
                    }
                }

                foreach (XmlNode enemyCategoryElement in aiTypeElement.ChildNodes)
                {
                    if (enemyCategoryElement.Name != "EnemyCategory") continue;

                    var enemyCategory = EnemyCategory.Unknown;

                    foreach (XmlAttribute categoryData in enemyCategoryElement.Attributes)
                    {
                        switch (categoryData.Name)
                        {
                            case "name":
                                enemyCategory = Enum.Parse<EnemyCategory>(categoryData.Value);
                                break;
                        }
                    }

                    foreach (XmlNode enemy in enemyCategoryElement.ChildNodes)
                    {
                        if (enemy.Name != "Enemy") continue;

                        var prefabName = string.Empty;

                        foreach (XmlAttribute enemyData in enemy.Attributes)
                        {
                            switch (enemyData.Name)
                            {
                                case "name":
                                    prefabName = enemyData.Value;
                                    break;
                            }
                        }

                        var enemyModel = new EnemyModel()
                        {
                            AiType = aiType,
                            EnemyCategory = enemyCategory
                        };

                        foreach (XmlNode data in enemy.ChildNodes)
                        {
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
                                            {
                                                if (enemyResourceData.Name.Equals("type"))
                                                    resourceType = enemyResourceData.Value;
                                                else if (enemyResourceData.Name.Equals("name"))
                                                    resourceName = enemyResourceData.Value;
                                            }

                                            if (!string.IsNullOrEmpty(resourceType))
                                                resources.Add(new EnemyResourceModel(resourceType, resourceName));
                                        }

                                        switch (stateType)
                                        {
                                            case StateType.Patrol:
                                                var speed = 0f;
                                                var smoothMove = true;
                                                var endPathWaitTime = 0f;

                                                foreach (XmlAttribute behaviorData in behavior.Attributes)
                                                {
                                                    switch (behaviorData.Name)
                                                    {
                                                        case "speed":
                                                            speed = float.Parse(behaviorData.Value);
                                                            break;
                                                        case "smoothMove":
                                                            smoothMove = bool.Parse(behaviorData.Value);
                                                            break;
                                                        case "endPathWaitTime":
                                                            endPathWaitTime = float.Parse(behaviorData.Value);
                                                            break;
                                                    }
                                                }

                                                state = new PatrolState(speed, smoothMove, endPathWaitTime, resources);
                                                break;
                                            case StateType.Aggro:
                                                var aggroSpeed = 0f;
                                                var moveBeyondTargetDistance = 0f;
                                                var stayOnPatrolPath = true;
                                                var aggroAttackBeyondPatrolLine = 0f;
                                                var useAttackBeyondPatrolLine = true;
                                                var detectionRangeUpY = 0f;
                                                var detectionRangeDownY = 0f;

                                                foreach (XmlAttribute behaviorData in behavior.Attributes)
                                                {
                                                    switch (behaviorData.Name)
                                                    {
                                                        case "speed":
                                                            aggroSpeed = float.Parse(behaviorData.Value);
                                                            break;
                                                        case "moveBeyondTargetDistance":
                                                            moveBeyondTargetDistance = float.Parse(behaviorData.Value);
                                                            break;
                                                        case "stayOnPatrolPath":
                                                            stayOnPatrolPath = bool.Parse(behaviorData.Value);
                                                            break;
                                                        case "attackBeyondPatrolLine":
                                                            aggroAttackBeyondPatrolLine = float.Parse(behaviorData.Value);
                                                            break;
                                                        case "useAttackBeyondPatrolLine":
                                                            useAttackBeyondPatrolLine = bool.Parse(behaviorData.Value);
                                                            break;
                                                        case "detectionRangeUpY":
                                                            detectionRangeUpY = float.Parse(behaviorData.Value);
                                                            break;
                                                        case "detectionRangeDownY":
                                                            detectionRangeDownY = float.Parse(behaviorData.Value);
                                                            break;
                                                    }
                                                }

                                                state = new AggroState(aggroSpeed, moveBeyondTargetDistance, stayOnPatrolPath, aggroAttackBeyondPatrolLine,
                                                    useAttackBeyondPatrolLine, detectionRangeUpY, detectionRangeDownY, resources);
                                                break;
                                            case StateType.LookAround:
                                                var lookTime = 0f;
                                                var startDirection = 0f;
                                                var forceDirection = 0f;
                                                var initialProgressRatio = 0f;
                                                var snapOnGround = true;

                                                foreach (XmlAttribute behaviorData in behavior.Attributes)
                                                {
                                                    switch (behaviorData.Name)
                                                    {
                                                        case "lookTime":
                                                            lookTime = float.Parse(behaviorData.Value);
                                                            break;
                                                        case "startDirection":
                                                            startDirection = float.Parse(behaviorData.Value);
                                                            break;
                                                        case "forceDirection":
                                                            forceDirection = float.Parse(behaviorData.Value);
                                                            break;
                                                        case "initialProgressRatio":
                                                            initialProgressRatio = float.Parse(behaviorData.Value);
                                                            break;
                                                        case "snapOnGround":
                                                            snapOnGround = bool.Parse(behaviorData.Value);
                                                            break;
                                                    }
                                                }

                                                state = new LookAroundState(lookTime, startDirection, forceDirection, initialProgressRatio, snapOnGround, resources);
                                                break;
                                            case StateType.ComeBack:
                                                var comeBackSpeed = 1f;

                                                foreach (XmlAttribute behaviorData in behavior.Attributes)
                                                {
                                                    switch (behaviorData.Name)
                                                    {
                                                        case "speed":
                                                            comeBackSpeed = float.Parse(behaviorData.Value);
                                                            break;
                                                    }
                                                }

                                                state = new ComeBackState(comeBackSpeed, resources);
                                                break;
                                            case StateType.Shooting:
                                                var nbBulletsPerRound = 1;
                                                var fireSpreadAngle = 0f;
                                                var delayBetweenBullet = 0f;
                                                var delayShootAnim = 0f;
                                                var nbFireRounds = 1;
                                                var delayBetweenFireRound = 0f;
                                                var startCoolDownTime = 0f;
                                                var endCoolDownTime = 0f;
                                                var projectileSpeed = 0f;
                                                var fireSpreadClockwise = true;
                                                var fireSpreadStartAngle = 0f;

                                                foreach (XmlAttribute behaviorData in behavior.Attributes)
                                                {
                                                    switch (behaviorData.Name)
                                                    {
                                                        case "nbBulletsPerRound":
                                                            nbBulletsPerRound = int.Parse(behaviorData.Value);
                                                            break;
                                                        case "fireSpreadAngle":
                                                            fireSpreadAngle = float.Parse(behaviorData.Value);
                                                            break;
                                                        case "delayBetweenBullet":
                                                            delayBetweenBullet = float.Parse(behaviorData.Value);
                                                            break;
                                                        case "delayShoot_Anim":
                                                            delayShootAnim = float.Parse(behaviorData.Value);
                                                            break;
                                                        case "delayBetweenFireRound":
                                                            delayBetweenFireRound = float.Parse(behaviorData.Value);
                                                            break;
                                                        case "nbFireRounds":
                                                            nbFireRounds = int.Parse(behaviorData.Value);
                                                            break;
                                                        case "startCoolDownTime":
                                                            startCoolDownTime = float.Parse(behaviorData.Value);
                                                            break;
                                                        case "endCoolDownTime":
                                                            endCoolDownTime = float.Parse(behaviorData.Value);
                                                            break;
                                                        case "projectileSpeed":
                                                            projectileSpeed = float.Parse(behaviorData.Value);
                                                            break;
                                                        case "fireSpreadClockwise":
                                                            fireSpreadClockwise = bool.Parse(behaviorData.Value);
                                                            break;
                                                        case "fireSpreadStartAngle":
                                                            fireSpreadStartAngle = float.Parse(behaviorData.Value);
                                                            break;
                                                    }
                                                }

                                                state = new ShootingState(nbBulletsPerRound, fireSpreadAngle, delayBetweenBullet, delayShootAnim,
                                                    nbFireRounds, delayBetweenFireRound, startCoolDownTime, endCoolDownTime,
                                                    projectileSpeed, fireSpreadClockwise, fireSpreadStartAngle, resources);
                                                break;
                                            case StateType.Bomber:
                                                var inTime = 0f;
                                                var loopTime = 0f;
                                                var bombRadius = 0f;

                                                foreach (XmlAttribute behaviorData in behavior.Attributes)
                                                {
                                                    switch (behaviorData.Name)
                                                    {
                                                        case "inTime":
                                                            inTime = float.Parse(behaviorData.Value);
                                                            break;
                                                        case "loopTime":
                                                            loopTime = float.Parse(behaviorData.Value);
                                                            break;
                                                        case "bombRadius":
                                                            bombRadius = float.Parse(behaviorData.Value);
                                                            break;
                                                    }
                                                }

                                                state = new BomberState(inTime, loopTime, bombRadius, resources);
                                                break;
                                            case StateType.Grenadier:
                                                var gInTime = 0f;
                                                var gLoopTime = 0f;
                                                var gOutTime = 0f;
                                                var isTracking = true;
                                                var projCount = 1;
                                                var projSpeed = 1f;
                                                var maxHeight = 1f;

                                                foreach (XmlAttribute behaviorData in behavior.Attributes)
                                                {
                                                    switch (behaviorData.Name)
                                                    {
                                                        case "inTime":
                                                            gInTime = float.Parse(behaviorData.Value);
                                                            break;
                                                        case "loopTime":
                                                            gLoopTime = float.Parse(behaviorData.Value);
                                                            break;
                                                        case "outTime":
                                                            gOutTime = float.Parse(behaviorData.Value);
                                                            break;
                                                        case "isTracking":
                                                            isTracking = bool.Parse(behaviorData.Value);
                                                            break;
                                                        case "projCount":
                                                            projCount = int.Parse(behaviorData.Value);
                                                            break;
                                                        case "projSpeed":
                                                            projSpeed = float.Parse(behaviorData.Value);
                                                            break;
                                                        case "maxHeight":
                                                            maxHeight = float.Parse(behaviorData.Value);
                                                            break;
                                                    }
                                                }

                                                state = new GrenadierState(gInTime, gLoopTime, gOutTime, isTracking, projCount, projSpeed, maxHeight, resources);
                                                break;
                                            case StateType.Stomper:
                                                var attackTime = 0f;
                                                var impactTime = 0f;
                                                var damageDistance = 0f;
                                                var damageOffset = 0f;

                                                foreach (XmlAttribute behaviorData in behavior.Attributes)
                                                {
                                                    switch (behaviorData.Name)
                                                    {
                                                        case "attackTime":
                                                            attackTime = float.Parse(behaviorData.Value);
                                                            break;
                                                        case "impactTime":
                                                            impactTime = float.Parse(behaviorData.Value);
                                                            break;
                                                        case "damageDistance":
                                                            damageDistance = float.Parse(behaviorData.Value);
                                                            break;
                                                        case "damageOffset":
                                                            damageOffset = float.Parse(behaviorData.Value);
                                                            break;
                                                    }
                                                }

                                                state = new StomperState(attackTime, impactTime, damageDistance, damageOffset, resources);
                                                break;
                                            case StateType.Stinger:
                                                var speedForward = 0f;
                                                var speedBackward = 0f;
                                                var inDurationForward = 0f;
                                                var attackDuration = 0f;
                                                var damageAttackTimeOffset = 0f;
                                                var inDurationBackward = 0f;
                                                var stingerDamageDistance = 0f;

                                                foreach (XmlAttribute behaviorData in behavior.Attributes)
                                                {
                                                    switch (behaviorData.Name)
                                                    {
                                                        case "speedForward":
                                                            speedForward = float.Parse(behaviorData.Value);
                                                            break;
                                                        case "speedBackward":
                                                            speedBackward = float.Parse(behaviorData.Value);
                                                            break;
                                                        case "inDurationForward":
                                                            inDurationForward = float.Parse(behaviorData.Value);
                                                            break;
                                                        case "attackDuration":
                                                            attackDuration = float.Parse(behaviorData.Value);
                                                            break;
                                                        case "damageAttackTimeOffset":
                                                            damageAttackTimeOffset = float.Parse(behaviorData.Value);
                                                            break;
                                                        case "inDurationBackward":
                                                            inDurationBackward = float.Parse(behaviorData.Value);
                                                            break;
                                                        case "damageDistance":
                                                            stingerDamageDistance = float.Parse(behaviorData.Value);
                                                            break;
                                                    }
                                                }

                                                state = new StingerState(speedForward, speedBackward, inDurationForward, attackDuration,
                                                    damageAttackTimeOffset, inDurationBackward, stingerDamageDistance, resources);
                                                break;
                                            default:
                                                Logger.LogError("Unimplemented state for: {State} ({EnemyName})", stateType, prefabName);
                                                break;
                                        }

                                        behaviors.Add(stateType, state);
                                    }

                                    enemyModel.BehaviorData = behaviors;
                                    break;
                                case "GenericScript":
                                    var attackBehavior = StateType.Unknown;
                                    var awareBehavior = StateType.Unknown;
                                    var unawareBehavior = StateType.Unknown;
                                    var awareBehaviorDuration = 0f;
                                    var healthRegenAmount = 0;
                                    var healthRegenFrequency = 0;

                                    foreach (XmlNode globalProperty in data.ChildNodes)
                                    {
                                        var gDataName = globalProperty.Name;
                                        var gDataValue = string.Empty;

                                        foreach (XmlAttribute globalPropValue in globalProperty.Attributes)
                                        {
                                            switch (globalPropValue.Name)
                                            {
                                                case "value":
                                                    gDataValue = globalPropValue.Value;
                                                    break;
                                                default:
                                                    Logger.LogWarning("Unknown global parameter: '{ParameterName}' for '{EnemyName}'", globalPropValue.Name, prefabName);
                                                    break;
                                            }
                                        }

                                        if (string.IsNullOrEmpty(gDataName) || string.IsNullOrEmpty(gDataValue))
                                            continue;

                                        switch (gDataName)
                                        {
                                            case "AttackBehavior":
                                                attackBehavior = Enum.Parse<StateType>(gDataValue);
                                                break;
                                            case "AwareBehavior":
                                                awareBehavior = Enum.Parse<StateType>(gDataValue);
                                                break;
                                            case "UnawareBehavior":
                                                unawareBehavior = Enum.Parse<StateType>(gDataValue);
                                                break;
                                            case "AwareBehaviorDuration":
                                                awareBehaviorDuration = Convert.ToSingle(gDataValue);
                                                break;
                                            case "HealthRegenAmount":
                                                healthRegenAmount = Convert.ToInt32(gDataValue);
                                                break;
                                            case "HealthRegenFrequency":
                                                healthRegenFrequency = Convert.ToInt32(gDataValue);
                                                break;
                                            default:
                                                Logger.LogWarning("Unknown generic property: '{PropertyName}' for '{EnemyName}'", gDataName, prefabName);
                                                break;
                                        }
                                    }

                                    enemyModel.GenericScript = new GenericScriptModel(attackBehavior, awareBehavior, unawareBehavior, awareBehaviorDuration, healthRegenAmount, healthRegenFrequency);
                                    break;
                                case "GlobalProperties":
                                    var detectionLimitedByPatrolLine = true;
                                    var frontDetectionRangeX = 0f;
                                    var frontDetectionRangeUpY = 0f;
                                    var frontDetectionRangeDownY = 0f;
                                    var backDetectionRangeX = 0f;
                                    var backDetectionRangeUpY = 0f;
                                    var backDetectionRangeDownY = 0f;
                                    var attackBeyondPatrolLine = 0f;
                                    var shootOffsetX = 0f;
                                    var shootOffsetY = 0f;
                                    var shootingProjectilePrefabName = "COL_PRJ_DamageProjectile";
                                    var viewOffsetY = 0f;

                                    // Values currently not specified in XML file
                                    var script = string.Empty;
                                    var disableCollision = false;
                                    var detectionSourceOnPatrolLine = true;

                                    foreach (XmlNode globalProperty in data.ChildNodes)
                                    {
                                        var gDataName = globalProperty.Name;
                                        var gDataValue = string.Empty;

                                        foreach (XmlAttribute globalPropValue in globalProperty.Attributes)
                                        {
                                            switch (globalPropValue.Name)
                                            {
                                                case "value":
                                                    gDataValue = globalPropValue.Value;
                                                    break;
                                                default:
                                                    Logger.LogWarning("Unknown global parameter: '{ParameterName}' for '{EnemyName}'", globalPropValue.Name, prefabName);
                                                    break;
                                            }
                                        }

                                        if (string.IsNullOrEmpty(gDataName) || string.IsNullOrEmpty(gDataValue))
                                            continue;

                                        switch (gDataName)
                                        {
                                            case "DetectionLimitedByPatrolLine":
                                                detectionLimitedByPatrolLine = Convert.ToBoolean(gDataValue);
                                                break;
                                            case "FrontDetectionRangeX":
                                                frontDetectionRangeX = Convert.ToSingle(gDataValue);
                                                break;
                                            case "FrontDetectionRangeUpY":
                                                frontDetectionRangeUpY = Convert.ToSingle(gDataValue);
                                                break;
                                            case "FrontDetectionRangeDownY":
                                                frontDetectionRangeDownY = Convert.ToSingle(gDataValue);
                                                break;
                                            case "BackDetectionRangeX":
                                                backDetectionRangeX = Convert.ToSingle(gDataValue);
                                                break;
                                            case "BackDetectionRangeUpY":
                                                backDetectionRangeUpY = Convert.ToSingle(gDataValue);
                                                break;
                                            case "BackDetectionRangeDownY":
                                                backDetectionRangeDownY = Convert.ToSingle(gDataValue);
                                                break;
                                            case "AttackBeyondPatrolLine":
                                                attackBeyondPatrolLine = Convert.ToSingle(gDataValue);
                                                break;
                                            case "ShootOffsetX":
                                                shootOffsetX = Convert.ToSingle(gDataValue);
                                                break;
                                            case "ShootOffsetY":
                                                shootOffsetY = Convert.ToSingle(gDataValue);
                                                break;
                                            case "ProjectilePrefabName":
                                                shootingProjectilePrefabName = gDataValue;
                                                break;
                                            case "ViewOffsetY":
                                                viewOffsetY = Convert.ToSingle(gDataValue);
                                                break;
                                            case "Script":
                                                script = gDataValue;
                                                break;
                                            case "DisableCollision":
                                                disableCollision = Convert.ToBoolean(gDataValue);
                                                break;
                                            case "DetectionSourceOnPatrolLine":
                                                detectionSourceOnPatrolLine = Convert.ToBoolean(gDataValue);
                                                break;
                                            default:
                                                Logger.LogWarning("Unknown global property: '{PropertyName}' for '{EnemyName}'", gDataName, prefabName);
                                                break;
                                        }
                                    }

                                    enemyModel.GlobalProperties =
                                        new GlobalPropertyModel(
                                            detectionLimitedByPatrolLine, backDetectionRangeX,
                                            viewOffsetY, backDetectionRangeUpY,
                                            backDetectionRangeDownY, shootOffsetX,
                                            shootOffsetY, frontDetectionRangeX,
                                            frontDetectionRangeUpY, frontDetectionRangeDownY,
                                            script, shootingProjectilePrefabName,
                                            disableCollision, detectionSourceOnPatrolLine,
                                            attackBeyondPatrolLine
                                        );
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
                                        {
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
                                        }

                                        lootTable.Add(new EnemyDropModel(dropType, dropId, dropChance, dropMinLevel, dropMaxLevel));
                                    }

                                    enemyModel.EnemyLootTable = lootTable;
                                    break;
                                case "Offset":
                                    var x = 0f;
                                    var y = 0f;
                                    var z = 0f;

                                    foreach (XmlAttribute hitboxData in data.Attributes)
                                    {
                                        switch (hitboxData.Name)
                                        {
                                            case "x":
                                                x = float.Parse(hitboxData.Value);
                                                break;
                                            case "y":
                                                y = float.Parse(hitboxData.Value);
                                                break;
                                            case "z":
                                                z = float.Parse(hitboxData.Value);
                                                break;
                                        }
                                    }

                                    enemyModel.Offset = new Vector3Model(x, y, z);
                                    break;
                                case "Hitbox":
                                    var width = 0f;
                                    var height = 0f;
                                    var xOffset = 0f;
                                    var yOffset = 0f;

                                    foreach (XmlAttribute hitboxData in data.Attributes)
                                    {
                                        switch (hitboxData.Name)
                                        {
                                            case "width":
                                                width = float.Parse(hitboxData.Value);
                                                break;
                                            case "height":
                                                height = float.Parse(hitboxData.Value);
                                                break;
                                            case "offsetX":
                                                xOffset = float.Parse(hitboxData.Value);
                                                break;
                                            case "offsetY":
                                                yOffset = float.Parse(hitboxData.Value);
                                                break;
                                        }
                                    }

                                    enemyModel.Hitbox = new HitboxModel(width, height, xOffset, yOffset);
                                    break;
                                default:
                                    Logger.LogError("Unknown enemy data type for: {DataType} ({EnemyName}", data.Name, prefabName);
                                    break;
                            }
                        }

                        enemyModel.EnsureValidData(prefabName, Logger);

                        EnemyInfoCatalog.Add(prefabName, enemyModel);
                    }
                }
            }
        }
    }
}
