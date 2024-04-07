using Server.Reawakened.Entities.Enemies.Behaviors.Abstractions;
using Server.Reawakened.Entities.Enemies.EnemyTypes;
using Server.Reawakened.XMLs.Data.Enemy.States;

namespace Server.Reawakened.Entities.Enemies.Behaviors;

public class AIBehaviorGrenadier(GrenadierState grenadierState) : AIBaseBehavior
{
    public float GInTime => grenadierState.GInTime;
    public float GLoopTime => grenadierState.GLoopTime;
    public float GOutTime => grenadierState.GOutTime;
    public bool IsTracking => grenadierState.IsTracking;
    public int ProjCount => grenadierState.ProjCount;
    public float ProjSpeed => grenadierState.ProjSpeed;
    public float MaxHeight => grenadierState.MaxHeight;

    public override bool ShouldDetectPlayers => false;

    protected override AI_Behavior GetBehaviour() =>
        new AI_Behavior_Grenadier(GInTime, GLoopTime, GOutTime, IsTracking, ProjCount, ProjSpeed, MaxHeight);

    public override object[] GetData() => [
        GInTime, GLoopTime, GOutTime,
        ProjCount, ProjSpeed, MaxHeight
    ];

    public override void NextState(BehaviorEnemy enemy) =>
        enemy.ChangeBehavior(enemy.GenericScript.AwareBehavior, enemy.Position.x, enemy.Position.y, enemy.AiData.Intern_Dir);
}
