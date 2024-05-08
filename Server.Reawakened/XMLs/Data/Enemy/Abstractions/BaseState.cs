using Server.Reawakened.Entities.Enemies.Behaviors.Abstractions;
using Server.Reawakened.Entities.Enemies.EnemyTypes;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.XMLs.Data.Enemy.Models;

namespace Server.Reawakened.XMLs.Data.Enemy.Abstractions;
public abstract class BaseState(List<EnemyResourceModel> resources)
{
    public List<EnemyResourceModel> Resources => resources;

    public abstract AIBaseBehavior GetBaseBehaviour(BehaviorEnemy enemy);

    public string ToResourcesString()
    {
        var assetList = new SeparatedStringBuilder('+');

        foreach (var prefab in Resources)
            assetList.Append(prefab.ToString());

        return assetList.ToString();
    }
}
