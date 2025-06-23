using Microsoft.Extensions.Logging;
using Server.Reawakened.XMLs.Data.Enemy.Abstractions;
using Server.Reawakened.XMLs.Data.Enemy.Enums;

namespace Server.Reawakened.XMLs.Data.Enemy.Models;

public class EnemyModel
{
    public AiType AiType { get; set; }
    public EnemyCategory EnemyCategory { get; set; }
    public Dictionary<StateType, BaseState> BehaviorData { get; set; }
    public List<EnemyDropModel> EnemyLootTable { get; set; }
    public GlobalPropertyModel GlobalProperties { get; set; }
    public GenericScriptModel GenericScript { get; set; }
    public HitboxModel Hitbox { get; set; }
    public Dictionary<string, Dictionary<string, string>> ComponentOverrides { get; set; }

    public void EnsureValidData(string enemyType, Microsoft.Extensions.Logging.ILogger logger)
    {
        if (EnemyLootTable == null)
        {
            logger.LogError("Enemy '{Name}' does not have a loot table attached!", enemyType);
            EnemyLootTable = [];
        }
    }
}
