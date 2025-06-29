using A2m.Server;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using Server.Reawakened.Entities.DataComponentAccessors.Base.States;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Base.States;
public class AIStateMoveComp : BaseAIState<AIStateMoveMQR, AI_State_Move>
{
    public override string StateName => "AIStateMove";

    public float MovementSpeed => ComponentData.MovementSpeed;

    private readonly vector3 _dampingVelocity = new(0f, 0f, 0f);

    // TODO!!!
    public vector3 TargetPosition = new(0f, 0f, 0f);

    public override AI_State_Move GetInitialAIState() => new (MovementSpeed);

    public override ExtLevelEditor.ComponentSettings GetSettings() => [
        Position.X.ToString(), Position.Y.ToString(), Position.Z.ToString(),
        TargetPosition.x.ToString(), TargetPosition.y.ToString(), TargetPosition.z.ToString()
    ];

    public override void OnAIStateIn()
    {
        State.Init(Position.ToVector3(), TargetPosition);
        StateMachine.SetForceDirectionX(Convert.ToInt32(TargetPosition.x - Position.X));
    }

    public override void Execute()
    {
        var position = Position.ToVector3();
        var currentPosition = State.CurrentPosition;
        var dampenedPosition = new vector3(0f, 0f, 0f);
        var springK = 200f;

        MathUtils.CriticallyDampedSpring1D(springK, position.x, currentPosition.x, ref _dampingVelocity.x, ref dampenedPosition.x, Room.DeltaTime);
        MathUtils.CriticallyDampedSpring1D(springK, position.y, currentPosition.y, ref _dampingVelocity.y, ref dampenedPosition.y, Room.DeltaTime);
        MathUtils.CriticallyDampedSpring1D(springK, position.z, currentPosition.z, ref _dampingVelocity.z, ref dampenedPosition.z, Room.DeltaTime);

        Position.SetPosition(dampenedPosition);
    }
}
