using static A2m.Server.ExtLevelEditor;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.States;
using Server.Reawakened.Entities.DataComponentAccessors.Spiker.States;
using Server.Reawakened.Rooms.Models.Planes;
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
        _prjTime = Room.Time + ProjectileTime;

        // I don't know why this starts a tick late
        // The 0.5f is just so the state runs on time
        _endTime = Room.Time + ShootTime - 0.5f;
    }

    public override void Execute()
    {
        if (_prjTime > 0 && Room.Time >= _prjTime)
            FireProjectiles();

        else if (_endTime > 0 && Room.Time >= _endTime)
        {
            AddNextState<AIStateWaitComp>();
            GoToNextState();
        }
    }

    public void FireProjectiles()
    {
        Logger.LogTrace("Launched Projectiles for {PrefabName}", PrefabName);;

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

            EnemyController.FireProjectile(
                new Vector3Model(Position.X, Position.Y + 1, Position.Z),
                projectileSpeed,
                false
            );
        }

        _prjTime = -1;
    }
}
