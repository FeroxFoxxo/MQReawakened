namespace Server.Reawakened.Levels.Models.Entities;

public abstract class SynchronizedEntity<T> : BaseSynchronizedEntity
    where T : DataComponentAccessor
{
    public T EntityData;

    public override string Name => typeof(T).Name;

    public void SetEntityData(T entityData, StoredEntityModel storedEntity)
    {
        EntityData = entityData;
        SetEntityData(storedEntity);
    }
}
