using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;

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
}
