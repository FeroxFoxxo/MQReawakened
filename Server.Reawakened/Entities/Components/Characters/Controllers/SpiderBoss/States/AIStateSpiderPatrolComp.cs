using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using UnityEngine;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.SpiderBoss.States;
public class AIStateSpiderPatrolComp : BaseAIState<AIStateSpiderPatrol, AI_State_Patrol>
{
    public override string StateName => "AIStateSpiderPatrol";

    public float[] MovementSpeed => ComponentData.MovementSpeed;
    public float[] IdleDurationAtTurnAround => ComponentData.IdleDurationAtTurnAround;
    public float FromY => ComponentData.FromY;
    public float ToY => ComponentData.ToY;
    public float[] MinPatrolTime => ComponentData.MinPatrolTime;
    public float[] MaxPatrolTime => ComponentData.MaxPatrolTime;

    private readonly vector3 _dampingVelocity = new(0f, 0f, 0f);

    public override AI_State_Patrol GetInitialAIState() => new (
        [
            new (0f, "Idle"),
            new (0f, "Move"),
            new (0f, "Idle2"),
            new (0f, "Move2")
        ], 0f, ToY - FromY, 0);


    public override ExtLevelEditor.ComponentSettings GetSettings() => [Position.X.ToString(), Position.Y.ToString(), Position.Z.ToString()];

    public override void StateIn()
    {
        var spiderBossController = (SpiderBossController)StateMachine;
        var length = Mathf.Abs(ToY - FromY) / MovementSpeed[spiderBossController.CurrentPhase];
        State.SetTime("Move", length);
        State.SetTime("Move2", length);
        State.SetTime("Idle", IdleDurationAtTurnAround[spiderBossController.CurrentPhase]);
        State.SetTime("Idle2", IdleDurationAtTurnAround[spiderBossController.CurrentPhase]);
        State.RecalculateTimes();
    }

    public override void Execute()
    {
        var position = Position.ToVector3();
        var currentPosition = State.GetCurrentPosition();
        var dampenedPosition = new vector3(0f, 0f, 0f);
        var springK = 200f;
        MathUtils.CriticallyDampedSpring1D(springK, position.x, currentPosition.x, ref _dampingVelocity.x, ref dampenedPosition.x, Room.DeltaTime);
        MathUtils.CriticallyDampedSpring1D(springK, position.y, currentPosition.y, ref _dampingVelocity.y, ref dampenedPosition.y, Room.DeltaTime);
        MathUtils.CriticallyDampedSpring1D(springK, position.z, currentPosition.z, ref _dampingVelocity.z, ref dampenedPosition.z, Room.DeltaTime);
        Position.SetPosition(dampenedPosition.x, dampenedPosition.y, dampenedPosition.z);
        StateMachine.SetForceDirectionX(State.GetDirection());
    }

    public void Move() => Logger.LogTrace("Move called for {StateName} on {PrefabName}", StateName, PrefabName);
    public void Move2() => Logger.LogTrace("Move2 called for {StateName} on {PrefabName}", StateName, PrefabName);
    public void Idle() => Logger.LogTrace("Idle called for {StateName} on {PrefabName}", StateName, PrefabName);
    public void Idle2() => Logger.LogTrace("Idle2 called for {StateName} on {PrefabName}", StateName, PrefabName);
}
