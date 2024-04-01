using Server.Reawakened.XMLs.Models.Enemy.Abstractions;
using Server.Reawakened.XMLs.Models.Enemy.Enums;

namespace Server.Reawakened.XMLs.Models.Enemy.Models;

public class BehaviorModel
{
    public Dictionary<StateTypes, BaseState> BehaviorData = [];
    public Dictionary<string, object> GlobalProperties = [];
    public List<EnemyDropModel> EnemyLootTable = [];
    public HitboxModel Hitbox = new(0, 0, 0, 0);

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

    public object GetGlobalProperty(string property)
    {
        if (GlobalProperties.TryGetValue(property, out var value))
            return value;

        //Returning anything other than a valid prefab for ProjectilePrefabName causes a serverwide crash
        return property.Equals("ProjectilePrefabName") ? "COL_PRJ_DamageProjectile" : 0;
    }
}
