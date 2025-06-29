using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using UnityEngine;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.SpiderBoss.States;
public class AIStateSpiderVenomComp : BaseAIState<AIStateSpiderVenom, AI_State>
{
    public override string StateName => "AIStateSpiderVenom";

    public string ProjectilePrefabName => ComponentData.ProjectilePrefabName;
    public float FirstProjectileSpeedX => ComponentData.FirstProjectileSpeedX;
    public float SecondProjectileSpeedX => ComponentData.SecondProjectileSpeedX;
    public float SecondProjectileSpeedY => ComponentData.SecondProjectileSpeedY;
    public float[] TimeDelayBetweenShotPerPhase => ComponentData.TimeDelayBetweenShotPerPhase;
    public int[] NumberOfSalvoPerPhase => ComponentData.NumberOfSalvoPerPhase;
    public float[] TimeDelayBetweenEverySalvoPerPhase => TimeDelayBetweenEverySalvoPerPhase;
    public float CooldownTime => ComponentData.CooldownTime;

    public TimerThread TimerThread { get; set; }

    public override AI_State GetInitialAIState() => new(
        [
            new (0f, "Shooting"),
            new (CooldownTime, "Cooldown")
        ], loop: false);

    public void Shooting()
    {
        Logger.LogTrace("Shooting called for {StateName} on {PrefabName}", StateName, PrefabName);

        var firstShot = TimeDelayBetweenShotPerPhase.FirstOrDefault();

        var firstProjectileTime = TimeSpan.FromSeconds(firstShot);

        if (firstProjectileTime > TimeSpan.Zero)
            TimerThread.RunDelayed(LaunchProjectile, new SpiderProjectile() { IsFirstProjectile = true, Component = this }, firstProjectileTime);

        var secondProjectileTime = firstProjectileTime.Add(TimeSpan.FromSeconds(firstShot));

        if (secondProjectileTime > TimeSpan.Zero)
            TimerThread.RunDelayed(LaunchProjectile, new SpiderProjectile() { IsFirstProjectile = false, Component = this }, secondProjectileTime);
    }

    public class SpiderProjectile() : ITimerData
    {
        public AIStateSpiderVenomComp Component { get; set; }
        public bool IsFirstProjectile { get; set; }

        public bool IsValid() => Component != null && Component.IsValid();
    }

    public static void LaunchProjectile(ITimerData data)
    {
        if (data is not SpiderProjectile projectile)
            return;

        var component = projectile.Component;

        var first = projectile.IsFirstProjectile;

        var speed = new Vector2(first ? component.FirstProjectileSpeedX : component.SecondProjectileSpeedX, component.SecondProjectileSpeedY);

        component.Room.AddRangedProjectile(component.Id, component.Position.ToUnityVector3(), speed, component.CooldownTime, 1, ItemEffectType.BluntDamage, false);
    }

    public void Cooldown()
    {
        Logger.LogTrace("Cooldown called for {StateName} on {PrefabName}", StateName, PrefabName);

        AddNextState<AIStateSpiderDropComp>();
        GoToNextState();
    }
}
