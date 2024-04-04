using Server.Reawakened.Entities.Components.AI.Stats;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies.Abstractions;
using Server.Reawakened.XMLs.Models.Enemy.Enums;
using Server.Reawakened.XMLs.Models.Enemy.States;

namespace Server.Reawakened.Entities.Enemies.BehaviorEnemies.BehaviourTypes;

public class AIBehaviorComeBack(ComeBackState comeBackState, AIStatsGlobalComp globalComp) : AIBaseBehavior
{
    public float ComeBackSpeed => globalComp.ComeBack_MoveSpeed != globalComp.Default.ComeBack_MoveSpeed ? globalComp.ComeBack_MoveSpeed : comeBackState.ComeBackSpeed;

    public override float ResetTime => 0;

    protected override AI_Behavior GetBehaviour() => new AI_Behavior_ComeBack(ComeBackSpeed);

    public override StateType GetBehavior() => StateType.ComeBack;

    public override object[] GetData() => [ ComeBackSpeed ];
}
