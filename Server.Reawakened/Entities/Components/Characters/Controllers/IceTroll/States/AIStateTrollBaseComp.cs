using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.IceTroll.States;
public class AIStateTrollBaseComp : BaseAIState<AIStateTrollBase>
{
    public float HealthRatioPhase01Trans => ComponentData.HealthRatioPhase01Trans;
    public float HealthRatioPhase02Trans => ComponentData.HealthRatioPhase02Trans;
}
