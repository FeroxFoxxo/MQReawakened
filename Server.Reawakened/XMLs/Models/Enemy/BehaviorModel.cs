public class BehaviorModel(Dictionary<string, BehaviorDataModel> behaviorData, Dictionary<string, object> global, List<EnemyDropModel> drops, HitboxModel hitbox)
{
    public Dictionary<string, BehaviorDataModel> BehaviorData { get; } = behaviorData;
    public Dictionary<string, object> GlobalProperties { get; } = global;
    public List<EnemyDropModel> EnemyLootTable { get; } = drops;
    public HitboxModel Hitbox { get; } = hitbox;

    public int IndexOf(string behaviorName)
    {
        var index = 0;
        foreach (var behavior in BehaviorData)
        {
            if (behavior.Key.Equals(behaviorName))
            {
                return index;
            }
            index++;
        }

        return 0;
    }

    public object GetBehaviorStat(string behaviorName, string statName)
    {
        foreach (var behavior in BehaviorData)
        {
            if (behavior.Key.Equals(behaviorName))
            {
                foreach (var data in behavior.Value.DataList)
                {
                    if (data.Key.Equals(statName))
                        return data.Value;
                }
            }
        }

        return 0;
    }

    public object GetGlobalProperty(string property)
    {
        foreach (var data in GlobalProperties)
        {
            if (data.Key.Equals(property))
            {
                return data.Value;
            }
        }

        //Returning anything other than a valid prefab for ProjectilePrefabName causes a serverwide crash
        return property.Equals("ProjectilePrefabName") ? "COL_PRJ_DamageProjectile" : 0;
    }
}
