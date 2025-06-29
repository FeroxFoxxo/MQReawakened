using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.IceTroll.States;
public class AIStateTrollDeactivatedComp : BaseAIState<AIStateTrollDeactivated, AI_State>
{
    public override string StateName => "AIStateTrollDeactivated";

    public override AI_State GetInitialAIState() => new(
        [
            new(1f, "Deactivated")
        ], loop: true);

    public void Deactivated() => Logger.LogTrace("Deactivated called for {StateName} on {PrefabName}", StateName, PrefabName);
}
