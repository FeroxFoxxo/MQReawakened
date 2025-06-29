using A2m.Server;
using Server.Base.Core.Abstractions;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.States;
using Server.Reawakened.Entities.DataComponentAccessors.Spiker.States;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms.Extensions;
using UnityEngine;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Spiker.States;

public class AIStateSpikerAttackComp : BaseAIState<AIStateSpikerAttackMQR>, ITimerData
{       
    public override string StateName => "AIStateSpikerAttack";

    public float ShootTime => ComponentData.ShootTime;
    public float ProjectileTime => ComponentData.ProjectileTime;
    public string Projectile => ComponentData.Projectile;
    public float ProjectileSpeed => ComponentData.ProjectileSpeed;
    public float FirstProjectileAngleOffset => ComponentData.FirstProjectileAngleOffset;
    public int NumberOfProjectiles => ComponentData.NumberOfProjectiles;
    public float AngleBetweenProjectiles => ComponentData.AngleBetweenProjectiles;

    private AIStatePatrolComp _patrolComp;

    public TimerThread TimerThread { get; set; }

    public override ExtLevelEditor.ComponentSettings GetSettings() => [_patrolComp.ForceDirectionX.ToString()];

    public override void DelayedComponentInitialization() => _patrolComp = Room.GetEntityFromId<AIStatePatrolComp>(Id);

    public override void StartState()
    {
        TimerThread.RunDelayed(FireProjectilesCallback, this, TimeSpan.FromSeconds(ShootTime));
        TimerThread.RunDelayed(ReturnToPatrolCallback, this, TimeSpan.FromSeconds(ProjectileTime));
    }

    public static void FireProjectilesCallback(ITimerData data)
    {
        if (data is not AIStateSpikerAttackComp spikerAttack)
            return;

        if (spikerAttack._patrolComp == null)
            return;

        var closestPlayer = spikerAttack._patrolComp.GetClosestPlayer();
        
        if (closestPlayer != null)
            spikerAttack.FireProjectiles(closestPlayer);
    }

    private void FireProjectiles(Player targetPlayer)
    {
        var aiState = StateMachine.GetAiStateEnemy();

        if (aiState == null)
            return;

        var directionToPlayer = GetDirectionToPlayer(targetPlayer);
        var baseAngle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;

        var startingAngle = baseAngle + FirstProjectileAngleOffset;

        for (var i = 0; i < NumberOfProjectiles; i++)
        {
            var currentAngle = startingAngle + i * AngleBetweenProjectiles;
            var angleInRadians = currentAngle * Mathf.Deg2Rad;

            var projectileDirection = new Vector2(
                Mathf.Cos(angleInRadians),
                Mathf.Sin(angleInRadians)
            );

            var projectileSpeed = projectileDirection * ProjectileSpeed;

            aiState.FireProjectile(
                Position.ToUnityVector3(),
                projectileSpeed,
                false
            );
        }
    }

    public static void ReturnToPatrolCallback(ITimerData data)
    {
        if (data is not AIStateSpikerAttackComp spikerAttack)
            return;

        spikerAttack.AddNextState<AIStatePatrolComp>();
        spikerAttack.GoToNextState();
    }
}
