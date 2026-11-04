using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.States;
using Server.Reawakened.Entities.Components.Characters.Controllers.Spiker.States;
using Server.Reawakened.Entities.DataComponentAccessors.Spiker;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Spiker;
public class SpikerComponentComp : DamagableAiStateMachine<SpikerControllerMQR>
{
    /* 
     * -- AI STATES --
     * AIStateSpikerAttackComp
     * 
     * AIStatePatrol
     * AIStateIdle
     * AIStateWait
     * AIStateStunned
    */

    public bool StartIdle => ComponentData.StartIdle;
    public float TimeToDirtFXInTaunt => ComponentData.TimeToDirtFXInTaunt;

    private AIStatePatrolComp _patrol;

    private bool _hasStarted = false;

    public override void DelayedComponentInitialization()
    {
        SetupStateVariables();

        Room.GetCollidersById(Id).FirstOrDefault()?.Active = false;
        
        if (StartIdle)
            AddNextState<AIStateIdleComp>();
        else
            AddNextState<AIStatePatrolComp>();
    }

    public override void Update()
    {
        if (Room == null)
            return;

        if (CurrentStates.Length <= 0)
            GoToNextState();

        else if (!_hasStarted && CurrentStates.Any(state => state is AIStateIdleComp) && _patrol is not null)
        {
            if (_patrol.GetClosestPlayer() is not null)
            {
                _patrol.StartDetectedState();
                _hasStarted = true;
            }
        }

        else if (CurrentStates.OfType<AIStateWaitComp>().FirstOrDefault() is AIStateWaitComp wait)
        {
            if (Room.Time >= wait.WaitTime)
            {
                AddNextState<AIStateIdleComp>();
                GoToNextState();
                
                Room.GetCollidersById(Id).FirstOrDefault()?.Active = false;
            }
        }
        
        else if (CurrentStates.OfType<AIStateStunnedComp>().FirstOrDefault() is AIStateStunnedComp stunned)
        {
            if (Room.Time >= stunned.WaitTime)
            {
                AddNextState<AIStateWaitComp>();
                GoToNextState();
            }
        }
        
        else if (CurrentStates.OfType<AIStateIdleComp>().FirstOrDefault() is AIStateIdleComp idle)
        {
            if (Room.Time >= idle.WaitTime)
            {
                AddNextState<AIStatePatrolComp>();
                GoToNextState();
            }
        }

        base.Update();
    }

    private void SetupStateVariables()
    {
        var patrolComp = Room.GetEntityFromId<AIStatePatrolComp>(Id);
        var attackComp = Room.GetEntityFromId<AIStateSpikerAttackComp>(Id);

        _patrol = patrolComp;

        if (patrolComp != null && attackComp != null)
            patrolComp.DetectionAiState = attackComp;
    }

    public override void EnemyDamaged(bool isDead)
    {
        if (isDead)
            return;

        if (CurrentStates.Any(state => state is AIStateIdleComp or AIStatePatrolComp) && _patrol is not null &&
            Room.Time - _patrol.DetectionStartTime >= _patrol.MinimumTimeBeforeDetection)
            _patrol.StartDetectedState();
    }
}
