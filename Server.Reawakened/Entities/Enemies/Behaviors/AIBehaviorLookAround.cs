using Server.Reawakened.Entities.Enemies.Behaviors.Abstractions;
using Server.Reawakened.Entities.Enemies.EnemyTypes;
using Server.Reawakened.XMLs.Data.Enemy.Enums;

namespace Server.Reawakened.Entities.Enemies.Behaviors;

public class AIBehaviorLookAround(LookAroundProperties properties, BehaviorEnemy enemy) : AIBaseBehavior(enemy)
{
    public float LookTime => Enemy.Global.LookAround_LookTime != Enemy.Global.Default.LookAround_LookTime ? Enemy.Global.LookAround_LookTime : properties.lookAround_LookTime;
    public int StartDirection => Enemy.Global.LookAround_StartDirection != Enemy.Global.Default.LookAround_StartDirection ? Enemy.Global.LookAround_StartDirection : properties.lookAround_StartDirection;
    public int ForceDirection => Enemy.Global.LookAround_ForceDirection != Enemy.Global.Default.LookAround_ForceDirection ? Enemy.Global.LookAround_ForceDirection : properties.lookAround_ForceDirection;
    public float InitialProgressRatio => Enemy.Global.LookAround_InitialProgressRatio != Enemy.Global.Default.LookAround_InitialProgressRatio ? Enemy.Global.LookAround_InitialProgressRatio : properties.lookAround_InitialProgressRatio;
    public bool SnapOnGround => Enemy.Global.LookAround_SnapOnGround != Enemy.Global.Default.LookAround_SnapOnGround ? Enemy.Global.LookAround_SnapOnGround : properties.lookAround_SnapOnGround;

    public override bool ShouldDetectPlayers => true;

    public override StateType State => StateType.LookAround;

    public override object[] GetProperties() => [
            LookTime, StartDirection, ForceDirection,
            InitialProgressRatio, SnapOnGround
        ];

    public override object[] GetStartArgs() => [];

    public override void NextState()
    {
        Enemy.ChangeBehavior(
            Enemy.GenericScript.UnawareBehavior,
            Enemy.Position.x,
            Enemy.GenericScript.UnawareBehavior == StateType.ComeBack ? Enemy.AiData.Intern_SpawnPosY : Enemy.Position.y,
            Enemy.Generic.Patrol_ForceDirectionX
        );

        Enemy.AiBehavior.MustDoComeback();
    }
}
