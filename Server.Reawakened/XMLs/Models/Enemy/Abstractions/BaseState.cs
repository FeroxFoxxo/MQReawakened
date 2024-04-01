using Server.Reawakened.Entities.Components;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies.BehaviourTypes;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.XMLs.Models.Enemy.Models;

namespace Server.Reawakened.XMLs.Models.Enemy.Abstractions;
public abstract class BaseState(List<EnemyResourceModel> resources)
{
    public List<EnemyResourceModel> Resources { get; } = resources;

    public abstract AIBaseBehavior CreateBaseBehaviour(AIStatsGenericComp generic);

    public abstract string[] GetStartArgs(BehaviorEnemy behaviorEnemy);

    public abstract string ToStateString(AIStatsGenericComp generic);

    public string ToResourcesString()
    {
        var assetList = new SeparatedStringBuilder('+');

        foreach (var prefab in Resources)
            assetList.Append(prefab.ToString());

        return assetList.ToString();
    }
}
