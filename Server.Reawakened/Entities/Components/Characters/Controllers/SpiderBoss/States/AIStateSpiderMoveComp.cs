using A2m.Server;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using Server.Reawakened.Entities.DataComponentAccessors.SpiderBoss.States;
using UnityEngine;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.SpiderBoss.States;
public class AIStateSpiderMoveComp : BaseAIState<AIStateSpiderMoveMQR, AI_State_Move>
{
    public override string StateName => "AIStateSpiderMove";

    public float[] MovementSpeed => ComponentData.MovementSpeed;
    public float CeilingY => ComponentData.CeilingY;
    public float PatrolFromY => ComponentData.PatrolFromY;

    private readonly vector3 _dampingVelocity = new (0f, 0f, 0f);

    // TODO!!
    public Vector3 TargetPosition = new (0f, 0f, 0f);

    public override AI_State_Move GetInitialAIState() => new (0);

    public override ExtLevelEditor.ComponentSettings GetSettings() => [
        Position.X.ToString(), Position.Y.ToString(), Position.Z.ToString(),
        TargetPosition.x.ToString(), TargetPosition.y.ToString(), TargetPosition.z.ToString()
    ];

    public override void Execute()
    {
        var position = Position.ToVector3();
        var currentPosition = State.CurrentPosition;
        var vector5 = new vector3(0f, 0f, 0f);
        var springK = 200f;
        MathUtils.CriticallyDampedSpring1D(springK, position.x, currentPosition.x, ref _dampingVelocity.x, ref vector5.x, Room.DeltaTime);
        MathUtils.CriticallyDampedSpring1D(springK, position.y, currentPosition.y, ref _dampingVelocity.y, ref vector5.y, Room.DeltaTime);
        MathUtils.CriticallyDampedSpring1D(springK, position.z, currentPosition.z, ref _dampingVelocity.z, ref vector5.z, Room.DeltaTime);
        Position.SetPosition(vector5);
    }

    public override void OnAIStateIn() => (StateMachine as SpiderBossControllerComp).OnGround = false;
}
