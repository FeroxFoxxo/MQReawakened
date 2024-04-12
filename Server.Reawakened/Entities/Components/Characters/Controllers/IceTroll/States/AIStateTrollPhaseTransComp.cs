using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.IceTroll.States;
public class AIStateTrollPhaseTransComp : BaseAIState<AIStateTrollPhaseTrans>
{
    public override string StateName => "AIStateTrollPhaseTrans";

    public float TransTime => ComponentData.TransTime;
}
