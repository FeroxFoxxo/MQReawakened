using UnityEngine;

namespace Server.Reawakened.Rooms.Models.Entities;

public abstract class Component<T> : BaseComponent
{
    public T ComponentData;

    public override string Name => typeof(T).Name;

    public void SetComponentData(T componentData, Entity storedEntity)
    {
        ComponentData = componentData;
        SetComponentData(storedEntity);
    }
}
