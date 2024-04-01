using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.Enums;
using Server.Reawakened.XMLs.Abstractions;
using Server.Reawakened.XMLs.Enums;
using Server.Reawakened.XMLs.Models.Enemy.Abstractions;
using Server.Reawakened.XMLs.Models.Enemy.Enums;
using Server.Reawakened.XMLs.Models.Enemy.Models;
using Server.Reawakened.XMLs.Models.Enemy.States;
using System.Xml;

namespace Server.Reawakened.XMLs.BundlesInternal;

public class InternalDefaultEnemies : InternalXml
{
    public override string BundleName => "InternalDefaultEnemies";
    public override BundlePriority Priority => BundlePriority.Low;

    public ILogger<InternalDefaultEnemies> Logger { get; set; }

    public Dictionary<string, BehaviorModel> EnemyInfoCatalog;

    public override void InitializeVariables() => EnemyInfoCatalog = [];

    public BehaviorModel GetBehaviorsByName(string enemyName) =>
        EnemyInfoCatalog.TryGetValue(enemyName, out var behaviors) ?
            behaviors :
            new BehaviorModel();

    public override void ReadDescription(XmlDocument xmlDocument)
    {
        foreach (XmlNode enemyXml in xmlDocument.ChildNodes)
        {
            if (enemyXml.Name != "InternalDefaultEnemies") continue;

            foreach (XmlNode enemy in enemyXml.ChildNodes)
            {
                if (enemy.Name != "Enemy") continue;

                var enemyType = string.Empty;
                var behaviorModel = new BehaviorModel();

                foreach (XmlAttribute enemyName in enemy.Attributes)
                {
                    if (enemyName.Name == "name")
                    {
                        enemyType = enemyName.Value;
                        break;
                    }
                }

                foreach (XmlNode data in enemy.ChildNodes)
                {
                    switch (data.Name)
                    {
                        case "Behaviors":
                            var behaviors = new Dictionary<StateTypes, BaseState>();

                            foreach (XmlNode behavior in data.ChildNodes)
                            {
                                BaseState state = null;
                                var stateType = Enum.Parse<StateTypes>(behavior.Name);
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
                                    case StateTypes.Patrol:
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
                                    case StateTypes.Aggro:
                                        var aggroSpeed = 0f;
                                        var moveBeyondTargetDistance = 0f;
                                        var stayOnPatrolPath = true;
                                        var attackBeyondPatrolLine = 0f;
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
                                                    attackBeyondPatrolLine = float.Parse(behaviorData.Value);
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

                                        state = new AggroState(aggroSpeed, moveBeyondTargetDistance, stayOnPatrolPath, attackBeyondPatrolLine,
                                            useAttackBeyondPatrolLine, detectionRangeUpY, detectionRangeDownY, resources);
                                        break;
                                    case StateTypes.LookAround:
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
                                    case StateTypes.ComeBack:
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
                                    case StateTypes.Shooting:
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
                                    case StateTypes.Bomber:
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
                                    case StateTypes.Grenadier:
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
                                    case StateTypes.Stomper:
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
                                    case StateTypes.Stinger:
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
                                                    speedForward = float.Parse(behaviorData.Value);
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
                                        Logger.LogError("Unimplemented state for: {State} ({EnemyName})", stateType, enemyType);
                                        break;
                                }

                                behaviors.Add(stateType, state);
                            }

                            behaviorModel.BehaviorData = behaviors;
                            break;
                        case "GlobalProperties":
                            var globalProperties = new Dictionary<string, object>();

                            foreach (XmlNode globalProperty in data.ChildNodes)
                            {
                                var gDataName = string.Empty;
                                object gDataValue = null;

                                foreach (XmlAttribute globalPropValue in globalProperty.Attributes)
                                {
                                    gDataName = globalProperty.Name;
                                    gDataValue = globalPropValue.Value;
                                }

                                globalProperties.Add(gDataName, gDataValue);
                            }

                            behaviorModel.GlobalProperties = globalProperties;
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

                            behaviorModel.EnemyLootTable = lootTable;
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

                            behaviorModel.Hitbox = new HitboxModel(width, height, xOffset, yOffset);
                            break;
                        default:
                            Logger.LogError("Unknown enemy data type for: {DataType} ({EnemyName}", data.Name, enemyType);
                            break;
                    }
                }

                EnemyInfoCatalog.Add(enemyType, behaviorModel);
            }
        }
    }
}
