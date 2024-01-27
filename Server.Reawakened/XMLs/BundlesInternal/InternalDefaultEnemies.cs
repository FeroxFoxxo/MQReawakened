using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.Rooms.Models.Entities.ColliderType;
using Server.Reawakened.XMLs.Abstractions;
using Server.Reawakened.XMLs.Bundles;
using Server.Reawakened.XMLs.Enums;
using Server.Reawakened.XMLs.Extensions;
using Server.Reawakened.XMLs.Models.Npcs;
using System.Xml;
using UnityEngine;

namespace Server.Reawakened.XMLs.BundlesInternal;

public class InternalDefaultEnemies : IBundledXml<InternalDefaultEnemies>
{
    public string BundleName => "InternalDefaultEnemies";
    public BundlePriority Priority => BundlePriority.Low;

    public ILogger<InternalDefaultEnemies> Logger { get; set; }
    public IServiceProvider Services { get; set; }

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
                var behaviorModel = new BehaviorModel(new Dictionary<string, BehaviorDataModel>(), new Dictionary<string, object>());

                foreach (XmlAttribute enemyName in enemy.Attributes)
                    if (enemyName.Name == "name")
                    {
                        enemyType = enemyName.Value;
                        continue;
                    }
                foreach (XmlNode behavior in enemy.ChildNodes)
                {
                    var behaviorDataModel = new BehaviorDataModel(new Dictionary<string, object>(), new List<EnemyResourceModel>());

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
                                    case "attackTime":
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
        EnemyInfoCatalog.TryGetValue(enemyName, out var behaviors) ? behaviors : new BehaviorModel(new Dictionary<string, BehaviorDataModel>(), new Dictionary<string, object>());
}
