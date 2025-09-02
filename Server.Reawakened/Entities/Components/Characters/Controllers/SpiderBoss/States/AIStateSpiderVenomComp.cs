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
    public float[] TimeDelayBetweenEverySalvoPerPhase => ComponentData.TimeDelayBetweenEverySalvoPerPhase;
    public float CooldownTime => ComponentData.CooldownTime;

    public TimerThread TimerThread { get; set; }

    public override AI_State GetInitialAIState() => new(
        [
            new (0f, "Shooting"),
            new (CooldownTime, "Cooldown")
        ], loop: false);

    public override void OnAIStateIn()
    {
        var controller = StateMachine as SpiderBossControllerComp;
        var phase = controller?.CurrentPhase ?? 0;

        Logger.LogTrace("Enter Venom: phase={Phase}, salvos={Salvos}, shotDelay={ShotDelay}, betweenSalvo={Between}",
            phase,
            (phase < NumberOfSalvoPerPhase.Length) ? NumberOfSalvoPerPhase[phase] : NumberOfSalvoPerPhase.FirstOrDefault(),
            (phase < TimeDelayBetweenShotPerPhase.Length) ? TimeDelayBetweenShotPerPhase[phase] : TimeDelayBetweenShotPerPhase.FirstOrDefault(),
            (phase < TimeDelayBetweenEverySalvoPerPhase.Length) ? TimeDelayBetweenEverySalvoPerPhase[phase] : TimeDelayBetweenEverySalvoPerPhase.FirstOrDefault());
    }

    public void Shooting()
    {
        Logger.LogTrace("Shooting called for {StateName} on {PrefabName}", StateName, PrefabName);
        var controller = StateMachine as SpiderBossControllerComp;
        var phase = controller?.CurrentPhase ?? 0;

        var shotDelay = (phase < TimeDelayBetweenShotPerPhase.Length) ? TimeDelayBetweenShotPerPhase[phase] : TimeDelayBetweenShotPerPhase.FirstOrDefault();
        var salvoCount = (phase < NumberOfSalvoPerPhase.Length) ? NumberOfSalvoPerPhase[phase] : NumberOfSalvoPerPhase.FirstOrDefault();
        var betweenSalvoDelay = (phase < TimeDelayBetweenEverySalvoPerPhase.Length) ? TimeDelayBetweenEverySalvoPerPhase[phase] : TimeDelayBetweenEverySalvoPerPhase.FirstOrDefault();

        var t = TimeSpan.Zero;

        for (var i = 0; i < salvoCount; i++)
        {
            var firstT = t + TimeSpan.FromSeconds(shotDelay);
            var secondT = firstT + TimeSpan.FromSeconds(shotDelay);

            if (firstT > TimeSpan.Zero)
                TimerThread.RunDelayed(LaunchProjectile, new SpiderProjectile { IsFirstProjectile = true, Component = this }, firstT);

            if (secondT > TimeSpan.Zero)
                TimerThread.RunDelayed(LaunchProjectile, new SpiderProjectile { IsFirstProjectile = false, Component = this }, secondT);

            t = secondT + TimeSpan.FromSeconds(betweenSalvoDelay);
        }
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
        Logger.LogTrace("Exit Venom -> Drop");

        AddNextState<AIStateSpiderDropComp>();
        GoToNextState();
    }
}
