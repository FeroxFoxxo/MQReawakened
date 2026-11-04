using Server.Base.Timers.Services;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Entities.Colliders;
using Server.Reawakened.Entities.Components.GameObjects.Platforms.Abstractions;

namespace Server.Reawakened.Entities.Components.GameObjects.Stompers;

public class StomperControllerComp : BaseMovingObjectControllerComp<StomperController>
{
    public float WaitTimeUp => ComponentData.WaitTimeUp;
    public float WaitTimeDown => ComponentData.WaitTimeDown;
    public float DownMoveTime => ComponentData.DownMoveTime;
    public float UpMoveTime => ComponentData.UpMoveTime;
    public float VerticalDistance => ComponentData.VerticalDistance;
    public bool Hazard => ComponentData.Hazard;

    private StomperZoneCollider _collider;
    private Stomper_Movement _stomperMovement;

    public override void InitializeComponent()
    {
        _stomperMovement = new Stomper_Movement(
            DownMoveTime,
            WaitTimeDown,
            UpMoveTime,
            WaitTimeUp,
            VerticalDistance
        );
        Movement = _stomperMovement;

        _stomperMovement.Init(
            Position.ToVector3(),
            true,
            0f,
            InitialProgressRatio
        );
        _stomperMovement.Activate(Room.Time);

        _collider = new StomperZoneCollider(this);

        base.InitializeComponent();
    }

    public override void Update()
    {
        var room = Room;
        if (room == null || _stomperMovement == null)
            return;

        base.Update();

        var movement = (Stomper_Movement) Movement;

        if (movement == null || Room == null)
            return;

        movement.UpdateState(Room.Time);

        if (movement.CurrentStep == Stomper_Movement.StomperState.WaitDown)
            _collider.RunCollisionDetection();
    }
}
