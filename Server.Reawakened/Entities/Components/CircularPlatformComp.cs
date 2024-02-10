using Server.Reawakened.Entities.AbstractComponents;
using Server.Reawakened.Rooms.Models.Entities;

namespace Server.Reawakened.Entities.Components;

public class CircularPlatformComp : MovingObjectControllerComp<CircularPlatform>
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
