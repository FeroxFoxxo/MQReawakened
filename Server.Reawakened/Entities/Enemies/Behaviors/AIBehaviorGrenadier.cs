using Server.Reawakened.Entities.Enemies.Behaviors.Abstractions;
using Server.Reawakened.Entities.Enemies.EnemyTypes;
using Server.Reawakened.XMLs.Data.Enemy.Enums;

namespace Server.Reawakened.Entities.Enemies.Behaviors;

public class AIBehaviorGrenadier(BehaviorEnemy enemy, GrenadierProperties fallback) : AIBaseBehavior(enemy.AiData, enemy.Room)
{
    public override bool ShouldDetectPlayers => false;

    public override AiProperties GetProperties() => fallback;

    public override object[] GetStartArgs() => [];

    public override StateType GetStateType() => StateType.Grenadier;

    public override void NextState() =>
        enemy.ChangeBehavior(enemy.Global.AwareBehavior, enemy.Position.x, enemy.Position.y, _aiData.Intern_Dir);
}
