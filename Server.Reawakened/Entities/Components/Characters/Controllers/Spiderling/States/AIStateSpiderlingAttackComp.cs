using static A2m.Server.ExtLevelEditor;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
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
    
    private float _prjTime;
    private float _endTime;
    
    public override AI_State GetInitialAIState() => new(
        [
            new (ShootTime, "Shoot")
        ], loop: false);

    public override ComponentSettings GetSettings() => [StateMachine.GetForceDirectionX().ToString()];
    
    public override void InitializeComponent()
    {
        base.InitializeComponent();
        _prjTime = -1;
        _endTime = -1;
    }
    
    public void Shoot()
    {
        Logger.LogTrace("Shoot called for {StateName} on {PrefabName}", StateName, PrefabName);
        _prjTime = Room.Time + ShootDelay;
        _endTime = Room.Time + ShootTime - 9f;
    }

    public override void Execute()
    {
        if (_prjTime > 0 && Room.Time >= _prjTime)
            FireProjectiles();

        else if (_endTime > 0 && Room.Time >= _endTime)
        {
            AddNextState<AIStateSpiderlingAlertComp>();
            GoToNextState();
        }
    }
    
    public void FireProjectiles()
    {
        Logger.LogTrace("Launched Projectiles for {PrefabName}", PrefabName);
        
        for (var i = 0; i < NumberOfProjectiles; i++)
        {
            var currentAngle = FirstProjectileAngleOffset + i * AngleBetweenProjectiles;
            var angleInRadians = currentAngle * Mathf.Deg2Rad;

            var facingDir = StateMachine.GetForceDirectionX();
            var projectileDirection = new Vector2(
                Mathf.Cos(angleInRadians) * facingDir,
                Mathf.Sin(angleInRadians)
            );

            var projectileSpeed = projectileDirection * ProjectileSpeed;

            EnemyController.FireProjectile(Position, projectileSpeed, true);
        }
        
        _prjTime = -1;
    }
}
