using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.States;
using Server.Reawakened.Entities.Components.Characters.Controllers.Hamster.States;
using Server.Reawakened.Entities.DataComponentAccessors.Hampster;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Hamster;
public class HamsterControllerComp : DamagableAiStateMachine<HamsterControllerMQR>
{
    /* 
     * -- AI STATES --
     * AIStateHamsterAttack
     * 
     * AIStateIdle
     * AIStateMove
     * AIStatePatrol
     * AIStateStunned
     * AIStateWait
     * AIStateWait2
    */

    public bool StartIdle => ComponentData.StartIdle;
    public float TimeToDirtFXInStunOut => ComponentData.TimeToDirtFXInStunOut;

    public override void DelayedComponentInitialization()
    {
        SetupStateVariables();

        if (StartIdle)
            AddNextState<AIStateIdleComp>();
        else
            AddNextState<AIStatePatrolComp>();

        GoToNextState();
    }

    private void SetupStateVariables()
    {
        var waitComp = Room.GetEntityFromId<AIStateWaitComp>(Id);

        if (waitComp != null)
            waitComp.FxWaitDuration = TimeToDirtFXInStunOut;

        var patrolComp = Room.GetEntityFromId<AIStatePatrolComp>(Id);
        var attackComp = Room.GetEntityFromId<AIStateHamsterAttackComp>(Id);

        if (patrolComp != null && attackComp != null)
            patrolComp.DetectionAiState = attackComp;
    }
}
