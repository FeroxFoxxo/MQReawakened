using A2m.Server;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Drake.States;
public class AIStateDrakePlacementComp : BaseAIState<AIStateDrakePlacement>
{
    public override string StateName => "AIStateDrakePlacement";

    public float MovementSpeed => ComponentData.MovementSpeed;
    public float AttackInAnimDuration => ComponentData.AttackInAnimDuration;
    public float AttackLoopAnimDuration => ComponentData.AttackLoopAnimDuration;
    public float AttackRange => ComponentData.AttackRange;
    public float AttackRangeMaximum => ComponentData.AttackRangeMaximum;

    // Provide Initial And Placement Positions
    public override ExtLevelEditor.ComponentSettings GetSettings() => throw new NotImplementedException();
}
