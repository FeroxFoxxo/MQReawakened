using A2m.Server;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using Server.Reawakened.Entities.Enemies.Extensions;
using Server.Reawakened.Entities.Projectiles;
using Server.Reawakened.Rooms.Extensions;
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
        TimerThread.DelayCall(LaunchProjectile, true, TimeSpan.FromSeconds(EnemyRConfig.SpiderTeaserBossFirstProjectileDelay), TimeSpan.Zero, 1);
        TimerThread.DelayCall(LaunchProjectile, false, TimeSpan.FromSeconds(EnemyRConfig.SpiderTeaserBossSecondProjectileDelay), TimeSpan.Zero, 1);

        TimerThread.DelayCall(RunDropState, null, TimeSpan.FromSeconds(EnemyRConfig.SpiderTeaserBossDropDelay), TimeSpan.Zero, 1);
    }

    public void LaunchProjectile(object isFirstProjectile)
    {
        var projectileId = Room.CreateProjectileId();

        Room.SendSyncEvent(
            AISyncEventHelper.AILaunchItem(
                Id, Room.Time, Position.X, Position.Y + EnemyRConfig.SpiderTeaserBossProjectileYOffset, Position.Z,
                -EnemyRConfig.SpiderTeaserBossProjectileSpeed, Convert.ToBoolean(isFirstProjectile) ? 0 : EnemyRConfig.SpiderTeaserBossProjectileSpeed,
                EnemyRConfig.SpiderTeaserBossProjectileLifeTime, projectileId, Convert.ToBoolean(0)
            )
        );

        Room.AddProjectile(
            new AIProjectile(
                Room, Id, projectileId.ToString(),
                new Vector3(Position.X, Position.Y + EnemyRConfig.SpiderTeaserBossProjectileYOffset, Position.Z),
                new Vector2(-EnemyRConfig.SpiderTeaserBossProjectileSpeed, Convert.ToBoolean(isFirstProjectile) ? 0 : EnemyRConfig.SpiderTeaserBossProjectileSpeed),
                EnemyRConfig.SpiderTeaserBossProjectileLifeTime, TimerThread, 1, ItemEffectType.BluntDamage, false, ServerRConfig, ItemCatalog, ItemRConfig
            )
        );
    }

    public void RunDropState(object _)
    {
        if (Room == null)
            return;

        AddNextState<AIStateSpiderDropComp>();

        GoToNextState();
    }
}
