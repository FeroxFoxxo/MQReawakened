using Server.Base.Core.Abstractions;

namespace Server.Reawakened.Core.Configs;
public class EnemyRConfig : IRConfig
{
    public float SpiderTeaserBossProjectileSpeed { get; }
    public float SpiderTeaserBossProjectileYOffset { get; }
    public float SpiderTeaserBossProjectileLifeTime { get; }
    public float SpiderTeaserBossFirstProjectileDelay { get; }
    public float SpiderTeaserBossSecondProjectileDelay { get; }
    public float SpiderTeaserBossDropDelay { get; }

    public EnemyRConfig()
    {
        SpiderTeaserBossProjectileSpeed = 7f;
        SpiderTeaserBossProjectileYOffset = 0.5f;
        SpiderTeaserBossProjectileLifeTime = 3f;
        SpiderTeaserBossFirstProjectileDelay = 1f;
        SpiderTeaserBossSecondProjectileDelay = 1.5f;
        SpiderTeaserBossDropDelay = 3f;
    }
}
