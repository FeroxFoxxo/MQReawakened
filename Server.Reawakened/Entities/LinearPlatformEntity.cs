using Server.Base.Network;

namespace Server.Reawakened.Entities;

public class LinearPlatformEntity : MovingObjectControllerEntity<LinearPlatform>
{
    public override void InitializeEntity()
    {
        var distance = new vector3(EntityData.DistanceX, EntityData.DistanceY, EntityData.DistanceZ);

        Movement = new Platform_Linear_Movement(EntityData.WaitTime, distance, EntityData.DistanceTime,
            EntityData.DelayBeforeStart, EntityData.SmoothMove, EntityData.StayHalfwayWhileTriggered,
            EntityData.StopIfNotTriggered);

        Movement.Init(
            new vector3(Position.X, Position.Y, Position.Z),
            true, Level.Time, EntityData.InitialProgressRatio
        );
    }

    public override object[] GetInitData(NetState netState) => new object[]
    {
        Level.Time,
        Movement.GetBehaviorRatio(Level.Time),
        Movement.Activated
    };

    public override void Update()
    {
        base.Update();
        var movement = (Platform_Linear_Movement) Movement;
        movement.UpdateState(Level.Time);
    }
}
