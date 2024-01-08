using Server.Reawakened.Players;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.Rooms.Models.Planes;
using System;

namespace Server.Reawakened.Entities.AbstractComponents;

public abstract class MovingObjectControllerComp<T> : Component<T>, IMoveable, IRecieverTriggered where T : MovingObjectController
{
    public float InitialProgressRatio => ComponentData.InitialProgressRatio;

    public IMovement Movement;

    public IMovement GetMovement() => Movement;

    public override void InitializeComponent()
    {
        if (!Room.Entities[Id].OfType<ICoopTriggered>().Any())
            return;

        Movement?.Activate(Room.Time);
    }

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

    public void Activate() => Movement?.Activate(Room.Time);

    public void Deactivate() => Movement?.Deactivate(Room.Time);

    public void RecievedTrigger(bool triggered)
    {
        if (triggered)
            GetMovement().Activate(Room.Time);
        else
            GetMovement().Deactivate(Room.Time);
    }
}
