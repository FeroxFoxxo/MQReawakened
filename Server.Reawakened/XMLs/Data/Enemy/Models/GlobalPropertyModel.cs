using Server.Reawakened.Entities.Components.AI.Stats;

namespace Server.Reawakened.XMLs.Data.Enemy.Models;

public class GlobalPropertyModel(bool detectionLimitedByPatrolLine, float backDetectionRangeX, float viewOffsetY,
    float backDetectionRangeUpY, float backDetectionRangeDownY, float shootOffsetX, float shootOffsetY, float frontDetectionRangeX,
    float frontDetectionRangeUpY, float frontDetectionRangeDownY, string script, string shootingProjectilePrefabName,
    bool disableCollision, bool detectionSourceOnPatrolLine, float attackBeyondPatrolLine)
{
    private static readonly Default_AI_Stats_Global genericGlobalStats = new();

    public void ApplyGlobalPropertiesFromModel(AIStatsGlobalComp globalStats)
    {
        if (genericGlobalStats.Global_DetectionLimitedByPatrolLine == globalStats.Global_DetectionLimitedByPatrolLine)
            globalStats.ComponentData.Global_DetectionLimitedByPatrolLine = detectionLimitedByPatrolLine;

        if (genericGlobalStats.Global_BackDetectionRangeX == globalStats.Global_BackDetectionRangeX)
            globalStats.ComponentData.Global_BackDetectionRangeX = backDetectionRangeX;

        if (genericGlobalStats.Global_viewOffsetY == globalStats.Global_viewOffsetY)
            globalStats.ComponentData.Global_viewOffsetY = viewOffsetY;

        if (genericGlobalStats.Global_BackDetectionRangeUpY == globalStats.Global_BackDetectionRangeUpY)
            globalStats.ComponentData.Global_BackDetectionRangeUpY = backDetectionRangeUpY;

        if (genericGlobalStats.Global_BackDetectionRangeDownY == globalStats.Global_BackDetectionRangeDownY)
            globalStats.ComponentData.Global_BackDetectionRangeDownY = backDetectionRangeDownY;

        if (genericGlobalStats.Global_ShootOffsetX == globalStats.Global_ShootOffsetX)
            globalStats.ComponentData.Global_ShootOffsetX = shootOffsetX;

        if (genericGlobalStats.Global_ShootOffsetY == globalStats.Global_ShootOffsetY)
            globalStats.ComponentData.Global_ShootOffsetY = shootOffsetY;

        if (genericGlobalStats.Global_FrontDetectionRangeX == globalStats.Global_FrontDetectionRangeX)
            globalStats.ComponentData.Global_FrontDetectionRangeX = frontDetectionRangeX;

        if (genericGlobalStats.Global_FrontDetectionRangeUpY == globalStats.Global_FrontDetectionRangeUpY)
            globalStats.ComponentData.Global_FrontDetectionRangeUpY = frontDetectionRangeUpY;

        if (genericGlobalStats.Global_FrontDetectionRangeDownY == globalStats.Global_FrontDetectionRangeDownY)
            globalStats.ComponentData.Global_FrontDetectionRangeDownY = frontDetectionRangeDownY;

        if (genericGlobalStats.Global_Script == globalStats.Global_Script)
            globalStats.ComponentData.Global_Script = script;

        if (genericGlobalStats.Global_ShootingProjectilePrefabName == globalStats.Global_ShootingProjectilePrefabName)
            globalStats.ComponentData.Global_ShootingProjectilePrefabName = shootingProjectilePrefabName;

        if (genericGlobalStats.Global_DisableCollision == globalStats.Global_DisableCollision)
            globalStats.ComponentData.Global_DisableCollision = disableCollision;

        if (genericGlobalStats.Global_DetectionSourceOnPatrolLine == globalStats.Global_DetectionSourceOnPatrolLine)
            globalStats.ComponentData.Global_DetectionSourceOnPatrolLine = detectionSourceOnPatrolLine;

        if (genericGlobalStats.Aggro_AttackBeyondPatrolLine == globalStats.Aggro_AttackBeyondPatrolLine)
            globalStats.ComponentData.Aggro_AttackBeyondPatrolLine = attackBeyondPatrolLine;
    }
}