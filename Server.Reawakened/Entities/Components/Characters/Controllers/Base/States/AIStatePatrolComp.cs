using A2m.Server;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms.Extensions;
using UnityEngine;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Base.States;
public class AIStatePatrolComp : BaseAIState<AIStatePatrol, AI_State_Patrol>
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
    private Vector3 _dampingVelocity = new(0, 0, 0);

    public IAIState DetectionAiState;
    private float _detectionStartTime = 0f;
    private bool _isDetecting = false;

    public override AI_State_Patrol GetInitialAIState()
    {
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

        return new AI_State_Patrol(
        [
            new (IdleDurationAtTurnAround, "Idle"),
            new (length, "Move"),
            new (IdleDurationAtTurnAround, "Idle2"),
            new (length, "Move2"),
            new (0f, "Attack")
        ], movementX, movementY, SinusPathNbHalfPeriod);
    }

    public override ExtLevelEditor.ComponentSettings GetSettings() =>
        [Position.X.ToString(), Position.Y.ToString(), Position.Z.ToString()];

    public override void OnAIStateIn()
    {
        _detectionStartTime = 0f;
        _isDetecting = false;

        State.Init(Position.ToVector3());
    }

    public Player GetClosestPlayer() => Room.GetClosetPlayer(Position.ToUnityVector3(), DetectionRange);

    public override void Execute()
    {
        var inPosition = Position.ToVector3();
        var currentPosition = State.GetCurrentPosition();
        var dampenedPosition = new vector3(0f, 0f, 0f);
        var springK = 200f;

        MathUtils.CriticallyDampedSpring1D(springK, inPosition.x, currentPosition.x, ref _dampingVelocity.x, ref dampenedPosition.x, Room.DeltaTime);
        MathUtils.CriticallyDampedSpring1D(springK, inPosition.y, currentPosition.y, ref _dampingVelocity.y, ref dampenedPosition.y, Room.DeltaTime);
        MathUtils.CriticallyDampedSpring1D(springK, inPosition.z, currentPosition.z, ref _dampingVelocity.z, ref dampenedPosition.z, Room.DeltaTime);

        Position.SetPosition(dampenedPosition);

        StateMachine.SetForceDirectionX(State.GetDirection());

        if (DetectionAiState == null)
            return;

        var hasDetectedPlayer = Room.GetPlayers().Any(CanDetectPlayer);

        if (hasDetectedPlayer)
        {
            if (!_isDetecting)
            {
                _isDetecting = true;
                _detectionStartTime = Room.Time;
            }
            else if (Room.Time - _detectionStartTime >= MinimumTimeBeforeDetection)
            {
                AddNextState(DetectionAiState.GetType());
                GoToNextState();
            }
        } else {
            _isDetecting = false;
            _detectionStartTime = 0f;
        }
    }

    private bool CanDetectPlayer(Player player)
    {
        var playerPos = player.TempData.Position;
        var enemyPos = Position.ToUnityVector3();

        if (Mathf.Abs(playerPos.y - enemyPos.y) > MaximumYDifferenceOnDetection)
            return false;

        var distance = Vector3.Distance(enemyPos, playerPos);

        if (distance < MinimumRange || distance > DetectionRange)
            return false;

        if (DetectOnlyInPatrolZone && !IsPlayerInPatrolZone(player))
            return false;

        return true;
    }

    private bool IsPlayerInPatrolZone(Player player)
    {
        var playerPos = player.TempData.Position;
        var minX = Mathf.Min(Position.X + Patrol1.x, Position.X + Patrol2.x) - PatrolZoneSizeOffset;
        var maxX = Mathf.Max(Position.X + Patrol1.x, Position.X + Patrol2.x) + PatrolZoneSizeOffset;
        var minY = Mathf.Min(Position.Y + Patrol1.y, Position.Y + Patrol2.y) - PatrolZoneSizeOffset;
        var maxY = Mathf.Max(Position.Y + Patrol1.y, Position.Y + Patrol2.y) + PatrolZoneSizeOffset;

        return playerPos.x >= minX && playerPos.x <= maxX && playerPos.y >= minY && playerPos.y <= maxY;
    }
}
