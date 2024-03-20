using A2m.Server;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Entities.AbstractComponents;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities.ColliderType;
using static Stomper_Movement;

namespace Server.Reawakened.Entities.Components;

public class StomperControllerComp : BaseMovingObjectControllerComp<StomperController>
{
    public float WaitTimeUp => ComponentData.WaitTimeUp;
    public float WaitTimeDown => ComponentData.WaitTimeDown;
    public float DownMoveTime => ComponentData.DownMoveTime;
    public float UpMoveTime => ComponentData.UpMoveTime;
    public float VerticalDistance => ComponentData.VerticalDistance;
    public bool Hazard => ComponentData.Hazard;

    private float _firstStep;
    private float _secondStep;
    private float _thirdStep;
    private float _fullBehaviorTime;

    public StomperState State;
    public bool ApplyStompDamage = true;

    public TimerThread TimerThread { get; set; }

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

        Room.Colliders.Add(Id, new HazardEffectCollider(Id, Position, Rectangle, ParentPlane, Room));
    }

    public override void Update()
    {
        base.Update();
        
        var movement = (Stomper_Movement)Movement;
        movement.GetBehaviorRatio(Room.Time);

        State = GetState(Room.Time);

        if (Id == "538")
            Console.WriteLine("St: " + Room.GetEntitiesFromType<StomperControllerComp>().First().State);
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
