using Server.Reawakened.Entities.AbstractComponents;

namespace Server.Reawakened.Entities.Components;

public class StomperControllerComp : MovingObjectControllerComp<StomperController>
{
    public float WaitTimeUp => ComponentData.WaitTimeUp;
    public float WaitTimeDown => ComponentData.WaitTimeDown;
    public float DownMoveTime => ComponentData.DownMoveTime;
    public float UpMoveTime => ComponentData.UpMoveTime;
    public float VerticalDistance => ComponentData.VerticalDistance;
    public bool Hazard => ComponentData.Hazard;
    public enum StomperState
    {
        WaitUp,
		GoingDown,
		WaitDown,
		GoingUp
    }
    private float _firstStep;
    private float _secondStep;
    private float _thirdStep;
    private float _fullBehaviorTime;

    public override void InitializeComponent()
    {
        _firstStep = WaitTimeUp;
        _secondStep = _firstStep + DownMoveTime;
        _thirdStep = _secondStep + WaitTimeDown;
        _fullBehaviorTime = _thirdStep + UpMoveTime;
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
        // Don't touch this, it's debug!
        //if (Id == 340)
        //{
        //    Console.WriteLine(GetState(Room.Time));
        //    Console.WriteLine(Room.Time);
        //}
        movement.GetBehaviorRatio(Room.Time);
    }

    public StomperState GetState(float time)
    {
        var state = StomperState.WaitUp;
        var progressRatio = time % _fullBehaviorTime;
        if (progressRatio >= _firstStep && progressRatio <= _secondStep)
            state = StomperState.GoingDown;
        else if (progressRatio >= _secondStep && progressRatio <= _thirdStep)
            state = StomperState.WaitDown;
        else if (progressRatio >= _thirdStep && progressRatio <= _fullBehaviorTime)
            state = StomperState.GoingUp;
        return state;
    }
}
