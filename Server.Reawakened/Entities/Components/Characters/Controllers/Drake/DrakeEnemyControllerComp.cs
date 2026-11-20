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

    public bool IsImmune = true;
    public bool IsSpinning = false;
    
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

        else if (CurrentState is AIStateDrakeAttackComp attack)
        {
            if (attack.EndTime != 0f && Room.Time >= attack.EndTime)
            {
                AddNextState<AIStateMoveComp>();
                GoToNextState();
            }
        }
        
        else if (CurrentState is AIStateMoveComp move)
        {
            if (Room.Time >= move.EndTime)
            {
                AddNextState<AIStatePatrolComp>();
                GoToNextState();

                _hasStarted = false;
            }
        }
        
        else if (CurrentState is AIStateStunnedComp stunned)
        {
            if (Room.Time >= stunned.WaitTime)
            {
                AddNextState<AIStateMoveComp>();
                GoToNextState();
            }
        }

        base.Update();
    }

    private void SetupStateVariables()
    {
        var patrolComp = Room.GetEntityFromId<AIStatePatrolComp>(Id);
        var attackComp = Room.GetEntityFromId<AIStateDrakePlacementComp>(Id);

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
