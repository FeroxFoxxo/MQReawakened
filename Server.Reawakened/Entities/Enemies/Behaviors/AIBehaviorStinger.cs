using Server.Reawakened.Entities.Enemies.Behaviors.Abstractions;
using Server.Reawakened.Entities.Enemies.EnemyTypes;
using Server.Reawakened.XMLs.Data.Enemy.Enums;

namespace Server.Reawakened.Entities.Enemies.Behaviors;

public class AIBehaviorStinger(BehaviorEnemy enemy, StingerProperties fallback) : AIBaseBehavior(enemy.AiData, enemy.Room)
{
    public override bool ShouldDetectPlayers => false;

    public override AiProperties GetProperties() => GetInternalProperties();

    private StingerProperties GetInternalProperties() => fallback;

    public override object[] GetStartArgs() {
        var properties = GetInternalProperties();

        return [
            _aiData.Sync_TargetPosX,
            _aiData.Sync_TargetPosY,
            0, // Target will always be on first plane
            _aiData.Intern_SpawnPosX,
            _aiData.Intern_SpawnPosY,
            _aiData.Intern_SpawnPosZ,
            properties.speedForward,
            properties.speedBackward
        ]; }

    public override StateType GetStateType() => StateType.Stinger;

    public override void NextState() =>
        enemy.ChangeBehavior(enemy.Global.AwareBehavior, enemy.Position.x, enemy.Position.y, _aiData.Intern_Dir);
}
