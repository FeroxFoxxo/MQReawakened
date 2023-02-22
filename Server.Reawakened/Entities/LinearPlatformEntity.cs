using Server.Reawakened.Entities.Abstractions;

namespace Server.Reawakened.Entities;

public class LinearPlatformEntity : AbstractMovingObject<LinearPlatform>
{
    public float WaitTime => EntityData.WaitTime;
    public float DistanceX => EntityData.DistanceX;
    public float DistanceY => EntityData.DistanceY;
    public float DistanceZ => EntityData.DistanceZ;
    public float DistanceTime => EntityData.DistanceTime;
    public bool SmoothMove => EntityData.SmoothMove;
    public bool StayHalfwayWhileTriggered => EntityData.StayHalfwayWhileTriggered;
    public bool StopIfNotTriggered => EntityData.StopIfNotTriggered;
    public float DelayBeforeStart => EntityData.DelayBeforeStart;
    public bool TriggeredBySwitch => EntityData.TriggeredBySwitch;

    public override void InitializeEntity()
    {
        var distance = new vector3(DistanceX, DistanceY, DistanceZ);

        Movement = new Platform_Linear_Movement(WaitTime, distance, DistanceTime, DelayBeforeStart, SmoothMove,
            StayHalfwayWhileTriggered, StopIfNotTriggered);

        Movement.Init(
            new vector3(Position.X, Position.Y, Position.Z),
            Movement.Activated, Room.Time, InitialProgressRatio
        );

        base.InitializeEntity();
    }

    public override void Update()
    {
        base.Update();
        var movement = (Platform_Linear_Movement)Movement;
        movement.UpdateState(Room.Time);
    }
}
