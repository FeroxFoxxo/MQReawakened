using Server.Reawakened.Entities.Components;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies.Abstractions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.XMLs.Models.Enemy.Models;

namespace Server.Reawakened.XMLs.Models.Enemy.Abstractions;
public abstract class BaseState(List<EnemyResourceModel> resources)
{
    public List<EnemyResourceModel> Resources { get; } = resources;

    protected abstract AIBaseBehavior GetBaseBehaviour(AIStatsGlobalComp globalComp, AIStatsGenericComp genericComp);

    public AIBaseBehavior CreateBaseBehaviour(AIStatsGlobalComp globalComp, AIStatsGenericComp genericComp)
    {
        var behaviour = GetBaseBehaviour(globalComp, genericComp);
        behaviour.SetBehaviour();
        return behaviour;
    }

    public virtual string[] GetStartArgs(BehaviorEnemy behaviorEnemy) =>
        CreateBaseBehaviour(behaviorEnemy.Global, behaviorEnemy.Generic).GetInitArgs();

    public string ToResourcesString()
    {
        var assetList = new SeparatedStringBuilder('+');

        foreach (var prefab in Resources)
            assetList.Append(prefab.ToString());

        return assetList.ToString();
    }

    public object ToStateString(AIStatsGlobalComp globalComp, AIStatsGenericComp genericComp) =>
        CreateBaseBehaviour(globalComp, genericComp).ToString();
}
