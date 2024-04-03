using Server.Reawakened.Entities.Enemies.BehaviorEnemies.Abstractions;
using Server.Reawakened.XMLs.Models.Enemy.Enums;
using Server.Reawakened.XMLs.Models.Enemy.States;

namespace Server.Reawakened.Entities.Enemies.BehaviorEnemies.BehaviourTypes;

public class AIBehaviorGrenadier(GrenadierState grenadierState) : AIBaseBehavior
{
    public float GInTime => grenadierState.GInTime;
    public float GLoopTime => grenadierState.GLoopTime;
    public float GOutTime => grenadierState.GOutTime;
    public bool IsTracking => grenadierState.IsTracking;
    public int ProjCount => grenadierState.ProjCount;
    public float ProjSpeed => grenadierState.ProjSpeed;
    public float MaxHeight => grenadierState.MaxHeight;

    public override float ResetTime => GInTime + GLoopTime + GOutTime;

    protected override AI_Behavior GetBehaviour() => new AI_Behavior_Grenadier(GInTime, GLoopTime, GOutTime, IsTracking, ProjCount, ProjSpeed, MaxHeight);

    public override StateType GetBehavior() => StateType.Grenadier;

    public override object[] GetData() => [
        GInTime, GLoopTime, GOutTime,
        IsTracking, ProjCount, ProjSpeed, MaxHeight
    ];
}
