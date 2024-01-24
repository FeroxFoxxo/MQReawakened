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

    public Dictionary<string, Dictionary<string, Dictionary<string, object>>> EnemyInfoCatalog;

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
                var behaviorDict = new Dictionary<string, Dictionary<string, object>>();

                foreach (XmlAttribute enemyName in enemy.Attributes)
                    if (enemyName.Name == "name")
                    {
                        enemyType = enemyName.Value;
                        continue;
                    }
                foreach (XmlNode behavior in enemy.ChildNodes)
                {
                    var behaviorDataDict = new Dictionary<string, object>();

                    switch (behavior.Name)
                    {
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
                                        behaviorDataDict.Add(behaviorData.Name, speed);
                                        continue;
                                    case "smoothMove":
                                        smoothMove = bool.Parse(behaviorData.Value) ? 1 : 0;
                                        behaviorDataDict.Add(behaviorData.Name, smoothMove);
                                        continue;
                                    case "endPathWaitTime":
                                        endPathWaitTime = float.Parse(behaviorData.Value);
                                        behaviorDataDict.Add(behaviorData.Name, endPathWaitTime);
                                        continue;
                                }
                            }
                            break;

                        case "Aggro":
                            var aggroSpeed = 0f;
                            var moveBeyondTargetDistance = 0f;
                            var stayOnPatrolPath = 0;
                            var attackBeyondPatrolLine = 0f;
                            var detectionRangeUpY = 0f;
                            var detectionRangeDownY = 0f;
                            foreach (XmlAttribute behaviorData in behavior.Attributes)
                            {
                                switch (behaviorData.Name)
                                {
                                    case "speed":
                                        aggroSpeed = float.Parse(behaviorData.Value);
                                        behaviorDataDict.Add(behaviorData.Name, aggroSpeed);
                                        continue;
                                    case "moveBeyondTargetDistance":
                                        moveBeyondTargetDistance = float.Parse(behaviorData.Value);
                                        behaviorDataDict.Add(behaviorData.Name, moveBeyondTargetDistance);
                                        continue;
                                    case "stayOnPatrolPath":
                                        stayOnPatrolPath = bool.Parse(behaviorData.Value) ? 1 : 0;
                                        behaviorDataDict.Add(behaviorData.Name, stayOnPatrolPath);
                                        continue;
                                    case "attackBeyondPatrolLine":
                                        attackBeyondPatrolLine = float.Parse(behaviorData.Value);
                                        behaviorDataDict.Add(behaviorData.Name, attackBeyondPatrolLine);
                                        continue;
                                    case "detectionRangeUpY":
                                        detectionRangeUpY = float.Parse(behaviorData.Value);
                                        behaviorDataDict.Add(behaviorData.Name, detectionRangeUpY);
                                        continue;
                                    case "detectionRangeDownY":
                                        detectionRangeDownY = float.Parse(behaviorData.Value);
                                        behaviorDataDict.Add(behaviorData.Name, detectionRangeDownY);
                                        continue;
                                }
                            }
                            break;

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
                                        behaviorDataDict.Add(behaviorData.Name, lookTime);
                                        continue;
                                    case "startDirection":
                                        startDirection = float.Parse(behaviorData.Value);
                                        behaviorDataDict.Add(behaviorData.Name, startDirection);
                                        continue;
                                    case "forceDirection":
                                        forceDirection = float.Parse(behaviorData.Value);
                                        behaviorDataDict.Add(behaviorData.Name, forceDirection);
                                        continue;
                                    case "initialProgressRatio":
                                        initialProgressRatio = float.Parse(behaviorData.Value);
                                        behaviorDataDict.Add(behaviorData.Name, initialProgressRatio);
                                        continue;
                                    case "snapOnGround":
                                        snapOnGround = bool.Parse(behaviorData.Value) ? 1 : 0;
                                        behaviorDataDict.Add(behaviorData.Name, snapOnGround);
                                        continue;
                                }
                            }
                            break;
                    }
                    behaviorDict.Add(behavior.Name, behaviorDataDict);
                }
                EnemyInfoCatalog.Add(enemyType, behaviorDict);
            }
        }

        // Debug purposes. Prints all behaviors to console.
        //foreach (var enemy in EnemyInfoCatalog)
        //{
        //    Console.WriteLine(enemy.Key);
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

    public Dictionary<string, Dictionary<string, object>> GetBehaviorsByName(string enemyName) =>
        EnemyInfoCatalog.TryGetValue(enemyName, out var behaviors) ? behaviors : [];
}
