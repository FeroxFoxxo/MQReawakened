using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using Server.Reawakened.Entities.DataComponentAccessors.Base.States;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Base.States;
public class AIStateWaitComp : BaseAIState<AIStateWaitMQR>
{
    public override string StateName => "AIStateWait";

    public float FxWaitDuration = 0;
    public float WaitDuration => ComponentData.WaitDuration + FxWaitDuration;

    private float _waitTime = 0;

    public override void StartState() => _waitTime = Room.Time + WaitDuration;

    public override void UpdateState()
    {
        if (Room.Time < _waitTime)
            return;

        AddNextState<AIStatePatrolComp>();
        GoToNextState();
    }
}
