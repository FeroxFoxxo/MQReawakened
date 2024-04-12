using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.SpiderBoss.States;

public class AIStateSpiderTeaserEntranceComp : BaseAIState<AIStateSpiderTeaserEntrance>
{
    public override string StateName => "AIStateSpiderTeaserEntrance";

    public float DelayBeforeEntranceDuration => ComponentData.DelayBeforeEntranceDuration;
    public float EntranceDuration => ComponentData.EntranceDuration;
    public float IntroDuration => ComponentData.IntroDuration;
    public float MinimumLifeRatioAccepted => ComponentData.MinimumLifeRatioAccepted;
    public float LifeRatioAtHeal => ComponentData.LifeRatioAtHeal;

    public TimerThread TimerThread { get; set; }

    public override void StartState() =>
        TimerThread.DelayCall(RunExitEntrance, null, TimeSpan.FromSeconds(IntroDuration), TimeSpan.Zero, 1);

    private void RunExitEntrance(object _)
    {
        if (Room == null)
            return;

        AddNextState<AIStateSpiderDropComp>();
        AddNextState<AIStateSpiderIdleComp>();

        GoToNextState();
    }
}
