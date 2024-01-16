using Server.Base.Logging;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms.Models.Planes;
using System.Text;

namespace Server.Reawakened.Rooms.Models.Entities;

public abstract class BaseComponent
{
    public Entity Entity { get; private set; }

    public Room Room => Entity.Room;

    public int Id => Entity.GameObject.ObjectInfo.ObjectId;
    public string PrefabName => Entity.GameObject.ObjectInfo.PrefabName;
    public string ParentPlane => Entity.GameObject.ObjectInfo.ParentPlane;
    public RectModel Rectangle => Entity.GameObject.ObjectInfo.Rectangle;
    public Vector3Model Position => Entity.GameObject.ObjectInfo.Position;
    public Vector3Model Rotation => Entity.GameObject.ObjectInfo.Rotation;
    public Vector3Model Scale => Entity.GameObject.ObjectInfo.Scale;

    public abstract string Name { get; }

    public virtual void InitializeComponent()
    {
    }

    public virtual object[] GetInitData(Player player) => [];

    public virtual void SendDelayedData(Player player)
    {
    }

    public virtual void Update()
    {
    }

    public virtual void NotifyCollision(NotifyCollision_SyncEvent notifyCollisionEvent, Player player) =>
        SendComponentMethodUnknown("unrun-collisions", "Failed Collision", "NotifyCollision",
            $"Collision Event: {notifyCollisionEvent.EncodeData()}");

    public virtual void RunSyncedEvent(SyncEvent syncEvent, Player player) =>
        SendComponentMethodUnknown("unrun-synced-events", "Failed Sync Event", "RunSyncedEvent",
            $"Sync Data: {syncEvent.EncodeData()}\nType:{syncEvent.Type}");

    public void SendComponentMethodUnknown(string file, string title, string method, string data = "")
    {
        var sb = new StringBuilder();

        sb.AppendLine($"Component: {Id}")
            .AppendLine($"Name: {GetType().Name}");

        if (!string.IsNullOrEmpty(data))
            sb.AppendLine(data);

        sb.Append($"No override for '{method}'");

        Entity.Logger.WriteGenericLog<BaseComponent>(file, title, sb.ToString(), LoggerType.Warning);
    }

    protected void SetComponentData(Entity storedEntity) =>
        Entity = storedEntity;
}
