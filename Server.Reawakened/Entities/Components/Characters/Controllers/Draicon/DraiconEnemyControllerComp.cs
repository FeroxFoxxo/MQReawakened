using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.States;
using Server.Reawakened.Entities.Components.Characters.Controllers.Draicon.States;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Draicon;
public class DraiconEnemyControllerComp : DamagableAiStateMachine<DraiconEnemyController>
{
    /* 
     * -- AI STATES --
     * AIStateDraiconAttack
     * 
     * AIStatePatrol
     * AIStateWait
     * AIStateStunned
    */

    public bool IsImmune = false;

    public override void DelayedComponentInitialization()
    {
        SetupStateVariables();

        AddNextState<AIStatePatrolComp>();

        GoToNextState();
    }

    private void SetupStateVariables()
    {
        var patrolComp = Room.GetEntityFromId<AIStatePatrolComp>(Id);
        var attackComp = Room.GetEntityFromId<AIStateDraiconAttackComp>(Id);

        if (patrolComp != null && attackComp != null)
            patrolComp.DetectionAiState = attackComp;
    }
}
