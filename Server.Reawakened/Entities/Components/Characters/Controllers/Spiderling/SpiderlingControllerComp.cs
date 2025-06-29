using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.States;
using Server.Reawakened.Entities.Components.Characters.Controllers.Spiderling.States;
using Server.Reawakened.Entities.DataComponentAccessors.Spiderling;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Spiderling;
public class SpiderlingControllerComp : DamagableAiStateMachine<SpiderlingControllerMQR>
{
    /* 
     * -- AI STATES --
     * AIStateSpiderlingAlert
     * AIStateSpiderlingAttack
     * AIStateSpiderlingDigOut
     * 
     * AIStatePatrol
     * AIStateStunned
    */

    public bool StartIdle => ComponentData.StartIdle;
    public float TimeToDirtFXInTaunt => ComponentData.TimeToDirtFXInTaunt;

    public override void DelayedComponentInitialization()
    {
        SetupStateVariables();

        if (StartIdle)
            AddNextState<AIStateSpiderlingDigOutComp>();
        else
            AddNextState<AIStatePatrolComp>();

        GoToNextState();
    }

    private void SetupStateVariables()
    {
        var alertComp = Room.GetEntityFromId<AIStateSpiderlingAlertComp>(Id);

        if (alertComp != null)
            alertComp.FxWaitDuration = TimeToDirtFXInTaunt;

        var patrolComp = Room.GetEntityFromId<AIStatePatrolComp>(Id);
        var attackComp = Room.GetEntityFromId<AIStateSpiderlingAttackComp>(Id);

        if (patrolComp != null && attackComp != null)
            patrolComp.DetectionAiState = attackComp;
    }
}
