using Server.Reawakened.Entities.Enemies.BehaviorEnemies.Abstractions;
using Server.Reawakened.XMLs.Models.Enemy.Enums;

namespace Server.Reawakened.Entities.Enemies.BehaviorEnemies.BehaviourTypes;
public class AIBehaviorIdle : AIBaseBehavior
{
    public override float ResetTime => 0;

    protected override AI_Behavior GetBehaviour() => new AI_Behavior_Idle();

    public override StateType GetBehavior() => StateType.Idle;

    public override object[] GetData() => [];
}
