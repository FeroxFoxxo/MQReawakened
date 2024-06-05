using A2m.Server;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using Server.Reawakened.Entities.DataComponentAccessors.Base.States;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Base.States;
public class AIStateMoveComp : BaseAIState<AIStateMoveMQR>
{
    public override string StateName => "AIStateMove";

    public float MovementSpeed => ComponentData.MovementSpeed;

    // Provide Initial And Target Positions
    public override ExtLevelEditor.ComponentSettings GetSettings() => throw new NotImplementedException();
}
