using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.IceTroll.States;
public class AIStateTrollArmorBreakComp : BaseAIState<AIStateTrollArmorBreak>
{
    public override string StateName => "AIStateTrollArmorBreak";

    public float StunTime => ComponentData.StunTime;
    public float RoarTime => ComponentData.RoarTime;
}
