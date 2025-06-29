using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Drake.States;
public class AIStateDrakePlacementComp : BaseAIState<AIStateDrakePlacement, AI_State_DrakePlacement>
{
    public override string StateName => "AIStateDrakePlacement";

    public float MovementSpeed => ComponentData.MovementSpeed;
    public float AttackInAnimDuration => ComponentData.AttackInAnimDuration;
    public float AttackLoopAnimDuration => ComponentData.AttackLoopAnimDuration;
    public float AttackRange => ComponentData.AttackRange;
    public float AttackRangeMaximum => ComponentData.AttackRangeMaximum;

    private readonly vector3 _dampingVelocity = new (0f, 0f, 0f);

    // TODO!!!
    public readonly vector3 PlacementPosition = new (0f, 0f, 0f);

    public override AI_State_DrakePlacement GetInitialAIState() => new(
        [
            new AIDataEvent(0f, "Placement"),
            new AIDataEvent(AttackInAnimDuration, "AttackIn"),
            new AIDataEvent(AttackLoopAnimDuration, "AttackLoop")
        ], MovementSpeed);

    public override ExtLevelEditor.ComponentSettings GetSettings() => [
        Position.X.ToString(), Position.Y.ToString(), Position.Z.ToString(),
        PlacementPosition.x.ToString(), PlacementPosition.y.ToString(), PlacementPosition.z.ToString()
    ];

    public override void OnAIStateIn() =>
        State.Init(Position.ToVector3(), PlacementPosition);

    public override void Execute()
    {
        var currentPosition = State.CurrentPosition;

        if (currentPosition != null)
        {
            var position = Position.ToVector3();
            var dampingPosition = new vector3(0f, 0f, 0f);
            var springK = 200f;

            MathUtils.CriticallyDampedSpring1D(springK, position.x, State.CurrentPosition.x, ref _dampingVelocity.x, ref dampingPosition.x, Room.DeltaTime);
            MathUtils.CriticallyDampedSpring1D(springK, position.y, State.CurrentPosition.y, ref _dampingVelocity.y, ref dampingPosition.y, Room.DeltaTime);
            MathUtils.CriticallyDampedSpring1D(springK, position.z, State.CurrentPosition.z, ref _dampingVelocity.z, ref dampingPosition.z, Room.DeltaTime);
            
            Position.SetPosition(dampingPosition);
            StateMachine.SetForceDirectionX(State.Direction);
        }
    }

    public void Placement() => Logger.LogTrace("Placement called for {StateName} on {PrefabName}", StateName, PrefabName);

    public void AttackIn()
    {
        Logger.LogTrace("AttackIn called for {StateName} on {PrefabName}", StateName, PrefabName);

        (StateMachine as DrakeEnemyControllerComp).IsSpinning = true;
    }

    public void AttackLoop() => Logger.LogTrace("AttackLoop called for {StateName} on {PrefabName}", StateName, PrefabName);
}
