using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms.Extensions;
using System;
using UnityEngine;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Base.States;
public class AIStatePatrolComp : BaseAIState<AIStatePatrol>
{
    public override string StateName => "AIStatePatrol";

    private Vector2 StartPatrol1 => ComponentData.Patrol1;
    private Vector2 StartPatrol2 => ComponentData.Patrol2;

    public int SinusPathNbHalfPeriod => ComponentData.SinusPathNbHalfPeriod;
    public float MovementSpeed => ComponentData.MovementSpeed;
    public float IdleDurationAtTurnAround => ComponentData.IdleDurationAtTurnAround;
    public float DetectionRange => ComponentData.DetectionRange;
    public float MinimumRange => ComponentData.MinimumRange;
    public float MinimumTimeBeforeDetection => ComponentData.MinimumTimeBeforeDetection;
    public float MaximumYDifferenceOnDetection => ComponentData.MaximumYDifferenceOnDetection;
    public bool RayCastDetection => ComponentData.RayCastDetection;
    public bool DetectOnlyInPatrolZone => ComponentData.DetectOnlyInPatrolZone;
    public float PatrolZoneSizeOffset => ComponentData.PatrolZoneSizeOffset;

    public Vector2 Patrol1;
    public Vector2 Patrol2;

    private AI_State_Patrol _state;
    private static Vector3 _dampingVelocity = new (0, 0, 0);

    public ILogger<AIStatePatrolComp> Logger { get; set; }

    public IAIState DetectionAiState;
    public int ForceDirectionX = 0;

    private float _lastUpdateTime = 0f;
    private float _deltaTime = 0f;

    public override void StartState()
    {
        Position.SetPositionViaPlane(ParentPlane, PrefabName, Logger);

        _lastUpdateTime = Room.Time;

        Patrol1 = StartPatrol1;
        Patrol2 = StartPatrol2;

        if (Patrol1.x > 0f && Patrol2.x > 0f || Patrol1.x < 0f && Patrol2.x < 0f)
        {
            Patrol1.x = (!(Patrol1.x > 0f)) ? Mathf.Min(Patrol1.x, Patrol2.x) : Mathf.Max(Patrol1.x, Patrol2.x);
            Patrol2.x = 0f;
        }

        if (Patrol1.y > 0f && Patrol2.y > 0f || Patrol1.y < 0f && Patrol2.y < 0f)
        {
            Patrol1.y = (!(Patrol1.y > 0f)) ? Mathf.Min(Patrol1.y, Patrol2.y) : Mathf.Max(Patrol1.y, Patrol2.y);
            Patrol2.y = 0f;
        }

        var movementX = (Mathf.Abs(Patrol1.x) + Mathf.Abs(Patrol2.x)) * ((!(Patrol1.x > Patrol2.x)) ? (-1f) : 1f);
        var movementY = (Mathf.Abs(Patrol1.y) + Mathf.Abs(Patrol2.y)) * ((!(Patrol1.y > Patrol2.y)) ? (-1f) : 1f);
        var length = new Vector2(movementX, movementY).magnitude / MovementSpeed;

        _state = new AI_State_Patrol(
        [
            new (IdleDurationAtTurnAround, "Idle"),
            new (length, "Move"),
            new (IdleDurationAtTurnAround, "Idle2"),
            new (length, "Move2"),
            new (0f, "Attack")
        ], movementX, movementY, SinusPathNbHalfPeriod);

        _state.Init(Position.ToVector3());
    }

    public override ExtLevelEditor.ComponentSettings GetSettings() =>
        [Position.X.ToString(), Position.Y.ToString(), Position.Z.ToString()];

    public Player GetClosestPlayer() => Room.GetClosetPlayer(Position.ToUnityVector3(), DetectionRange);

    public override void UpdateState()
    {
        _deltaTime = Room.Time - _lastUpdateTime;
        _lastUpdateTime = Room.Time;

        var inPosition = Position.ToVector3();
        var currentPosition = _state.GetCurrentPosition();
        var dampenedPosition = new vector3(0f, 0f, 0f);
        var springK = 200f;

        MathUtils.CriticallyDampedSpring1D(springK, inPosition.x, currentPosition.x, ref _dampingVelocity.x, ref dampenedPosition.x, _deltaTime);
        MathUtils.CriticallyDampedSpring1D(springK, inPosition.y, currentPosition.y, ref _dampingVelocity.y, ref dampenedPosition.y, _deltaTime);
        MathUtils.CriticallyDampedSpring1D(springK, inPosition.z, currentPosition.z, ref _dampingVelocity.z, ref dampenedPosition.z, _deltaTime);

        Position.SetPosition(dampenedPosition);

        ForceDirectionX = _state.GetDirection();

        if (DetectionAiState == null)
            return;

        if (GetClosestPlayer() == null)
            return;
        
        AddNextState(DetectionAiState.GetType());
        GoToNextState();
    }
}
