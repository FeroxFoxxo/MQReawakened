using Server.Reawakened.Rooms.Models.Entities;

namespace Server.Reawakened.Entities.Enemies.AIStateEnemies.Drake.AIStates;
public class AIStateDrakePlacementComp : Component<AIStateDrakePlacement>
{
    public float MovementSpeed => ComponentData.MovementSpeed;
    public float AttackInAnimDuration => ComponentData.AttackInAnimDuration;
    public float AttackLoopAnimDuration => ComponentData.AttackLoopAnimDuration;
    public float AttackRange => ComponentData.AttackRange;
    public float AttackRangeMaximum => ComponentData.AttackRangeMaximum;
}
