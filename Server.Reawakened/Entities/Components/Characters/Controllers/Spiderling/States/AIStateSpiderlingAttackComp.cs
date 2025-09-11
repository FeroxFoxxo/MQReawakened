using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.States;
using Server.Reawakened.Entities.DataComponentAccessors.Spiderling.States;
using UnityEngine;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Spiderling.States;
public class AIStateSpiderlingAttackComp : BaseAIState<AIStateSpiderlingAttackMQR, AI_State>
{
    public override string StateName => "AIStateSpiderlingAttack";

    public float ShotInterval => ComponentData.ShotInterval;
    public float ShootTime => ComponentData.ShootTime;
    public float ShootDelay => ComponentData.ShootDelay;
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

        var targetPos = targetPlayer.TempData.Position;
        var startPos = Position;

        var distanceX = targetPos.X - startPos.X;
        var distanceY = targetPos.Y - startPos.Y;

        const float g = 15f;
        var v = ProjectileSpeed;
        var v2 = v * v;
        var v4 = v2 * v2;
        
        float baseAngleDeg;

        var discriminant = v4 - g * (g * distanceX * distanceX + 2 * distanceY * v2);

        if (discriminant < 0)
        {
            baseAngleDeg = Mathf.Atan2(distanceY, distanceX) * Mathf.Rad2Deg;
        }
        else
        {
            var tanTheta = (v2 - Mathf.Sqrt(discriminant)) / (g * distanceX);
            baseAngleDeg = Mathf.Atan(tanTheta) * Mathf.Rad2Deg;
        }

        var startAngle = baseAngleDeg + FirstProjectileAngleOffset;

        for (var i = 0; i < NumberOfProjectiles; i++)
        {
            var currentAngleDeg = startAngle + i * AngleBetweenProjectiles;
            var currentAngleRad = currentAngleDeg * Mathf.Deg2Rad;

            var projectileVelocity = new Vector2(
                Mathf.Cos(currentAngleRad) * v,
                Mathf.Sin(currentAngleRad) * v
            );
            
            EnemyController.FireProjectile(Position, projectileVelocity, true);
        }
    }
}
