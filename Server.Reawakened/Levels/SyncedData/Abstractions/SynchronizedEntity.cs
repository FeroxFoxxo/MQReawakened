namespace Server.Reawakened.Levels.SyncedData.Abstractions;

public abstract class SynchronizedEntity<T> : BaseSynchronizedEntity
    where T : DataComponentAccessor
{
    public readonly T EntityData;

    protected SynchronizedEntity(StoredEntityModel storedEntity,
        T entityData) : base(storedEntity) =>
        EntityData = entityData;
}
