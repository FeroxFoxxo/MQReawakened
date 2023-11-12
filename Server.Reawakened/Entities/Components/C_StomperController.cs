namespace Server.Reawakened.Entities.Components;

public class C_StomperController : AC_MovingObjectController<StomperController>
{
    public float WaitTimeUp => ComponentData.WaitTimeUp;
    public float WaitTimeDown => ComponentData.WaitTimeDown;
    public float DownMoveTime => ComponentData.DownMoveTime;
    public float UpMoveTime => ComponentData.UpMoveTime;
    public float VerticalDistance => ComponentData.VerticalDistance;
    public bool Hazard => ComponentData.Hazard;

    public override void InitializeComponent()
    {
        Movement = new Stomper_Movement(DownMoveTime, WaitTimeDown, UpMoveTime, WaitTimeUp, VerticalDistance);

        Movement.Init(
            new vector3(Position.X, Position.Y, Position.Z),
            Movement.Activated, Room.Time, InitialProgressRatio
        );

        base.InitializeComponent();
    }

    public override void Update()
    {
        base.Update();
        var movement = (Stomper_Movement)Movement;
        movement.UpdateState(Room.Time);
    }
}
