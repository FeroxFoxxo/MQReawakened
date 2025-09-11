using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using Server.Reawakened.Rooms.Extensions;
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
        var (phase, shotDelay, salvoCount, betweenSalvoDelay) = ResolveVenomParams();

        var shootingDuration = (salvoCount > 0)
            ? 2f * shotDelay * salvoCount + betweenSalvoDelay * (salvoCount - 1)
            : 0f;

        State.SetTime("Shooting", shootingDuration);
        State.RecalculateTimes();

        Logger.LogTrace("Enter Venom: phase={Phase}, salvos={Salvos}, shotDelay={ShotDelay}, betweenSalvo={Between}, shootingDuration={ShootDur}",
            phase, salvoCount, shotDelay, betweenSalvoDelay, shootingDuration);
    }

    public void Shooting()
    {
        Logger.LogTrace("Shooting called for {StateName} on {PrefabName}", StateName, PrefabName);
        var (_, shotDelay, salvoCount, betweenSalvoDelay) = ResolveVenomParams();
        ScheduleVenom(shotDelay, salvoCount, betweenSalvoDelay);
    }

    private (int phase, float shotDelay, int salvoCount, float betweenSalvoDelay) ResolveVenomParams()
    {
        var controller = StateMachine as SpiderBossControllerComp;
        var phase = controller?.CurrentPhase ?? 0;

        var shotDelay = (phase < TimeDelayBetweenShotPerPhase.Length) ? TimeDelayBetweenShotPerPhase[phase] : TimeDelayBetweenShotPerPhase.FirstOrDefault();
        var salvoCount = (phase < NumberOfSalvoPerPhase.Length) ? NumberOfSalvoPerPhase[phase] : NumberOfSalvoPerPhase.FirstOrDefault();
        var betweenSalvoDelay = (phase < TimeDelayBetweenEverySalvoPerPhase.Length) ? TimeDelayBetweenEverySalvoPerPhase[phase] : TimeDelayBetweenEverySalvoPerPhase.FirstOrDefault();

        return (phase, shotDelay, salvoCount, betweenSalvoDelay);
    }

    private void ScheduleVenom(float shotDelay, int salvoCount, float betweenSalvoDelay)
    {
        var t = TimeSpan.Zero;

        for (var i = 0; i < salvoCount; i++)
        {
            var firstT = t + TimeSpan.FromSeconds(shotDelay);
            var secondT = firstT + TimeSpan.FromSeconds(shotDelay);

            var firstData = new SpiderProjectile { IsFirstProjectile = true, Component = this };
            var secondData = new SpiderProjectile { IsFirstProjectile = false, Component = this };

            if (firstT <= TimeSpan.Zero)
                TimerThread.RunInstantly(LaunchProjectile, firstData);
            else
                TimerThread.RunDelayed(LaunchProjectile, firstData, firstT);

            if (secondT <= TimeSpan.Zero)
                TimerThread.RunInstantly(LaunchProjectile, secondData);
            else
                TimerThread.RunDelayed(LaunchProjectile, secondData, secondT);

            t = secondT + TimeSpan.FromSeconds(betweenSalvoDelay);
        }
    }

    public class SpiderProjectile : ITimerData
    {
        public AIStateSpiderVenomComp Component { get; set; }
        public bool IsFirstProjectile { get; set; }

        public bool IsValid() => Component != null && Component.IsValid() && !Component.Room.IsObjectKilled(Component.Id);
    }

    public static void LaunchProjectile(ITimerData data)
    {
        if (data is not SpiderProjectile projectile)
            return;

        var component = projectile.Component;

        if (component?.Room == null)
            return;

        var player = component.Room.GetClosestPlayer(component.Position.ToUnityVector3(), 100f);

        if (player == null)
            return;

        var targetPos = player.TempData.Position;
        var direction = (targetPos.ToUnityVector3() - component.Position.ToUnityVector3()).normalized;

        var speed = projectile.IsFirstProjectile
            ? component.FirstProjectileSpeedX
            : component.SecondProjectileSpeedX;

        var velocity = new Vector2(direction.x * speed, direction.y * speed);

        component.EnemyController.FireProjectile(component.Position, velocity, false);
    }

    public void Cooldown()
    {
        Logger.LogTrace("Exit Venom -> Idle");

        AddNextState<AIStateSpiderIdleComp>();
        GoToNextState();
    }
}
