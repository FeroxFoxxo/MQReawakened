namespace Server.Reawakened.Levels.SyncedData.Abstractions;

public abstract class SynchronizedEntity<T> : BaseSynchronizedEntity
    where T : DataComponentAccessor
{
    public readonly T EntityData;

    public override string Name => typeof(T).Name;

    protected SynchronizedEntity(StoredEntityModel storedEntity,
        T entityData) : base(storedEntity) =>
        EntityData = entityData;
}
