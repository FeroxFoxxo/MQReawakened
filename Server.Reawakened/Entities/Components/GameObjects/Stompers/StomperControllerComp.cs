using Server.Base.Timers.Services;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Entities.Colliders;
using Server.Reawakened.Entities.Components.GameObjects.Platforms.Abstractions;
using UnityEngine;

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
    public TimerThread TimerThread { get; set; }
    public ServerRConfig ServerRConfig { get; set; }

    public override void InitializeComponent()
    {
        Movement = new Stomper_Movement(DownMoveTime, WaitTimeDown, UpMoveTime, WaitTimeUp, VerticalDistance);
        Movement.Init(
            Position.ToVector3(),
            true, 0, InitialProgressRatio
        );
        Movement.Activate(Room.Time);

        _collider = new StomperZoneCollider(this);

        base.InitializeComponent();
    }

    public override void Update()
    {
        if (Room == null)
            return;

        base.Update();

        var movement = (Stomper_Movement)Movement;
        movement.UpdateState(Room.Time);

        if (movement.CurrentStep == Stomper_Movement.StomperState.WaitDown)
            _collider.RunCollisionDetection();
    }
}
