using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.SpiderBoss.States;
public class AIStateSpiderIdleComp : BaseAIState<AIStateSpiderIdle>
{
    public override string StateName => "AIStateSpiderIdle";

    public float[] Durations => ComponentData.Durations;

    public TimerThread TimerThread { get; set; }

    public override void StartState() =>
        TimerThread.DelayCall(RunPhase1, null, TimeSpan.FromSeconds(3), TimeSpan.Zero, 1);

    public void RunPhase1(object _)
    {
        AddNextState<AIStateSpiderVenomComp>();       

        GoToNextState();
    }
}
