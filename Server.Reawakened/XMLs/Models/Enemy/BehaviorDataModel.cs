using UnityEngine;

public class BehaviorDataModel(Dictionary<string, object> data, string asset)
{
    public Dictionary<string, object> DataList { get; } = data;
    public string Resource { get; } = asset;
}
