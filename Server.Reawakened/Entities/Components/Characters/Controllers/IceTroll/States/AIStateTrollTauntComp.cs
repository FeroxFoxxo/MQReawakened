using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.IceTroll.States;
public class AIStateTrollTauntComp : BaseAIState<AIStateTrollTaunt, AI_State>
{
    public override string StateName => "AIStateTrollTaunt";

    public float Duration => ComponentData.Duration;

    public override AI_State GetInitialAIState() => new (
        [
            new AIDataEvent(Duration, "Done")
        ], loop: false);

    public void Done() => Logger.LogTrace("Done called for {StateName} on {PrefabName}", StateName, PrefabName);
}
