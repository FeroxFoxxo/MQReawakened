using Server.Reawakened.Entities.Enemies.Behaviors.Abstractions;
using Server.Reawakened.Entities.Enemies.EnemyTypes;
using Server.Reawakened.XMLs.Data.Enemy.Enums;

namespace Server.Reawakened.Entities.Enemies.Behaviors;
public class AIBehaviorSpike(BehaviorEnemy enemy, SpikeProperties fallback) : AIBaseBehavior(enemy.AiData, enemy.Room)
{
    public override bool ShouldDetectPlayers => false;

    public override AiProperties GetProperties() => GetInternalProperties();

    private SpikeProperties GetInternalProperties() => fallback;

    public override object[] GetStartArgs()
    {
        var properties = GetInternalProperties();
        return [
            _aiData.Sync_TargetPosX,
            _aiData.Sync_TargetPosY,
            _aiData.Intern_SpawnPosZ,
            properties.speedForward,
            properties.speedBackward
        ] ;
    }

    public override StateType GetStateType() => StateType.Spike;

    public override void NextState() =>
        enemy.ChangeBehavior(enemy.Global.AwareBehavior, enemy.Position.x, enemy.Position.y, _aiData.Intern_Dir);
}
