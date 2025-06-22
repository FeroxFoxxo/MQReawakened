using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.States;
using Server.Reawakened.Entities.Components.Characters.Controllers.Spiker.States;
using Server.Reawakened.Entities.DataComponentAccessors.Spiker;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Spiker;
public class SpikerComponentComp : BaseAIStateMachine<SpikerControllerMQR>, IAIDamageEnemy
{
    public bool StartIdle => ComponentData.StartIdle;
    public float TimeToDirtFXInTaunt => ComponentData.TimeToDirtFXInTaunt;

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
            waitComp.FxWaitDuration = TimeToDirtFXInTaunt;

        var patrolComp = Room.GetEntityFromId<AIStatePatrolComp>(Id);
        var attackComp = Room.GetEntityFromId<AIStateSpikerAttackComp>(Id);

        if (patrolComp != null && attackComp != null)
            patrolComp.DetectionAiState = attackComp;
    }

    public void EnemyDamaged(bool isDead)
    {
        AddNextState<AIStateStunnedComp>();

        if (!isDead)
        {
            foreach (var aiState in CurrentStates)
            {
                if (typeof(AIStateStunnedComp).IsInstanceOfType(aiState))
                    continue;

                AddNextState(aiState.GetType());
            }
        }
        
        GoToNextState();
    }
}
