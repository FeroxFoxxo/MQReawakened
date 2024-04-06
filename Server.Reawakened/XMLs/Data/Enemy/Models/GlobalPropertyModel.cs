using Server.Reawakened.Entities.Components.AI.Stats;
using Server.Reawakened.Rooms.Services;

namespace Server.Reawakened.XMLs.Data.Enemy.Models;

public class GlobalPropertyModel(bool detectionLimitedByPatrolLine, float backDetectionRangeX, float viewOffsetY,
    float backDetectionRangeUpY, float backDetectionRangeDownY, float shootOffsetX, float shootOffsetY, float frontDetectionRangeX,
    float frontDetectionRangeUpY, float frontDetectionRangeDownY, string script, string shootingProjectilePrefabName,
    bool disableCollision, bool detectionSourceOnPatrolLine, float attackBeyondPatrolLine)
{
    public GlobalProperties GenerateGlobalPropertiesFromModel(ClassCopier classCopier, AIStatsGlobalComp globalStats)
    {
        var properties = new GlobalProperties(
            detectionLimitedByPatrolLine, backDetectionRangeX,
            viewOffsetY, backDetectionRangeUpY, backDetectionRangeDownY,
            shootOffsetX, shootOffsetY, frontDetectionRangeX,
            frontDetectionRangeUpY, frontDetectionRangeDownY,
            script, shootingProjectilePrefabName, disableCollision,
            detectionSourceOnPatrolLine, attackBeyondPatrolLine
        );

        // Breaks enemy behavior stats, commenting out for now
        globalStats?.MixGlobalProperties(classCopier, properties);

        return properties;
    }
}
