using Server.Reawakened.Entities.Enemies.Behaviors.Abstractions;
using Server.Reawakened.Entities.Enemies.EnemyTypes;
using Server.Reawakened.XMLs.Data.Enemy.Enums;

namespace Server.Reawakened.Entities.Enemies.Behaviors;

public class AIBehaviorStinger(BehaviorEnemy enemy, StingerProperties fallback) : AIBaseBehavior(enemy.AiData, enemy.Room)
{
    public override bool ShouldDetectPlayers => false;
    public override bool ShouldAggroOnHit => false;

    public override AiProperties GetProperties() => GetInternalProperties();

    private StingerProperties GetInternalProperties() => 
        // These are "hardcoded", but they just match the data in the scripts
        new StingerProperties(
            16,
            8,
            0.43f,
            2.67f,
            1f,
            1f,
            3
        );

    public override object[] GetStartArgs(bool _) {
        var properties = GetInternalProperties();

        // Some of these are hardcoded
        return [
            _aiData.Sync_TargetPosX,
            _aiData.Sync_TargetPosY,
            1,
            _aiData.Sync_TargetPosX,
            _aiData.Sync_TargetPosY,
            10,
            16,
            8
        ]; }

    public override StateType GetStateType() => StateType.Stinger;

    public override void NextState() =>
        enemy.ChangeBehavior(enemy.Global.AwareBehavior, enemy.Position.X, enemy.Position.Y, _aiData.Intern_Dir);
}
