using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.States;
using Server.Reawakened.Entities.Components.Characters.Controllers.Drake.States;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Drake;

public class DrakeEnemyControllerComp : DamagableAiStateMachine<DrakeEnemyController>
{
    /* 
     * -- AI STATES --
     * AIStateDrakeAttack
     * AIStateDrakePlacement
     * 
     * AIStateMove
     * AIStatePatrol
     * AIStateStunned
    */

    public bool IsImmune = false;
    public bool IsSpinning = false;

    public override void DelayedComponentInitialization()
    {
        SetupStateVariables();

        AddNextState<AIStatePatrolComp>();

        GoToNextState();
    }

    private void SetupStateVariables()
    {
        var patrolComp = Room.GetEntityFromId<AIStatePatrolComp>(Id);
        var attackComp = Room.GetEntityFromId<AIStateDrakePlacementComp>(Id);

        if (patrolComp != null && attackComp != null)
            patrolComp.DetectionAiState = attackComp;
    }
}
