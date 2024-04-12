using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.IceTroll.States;
public class AIStateTrollTauntComp : BaseAIState<AIStateTrollTaunt>
{
    public float Duration => ComponentData.Duration;
}
