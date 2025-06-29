using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.SpiderBoss.States;
public class AIStateSpiderBaseComp : BaseAIState<AIStateSpiderBase, AI_State>
{
    public override string StateName => "AIStateSpiderBase";

    public float HealthRatioPhase01Trans => ComponentData.HealthRatioPhase01Trans;
    public float HealthRatioPhase02Trans => ComponentData.HealthRatioPhase02Trans;
    public float TeaserEndLifeRatio => ComponentData.TeaserEndLifeRatio;
    public float TeaserEndTimeLimit => ComponentData.TeaserEndTimeLimit;

    public override AI_State GetInitialAIState() => new([], loop: false);
}
