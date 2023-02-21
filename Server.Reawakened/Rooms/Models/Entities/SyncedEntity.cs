namespace Server.Reawakened.Rooms.Models.Entities;

public abstract class SyncedEntity<T> : BaseSyncedEntity
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
