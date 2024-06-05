using Server.Reawakened.Entities.Enemies.Behaviors.Abstractions;
using Server.Reawakened.Entities.Enemies.EnemyTypes;
using Server.Reawakened.XMLs.Data.Enemy.Enums;

namespace Server.Reawakened.Entities.Enemies.Behaviors;
public class AIBehaviorActing(ActingProperties properties, BehaviorEnemy enemy, StateType state) : AIBaseBehavior(enemy, state)
{
    // TODO: CREATE LOGIC FLOW FOR ACTING STATES
    public ActingState ActingState = ActingState.Idle;

    public bool SnapOnGround => properties.lookAround_SnapOnGround;

    public override bool ShouldDetectPlayers => false;

    public override AiProperties GetProperties() => properties;

    public override object[] GetStartArgs() => [Enum.GetName(ActingState)];

    // TODO: ADD CODE FOR CALCULATING NEXT STATE
    public override void NextState() => throw new NotImplementedException();
}
