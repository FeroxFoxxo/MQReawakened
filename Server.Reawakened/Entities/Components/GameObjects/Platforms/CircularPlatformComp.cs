using Server.Reawakened.Entities.AbstractComponents;

namespace Server.Reawakened.Entities.Components.GameObjects.Platforms;

public class CircularPlatformComp : BaseMovingObjectControllerComp<CircularPlatform>
{
    public float RadiusX => ComponentData.RadiusX;
    public float RadiusY => ComponentData.RadiusY;
    public float FullTurnTime => ComponentData.FullTurnTime;
    public bool CounterClockwise => ComponentData.CounterClockwise;

    public override void InitializeComponent()
    {
        Movement = new Platform_Circular_Movement(RadiusX, RadiusY, FullTurnTime, CounterClockwise);

        Movement.Init(
            new vector3(Position.X, Position.Y, Position.Z),
            Movement.Activated, Room.Time, InitialProgressRatio
        );

        base.InitializeComponent();
    }
}
