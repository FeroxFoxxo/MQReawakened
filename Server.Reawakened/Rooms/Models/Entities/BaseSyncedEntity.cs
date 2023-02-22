using Microsoft.Extensions.Logging;
using Server.Base.Logging;
using Server.Base.Network;
using Server.Reawakened.Rooms.Models.Planes;
using System.Text;

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

    public virtual void NotifyCollision(NotifyCollision_SyncEvent notifyCollisionEvent, NetState netState) =>
        SendEntityMethodUnknown("unran-collision", "Failed Collision", "NotifyCollision",
            $"Collision Event: {notifyCollisionEvent.EncodeData()}");
    
    public virtual void RunSyncedEvent(SyncEvent syncEvent, NetState netState) =>
        SendEntityMethodUnknown("unran-synced-events", "Failed Sync Event", "RunSyncedEvent",
            $"Sync Data: {syncEvent.EncodeData()}");

    public void SendEntityMethodUnknown(string file, string title, string method, string data = "")
    {
        var sb = new StringBuilder();

        sb.AppendLine($"Entity: {Id}")
            .AppendLine($"Name: {GetType().Name}");

        if (!string.IsNullOrEmpty(data))
            sb.AppendLine(data);

        sb.Append($"No override for '{method}'");

        StoredEntity.Logger.WriteGenericLog<BaseSyncedEntity>(file, title, sb.ToString(), LoggerType.Warning);
    }

    protected void SetEntityData(StoredEntityModel storedEntity) =>
        StoredEntity = storedEntity;
}
