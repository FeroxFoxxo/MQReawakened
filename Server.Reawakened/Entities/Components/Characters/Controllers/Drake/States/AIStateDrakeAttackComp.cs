using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Drake.States;
public class AIStateDrakeAttackComp : BaseAIState<AIStateDrakeAttack, AI_State_DrakeAttack>
{
    public override string StateName => "AIStateDrakeAttack";

    public float RamSpeed => ComponentData.RamSpeed;
    public float AttackOutAnimDuration => ComponentData.AttackOutAnimDuration;
    public float StunDuration => ComponentData.StunDuration;
    public float FleeSpeed => ComponentData.FleeSpeed;
    public float ReloadDuration => ComponentData.ReloadDuration;
    public float TauntAnimDuration => ComponentData.TauntAnimDuration;

    private readonly vector3 _dampingVelocity = new(0f, 0f, 0f);

    // TODO!!!
    public static vector3 PlacementPosition => new(0f, 0f, 0f);
    // TODO!!!
    public static vector3 TargetPosition => new(0f, 0f, 0f);

    public override AI_State_DrakeAttack GetInitialAIState() => new (
        [
            new AIDataEvent(0f, "AttackLoop"),
            new AIDataEvent(AttackOutAnimDuration, "AttackOut"),
            new AIDataEvent(StunDuration, "Stun"),
            new AIDataEvent(0f, "Flee"),
            new AIDataEvent(ReloadDuration, "Reload"),
            new AIDataEvent(TauntAnimDuration, "Taunt")
        ], FleeSpeed, RamSpeed);

    public override ExtLevelEditor.ComponentSettings GetSettings() =>
        [
            Position.X.ToString(), Position.Y.ToString(), Position.Z.ToString(),
            PlacementPosition.x.ToString(), PlacementPosition.y.ToString(), PlacementPosition.z.ToString(),
            TargetPosition.x.ToString(), TargetPosition.y.ToString(), TargetPosition.z.ToString()
        ];

    public override void OnAIStateIn() =>
        State.Init(Position.ToVector3(), PlacementPosition, TargetPosition);

    public override void Execute()
    {
        var currentPosition = State.CurrentPosition;

        if (currentPosition != null)
        {
            var position = Position.ToVector3();
            var dampenedPosition = new vector3(0f, 0f, 0f);
            var springK = 200f;

            MathUtils.CriticallyDampedSpring1D(springK, position.x, currentPosition.x, ref _dampingVelocity.x, ref dampenedPosition.x, Room.DeltaTime);
            MathUtils.CriticallyDampedSpring1D(springK, position.y, currentPosition.y, ref _dampingVelocity.y, ref dampenedPosition.y, Room.DeltaTime);
            MathUtils.CriticallyDampedSpring1D(springK, position.z, currentPosition.z, ref _dampingVelocity.z, ref dampenedPosition.z, Room.DeltaTime);

            Position.SetPosition(dampenedPosition);
            StateMachine.SetForceDirectionX(State.Direction);
        }
    }

    public void AttackLoop() => Logger.LogTrace("AttackLoop called for {StateName} on {PrefabName}", StateName, PrefabName);

    public void AttackOut()
    {
        Logger.LogTrace("AttackOut called for {StateName} on {PrefabName}", StateName, PrefabName);

        (StateMachine as DrakeEnemyControllerComp).IsSpinning = false;
        (StateMachine as DrakeEnemyControllerComp).IsImmune = false;
    }

    public void Stun() => Logger.LogTrace("Stun called for {StateName} on {PrefabName}", StateName, PrefabName);

    public void Flee() => Logger.LogTrace("Flee called for {StateName} on {PrefabName}", StateName, PrefabName);

    public void Reload()
    {
        Logger.LogTrace("Reload called for {StateName} on {PrefabName}", StateName, PrefabName);

        (StateMachine as DrakeEnemyControllerComp).IsImmune = true;
    }

    public void Taunt() => Logger.LogTrace("Taunt called for {StateName} on {PrefabName}", StateName, PrefabName);
}
