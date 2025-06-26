using Server.Reawakened.Entities.Enemies.Behaviors.Abstractions;
using Server.Reawakened.Entities.Enemies.EnemyTypes;
using Server.Reawakened.XMLs.Data.Enemy.Enums;

namespace Server.Reawakened.Entities.Enemies.Behaviors;

public class AIBehaviorStomper(BehaviorEnemy enemy, StomperProperties fallback) : AIBaseBehavior(enemy.AiData, enemy.Room)
{
    public override bool ShouldDetectPlayers => false;

    public override AiProperties GetProperties() => fallback;

    public override object[] GetStartArgs() => [];

    public override StateType GetStateType() => StateType.Stomper;

    public override void NextState() =>
        enemy.ChangeBehavior(StateType.LookAround, enemy.Position.x, enemy.Position.y, enemy.Generic.Patrol_ForceDirectionX);
}
