using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.IceTroll.States;
public class AIStateTrollVacuumComp : BaseAIState<AIStateTrollVacuum>
{
    public override string StateName => "AIStateTrollVacuum";

    public float CastingTime => ComponentData.CastingTime;
    public float Duration => ComponentData.Duration;
    public float CooldownTime => ComponentData.CooldownTime;
    public int AirFlowID => ComponentData.AirFlowID;
}
