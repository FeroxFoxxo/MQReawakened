using UnityEngine;

public class BehaviorDataModel(Dictionary<string, object> data, List<EnemyResourceModel> resources)
{
    public Dictionary<string, object> DataList { get; } = data;
    public List<EnemyResourceModel> Resources { get; } = resources;
}
