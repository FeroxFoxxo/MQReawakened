using Server.Reawakened.Entities.Enemies.Behaviors.Abstractions;
using Server.Reawakened.Entities.Enemies.EnemyTypes;
using Server.Reawakened.XMLs.Data.Enemy.Enums;

namespace Server.Reawakened.Entities.Enemies.Behaviors;

public class AIBehaviorLookAround(BehaviorEnemy enemy, LookAroundProperties fallback) : AIBaseBehavior(enemy.AiData, enemy.Room)
{
    public override bool ShouldDetectPlayers => true;

    public override AiProperties GetProperties() =>
        new LookAroundProperties(
            Fallback(enemy.Global.LookAround_LookTime, fallback.lookAround_LookTime),
            Fallback(enemy.Global.LookAround_StartDirection, fallback.lookAround_StartDirection),
            Fallback(enemy.Global.LookAround_ForceDirection, fallback.lookAround_ForceDirection),
            Fallback(enemy.Global.LookAround_InitialProgressRatio, fallback.lookAround_InitialProgressRatio),
            Fallback(enemy.Global.LookAround_SnapOnGround, fallback.lookAround_SnapOnGround)
        );

    public override object[] GetStartArgs() => [];

    public override StateType GetStateType() => StateType.LookAround;

    public override void NextState()
    {
        enemy.ChangeBehavior(
            enemy.Global.UnawareBehavior,
            enemy.Position.x,
            enemy.Global.UnawareBehavior == StateType.ComeBack ? _aiData.Intern_SpawnPosY : enemy.Position.y,
            _aiData.Intern_Dir
        );

        enemy.CurrentBehavior.MustDoComeback();
    }

    public override float GetBehaviorTime() => enemy.Global.LookAround_LookTime;
}
