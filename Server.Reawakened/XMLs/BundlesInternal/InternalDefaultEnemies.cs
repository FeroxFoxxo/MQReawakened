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
                var behaviorModel = new BehaviorModel(new Dictionary<string, BehaviorDataModel>());

                foreach (XmlAttribute enemyName in enemy.Attributes)
                    if (enemyName.Name == "name")
                    {
                        enemyType = enemyName.Value;
                        continue;
                    }
                foreach (XmlNode behavior in enemy.ChildNodes)
                {
                    var behaviorDataModel = new BehaviorDataModel(new Dictionary<string, object>(), string.Empty);

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

                    }
                    behaviorModel.BehaviorData.Add(behavior.Name, behaviorDataModel);
                }
                EnemyInfoCatalog.Add(enemyType, behaviorModel);
            }
        }

        // Debug purposes. Prints all behaviors to console.
        //foreach (var enemy in EnemyInfoCatalog)
        //{
        //    moveLine(enemy.Key);
        //    foreach(var behavior in enemy.Value)
        //    {
        //        Console.WriteLine("Behavior: " + behavior.Key);
        //        foreach(var data in behavior.Value)
        //        {
        //            Console.WriteLine("- " + data.Key + ": " + data.Value);
        //        }
        //    }
        //    Console.WriteLine("--------------");
        //}
    }

    public void FinalizeBundle()
    {
    }

    public BehaviorModel GetBehaviorsByName(string enemyName) =>
        EnemyInfoCatalog.TryGetValue(enemyName, out var behaviors) ? behaviors : new BehaviorModel(new Dictionary<string, BehaviorDataModel>());
}
