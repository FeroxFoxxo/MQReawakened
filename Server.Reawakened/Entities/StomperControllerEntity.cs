using Server.Reawakened.Entities.Abstractions;

namespace Server.Reawakened.Entities;

public class StomperControllerEntity : AbstractMovingObject<StomperController>
{
    public float WaitTimeUp => EntityData.WaitTimeUp;
    public float WaitTimeDown => EntityData.WaitTimeDown;
    public float DownMoveTime => EntityData.DownMoveTime;
    public float UpMoveTime => EntityData.UpMoveTime;
    public float VerticalDistance => EntityData.VerticalDistance;
    public bool Hazard => EntityData.Hazard;

    public override void InitializeEntity()
    {
        Movement = new Stomper_Movement(DownMoveTime, WaitTimeDown, UpMoveTime, WaitTimeUp, VerticalDistance);

        Movement.Init(
            new vector3(Position.X, Position.Y, Position.Z),
            Movement.Activated, Room.Time, InitialProgressRatio
        );

        base.InitializeEntity();
    }

    public override void Update()
    {
        base.Update();
        var movement = (Stomper_Movement)Movement;
        movement.UpdateState(Room.Time);
    }
}
