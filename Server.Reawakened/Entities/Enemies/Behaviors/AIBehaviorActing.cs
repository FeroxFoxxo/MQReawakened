using Server.Reawakened.Entities.Enemies.Behaviors.Abstractions;
using Server.Reawakened.Entities.Enemies.EnemyTypes;
using Server.Reawakened.XMLs.Data.Enemy.Enums;

namespace Server.Reawakened.Entities.Enemies.Behaviors;
public class AIBehaviorActing(BehaviorEnemy enemy, ActingProperties fallback) : AIBaseBehavior(enemy.AiData, enemy.Room)
{
    // TODO: CREATE LOGIC FLOW FOR ACTING STATES
    public ActingState ActingState = ActingState.Idle;

    public bool SnapOnGround => GetInternalProperties().lookAround_SnapOnGround;

    public override bool ShouldDetectPlayers => false;

    public override AiProperties GetProperties() => GetInternalProperties();
    private ActingProperties GetInternalProperties() => new(
        Fallback(enemy.Global.LookAround_SnapOnGround, fallback.lookAround_SnapOnGround)
    );

    public override object[] GetStartArgs() => [Enum.GetName(ActingState)];

    public override StateType GetStateType() => StateType.Acting;

    // TODO: ADD CODE FOR CALCULATING NEXT STATE
    public override void NextState() => throw new NotImplementedException();
}
