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
        
        else if (CurrentStates.OfType<AIStateWait2Comp>().FirstOrDefault() is AIStateWait2Comp wait2)
        {
            if (Room.Time >= wait2.WaitTime)
            {
                AddNextState<AIStateWaitComp>();
                GoToNextState();
            }
        }
        
        else if (CurrentStates.OfType<AIStateWaitComp>().FirstOrDefault() is AIStateWaitComp wait)
        {
            if (Room.Time >= wait.WaitTime)
            {
                AddNextState<AIStatePatrolComp>();
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

        base.Update();
    }
    
    private void SetupStateVariables()
    {
        var patrolComp = Room.GetEntityFromId<AIStatePatrolComp>(Id);
        var attackComp = Room.GetEntityFromId<AIStateHamsterAttackComp>(Id);

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
