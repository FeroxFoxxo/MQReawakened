using Microsoft.Extensions.Logging;
using Server.Base.Network;
using Server.Reawakened.Rooms.Models.Planes;

namespace Server.Reawakened.Rooms.Models.Entities;

public abstract class BaseSyncedEntity
{
    public StoredEntityModel StoredEntity { get; private set; }

    public Room Room => StoredEntity.Room;
    public int Id => StoredEntity.GameObject.ObjectInfo.ObjectId;
    public string PrefabName => StoredEntity.GameObject.ObjectInfo.PrefabName;
    public Vector3Model Position => StoredEntity.GameObject.ObjectInfo.Position;
    public Vector3Model Rotation => StoredEntity.GameObject.ObjectInfo.Rotation;
    public Vector3Model Scale => StoredEntity.GameObject.ObjectInfo.Scale;

    public abstract string Name { get; }

    public virtual void InitializeEntity()
    {
    }
    
    public virtual object[] GetInitData(NetState netState) => Array.Empty<object>();

    public virtual void SendDelayedData(NetState netState)
    {
    }

    public virtual void Update()
    {
    }

    public virtual void NotifyCollision(NotifyCollision_SyncEvent notifyCollisionEvent, NetState netState)
    {
    }

    public virtual void RunSyncedEvent(SyncEvent syncEvent, NetState netState) =>
        StoredEntity.Room.Logger.LogError(
            "The entity '{Id}' of type '{Type}' has no sync event override. Skipping...", Id, GetType().Name);
    
    protected void SetEntityData(StoredEntityModel storedEntity) =>
        StoredEntity = storedEntity;
}
