using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.SpiderBoss.States;

public class AIStateSpiderTeaserEntranceComp : BaseAIState<AIStateSpiderTeaserEntrance, AI_State>
{
    public override string StateName => "AIStateSpiderTeaserEntrance";

    public float DelayBeforeEntranceDuration => ComponentData.DelayBeforeEntranceDuration;
    public float EntranceDuration => ComponentData.EntranceDuration;
    public float IntroDuration => ComponentData.IntroDuration;
    public float MinimumLifeRatioAccepted => ComponentData.MinimumLifeRatioAccepted;
    public float LifeRatioAtHeal => ComponentData.LifeRatioAtHeal;

    public override AI_State GetInitialAIState() => new(
         [
            new (DelayBeforeEntranceDuration, "Delay"),
            new (EntranceDuration, "Entrance"),
            new (IntroDuration - EntranceDuration - DelayBeforeEntranceDuration, "Talk"),
            new (1f, "Transition")
        ], loop: false);

    public void Delay() => Logger.LogTrace("Delay called for {StateName} on {PrefabName}", StateName, PrefabName);

    public void Entrance() => Logger.LogTrace("Entrance called for {StateName} on {PrefabName}", StateName, PrefabName);

    public void Talk() => Logger.LogTrace("Talk called for {StateName} on {PrefabName}", StateName, PrefabName);

    public void Transition()
    {
        Logger.LogTrace("Transition called for {StateName} on {PrefabName}", StateName, PrefabName);

        RunExitEntrance();
    }

    private void RunExitEntrance()
    {
        AddNextState<AIStateSpiderDropComp>();
        GoToNextState();
    }
}
