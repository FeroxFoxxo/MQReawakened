using Server.Reawakened.Entities.Enums;
using Server.Reawakened.XMLs.Abstractions;
using Server.Reawakened.XMLs.Enums;
using System.Xml;

namespace Server.Reawakened.XMLs.BundlesInternal;

public class InternalDefaultEnemies : IBundledXml
{
    public string BundleName => "InternalDefaultEnemies";
    public BundlePriority Priority => BundlePriority.Low;


    public Dictionary<string, BehaviorModel> EnemyInfoCatalog;

    public void InitializeVariables() => EnemyInfoCatalog = [];

    public void EditDescription(XmlDocument xml)
    {
    }

    public void ReadDescription(string xml)
    {
        var xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(xml);

        foreach (XmlNode enemyXml in xmlDocument.ChildNodes)
        {
            if (enemyXml.Name != "InternalDefaultEnemies") continue;

            foreach (XmlNode enemy in enemyXml.ChildNodes)
            {
                if (enemy.Name != "Enemy") continue;

                var enemyType = string.Empty;
                var behaviorModel = new BehaviorModel([], [], [], new HitboxModel(0, 0, 0, 0));

                foreach (XmlAttribute enemyName in enemy.Attributes)
                    if (enemyName.Name == "name")
                    {
                        enemyType = enemyName.Value;
                        continue;
                    }
                foreach (XmlNode behavior in enemy.ChildNodes)
                {
                    var behaviorDataModel = new BehaviorDataModel([], []);

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
                            behaviorDataModel.Resources.Add(new EnemyResourceModel(resourceType, resourceName));
                    }

                    switch (behavior.Name)
                    {

                        // Patrol Behavior
                        case "Patrol":
                            var speed = 0f;
                            var smoothMove = 0;
                            var endPathWaitTime = 0f;
                            foreach (XmlAttribute behaviorData in behavior.Attributes)
                            {
                                switch (behaviorData.Name)
                                {
                                    case "speed":
                                        speed = float.Parse(behaviorData.Value);
                                        behaviorDataModel.DataList.Add(behaviorData.Name, speed);
                                        continue;
                                    case "smoothMove":
                                        smoothMove = bool.Parse(behaviorData.Value) ? 1 : 0;
                                        behaviorDataModel.DataList.Add(behaviorData.Name, smoothMove);
                                        continue;
                                    case "endPathWaitTime":
                                        endPathWaitTime = float.Parse(behaviorData.Value);
                                        behaviorDataModel.DataList.Add(behaviorData.Name, endPathWaitTime);
                                        continue;
                                }
                            }
                            break;

                        // Aggro Behavior
                        case "Aggro":
                            var aggroSpeed = 0f;
                            var moveBeyondTargetDistance = 0f;
                            var stayOnPatrolPath = 0;
                            var attackBeyondPatrolLine = 0f;
                            var useAttackBeyondPatrolLine = 0f;
                            var detectionRangeUpY = 0f;
                            var detectionRangeDownY = 0f;
                            foreach (XmlAttribute behaviorData in behavior.Attributes)
                            {
                                switch (behaviorData.Name)
                                {
                                    case "speed":
                                        aggroSpeed = float.Parse(behaviorData.Value);
                                        behaviorDataModel.DataList.Add(behaviorData.Name, aggroSpeed);
                                        continue;
                                    case "moveBeyondTargetDistance":
                                        moveBeyondTargetDistance = float.Parse(behaviorData.Value);
                                        behaviorDataModel.DataList.Add(behaviorData.Name, moveBeyondTargetDistance);
                                        continue;
                                    case "stayOnPatrolPath":
                                        stayOnPatrolPath = bool.Parse(behaviorData.Value) ? 1 : 0;
                                        behaviorDataModel.DataList.Add(behaviorData.Name, stayOnPatrolPath);
                                        continue;
                                    case "attackBeyondPatrolLine":
                                        attackBeyondPatrolLine = float.Parse(behaviorData.Value);
                                        behaviorDataModel.DataList.Add(behaviorData.Name, attackBeyondPatrolLine);
                                        continue;
                                    case "useAttackBeyondPatrolLine":
                                        useAttackBeyondPatrolLine = bool.Parse(behaviorData.Value) ? 1 : 0;
                                        behaviorDataModel.DataList.Add(behaviorData.Name, useAttackBeyondPatrolLine);
                                        continue;
                                    case "detectionRangeUpY":
                                        detectionRangeUpY = float.Parse(behaviorData.Value);
                                        behaviorDataModel.DataList.Add(behaviorData.Name, detectionRangeUpY);
                                        continue;
                                    case "detectionRangeDownY":
                                        detectionRangeDownY = float.Parse(behaviorData.Value);
                                        behaviorDataModel.DataList.Add(behaviorData.Name, detectionRangeDownY);
                                        continue;
                                }
                            }
                            break;

                        // LookAround Behavior
                        case "LookAround":
                            var lookTime = 0f;
                            var startDirection = 0f;
                            var forceDirection = 0f;
                            var initialProgressRatio = 0f;
                            var snapOnGround = 0;
                            foreach (XmlAttribute behaviorData in behavior.Attributes)
                            {
                                switch (behaviorData.Name)
                                {
                                    case "lookTime":
                                        lookTime = float.Parse(behaviorData.Value);
                                        behaviorDataModel.DataList.Add(behaviorData.Name, lookTime);
                                        continue;
                                    case "startDirection":
                                        startDirection = float.Parse(behaviorData.Value);
                                        behaviorDataModel.DataList.Add(behaviorData.Name, startDirection);
                                        continue;
                                    case "forceDirection":
                                        forceDirection = float.Parse(behaviorData.Value);
                                        behaviorDataModel.DataList.Add(behaviorData.Name, forceDirection);
                                        continue;
                                    case "initialProgressRatio":
                                        initialProgressRatio = float.Parse(behaviorData.Value);
                                        behaviorDataModel.DataList.Add(behaviorData.Name, initialProgressRatio);
                                        continue;
                                    case "snapOnGround":
                                        snapOnGround = bool.Parse(behaviorData.Value) ? 1 : 0;
                                        behaviorDataModel.DataList.Add(behaviorData.Name, snapOnGround);
                                        continue;
                                }
                            }
                            break;

                        // ComeBack Behavior
                        case "ComeBack":
                            var comeBackSpeed = 1f;
                            foreach (XmlAttribute behaviorData in behavior.Attributes)
                            {
                                switch (behaviorData.Name)
                                {
                                    case "speed":
                                        comeBackSpeed = float.Parse(behaviorData.Value);
                                        behaviorDataModel.DataList.Add(behaviorData.Name, comeBackSpeed);
                                        continue;
                                }
                            }
                            break;

                        // Shooting
                        case "Shooting":
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
                                        behaviorDataModel.DataList.Add(behaviorData.Name, nbBulletsPerRound);
                                        continue;
                                    case "fireSpreadAngle":
                                        fireSpreadAngle = float.Parse(behaviorData.Value);
                                        behaviorDataModel.DataList.Add(behaviorData.Name, fireSpreadAngle);
                                        continue;
                                    case "delayBetweenBullet":
                                        delayBetweenBullet = float.Parse(behaviorData.Value);
                                        behaviorDataModel.DataList.Add(behaviorData.Name, delayBetweenBullet);
                                        continue;
                                    case "delayShoot_Anim":
                                        delayShootAnim = float.Parse(behaviorData.Value);
                                        behaviorDataModel.DataList.Add(behaviorData.Name, delayShootAnim);
                                        continue;
                                    case "delayBetweenFireRound":
                                        delayBetweenFireRound = float.Parse(behaviorData.Value);
                                        behaviorDataModel.DataList.Add(behaviorData.Name, delayBetweenFireRound);
                                        continue;
                                    case "nbFireRounds":
                                        nbFireRounds = int.Parse(behaviorData.Value);
                                        behaviorDataModel.DataList.Add(behaviorData.Name, nbFireRounds);
                                        continue;
                                    case "startCoolDownTime":
                                        startCoolDownTime = float.Parse(behaviorData.Value);
                                        behaviorDataModel.DataList.Add(behaviorData.Name, startCoolDownTime);
                                        continue;
                                    case "endCoolDownTime":
                                        endCoolDownTime = float.Parse(behaviorData.Value);
                                        behaviorDataModel.DataList.Add(behaviorData.Name, endCoolDownTime);
                                        continue;
                                    case "projectileSpeed":
                                        projectileSpeed = float.Parse(behaviorData.Value);
                                        behaviorDataModel.DataList.Add(behaviorData.Name, projectileSpeed);
                                        continue;
                                    case "fireSpreadClockwise":
                                        fireSpreadClockwise = bool.Parse(behaviorData.Value);
                                        behaviorDataModel.DataList.Add(behaviorData.Name, fireSpreadClockwise);
                                        continue;
                                    case "fireSpreadStartAngle":
                                        fireSpreadStartAngle = float.Parse(behaviorData.Value);
                                        behaviorDataModel.DataList.Add(behaviorData.Name, fireSpreadStartAngle);
                                        continue;
                                }
                            }
                            break;

                        // Bomber
                        case "Bomber":
                            var inTime = 0f;
                            var loopTime = 0f;
                            var bombRadius = 0f;
                            foreach (XmlAttribute behaviorData in behavior.Attributes)
                            {
                                switch (behaviorData.Name)
                                {
                                    case "inTime":
                                        inTime = float.Parse(behaviorData.Value);
                                        behaviorDataModel.DataList.Add(behaviorData.Name, inTime);
                                        continue;
                                    case "loopTime":
                                        loopTime = float.Parse(behaviorData.Value);
                                        behaviorDataModel.DataList.Add(behaviorData.Name, loopTime);
                                        continue;
                                    case "bombRadius":
                                        bombRadius = float.Parse(behaviorData.Value);
                                        behaviorDataModel.DataList.Add(behaviorData.Name, bombRadius);
                                        continue;
                                }
                            }
                            break;

                        // Grenadier
                        case "Grenadier":
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
                                        behaviorDataModel.DataList.Add(behaviorData.Name, gInTime);
                                        continue;
                                    case "loopTime":
                                        gLoopTime = float.Parse(behaviorData.Value);
                                        behaviorDataModel.DataList.Add(behaviorData.Name, gLoopTime);
                                        continue;
                                    case "outTime":
                                        gOutTime = float.Parse(behaviorData.Value);
                                        behaviorDataModel.DataList.Add(behaviorData.Name, gOutTime);
                                        continue;
                                    case "isTracking":
                                        isTracking = bool.Parse(behaviorData.Value);
                                        behaviorDataModel.DataList.Add(behaviorData.Name, isTracking);
                                        continue;
                                    case "projCount":
                                        projCount = int.Parse(behaviorData.Value);
                                        behaviorDataModel.DataList.Add(behaviorData.Name, projCount);
                                        continue;
                                    case "projSpeed":
                                        projSpeed = float.Parse(behaviorData.Value);
                                        behaviorDataModel.DataList.Add(behaviorData.Name, projSpeed);
                                        continue;
                                    case "maxHeight":
                                        maxHeight = float.Parse(behaviorData.Value);
                                        behaviorDataModel.DataList.Add(behaviorData.Name, maxHeight);
                                        continue;
                                }
                            }
                            break;

                        // Stomper
                        case "Stomper":
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
                                        behaviorDataModel.DataList.Add(behaviorData.Name, attackTime);
                                        continue;
                                    case "impactTime":
                                        impactTime = float.Parse(behaviorData.Value);
                                        behaviorDataModel.DataList.Add(behaviorData.Name, impactTime);
                                        continue;
                                    case "damageDistance":
                                        damageDistance = float.Parse(behaviorData.Value);
                                        behaviorDataModel.DataList.Add(behaviorData.Name, damageDistance);
                                        continue;
                                    case "damageOffset":
                                        damageOffset = float.Parse(behaviorData.Value);
                                        behaviorDataModel.DataList.Add(behaviorData.Name, damageOffset);
                                        continue;
                                }
                            }
                            break;

                        // Stinger
                        case "Stinger":
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
                                        behaviorDataModel.DataList.Add(behaviorData.Name, speedForward);
                                        continue;
                                    case "speedBackward":
                                        speedForward = float.Parse(behaviorData.Value);
                                        behaviorDataModel.DataList.Add(behaviorData.Name, speedBackward);
                                        continue;
                                    case "inDurationForward":
                                        inDurationForward = float.Parse(behaviorData.Value);
                                        behaviorDataModel.DataList.Add(behaviorData.Name, inDurationForward);
                                        continue;
                                    case "attackDuration":
                                        attackDuration = float.Parse(behaviorData.Value);
                                        behaviorDataModel.DataList.Add(behaviorData.Name, attackDuration);
                                        continue;
                                    case "damageAttackTimeOffset":
                                        damageAttackTimeOffset = float.Parse(behaviorData.Value);
                                        behaviorDataModel.DataList.Add(behaviorData.Name, damageAttackTimeOffset);
                                        continue;
                                    case "inDurationBackward":
                                        inDurationBackward = float.Parse(behaviorData.Value);
                                        behaviorDataModel.DataList.Add(behaviorData.Name, inDurationBackward);
                                        continue;
                                    case "damageDistance":
                                        stingerDamageDistance = float.Parse(behaviorData.Value);
                                        behaviorDataModel.DataList.Add(behaviorData.Name, stingerDamageDistance);
                                        continue;
                                }
                            }
                            break;

                        case "GlobalProperties":
                            foreach (XmlNode globalProperty in behavior.ChildNodes)
                            {
                                var gDataName = string.Empty;
                                object gDataValue = null;
                                foreach (XmlAttribute globalPropValue in globalProperty.Attributes)
                                {
                                    gDataName = globalProperty.Name;
                                    gDataValue = globalPropValue.Value;
                                }
                                behaviorModel.GlobalProperties.Add(gDataName, gDataValue);
                            }
                            break;

                        case "LootTable":
                            foreach (XmlNode dynamicDrop in behavior.ChildNodes)
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
                                                    continue;
                                                case "randomArmor":
                                                    dropType = DynamicDropType.RandomArmor;
                                                    continue;
                                                case "randomIngredient":
                                                    dropType = DynamicDropType.RandomIngredient;
                                                    continue;
                                            }
                                            continue;
                                        case "id":
                                            dropId = int.Parse(dynamicDropAttributes.Value);
                                            continue;
                                        case "chance":
                                            dropChance = float.Parse(dynamicDropAttributes.Value);
                                            continue;
                                        case "minLevel":
                                            dropMinLevel = int.Parse(dynamicDropAttributes.Value);
                                            continue;
                                        case "maxLevel":
                                            dropMaxLevel = int.Parse(dynamicDropAttributes.Value);
                                            continue;
                                    }
                                }
                                behaviorModel.EnemyLootTable.Add(new EnemyDropModel(dropType, dropId, dropChance, dropMinLevel, dropMaxLevel));
                            }
                            break;

                        case "Hitbox":
                            foreach (XmlAttribute hitboxData in behavior.Attributes)
                            {
                                switch (hitboxData.Name)
                                {
                                    case "width":
                                        behaviorModel.Hitbox.Width = float.Parse(hitboxData.Value);
                                        continue;
                                    case "height":
                                        behaviorModel.Hitbox.Height = float.Parse(hitboxData.Value);
                                        continue;
                                    case "offsetX":
                                        behaviorModel.Hitbox.XOffset = float.Parse(hitboxData.Value);
                                        continue;
                                    case "offsetY":
                                        behaviorModel.Hitbox.YOffset = float.Parse(hitboxData.Value);
                                        continue;
                                }
                            }
                            break;
                    }
                    if (behaviorDataModel.DataList.ToList().Count > 0)
                        behaviorModel.BehaviorData.Add(behavior.Name, behaviorDataModel);
                }
                EnemyInfoCatalog.Add(enemyType, behaviorModel);
            }
        }
    }

    public void FinalizeBundle()
    {
    }

    public BehaviorModel GetBehaviorsByName(string enemyName) =>
        EnemyInfoCatalog.TryGetValue(enemyName, out var behaviors) ? behaviors : new BehaviorModel([], [], [], new HitboxModel(0, 0, 0, 0));
}
