using Server.Reawakened.Entities.Components;
using Server.Reawakened.XMLs.Models.Enemy.Enums;

namespace Server.Reawakened.XMLs.Models.Enemy.Models;

public class GlobalPropertyModel(bool detectionLimitedByPatrolLine, float backDetectionRangeX, float viewOffsetY,
    float backDetectionRangeUpY, float backDetectionRangeDownY, float shootOffsetX, float shootOffsetY, float frontDetectionRangeX,
    float frontDetectionRangeUpY, float frontDetectionRangeDownY, string script, string shootingProjectilePrefabName,
    bool disableCollision, bool detectionSourceOnPatrolLine, float attackBeyondPatrolLine,
    
    // Custom XML Attributes
    StateTypes offensiveBehavior, float minBehaviorTime)
{
    public StateTypes OffensiveBehavior { get; } = offensiveBehavior;
    public float MinBehaviorTime { get; } = minBehaviorTime;

    public GlobalProperties GenerateGlobalPropertiesFromModel(AIStatsGlobalComp globalStats)
    {
        var properties = new GlobalProperties(
            detectionLimitedByPatrolLine, backDetectionRangeX,
            viewOffsetY, backDetectionRangeUpY, backDetectionRangeDownY,
            shootOffsetX, shootOffsetY, frontDetectionRangeX,
            frontDetectionRangeUpY, frontDetectionRangeDownY,
            script, shootingProjectilePrefabName, disableCollision,
            detectionSourceOnPatrolLine, attackBeyondPatrolLine
        );

        globalStats?.MixGlobalProperties(properties);

        return properties;
    }
}
