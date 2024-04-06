using Server.Reawakened.Entities.Components.AI.Stats;
using Server.Reawakened.Entities.Enemies.Behaviors.Abstractions;
using Server.Reawakened.Entities.Enemies.EnemyTypes;
using Server.Reawakened.XMLs.Data.Enemy.Enums;
using Server.Reawakened.XMLs.Data.Enemy.States;

namespace Server.Reawakened.Entities.Enemies.Behaviors;

public class AIBehaviorComeBack(ComeBackState comeBackState, AIStatsGlobalComp globalComp) : AIBaseBehavior
{
    public float ComeBackSpeed => globalComp.ComeBack_MoveSpeed != globalComp.Default.ComeBack_MoveSpeed ? globalComp.ComeBack_MoveSpeed : comeBackState.ComeBackSpeed;

    public override bool ShouldDetectPlayers => true;

    protected override AI_Behavior GetBehaviour() => new AI_Behavior_ComeBack(ComeBackSpeed);

    public override object[] GetData() => [ComeBackSpeed];

    public override void NextState(BehaviorEnemy enemy) =>
        enemy.ChangeBehavior(StateType.Patrol, enemy.Position.x, enemy.Position.y, enemy.Generic.Patrol_ForceDirectionX);
}
