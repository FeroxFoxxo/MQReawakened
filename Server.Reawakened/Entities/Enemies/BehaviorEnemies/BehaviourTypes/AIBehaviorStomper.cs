using Server.Reawakened.Entities.Enemies.BehaviorEnemies.Abstractions;
using Server.Reawakened.XMLs.Models.Enemy.Enums;
using Server.Reawakened.XMLs.Models.Enemy.States;

namespace Server.Reawakened.Entities.Enemies.BehaviorEnemies.BehaviourTypes;

public class AIBehaviorStomper(StomperState stomperState) : AIBaseBehavior
{
    public float AttackTime => stomperState.AttackTime;
    public float ImpactTime => stomperState.ImpactTime;
    public float DamageDistance => stomperState.DamageDistance;
    public float DamageOffset => stomperState.DamageOffset;

    public override float ResetTime => AttackTime;

    protected override AI_Behavior GetBehaviour() => new AI_Behavior_Stomper(AttackTime, ImpactTime);

    public override StateType GetBehavior() => StateType.Stomper;

    public override object[] GetData() => [
        AttackTime, ImpactTime, DamageDistance, DamageOffset
    ];
}
