using Server.Reawakened.Entities.Enemies.BehaviorEnemies.Abstractions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.XMLs.Models.Enemy.Enums;
using Server.Reawakened.XMLs.Models.Enemy.States;

namespace Server.Reawakened.Entities.Enemies.BehaviorEnemies.BehaviourTypes;

public class AIBehaviorBomber(BomberState bomberState) : AIBaseBehavior
{
    public float InTime => bomberState.InTime;
    public float LoopTime => bomberState.LoopTime;
    public float BombRadius => bomberState.BombRadius;

    public override float ResetTime => InTime + LoopTime;

    protected override AI_Behavior GetBehaviour() => new AI_Behavior_Bomber(InTime, LoopTime);

    public override StateTypes GetBehavior() => StateTypes.Bomber;

    public override string ToString()
    {
        var sb = new SeparatedStringBuilder(';');

        sb.Append(InTime);
        sb.Append(LoopTime);
        sb.Append(BombRadius);

        return sb.ToString();
    }
}
