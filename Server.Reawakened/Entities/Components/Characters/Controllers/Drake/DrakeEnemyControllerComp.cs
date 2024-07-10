using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using Server.Reawakened.Entities.Components.Characters.Controllers.Drake.States;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Drake;

public class DrakeEnemyControllerComp : BaseAIStateMachine<DrakeEnemyController>
{
    /* 
     * -- AI STATES --
     * AIStateDrakeAttack
     * [DONE]AIStateDrakePlacement
     * 
     * AIStateMove
     * [DONE]AIStatePatrol
     * AIStateStunned
    */

    public override void DelayedComponentInitialization()
    {
        if (Room == null)
            return;

        AddNextState<AIStateDrakePlacementComp>();

        GoToNextState();
    }
}
