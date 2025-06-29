using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.IceTroll.States;
public class AIStateTrollPhaseTransComp : BaseAIState<AIStateTrollPhaseTrans, AI_State>
{
    public override string StateName => "AIStateTrollPhaseTrans";

    public float TransTime => ComponentData.TransTime;

    public override AI_State GetInitialAIState() => new(
        [
            new (TransTime, "Transition")
        ], loop: false);

    public void Transition() => Logger.LogTrace("Transition called for {StateName} on {PrefabName}", StateName, PrefabName);
}
