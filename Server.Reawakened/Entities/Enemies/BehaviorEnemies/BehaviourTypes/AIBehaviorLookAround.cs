using Server.Reawakened.Entities.Components;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies.Abstractions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.XMLs.Models.Enemy.Enums;
using Server.Reawakened.XMLs.Models.Enemy.States;

namespace Server.Reawakened.Entities.Enemies.BehaviorEnemies.BehaviourTypes;

public class AIBehaviorLookAround(LookAroundState lookAroundState, AIStatsGlobalComp globalComp) : AIBaseBehavior
{
    public float LookTime => globalComp.LookAround_LookTime != default ? globalComp.LookAround_LookTime : lookAroundState.LookTime;
    public float StartDirection => globalComp.LookAround_StartDirection != default ? globalComp.LookAround_StartDirection : lookAroundState.StartDirection;
    public float ForceDirection => globalComp.LookAround_ForceDirection != default ? globalComp.LookAround_ForceDirection : lookAroundState.ForceDirection;
    public float InitialProgressRatio => globalComp.LookAround_InitialProgressRatio != default ? globalComp.LookAround_InitialProgressRatio : lookAroundState.InitialProgressRatio;
    public bool SnapOnGround => globalComp.LookAround_SnapOnGround != default ? globalComp.LookAround_SnapOnGround : lookAroundState.SnapOnGround;

    public override float ResetTime => LookTime;

    protected override AI_Behavior GetBehaviour() => new AI_Behavior_LookAround(LookTime, InitialProgressRatio, SnapOnGround);

    public override StateTypes GetBehavior() => StateTypes.LookAround;

    public override string ToString()
    {
        var sb = new SeparatedStringBuilder(';');

        sb.Append(LookTime);
        sb.Append(StartDirection);
        sb.Append(ForceDirection);
        sb.Append(InitialProgressRatio);
        sb.Append(SnapOnGround ? 1 : 0);

        return sb.ToString();
    }
}
