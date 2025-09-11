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

        var toTarget = targetPos.ToUnityVector3() - Position.ToUnityVector3();
        var dir = toTarget;

        if (dir.sqrMagnitude <= 0f)
        {
            var forceX = 1f;
            try { forceX = StateMachine.GetForceDirectionX(); } catch { }
            dir = new Vector3(forceX, 0f, 0f);
        }

        var baseAngle = Mathf.Atan2(dir.y, dir.x);

        if (NumberOfProjectiles <= 0)
        {
            Logger.LogTrace("No projectiles to fire for {StateName} on {PrefabName}", StateName, PrefabName);
            return;
        }

        var startOffsetDeg = FirstProjectileAngleOffset;
        var betweenDeg = AngleBetweenProjectiles;

        for (var i = 0; i < NumberOfProjectiles; i++)
        {
            var angleDeg = startOffsetDeg + i * betweenDeg;
            var angle = baseAngle + angleDeg * Mathf.Deg2Rad;

            var velocity = new Vector2(Mathf.Cos(angle) * ProjectileSpeed, Mathf.Sin(angle) * ProjectileSpeed);

            EnemyController.FireProjectile(Position, velocity, false);
        }
    }
}
