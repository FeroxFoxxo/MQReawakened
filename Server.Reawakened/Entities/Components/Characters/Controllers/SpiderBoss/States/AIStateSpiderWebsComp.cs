using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;

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

    public void Shoot1() => Logger.LogTrace("Shoot1 called for {StateName} on {PrefabName}", StateName, PrefabName);

    public void Shoot2() => Logger.LogTrace("Shoot2 called for {StateName} on {PrefabName}", StateName, PrefabName);

    public void Shoot3() => Logger.LogTrace("Shoot3 called for {StateName} on {PrefabName}", StateName, PrefabName);

    public void Shoot4() => Logger.LogTrace("Shoot4 called for {StateName} on {PrefabName}", StateName, PrefabName);

    public void WebOut() => Logger.LogTrace("WebOut called for {StateName} on {PrefabName}", StateName, PrefabName);
}
