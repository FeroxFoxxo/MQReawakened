using Server.Base.Network;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.Rooms.Models.Planes;

namespace Server.Reawakened.Entities.Abstractions;

public abstract class AbstractMovingObject<T> : SyncedEntity<T>, IMoveable where T : MovingObjectController
{
    public float InitialProgressRatio => EntityData.InitialProgressRatio;

    public IMovement Movement;

    public override void InitializeEntity()
    {
        if (!Room.RoomEntities.Entities[Id].OfType<ITriggerable>().Any())
            return;

        Movement?.Activate(Room.Time);
    }

    public override void Update()
    {
        if (Movement == null)
            return;

        var position = Movement.GetPositionBasedOnTime(Room.Time);

        StoredEntity.GameObject.ObjectInfo.Position = new Vector3Model
        {
            X = position.x,
            Y = position.y,
            Z = position.z
        };
    }

    public override object[] GetInitData(NetState netState) => new object[]
    {
        Room.Time,
        Movement.GetBehaviorRatio(Room.Time),
        Movement.Activated ? 1 : 0
    };

    public void Activate() => Movement?.Activate(Room.Time);

    public void Deactivate() => Movement?.Deactivate(Room.Time);

    public IMovement GetMovement() => Movement;
}
