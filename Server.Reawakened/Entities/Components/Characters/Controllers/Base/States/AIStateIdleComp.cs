using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using Server.Reawakened.Entities.DataComponentAccessors.Base.States;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Base.States;
public class AIStateIdleComp : BaseAIState<AIStateIdleMQR>
{
    public override string StateName => "AIStateIdle";

    public override void UpdateState()
    {
        var patrolComp = Room.GetEntityFromId<AIStatePatrolComp>(Id);

        if (patrolComp == null)
            return;

        var closestPlayer = patrolComp.GetClosestPlayer();

        if (closestPlayer == null)
            return;

        AddNextState<AIStateWaitComp>();
        GoToNextState();
    }
}
