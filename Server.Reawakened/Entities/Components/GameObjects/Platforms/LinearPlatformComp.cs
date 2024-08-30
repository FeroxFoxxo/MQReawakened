using Server.Reawakened.Entities.Colliders;
using Server.Reawakened.Entities.Components.GameObjects.Platforms.Abstractions;

namespace Server.Reawakened.Entities.Components.GameObjects.Platforms;

public class LinearPlatformComp : BaseMovingObjectControllerComp<LinearPlatform>
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

    private MovingPlatformCollider _collider;
    public override void InitializeComponent()
    {
        var distance = new vector3(DistanceX, DistanceY, DistanceZ);

        Movement = new Platform_Linear_Movement(WaitTime, distance, DistanceTime, DelayBeforeStart, SmoothMove,
            StayHalfwayWhileTriggered, StopIfNotTriggered);

        Movement.Init(
            Position.ToVector3(),
            Movement.Activated, Room.Time, InitialProgressRatio
        );

        _collider = new MovingPlatformCollider(
            Id,
            Position.ToUnityVector3(),
            Rectangle.ToRect(),
            ParentPlane,
            Room
         );

        Room.AddCollider(_collider);

        base.InitializeComponent();
    }

    public override void Update()
    {
        base.Update();
        var movement = (Platform_Linear_Movement)Movement;
        movement.UpdateState(Room.Time);

        if (!ComponentData.TriggeredBySwitch)
            movement.Activate(Room.Time);

        _collider.Position = Position.ToUnityVector3();
    }
}
