using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.IceTroll.States;
public class AIStateTrollIdleComp : BaseAIState<AIStateTrollIdle>
{
    public override string StateName => "AIStateTrollIdle";

    public float Duration => ComponentData.Duration;
}
