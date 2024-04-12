using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using Server.Reawakened.Entities.DataComponentAccessors.Base.States;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Base.States;
public class AIStateWaitComp : BaseAIState<AIStateWaitMQR>
{
    public override string StateName => "AIStateWait";

    public float WaitDuration => ComponentData.WaitDuration;
}
