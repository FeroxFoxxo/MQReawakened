using Microsoft.Extensions.Logging;
using Server.Base.Network;

namespace Server.Reawakened.Levels.Models.Entities;

public abstract class BaseSynchronizedEntity
{
    public StoredEntityModel StoredEntity { get; private set; }

    public Level Level => StoredEntity.Level;
    public Microsoft.Extensions.Logging.ILogger Logger => StoredEntity.Logger;

    public abstract string Name { get; }
    
    public virtual void InitializeEntity() { }

    public virtual string[] GetInitData() => Array.Empty<string>();

    public virtual void SendDelayedData(NetState netState) { }

    public virtual void RunSyncedEvent(SyncEvent syncEvent, NetState netState) =>
        StoredEntity.Logger.LogError(
            "The synchronized entity {Id} has no override for events! Skipping...",
            StoredEntity.Id
        );

    protected void SetEntityData(StoredEntityModel storedEntity)
    {
        StoredEntity = storedEntity;
        InitializeEntity();
    }
}
