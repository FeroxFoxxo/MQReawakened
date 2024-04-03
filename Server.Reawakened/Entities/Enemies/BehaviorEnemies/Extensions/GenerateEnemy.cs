using Server.Reawakened.Configs;
using Server.Reawakened.Entities.Components;
using Server.Reawakened.Entities.Enemies.AIStateEnemies.Rachnok;
using Server.Reawakened.Entities.Enemies.AIStateEnemies.Spiderling;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies.EnemyTypes;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies.EnemyTypes.Blank;
using Server.Reawakened.Rooms;

namespace Server.Reawakened.Entities.Enemies.BehaviorEnemies.Extensions;
public static class GenerateEnemy
{
    public static Enemy GenerateEntityFromName(this Room room, string enemyPrefab, string entityId, EnemyControllerComp enemyController,
        IServiceProvider services, ServerRConfig config)
    {
        var enemyData = new EnemyData(room, entityId, enemyPrefab, enemyController, services);

        return enemyPrefab switch
        {
            string bird when bird.Contains(config.EnemyNameSearch[0]) => new EnemyBird(enemyData),
            string fish when fish.Contains(config.EnemyNameSearch[1]) => new EnemyFish(enemyData),
            string spider when spider.Contains(config.EnemyNameSearch[2]) => new EnemySpider(enemyData),
            string bathog when bathog.Contains(config.EnemyNameSearch[3]) => new EnemyBathog(enemyData),
            string bomber when bomber.Contains(config.EnemyNameSearch[4]) => new EnemyBomber(enemyData),
            string crawler when crawler.Contains(config.EnemyNameSearch[5]) => new EnemyCrawler(enemyData),
            string dragon when dragon.Contains(config.EnemyNameSearch[6]) => new EnemyDragon(enemyData),
            string grenadier when grenadier.Contains(config.EnemyNameSearch[7]) => new EnemyGrenadier(enemyData),
            string orchid when orchid.Contains(config.EnemyNameSearch[8]) => new EnemyOrchid(enemyData),
            string pincer when pincer.Contains(config.EnemyNameSearch[9]) => new EnemyPincer(enemyData),
            string stomper when stomper.Contains(config.EnemyNameSearch[10]) => new EnemyStomper(enemyData),
            string vespid when vespid.Contains(config.EnemyNameSearch[11]) => new EnemyVespid(enemyData),
            string spiderling when spiderling.Contains(config.EnemyNameSearch[12]) => new EnemySpiderling(enemyData),
            string teaserSpiderBoss when teaserSpiderBoss.Contains(config.EnemyNameSearch[13]) => new EnemyTeaserSpiderBoss(enemyData),
            string spiderBoss when spiderBoss.Contains(config.EnemyNameSearch[14]) => new EnemySpiderBoss(enemyData),
            _ => null,
        };
    }
}
