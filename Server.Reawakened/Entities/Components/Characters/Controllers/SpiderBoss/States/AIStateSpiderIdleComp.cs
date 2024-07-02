using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.SpiderBoss.States;
public class AIStateSpiderIdleComp : BaseAIState<AIStateSpiderIdle>
{
    public override string StateName => "AIStateSpiderIdle";

    public float[] Durations => ComponentData.Durations;

    public TimerThread TimerThread { get; set; }
    public ServerRConfig ServerRConfig { get; set; }

    public override void StartState() =>
        TimerThread.DelayCall(RunVenomState, null, TimeSpan.FromSeconds(ServerRConfig.SpiderTeaserBossFirstProjectileDelay), TimeSpan.Zero, 1);

    public void RunVenomState(object _)
    {
        AddNextState<AIStateSpiderVenomComp>();       

        GoToNextState();
    }
}
