using static A2m.Server.ExtLevelEditor;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using Server.Reawakened.Entities.DataComponentAccessors.Base.States;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Base.States;
public class AIStateWait2Comp : BaseAIState<AIStateWait2MQR, AI_State>
{
    public override string StateName => "AIStateWait2";
    public float WaitDuration => ComponentData.WaitDuration;

    public float WaitTime { get; private set; } = 0;
    
    public override AI_State GetInitialAIState() => new([], loop: false);

    public override void OnAIStateIn() => WaitTime = Room.Time + WaitDuration;
}
