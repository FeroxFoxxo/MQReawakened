using Microsoft.Extensions.Logging;
using Server.Base.Network;

namespace Server.Reawakened.Levels.SyncedData.Abstractions;

public abstract class BaseSynchronizedEntity
{
    public readonly StoredEntityModel StoredEntity;

    public abstract string Name { get; }
    
    protected BaseSynchronizedEntity(StoredEntityModel storedEntity) =>
        StoredEntity = storedEntity;

    public virtual string[] GetInitData() => Array.Empty<string>();

    public virtual void SendDelayedData(NetState netState) { }

    public virtual void RunSyncedEvent(SyncEvent syncEvent, NetState netState) =>
        StoredEntity.Logger.LogError(
            "The synchronized entity {Id} has no override for events! Skipping...",
            StoredEntity.Id
        );
}
