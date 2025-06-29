using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.States;
using Server.Reawakened.Entities.DataComponentAccessors.Spiderling.States;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Spiderling.States;
public class AIStateSpiderlingAlertComp : BaseAIState<AIStateSpiderlingAlertMQR>
{
    public override string StateName => "AIStateSpiderlingAlert";


    public float FxWaitDuration = 0;
    public float AlertTime => ComponentData.AlertTime + FxWaitDuration;

    private float _alertTime = 0;

    public override void StartState() => _alertTime = Room.Time + AlertTime;

    public override void UpdateState()
    {
        if (Room.Time < _alertTime)
            return;

        AddNextState<AIStatePatrolComp>();
        GoToNextState();
    }
}
