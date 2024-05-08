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

    public void EnsureValidData(string enemyType, Microsoft.Extensions.Logging.ILogger logger)
    {
        if (EnemyLootTable == null)
        {
            logger.LogError("Enemy '{Name}' does not have a loot table attached!", enemyType);
            EnemyLootTable = [];
        }

        if (Hitbox == null)
        {
            logger.LogError("Enemy '{Name}' does not have a hit box attached!", enemyType);
            Hitbox = new HitboxModel(1, 1, 0.5f, 0.5f);
        }

        if (AiType == AiType.Behavior)
        {
            if (BehaviorData == null)
            {
                logger.LogError("Enemy '{Name}' does not have a behavior data attached!", enemyType);
                BehaviorData = [];
            }

            if (GlobalProperties == null)
            {
                logger.LogError("Enemy '{Name}' does not have any global properties attached!", enemyType);
                GlobalProperties = new GlobalPropertyModel(false, 0, 0, 0, 0, 0, 0, 0, 0, 0, string.Empty, "COL_PRJ_DamageProjectile", false, false, 0);
            }

            if (GenericScript == null)
            {
                logger.LogError("Enemy '{Name}' does not have any generic script attached!", enemyType);
                GenericScript = new GenericScriptModel(StateType.Unknown, StateType.Unknown, StateType.Unknown, 0, 0, 0);
            }

            if (!BehaviorData.ContainsKey(GenericScript.AttackBehavior))
                logger.LogError("Enemy '{Name}' does not have the attack behavior '{Behavior}' defined!", enemyType, GenericScript.AttackBehavior);

            if (!BehaviorData.ContainsKey(GenericScript.AwareBehavior))
                logger.LogError("Enemy '{Name}' does not have the aware behavior '{Behavior}' defined!", enemyType, GenericScript.AwareBehavior);

            if (!BehaviorData.ContainsKey(GenericScript.UnawareBehavior))
                logger.LogError("Enemy '{Name}' does not have the unaware behavior '{Behavior}' defined!", enemyType, GenericScript.UnawareBehavior);
        }
    }
}
