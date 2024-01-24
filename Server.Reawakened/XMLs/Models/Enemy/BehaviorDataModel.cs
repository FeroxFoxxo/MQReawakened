using UnityEngine;

public class BehaviorDataModel(Dictionary<string, object> data)
{
    public Dictionary<string, object> DataList { get; } = data;
}
