using A2m.Server;
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
        TimerThread.DelayCall(RunPatrolState, null, TimeSpan.FromSeconds(1), TimeSpan.Zero, 1);

    // Provide Initial And Placement Positions
    public override ExtLevelEditor.ComponentSettings GetSettings()
    {
        var backPlaneZValue = ParentPlane == ServerRConfig.BackPlane ?
                      ServerRConfig.Planes[ServerRConfig.FrontPlane] :
                       ServerRConfig.Planes[ServerRConfig.BackPlane];

        return [Position.X.ToString(), Position.Y.ToString(), backPlaneZValue.ToString(),
                Position.X.ToString(), Position.Y.ToString(), backPlaneZValue.ToString()];
    }

    public void RunPatrolState(object _)
    {
        if (Room == null)
            return;

        AddNextState<AIStatePatrolComp>();

        GoToNextState();
    }
}
