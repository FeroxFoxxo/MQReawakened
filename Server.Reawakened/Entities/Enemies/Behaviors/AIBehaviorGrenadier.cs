using Server.Reawakened.Entities.Enemies.Behaviors.Abstractions;
using Server.Reawakened.Entities.Enemies.EnemyTypes;
using Server.Reawakened.XMLs.Data.Enemy.Enums;

namespace Server.Reawakened.Entities.Enemies.Behaviors;

public class AIBehaviorGrenadier(GrenadierProperties properties, BehaviorEnemy enemy) : AIBaseBehavior(enemy)
{
    public float GInTime => properties.inTime;
    public float GLoopTime => properties.loopTime;
    public float GOutTime => properties.outTime;
    public bool IsTracking => properties.isTracking;
    public int ProjCount => properties.projCount;
    public float ProjSpeed => properties.projSpeed;
    public float MaxHeight => properties.maxHeight;

    public override bool ShouldDetectPlayers => false;

    public override StateType State => StateType.Grenadier;

    public override object[] GetProperties() => [
        GInTime, GLoopTime, GOutTime,
        ProjCount, ProjSpeed, MaxHeight
    ];

    public override object[] GetStartArgs() => [];

    public override void NextState() =>
        Enemy.ChangeBehavior(Enemy.GenericScript.AwareBehavior, Enemy.Position.x, Enemy.Position.y, Enemy.AiData.Intern_Dir);
}
