using Server.Reawakened.Entities.Colliders;
using Server.Reawakened.Entities.Components.GameObjects.Platforms.Abstractions;
using UnityEngine;

namespace Server.Reawakened.Entities.Components.GameObjects.Platforms;

public class CircularPlatformComp : BaseMovingObjectControllerComp<CircularPlatform>
{
    public float RadiusX => ComponentData.RadiusX;
    public float RadiusY => ComponentData.RadiusY;
    public float FullTurnTime => ComponentData.FullTurnTime;
    public bool CounterClockwise => ComponentData.CounterClockwise;

    private MovingPlatformCollider _collider;

    public override void InitializeComponent()
    {
        Movement = new Platform_Circular_Movement(RadiusX, RadiusY, FullTurnTime, CounterClockwise);

        Movement.Init(
            new vector3(Position.X, Position.Y, Position.Z),
            true, Room.Time, InitialProgressRatio
        );

        _collider = new MovingPlatformCollider(
            Id,
            Position.ToUnityVector3(),
            new Rect(Rectangle.X, Rectangle.Y, Rectangle.Width, Rectangle.Height),
            ParentPlane,
            Room
         );

        base.InitializeComponent();
    }

    public override void Update()
    {
        base.Update();
        _collider.Position = Position.ToUnityVector3();
    }
}
