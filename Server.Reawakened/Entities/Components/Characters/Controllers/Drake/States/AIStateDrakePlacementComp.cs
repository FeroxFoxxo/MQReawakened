using A2m.Server;
using Server.Base.Core.Abstractions;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.States;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Drake.States;
public class AIStateDrakePlacementComp : BaseAIState<AIStateDrakePlacement>
{
    public override string StateName => "AIStateDrakePlacement";

    public float MovementSpeed => ComponentData.MovementSpeed;
    public float AttackInAnimDuration => ComponentData.AttackInAnimDuration;
    public float AttackLoopAnimDuration => ComponentData.AttackLoopAnimDuration;
    public float AttackRange => ComponentData.AttackRange;
    public float AttackRangeMaximum => ComponentData.AttackRangeMaximum;

    public ServerRConfig ServerRConfig { get; set; }
    public TimerThread TimerThread { get; set; }

    public override void StartState() =>
        TimerThread.RunDelayed(RunPatrolState, this, TimeSpan.FromSeconds(1));

    public static void RunPatrolState(ITimerData data)
    {
        if (data is not AIStateDrakePlacementComp drake)
            return;

        drake.AddNextState<AIStatePatrolComp>();
        drake.GoToNextState();
    }

    // Provide Initial And Placement Positions
    public override ExtLevelEditor.ComponentSettings GetSettings()
    {
        var backPlaneZValue = ParentPlane == ServerRConfig.BackPlane ?
                      ServerRConfig.Planes[ServerRConfig.FrontPlane] :
                       ServerRConfig.Planes[ServerRConfig.BackPlane];

        return [Position.X.ToString(), Position.Y.ToString(), backPlaneZValue.ToString(),
                Position.X.ToString(), Position.Y.ToString(), backPlaneZValue.ToString()];
    }
}
