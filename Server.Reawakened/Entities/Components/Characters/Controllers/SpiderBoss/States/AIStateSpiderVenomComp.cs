using A2m.Server;
using Server.Base.Core.Abstractions;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using Server.Reawakened.XMLs.Bundles.Base;
using UnityEngine;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.SpiderBoss.States;
public class AIStateSpiderVenomComp : BaseAIState<AIStateSpiderVenom>
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
    public EnemyRConfig EnemyRConfig { get; set; }
    public ServerRConfig ServerRConfig { get; set; }
    public ItemCatalog ItemCatalog { get; set; }
    public ItemRConfig ItemRConfig { get; set; }

    public override void StartState()
    {
        TimerThread.RunDelayed(LaunchProjectile, new SpiderProjectile() { IsFirstProjectile = true, Component = this }, TimeSpan.FromSeconds(EnemyRConfig.SpiderTeaserBossFirstProjectileDelay));
        TimerThread.RunDelayed(LaunchProjectile, new SpiderProjectile() { IsFirstProjectile = false, Component = this }, TimeSpan.FromSeconds(EnemyRConfig.SpiderTeaserBossSecondProjectileDelay));
        TimerThread.RunDelayed(RunDropState, this, TimeSpan.FromSeconds(EnemyRConfig.SpiderTeaserBossDropDelay));
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

        var position = new Vector3(component.Position.X, component.Position.Y + component.EnemyRConfig.SpiderTeaserBossProjectileYOffset, component.Position.Z);
        var speed = new Vector2(-component.EnemyRConfig.SpiderTeaserBossProjectileSpeed, Convert.ToBoolean(projectile.IsFirstProjectile) ? 0 : component.EnemyRConfig.SpiderTeaserBossProjectileSpeed);

        component.Room.AddRangedProjectile(component.Id, position, speed, component.EnemyRConfig.SpiderTeaserBossProjectileLifeTime, 1, ItemEffectType.BluntDamage, false);
    }

    public static void RunDropState(ITimerData data)
    {
        if (data is not AIStateSpiderVenomComp spider)
            return;

        spider.AddNextState<AIStateSpiderDropComp>();
        spider.GoToNextState();
    }
}
