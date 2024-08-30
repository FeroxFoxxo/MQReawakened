using Server.Base.Core.Abstractions;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.IceTroll.States;
public class AIStateTrollEntranceComp : BaseAIState<AIStateTrollEntrance>
{
    public override string StateName => "AIStateTrollEntrance";

    public float DelayBeforeEntranceDuration => ComponentData.DelayBeforeEntranceDuration;
    public float EntranceDuration => ComponentData.EntranceDuration;

    // NOTE: UNKNOWN VALUE! EXAMPLE OF HOW TO OVERRIDE DEFAULT VALUES
    public float IntroDuration => ComponentData.IntroDuration != default ? ComponentData.IntroDuration : 5f;

    public TimerThread TimerThread { get; set; }

    public override void StartState() =>
        TimerThread.RunDelayed(RunExitEntrance, this, TimeSpan.FromSeconds(IntroDuration));

    private static void RunExitEntrance(ITimerData data)
    {
        if (data is not AIStateTrollEntranceComp troll)
            return;

        troll.AddNextState<AIStateTrollIdleComp>();
        troll.GoToNextState();
    }
}
