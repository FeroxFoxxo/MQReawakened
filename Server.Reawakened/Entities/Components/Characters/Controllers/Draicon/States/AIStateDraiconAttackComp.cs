using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using UnityEngine;
using static AIStateDraiconAttack;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Draicon.States;
public class AIStateDraiconAttackComp : BaseAIState<AIStateDraiconAttack, AI_State_DraiconAttack>
{
    public override string StateName => "AIStateDraiconAttack";

    public float AttackRecoveryDuration => ComponentData.AttackRecoveryDuration;
    public float AttackRange => ComponentData.AttackRange;
    public float ReloadDuration => ComponentData.ReloadDuration;
    public float MovementSpeed => ComponentData.MovementSpeed;
    public float AttackAnimDuration => ComponentData.AttackAnimDuration;
    public float DelayBeforeFiring => ComponentData.DelayBeforeFiring;
    public float ProjectileSpeed => ComponentData.ProjectileSpeed;
    public ProjectilePatternType ProjectileAmount => ComponentData.ProjectileAmount;

    private readonly vector3 _dampingVelocity = new(0f, 0f, 0f);

    public vector3 TargetPosition = new(0f, 0f, 0f);
    
    private float _projectileAmount = 0;

    public float EndTime = 0f;

    public override AI_State_DraiconAttack GetInitialAIState()
    {
        _projectileAmount = ProjectileAmount switch
        {
            ProjectilePatternType.Projectile8 => 8,
            ProjectilePatternType.Projectile12 => 12,
            ProjectilePatternType.Projectile16 => 16,
            _ => _projectileAmount
        };
        
        return new AI_State_DraiconAttack(
        [
            new AIDataEvent(0f, "Aggro"),
            new AIDataEvent(DelayBeforeFiring, "Attack"),
            new AIDataEvent(AttackAnimDuration - DelayBeforeFiring, "Fire"),
            new AIDataEvent(AttackRecoveryDuration, "Resolution"),
            new AIDataEvent(0f, "Flee"),
            new AIDataEvent(ReloadDuration, "Reload")
        ], MovementSpeed);
    }

    public override ExtLevelEditor.ComponentSettings GetSettings() => [
        Position.X.ToString(), Position.Y.ToString(), Position.Z.ToString(),
        TargetPosition.x.ToString(), TargetPosition.y.ToString(), TargetPosition.z.ToString()
    ];

    public override void OnAIStateIn()
    {
        EndTime = 0f;
        
        State.Init(Position.ToVector3(), TargetPosition);
    }

    public override void Execute()
    {
        var currentPosition = State.GetCurrentPosition();

        if (currentPosition != null)
        {
            var position = Position.ToVector3();
            var dampenedPosition = new vector3(0f, 0f, 0f);
            var springK = 200f;

            MathUtils.CriticallyDampedSpring1D(springK, position.x, currentPosition.x, ref _dampingVelocity.x, ref dampenedPosition.x, Room.DeltaTime);
            MathUtils.CriticallyDampedSpring1D(springK, position.y, currentPosition.y, ref _dampingVelocity.y, ref dampenedPosition.y, Room.DeltaTime);
            MathUtils.CriticallyDampedSpring1D(springK, position.z, currentPosition.z, ref _dampingVelocity.z, ref dampenedPosition.z, Room.DeltaTime);

            Position.SetPosition(dampenedPosition);
            StateMachine.SetForceDirectionX(State.GetDirection());
        }
    }

    public void Aggro() => Logger.LogTrace("Aggro called for {StateName} on {PrefabName}", StateName, PrefabName);

    public void Attack() => Logger.LogTrace("Attack called for {StateName} on {PrefabName}", StateName, PrefabName);

    public void Fire()
    {
        Logger.LogTrace("Fire called for {StateName} on {PrefabName}", StateName, PrefabName);

        (StateMachine as DraiconEnemyControllerComp).IsImmune = false;
        
        var firstAngleOffset = 2f;
        var angleOffset = 45f;
        
        for (var i = 0; i < _projectileAmount; i++)
        {
            var currentAngle = firstAngleOffset + i * angleOffset;
            var angleInRadians = currentAngle * Mathf.Deg2Rad;

            var facingDir = StateMachine.GetForceDirectionX();
            var projectileDirection = new Vector2(
                Mathf.Cos(angleInRadians) * facingDir,
                Mathf.Sin(angleInRadians)
            );

            var projectileSpeed = projectileDirection * ProjectileSpeed;

            EnemyController.FireProjectile(Position, projectileSpeed, false);
        }
    }

    public void Resolution() => Logger.LogTrace("Resolution called for {StateName} on {PrefabName}", StateName, PrefabName);

    public void Flee()
    {
        Logger.LogTrace("Flee called for {StateName} on {PrefabName}", StateName, PrefabName);
        
        EndTime = Room.Time + 2f;
    }
}
