using Server.Reawakened.Configs;
using Server.Reawakened.Entities.Components;
using Server.Reawakened.Entities.Enemies.AIStateEnemies.Rachnok;
using Server.Reawakened.Entities.Enemies.AIStateEnemies.Spiderling;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies.EnemyTypes;
using Server.Reawakened.Rooms;

namespace Server.Reawakened.Entities.Enemies.BehaviorEnemies.Extensions;
public static class GenerateEnemy
{
    public static Enemy GenerateEntityFromName(this Room room, string enemyPrefab, string entityId, EnemyControllerComp enemyController,
        IServiceProvider services, ServerRConfig config) => enemyPrefab switch
        {
            string bird when bird.Contains(config.EnemyNameSearch[0]) => new EnemyBird(room, entityId, enemyPrefab, enemyController, services),
            string fish when fish.Contains(config.EnemyNameSearch[1]) => new EnemyFish(room, entityId, enemyPrefab, enemyController, services),
            string spider when spider.Contains(config.EnemyNameSearch[2]) => new EnemySpider(room, entityId, enemyPrefab, enemyController, services),
            string bathog when bathog.Contains(config.EnemyNameSearch[3]) => new EnemyBathog(room, entityId, enemyPrefab, enemyController, services),
            string bomber when bomber.Contains(config.EnemyNameSearch[4]) => new EnemyBomber(room, entityId, enemyPrefab, enemyController, services),
            string crawler when crawler.Contains(config.EnemyNameSearch[5]) => new EnemyCrawler(room, entityId, enemyPrefab, enemyController, services),
            string dragon when dragon.Contains(config.EnemyNameSearch[6]) => new EnemyDragon(room, entityId, enemyPrefab, enemyController, services),
            string grenadier when grenadier.Contains(config.EnemyNameSearch[7]) => new EnemyGrenadier(room, entityId, enemyPrefab, enemyController, services),
            string orchid when orchid.Contains(config.EnemyNameSearch[8]) => new EnemyOrchid(room, entityId, enemyPrefab, enemyController, services),
            string pincer when pincer.Contains(config.EnemyNameSearch[9]) => new EnemyPincer(room, entityId, enemyPrefab, enemyController, services),
            string stomper when stomper.Contains(config.EnemyNameSearch[10]) => new EnemyStomper(room, entityId, enemyPrefab, enemyController, services),
            string vespid when vespid.Contains(config.EnemyNameSearch[11]) => new EnemyVespid(room, entityId, enemyPrefab, enemyController, services),
            string spiderling when spiderling.Contains(config.EnemyNameSearch[12]) => new EnemySpiderling(room, entityId, enemyPrefab, enemyController, services),
            string teaserSpiderBoss when teaserSpiderBoss.Contains(config.EnemyNameSearch[13]) => new EnemyTeaserSpiderBoss(room, entityId, enemyPrefab, enemyController, services),
            string spiderBoss when spiderBoss.Contains(config.EnemyNameSearch[14]) => new EnemySpiderBoss(room, entityId, enemyPrefab, enemyController, services),
            _ => null,
        };
}
