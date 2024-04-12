using Server.Reawakened.Entities.Enemies.Behaviors.Abstractions;
using Server.Reawakened.Entities.Enemies.EnemyTypes;
using Server.Reawakened.XMLs.Data.Enemy.Enums;
using Server.Reawakened.XMLs.Data.Enemy.States;

namespace Server.Reawakened.Entities.Enemies.Behaviors;

public class AIBehaviorGrenadier(GrenadierState grenadierState, BehaviorEnemy enemy) : AIBaseBehavior(enemy)
{
    public float GInTime => grenadierState.GInTime;
    public float GLoopTime => grenadierState.GLoopTime;
    public float GOutTime => grenadierState.GOutTime;
    public bool IsTracking => grenadierState.IsTracking;
    public int ProjCount => grenadierState.ProjCount;
    public float ProjSpeed => grenadierState.ProjSpeed;
    public float MaxHeight => grenadierState.MaxHeight;

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
