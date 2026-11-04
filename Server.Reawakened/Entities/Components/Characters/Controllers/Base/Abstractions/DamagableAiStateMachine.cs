using Server.Reawakened.Entities.Components.Characters.Controllers.Base.States;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
public abstract class DamagableAiStateMachine<T> : BaseAIStateMachine<T>, IAIDamageEnemy
{
    public virtual void EnemyDamaged(bool isDead)
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
