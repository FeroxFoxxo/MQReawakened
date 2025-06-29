using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.SpiderBoss.States;
public class AIStateSpiderIdleComp : BaseAIState<AIStateSpiderIdle, AI_State>
{
    public override string StateName => "AIStateSpiderIdle";

    public float[] Durations => ComponentData.Durations;

    public override AI_State GetInitialAIState() => new (
        [
            new(Durations[0], "Done")
        ], loop: false);

    public void Done()
    {
        Logger.LogTrace("Done called for {StateName} on {PrefabName}", StateName, PrefabName);
        RunVenomState();
    }

    public void RunVenomState()
    {
        AddNextState<AIStateSpiderVenomComp>();
        GoToNextState();
    }
}
