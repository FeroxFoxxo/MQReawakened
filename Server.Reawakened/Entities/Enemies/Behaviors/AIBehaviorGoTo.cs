using Server.Reawakened.Entities.Enemies.Behaviors.Abstractions;
using Server.Reawakened.Entities.Enemies.EnemyTypes;
using Server.Reawakened.XMLs.Data.Enemy.Enums;

namespace Server.Reawakened.Entities.Enemies.Behaviors;
public class AIBehaviorGoTo(BehaviorEnemy enemy) : AIBaseBehavior(enemy.AiData, enemy.Room)
{
    public override bool ShouldDetectPlayers => true;

    // TO DO: CALCULATE WHAT THESE VALUES SHOULD BE
    public vector3 GoToPosition = new(0, 0, 0);
    public float Velocity = 0f;

    public override AiProperties GetProperties() => new EmptyAiProperties();

    public override object[] GetStartArgs() => [GoToPosition.x, GoToPosition.y, GoToPosition.z, Velocity];

    public override StateType GetStateType() => StateType.GoTo;

    // TODO: ADD CODE FOR CALCULATING NEXT STATE
    public override void NextState() => throw new NotImplementedException();
}
