using Server.Reawakened.Entities.Components.GameObjects.Trigger.Interfaces;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms.Models.Entities;

namespace Server.Reawakened.Entities.Components.GameObjects.Platforms.Abstractions;

public abstract class BaseMovingObjectControllerComp<T> : Component<T>, IRecieverTriggered where T : MovingObjectController
{
    public float InitialProgressRatio => ComponentData.InitialProgressRatio;

    public IMovement Movement;

    public override void InitializeComponent() =>
        Movement.Activate(Room.Time);

    public override void Update()
    {
        if (Movement == null || Room == null)
            return;

        var position = Movement.GetPositionBasedOnTime(Room.Time);

        Position.SetPosition(position);
    }

    public override void NotifyCollision(NotifyCollision_SyncEvent notifyCollisionEvent, Player player) { }

    public override object[] GetInitData(Player player) =>
    [
        Room.Time,
        Movement.GetBehaviorRatio(Room.Time),
        Movement.Activated ? 1 : 0
    ];

    public void RecievedTrigger(bool triggered)
    {
        if (triggered)
            Movement.Activate(Room.Time);
        else
            Movement.Deactivate(Room.Time);
    }
}
