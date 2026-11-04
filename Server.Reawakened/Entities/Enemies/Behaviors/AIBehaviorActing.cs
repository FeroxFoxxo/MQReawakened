using Server.Reawakened.Entities.Enemies.Behaviors.Abstractions;
using Server.Reawakened.Entities.Enemies.EnemyTypes;
using Server.Reawakened.XMLs.Data.Enemy.Enums;

namespace Server.Reawakened.Entities.Enemies.Behaviors;
public class AIBehaviorActing(BehaviorEnemy enemy, ActingProperties fallback) : AIBaseBehavior(enemy.AiData, enemy.Room)
{
    // TODO: CREATE LOGIC FLOW FOR ACTING STATES
    public string ActingState => enemy.AiData.Intern_AnimName;

    public bool SnapOnGround => GetInternalProperties().lookAround_SnapOnGround;

    public override bool ShouldDetectPlayers => false;
    public override bool ShouldAggroOnHit => false;

    public override AiProperties GetProperties() => GetInternalProperties();

    // Fallback here is false due to this being a bool value,
    // and also because it's typically false for all enemies.
    private ActingProperties GetInternalProperties() => new(
        Fallback(enemy.Global.LookAround_SnapOnGround, false)
    );

    public override object[] GetStartArgs(bool _) => [ActingState];

    public override StateType GetStateType() => StateType.Acting;

    // TODO: ADD CODE FOR CALCULATING NEXT STATE
    public override void NextState() =>
        enemy.ChangeBehavior(
            enemy.Global.AwareBehavior,
            enemy.Global.UnawareBehavior == StateType.ComeBack ? enemy.Position.X : enemy.AiData.Sync_TargetPosX,
            enemy.Global.UnawareBehavior == StateType.ComeBack ? enemy.Position.Y : enemy.AiData.Sync_TargetPosY,
            enemy.AiData.Intern_Dir
        );

    public override float GetBehaviorTime() => enemy.AiData.Intern_BehaviorRequestTime;
}
