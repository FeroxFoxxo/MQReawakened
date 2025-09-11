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

    public void EnsureValidData(string enemyType, Microsoft.Extensions.Logging.ILogger logger)
    {
        if (EnemyLootTable == null)
        {
            logger.LogError("Enemy '{Name}' does not have a loot table attached!", enemyType);
            EnemyLootTable = [];
        }

        switch (AiType)
        {
            case AiType.Behavior:
                if (BehaviorData == null)
                {
                    logger.LogError("Enemy '{Name}' does not have behavior data attached!", enemyType);
                    BehaviorData = [];
                }

                if (GlobalProperties != null)
                {
                    logger.LogWarning("Enemy '{Name}' has global properties attached, but is using AiType 'Behavior'. These will be ignored.", enemyType);
                }

                if (GenericScript != null)
                {
                    logger.LogWarning("Enemy '{Name}' has generic script properties attached, but is using AiType 'Behavior'. These will be ignored.", enemyType);
                }

                break;
        }
    }
}
