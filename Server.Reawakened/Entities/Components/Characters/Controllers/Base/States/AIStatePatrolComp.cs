using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using Server.Reawakened.Entities.Components.Characters.Controllers.Hamster.States;
using Server.Reawakened.Entities.Components.Characters.Controllers.Spiker.States;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms.Extensions;
using UnityEngine;
using static A2m.Server.ExtLevelEditor;

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
    public float DetectionStartTime = 0f;
    private bool _isDetecting = false;

    private float _spawnX;
    private float _spawnY;
    private float _spawnZ;

    private bool _initialized = false;

    public override void DelayedComponentInitialization()
    {
        base.InitializeComponent();

        _spawnX = Position.X;
        _spawnY = Position.Y;
        _spawnZ = Position.Z;
    }

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

    public override ComponentSettings GetSettings() => _initialized ? [] :
        [_spawnX.ToString(), _spawnY.ToString(), _spawnZ.ToString()];

    public override void OnAIStateIn()
    {
        DetectionStartTime = Room.Time;
        _isDetecting = false;

        if (!_initialized)
        {
            _initialized = true;
            State.Init(new vector3(_spawnX, _spawnY, _spawnZ));
        }
    }

    public Player GetClosestPlayer() => Room.GetClosestPlayer(Position.ToUnityVector3(), DetectionRange);

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

        if (hasDetectedPlayer && Room.Time - DetectionStartTime >= MinimumTimeBeforeDetection)
        {
            StartDetectedState();
        }
    }

    public void StartDetectedState()
    {
        var player = GetClosestPlayer();

        if (player != null)
        {
            var dirX = player.TempData.Position.X - Position.X;
            StateMachine.SetForceDirectionX(Math.Sign(dirX));

            var attackComp = DetectionAiState;

            switch (attackComp)
            {
                case AIStateHamsterAttackComp comp:
                    Room.GetCollidersById(Id).FirstOrDefault()?.Active = true;
                    comp.TargetPosition = player.TempData.Position.ToVector3();
                    break;
                case AIStateSpikerAttackComp:
                    Room.GetCollidersById(Id).FirstOrDefault()?.Active = true;
                    break;
            }
        }

        var move = Room.GetEntityFromId<AIStateMoveComp>(Id);
            
        if (move != null)
            move.TargetPosition = new vector3(_spawnX, _spawnY, _spawnZ);
        
        AddNextState(DetectionAiState.GetType());
        GoToNextState();
    }

    private bool CanDetectPlayer(Player player)
    {
        var playerPos = player.TempData.Position;
        var enemyPos = Position.ToUnityVector3();

        if (Mathf.Abs(playerPos.Y - enemyPos.y) > MaximumYDifferenceOnDetection)
            return false;

        var distance = Vector3.Distance(enemyPos, playerPos.ToUnityVector3());

        return distance >= MinimumRange && distance <= DetectionRange && (!DetectOnlyInPatrolZone || IsPlayerInPatrolZone(player));
    }

    private bool IsPlayerInPatrolZone(Player player)
    {
        var playerPos = player.TempData.Position;

        var minX = Mathf.Min(_spawnX + Patrol1.x, _spawnX + Patrol2.x) - PatrolZoneSizeOffset;
        var maxX = Mathf.Max(_spawnX + Patrol1.x, _spawnX + Patrol2.x) + PatrolZoneSizeOffset;
        var minY = Mathf.Min(_spawnY + Patrol1.y, _spawnY + Patrol2.y) - PatrolZoneSizeOffset;
        var maxY = Mathf.Max(_spawnY + Patrol1.y, _spawnY + Patrol2.y) + PatrolZoneSizeOffset;

        return playerPos.X >= minX && playerPos.X <= maxX && playerPos.Y >= minY && playerPos.Y <= maxY;
    }
}
