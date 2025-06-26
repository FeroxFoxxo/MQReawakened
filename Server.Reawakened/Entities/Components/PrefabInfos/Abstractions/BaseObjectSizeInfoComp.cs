using Server.Reawakened.Players;
using Server.Reawakened.Rooms.Models.Entities;
using UnityEngine;

namespace Server.Reawakened.Entities.Components.PrefabInfos.Abstractions;
public abstract class BaseObjectSizeInfoComp<T> : Component<T> where T : ObjectSizeInfo
{
    public Vector3 Size => ComponentData.Size;
    public Vector3 Offset => ComponentData.Offset;
    public float FloorHeight => ComponentData.FloorHeight;
    public bool DisableServerCollisionsForInstance => ComponentData.DisableServerCollisionsForInstance;
    public bool DisableUnityCollisionsForInstance => ComponentData.DisableUnityCollisionsForInstance;

    public override void NotifyCollision(NotifyCollision_SyncEvent notifyCollisionEvent, Player player) { }
}
