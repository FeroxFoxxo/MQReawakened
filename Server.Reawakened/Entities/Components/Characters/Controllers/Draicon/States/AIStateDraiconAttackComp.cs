using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using static AIStateDraiconAttack;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Draicon.States;
public class AIStateDraiconAttackComp : BaseAIState<AIStateDraiconAttack>
{
    public float AttackRecoveryDuration => ComponentData.AttackRecoveryDuration;
    public float AttackRange => ComponentData.AttackRange;
    public float ReloadDuration => ComponentData.ReloadDuration;
    public float MovementSpeed => ComponentData.MovementSpeed;
    public float AttackAnimDuration => ComponentData.AttackAnimDuration;
    public float DelayBeforeFiring => ComponentData.DelayBeforeFiring;
    public float ProjectileSpeed => ComponentData.ProjectileSpeed;
    public ProjectilePatternType ProjectileAmount => ComponentData.ProjectileAmount;
}
