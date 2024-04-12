using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using Server.Reawakened.Entities.DataComponentAccessors.Base.States;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Base.States;
public class AIStateWait2Comp : BaseAIState<AIStateWait2MQR>
{
    public override string StateName => "AIStateWait2";

    public float WaitDuration => ComponentData.WaitDuration;
}
