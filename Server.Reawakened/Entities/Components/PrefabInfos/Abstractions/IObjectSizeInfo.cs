using UnityEngine;

namespace Server.Reawakened.Entities.Components.PrefabInfos.Abstractions;
public interface IObjectSizeInfo
{
    Vector3 GetSize();
    Vector3 GetOffset();
}
