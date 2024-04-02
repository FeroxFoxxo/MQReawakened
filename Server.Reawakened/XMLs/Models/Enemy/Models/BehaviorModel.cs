using Microsoft.Extensions.Logging;
using Server.Reawakened.XMLs.Models.Enemy.Abstractions;
using Server.Reawakened.XMLs.Models.Enemy.Enums;

namespace Server.Reawakened.XMLs.Models.Enemy.Models;

public class BehaviorModel
{
    public Dictionary<StateTypes, BaseState> BehaviorData { get; set; }
    public List<EnemyDropModel> EnemyLootTable { get; set; }
    public GlobalPropertyModel GlobalProperties { get; set; }
    public HitboxModel Hitbox { get; set; }

    public void EnsureBehaviourValid(Microsoft.Extensions.Logging.ILogger logger, string enemyType)
    {
        if (BehaviorData == null)
        {
            logger.LogError("Enemy '{Name}' does not have a behavior data attached!", enemyType);
            BehaviorData = [];
        }

        if (EnemyLootTable == null)
        {
            logger.LogError("Enemy '{Name}' does not have a loot table attached!", enemyType);
            EnemyLootTable = [];
        }

        if (Hitbox == null)
        {
            logger.LogError("Enemy '{Name}' does not have a hit box attached!", enemyType);
            Hitbox = new HitboxModel(0, 0, 0, 0);
        }

        if (GlobalProperties == null)
        {
            logger.LogError("Enemy '{Name}' does not have any global properties attached!", enemyType);
            GlobalProperties = new GlobalPropertyModel(false, 0, 0, 0, 0, 0, 0, 0, 0, 0, string.Empty, "COL_PRJ_DamageProjectile", false, false, 0, StateTypes.Unknown, 0);
        }
    }

    public int IndexOf(StateTypes behaviorType)
    {
        var index = 0;

        foreach (var behavior in BehaviorData)
        {
            if (behavior.Key == behaviorType)
                return index;

            index++;
        }

        return 0;
    }
}
