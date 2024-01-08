using Server.Reawakened.Entities.AbstractComponents;

namespace Server.Reawakened.Entities.Components;

public class LinearPlatformComp : MovingObjectControllerComp<LinearPlatform>
{
    public float WaitTime => ComponentData.WaitTime;
    public float DistanceX => ComponentData.DistanceX;
    public float DistanceY => ComponentData.DistanceY;
    public float DistanceZ => ComponentData.DistanceZ;
    public float DistanceTime => ComponentData.DistanceTime;
    public bool SmoothMove => ComponentData.SmoothMove;
    public bool StayHalfwayWhileTriggered => ComponentData.StayHalfwayWhileTriggered;
    public bool StopIfNotTriggered => ComponentData.StopIfNotTriggered;
    public float DelayBeforeStart => ComponentData.DelayBeforeStart;
    public bool TriggeredBySwitch => ComponentData.TriggeredBySwitch;

    public override void InitializeComponent()
    {
        var distance = new vector3(DistanceX, DistanceY, DistanceZ);

        Movement = new Platform_Linear_Movement(WaitTime, distance, DistanceTime, DelayBeforeStart, SmoothMove,
            StayHalfwayWhileTriggered, StopIfNotTriggered);

        Movement.Init(
            new vector3(Position.X, Position.Y, Position.Z),
            Movement.Activated, Room.Time, InitialProgressRatio
        );

        base.InitializeComponent();
    }

    public override void Update()
    {
        base.Update();
        var movement = (Platform_Linear_Movement)Movement;
        movement.UpdateState(Room.Time);

        if (!ComponentData.TriggeredBySwitch)
            movement.Activate(Room.Time);
    }
}
