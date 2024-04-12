using Server.Reawakened.Entities.Components.AI.Stats;
using Server.Reawakened.Entities.Enemies.Behaviors.Abstractions;
using Server.Reawakened.Entities.Enemies.EnemyTypes;
using Server.Reawakened.XMLs.Data.Enemy.Enums;
using Server.Reawakened.XMLs.Data.Enemy.States;

namespace Server.Reawakened.Entities.Enemies.Behaviors;

public class AIBehaviorLookAround(LookAroundState lookAroundState, AIStatsGlobalComp globalComp) : AIBaseBehavior
{
    public float LookTime => globalComp.LookAround_LookTime != globalComp.Default.LookAround_LookTime ? globalComp.LookAround_LookTime : lookAroundState.LookTime;
    public float StartDirection => globalComp.LookAround_StartDirection != globalComp.Default.LookAround_StartDirection ? globalComp.LookAround_StartDirection : lookAroundState.StartDirection;
    public float ForceDirection => globalComp.LookAround_ForceDirection != globalComp.Default.LookAround_ForceDirection ? globalComp.LookAround_ForceDirection : lookAroundState.ForceDirection;
    public float InitialProgressRatio => globalComp.LookAround_InitialProgressRatio != globalComp.Default.LookAround_InitialProgressRatio ? globalComp.LookAround_InitialProgressRatio : lookAroundState.InitialProgressRatio;
    public bool SnapOnGround => globalComp.LookAround_SnapOnGround != globalComp.Default.LookAround_SnapOnGround ? globalComp.LookAround_SnapOnGround : lookAroundState.SnapOnGround;

    public override bool ShouldDetectPlayers => true;

    protected override AI_Behavior GetBehaviour() => new AI_Behavior_LookAround(LookTime, InitialProgressRatio, SnapOnGround);

    public override object[] GetData() => [
            LookTime, StartDirection, ForceDirection,
            InitialProgressRatio, SnapOnGround
        ];

    public override void NextState(BehaviorEnemy enemy)
    {
        enemy.ChangeBehavior(
            enemy.GenericScript.UnawareBehavior,
            enemy.Position.x,
            enemy.GenericScript.UnawareBehavior == StateType.ComeBack ? enemy.AiData.Intern_SpawnPosY : enemy.Position.y,
            enemy.Generic.Patrol_ForceDirectionX
        );

        enemy.AiBehavior.MustDoComeback(enemy.AiData);
    }
}
