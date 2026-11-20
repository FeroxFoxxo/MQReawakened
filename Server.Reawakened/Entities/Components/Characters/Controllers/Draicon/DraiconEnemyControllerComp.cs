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

    public bool IsImmune = true;
    
    private AIStatePatrolComp _patrol;
    
    private bool _hasStarted = false;

    public override void DelayedComponentInitialization()
    {
        SetupStateVariables();

        AddNextState<AIStatePatrolComp>();
    }
    
    public override void Update()
    {
        if (Room == null)
            return;

        if (CurrentState == null)
            GoToNextState();

        else if (!_hasStarted && CurrentState is AIStatePatrolComp && _patrol is not null)
        {
            if (_patrol.GetClosestPlayer() is not null)
            {
                _patrol.StartDetectedState();
                _hasStarted = true;
            }
        }
        
        else if (CurrentState is AIStateDraiconAttackComp attack)
        {
            if (attack.EndTime != 0f && Room.Time >= attack.EndTime)
            {
                AddNextState<AIStateWaitComp>();
                GoToNextState();
            }
        }
        
        else if (CurrentState is AIStateWaitComp wait)
        {
            if (Room.Time >= wait.WaitTime)
            {
                AddNextState<AIStatePatrolComp>();
                GoToNextState();
                
                _hasStarted = false;
                IsImmune = true;
            }
        }
        
        else if (CurrentState is AIStateStunnedComp stunned)
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
        var attackComp = Room.GetEntityFromId<AIStateDraiconAttackComp>(Id);

        _patrol = patrolComp;
        
        if (patrolComp != null && attackComp != null)
            patrolComp.DetectionAiState = attackComp;
    }
    
    public override void EnemyDamaged(bool isDead)
    {
        if (isDead)
            return;

        if (CurrentState is AIStatePatrolComp && _patrol is not null &&
            Room.Time - _patrol.DetectionStartTime >= _patrol.MinimumTimeBeforeDetection)
            _patrol.StartDetectedState();
    }
}
