using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using Server.Reawakened.Entities.DataComponentAccessors.Base.States;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Base.States;
public class AIStateWait2Comp : BaseAIState<AIStateWait2MQR>
{
    public float WaitDuration => ComponentData.WaitDuration;
}
