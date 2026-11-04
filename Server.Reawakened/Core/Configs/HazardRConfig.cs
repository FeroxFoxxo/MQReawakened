using Server.Base.Core.Abstractions;

namespace Server.Reawakened.Core.Configs;

public class HazardRConfig : IRConfig
{
    public int BreathTimerDuration { get; }
    public int UnderwaterDamageInterval { get; }
    public int UnderwaterDamageRatio { get; }
    public int PoisonEffectInterval { get; }
    public int PoisonDamageCountFromEnemy { get; }
    public int PoisonDamageOverTimeDeduction { get; }
    public float SpawnInvincibilityDuration { get; }
    public float SlowEffectInterval { get; }

    public HazardRConfig()
    {
        BreathTimerDuration = 31;
        UnderwaterDamageInterval = 2;
        UnderwaterDamageRatio = 10;

        PoisonEffectInterval = 2;
        PoisonDamageCountFromEnemy = 3;
        PoisonDamageOverTimeDeduction = 3;

        SpawnInvincibilityDuration = 1.5f;

        SlowEffectInterval = 0.50f;
    }
}
