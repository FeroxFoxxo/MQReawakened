using Server.Reawakened.Entities.Components.GameObjects.Trigger.Interfaces;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.Rooms.Models.Planes;

namespace Server.Reawakened.Entities.AbstractComponents;

public abstract class BaseMovingObjectControllerComp<T> : Component<T>, IRecieverTriggered where T : MovingObjectController
{
    public float InitialProgressRatio => ComponentData.InitialProgressRatio;

    public IMovement Movement;

    public override void InitializeComponent() =>
        Movement.Activate(Room.Time);

    public override void Update()
    {
        if (Movement == null)
            return;

        var position = Movement.GetPositionBasedOnTime(Room.Time);

        Entity.GameObject.ObjectInfo.Position = new Vector3Model
        {
            X = position.x,
            Y = position.y,
            Z = position.z
        };
    }

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
