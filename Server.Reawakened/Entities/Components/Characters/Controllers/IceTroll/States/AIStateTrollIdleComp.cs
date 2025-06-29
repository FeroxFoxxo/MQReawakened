using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.IceTroll.States;
public class AIStateTrollIdleComp : BaseAIState<AIStateTrollIdle, AI_State>
{
    public override string StateName => "AIStateTrollIdle";

    public float Duration => ComponentData.Duration;

    public override AI_State GetInitialAIState() => new(
        [
            new (Duration, "Done")
        ], loop: false);

    public void Done() => Logger.LogTrace("Done called for {StateName} on {PrefabName}", StateName, PrefabName);
}
