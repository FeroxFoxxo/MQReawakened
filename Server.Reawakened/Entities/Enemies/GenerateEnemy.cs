using Microsoft.Extensions.Logging;
using Server.Reawakened.Configs;
using Server.Reawakened.Entities.Components;
using Server.Reawakened.Entities.Enemies.AIStateEnemies.Rachnok;
using Server.Reawakened.Entities.Enemies.AIStateEnemies.Spiderling;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies.Abstractions;
using Server.Reawakened.Rooms;
using Server.Reawakened.XMLs.BundlesInternal;
using Server.Reawakened.XMLs.Models.Enemy.Enums;

namespace Server.Reawakened.Entities.Enemies;
public static class GenerateEnemy
{
    public static Enemy GenerateEntityFromName(this Room room, string enemyPrefab, string entityId, EnemyControllerComp enemyController,
        IServiceProvider services, ServerRConfig config, InternalEnemyData enemies, Microsoft.Extensions.Logging.ILogger logger)
    {
        if (!enemies.EnemyInfoCatalog.TryGetValue(enemyPrefab, out var enemyModel))
        {
            logger.LogError("Could not find enemy with name {EnemyPrefab}! Returning null...", enemyPrefab);
            return null;
        }

        var enemyData = new EnemyData(room, entityId, enemyPrefab, enemyController, enemyModel, services);

        switch (enemyModel.AiType)
        {
            case AiType.Behavior:
                return new BehaviorEnemy(enemyData);
            case AiType.State:
                return enemyPrefab switch
                {
                    string spiderling when spiderling.Contains(config.EnemyNameSearch[0]) => new EnemySpiderling(enemyData),
                    string teaserSpiderBoss when teaserSpiderBoss.Contains(config.EnemyNameSearch[1]) => new EnemyTeaserSpiderBoss(enemyData),
                    string spiderBoss when spiderBoss.Contains(config.EnemyNameSearch[2]) => new EnemySpiderBoss(enemyData),
                    _ => throw new ArgumentException($"No AI state generator found for: '{enemyPrefab}'!"),
                };
            default:
                logger.LogError("No enemy generator found with type: '{Type}' for enemy '{EnemyName}'. Returning null...",
                    enemyModel.AiType, enemyPrefab
                );
                return null;
        }
    }
}
