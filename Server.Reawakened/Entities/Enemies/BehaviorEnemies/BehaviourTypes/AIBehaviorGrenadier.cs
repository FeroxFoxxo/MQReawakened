using Server.Reawakened.Entities.Enemies.BehaviorEnemies.Abstractions;
using Server.Reawakened.Players.Helpers;
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

    public override float ResetTime => 0;

    protected override AI_Behavior GetBehaviour() => new AI_Behavior_Grenadier(GInTime, GLoopTime, GOutTime, IsTracking, ProjCount, ProjSpeed, MaxHeight);

    public override StateTypes GetBehavior() => StateTypes.Grenadier;

    public override string ToString()
    {
        var sb = new SeparatedStringBuilder(';');

        sb.Append(GInTime);
        sb.Append(GLoopTime);
        sb.Append(GOutTime);
        sb.Append(IsTracking ? 1 : 0);
        sb.Append(ProjCount);
        sb.Append(ProjSpeed);
        sb.Append(MaxHeight);

        return sb.ToString();
    }
}
