using Server.Base.Core.Abstractions;
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
        TimerThread.RunDelayed(RunExitEntrance, this, TimeSpan.FromSeconds(IntroDuration));

    private static void RunExitEntrance(ITimerData data)
    {
        if (data is not AIStateSpiderTeaserEntranceComp spider)
            return;

        spider.AddNextState<AIStateSpiderDropComp>();
        spider.GoToNextState();
    }
}
