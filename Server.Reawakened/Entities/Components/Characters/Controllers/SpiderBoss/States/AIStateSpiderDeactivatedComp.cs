using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.SpiderBoss.States;
public class AIStateSpiderDeactivatedComp : BaseAIState<AIStateSpiderDeactivated, AI_State>
{
    public override string StateName => "AIStateSpiderDeactivated";

    public override AI_State GetInitialAIState() => new(
        [
            new(1f, "Deactivated")
        ], loop: true);

    public void Deactivated() => Logger.LogTrace("Deactivated called for {StateName} on {PrefabName}", StateName, PrefabName);
}
