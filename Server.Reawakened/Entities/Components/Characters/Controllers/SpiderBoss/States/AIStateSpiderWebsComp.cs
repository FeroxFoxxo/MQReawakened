using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using Server.Reawakened.Rooms.Extensions;
using UnityEngine;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.SpiderBoss.States;
public class AIStateSpiderWebsComp : BaseAIState<AIStateSpiderWebs, AI_State>
{
    public override string StateName => "AIStateSpiderWebs";

    public float WebInTime => ComponentData.WebInTime;
    public float WebShootTime => ComponentData.WebShootTime;
    public float WebOutTime => ComponentData.WebOutTime;
    public int[] NumberOfShotPerPhase => ComponentData.NumberOfShotPerPhase;
    public string ProjectilePrefabName => ComponentData.ProjectilePrefabName;
    public string OnProjectileDestroyedPrefabCreation => ComponentData.OnProjectileDestroyedPrefabCreation;
    public float ProjectileSpeedY => ComponentData.ProjectileSpeedY;
    public float ProjectileSpeedMaxX => ComponentData.ProjectileSpeedMaxX;

    public override AI_State GetInitialAIState() => new(
        [
            new(WebInTime, "WebIn"),
            new(WebShootTime, "Shoot1"),
            new(WebShootTime, "Shoot2"),
            new(WebShootTime, "Shoot3"),
            new(WebShootTime, "Shoot4"),
            new(WebOutTime, "WebOut")
        ], loop: false);

    public override void OnAIStateIn()
    {
        var shots = NumberOfShotPerPhase[(StateMachine as SpiderBossControllerComp).CurrentPhase];
        State.SetTime("Shoot1", (shots < 1) ? 0f : WebShootTime);
        State.SetTime("Shoot2", (shots < 2) ? 0f : WebShootTime);
        State.SetTime("Shoot3", (shots < 3) ? 0f : WebShootTime);
        State.SetTime("Shoot4", (shots < 4) ? 0f : WebShootTime);
        State.RecalculateTimes();
    }

    public void WebIn() => Logger.LogTrace("WebIn called for {StateName} on {PrefabName}", StateName, PrefabName);
    public void Shoot1() { Logger.LogTrace("Shoot1 called for {StateName} on {PrefabName}", StateName, PrefabName); LaunchWebProjectile(); }
    public void Shoot2() { Logger.LogTrace("Shoot2 called for {StateName} on {PrefabName}", StateName, PrefabName); LaunchWebProjectile(); }
    public void Shoot3() { Logger.LogTrace("Shoot3 called for {StateName} on {PrefabName}", StateName, PrefabName); LaunchWebProjectile(); }
    public void Shoot4() { Logger.LogTrace("Shoot4 called for {StateName} on {PrefabName}", StateName, PrefabName); LaunchWebProjectile(); }

    private void LaunchWebProjectile()
    {
        var player = Room.GetClosestPlayer(Position.ToUnityVector3(), 100f);

        if (player == null)
            return;

        var targetPos = player.TempData.Position;

        var distanceX = targetPos.X - Position.X;
        var distanceY = targetPos.Y - Position.Y;

        const float projectileGravity = 15f; 

        var a = 0.5f * projectileGravity;
        var b = -ProjectileSpeedY;
        var c = distanceY;
        var discriminant = b * b - 4 * a * c;

        if (discriminant < 0)
        {
            var fallbackVx = Math.Sign(distanceX) * ProjectileSpeedMaxX;
            EnemyController.FireProjectile(Position, new Vector2(fallbackVx, ProjectileSpeedY), true);
            return;
        }

        var t = (-b + Math.Sqrt(discriminant)) / (2 * a);
        if (t <= 0)
        {
            var fallbackVx = Math.Sign(distanceX) * ProjectileSpeedMaxX;
            EnemyController.FireProjectile(Position, new Vector2(fallbackVx, ProjectileSpeedY), true);
            return;
        }

        var velocityX = distanceX / (float)t;

        if (Math.Abs(velocityX) > ProjectileSpeedMaxX)
        {
            velocityX = Math.Sign(velocityX) * ProjectileSpeedMaxX;
        }

        var finalVelocity = new Vector2(velocityX, ProjectileSpeedY);

        EnemyController.FireProjectile(Position, finalVelocity, true);
    }

    public void WebOut()
    {
        Logger.LogTrace("WebOut called for {StateName} on {PrefabName}", StateName, PrefabName);
        AddNextState<AIStateSpiderDropComp>();
        GoToNextState();
    }
}
