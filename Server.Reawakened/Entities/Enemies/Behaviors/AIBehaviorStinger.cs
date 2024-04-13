using Server.Reawakened.Entities.Enemies.Behaviors.Abstractions;
using Server.Reawakened.Entities.Enemies.EnemyTypes;
using Server.Reawakened.XMLs.Data.Enemy.Enums;

namespace Server.Reawakened.Entities.Enemies.Behaviors;

public class AIBehaviorStinger(StingerProperties properties, BehaviorEnemy enemy, StateType state) : AIBaseBehavior(enemy, state)
{
    public override bool ShouldDetectPlayers => false;

    public override AiProperties GetProperties() => properties;

    public override object[] GetStartArgs() =>
        [
            Enemy.AiData.Sync_TargetPosX,
            Enemy.AiData.Sync_TargetPosY,
            0, // Target will always be on first plane
            Enemy.AiData.Intern_SpawnPosX,
            Enemy.AiData.Intern_SpawnPosY,
            Enemy.AiData.Intern_SpawnPosZ,
            properties.speedForward,
            properties.speedBackward
        ];

    public override void NextState() =>
        Enemy.ChangeBehavior(Enemy.GenericScript.AwareBehavior, Enemy.Position.x, Enemy.Position.y, Enemy.AiData.Intern_Dir);
}
