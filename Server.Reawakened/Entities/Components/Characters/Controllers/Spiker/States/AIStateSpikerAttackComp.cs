using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.States;
using Server.Reawakened.Entities.DataComponentAccessors.Spiker.States;
using UnityEngine;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Spiker.States;

public class AIStateSpikerAttackComp : BaseAIState<AIStateSpikerAttackMQR, AI_State>
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

    public override AI_State GetInitialAIState() => new(
        [
            new (ShootTime, "Shoot")
        ], loop: false);

    public override ExtLevelEditor.ComponentSettings GetSettings() => [StateMachine.GetForceDirectionX().ToString()];

    public override void InitializeComponent()
    {
        base.InitializeComponent();
        _patrolComp = Room.GetEntityFromId<AIStatePatrolComp>(Id);
    }

    public void Shoot()
    {
        Logger.LogTrace("Shoot called for {StateName} on {PrefabName}", StateName, PrefabName);

        var targetPlayer = _patrolComp.GetClosestPlayer();

        if (targetPlayer == null)
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

            EnemyController.FireProjectile(
                Position.ToUnityVector3(),
                projectileSpeed,
                false
            );
        }
    }
}
