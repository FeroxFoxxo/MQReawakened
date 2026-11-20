using Server.Reawakened.Entities.Components.Characters.Controllers.Base.States;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
public abstract class DamagableAiStateMachine<T> : BaseAIStateMachine<T>, IAIDamageEnemy
{
    public virtual void EnemyDamaged(bool isDead)
    {
        if (isDead)
            return;
        
        AddNextState<AIStateStunnedComp>();
        GoToNextState();
    }
}
