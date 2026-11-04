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

    private AIStatePatrolComp _patrol;

    private bool _hasStarted = false;
    
    public override void DelayedComponentInitialization()
    {
        SetupStateVariables();

        if (StartIdle)
            AddNextState<AIStateSpiderlingAlertComp>();
        else
            AddNextState<AIStatePatrolComp>();
    }

    public override void Update()
    {
        if (Room == null)
            return;

        if (CurrentStates.Length <= 0)
            GoToNextState();

        else if (!_hasStarted && CurrentStates.Any(state => state is AIStateSpiderlingAlertComp) && _patrol is not null)
        {
            if (_patrol.GetClosestPlayer() is not null)
            {
                _patrol.StartDetectedState();
                _hasStarted = true;
            }
        }

        else if (CurrentStates.OfType<AIStateSpiderlingAlertComp>().FirstOrDefault() is AIStateSpiderlingAlertComp alert)
        {
            if (Room.Time >= alert.WaitTime)
            {
                AddNextState<AIStatePatrolComp>();
                GoToNextState();
            }
        }
        
        else if (CurrentStates.OfType<AIStateStunnedComp>().FirstOrDefault() is AIStateStunnedComp stunned)
        {
            if (Room.Time >= stunned.WaitTime)
            {
                AddNextState<AIStateSpiderlingAlertComp>();
                GoToNextState();
            }
        }

        base.Update();
    }

    private void SetupStateVariables()
    {
        var patrolComp = Room.GetEntityFromId<AIStatePatrolComp>(Id);
        var attackComp = Room.GetEntityFromId<AIStateSpiderlingAttackComp>(Id);

        _patrol = patrolComp;
        
        if (patrolComp != null && attackComp != null)
            patrolComp.DetectionAiState = attackComp;
    }
    
    public override void EnemyDamaged(bool isDead)
    {
        if (isDead)
            return;

        if (CurrentStates.Any(state => state is AIStateSpiderlingAlertComp or AIStatePatrolComp) && _patrol is not null &&
            Room.Time - _patrol.DetectionStartTime >= _patrol.MinimumTimeBeforeDetection)
            _patrol.StartDetectedState();
    }
}
