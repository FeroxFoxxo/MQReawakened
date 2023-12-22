﻿using Server.Reawakened.Players;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.Rooms.Models.Planes;

namespace Server.Reawakened.Entities.AbstractComponents;

public abstract class MovingObjectControllerComp<T> : Component<T>, IMoveable where T : MovingObjectController
{
    public IMovement Movement;
    public float InitialProgressRatio => ComponentData.InitialProgressRatio;

    public IMovement GetMovement() => Movement;

    public override void InitializeComponent()
    {
        if (!Room.Entities[Id].OfType<ITriggerable>().Any())
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
}
