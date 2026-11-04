using static A2m.Server.ExtLevelEditor;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using Server.Reawakened.Entities.DataComponentAccessors.Base.States;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Base.States;
public class AIStateIdleComp : BaseAIState<AIStateIdleMQR, AI_State>
{
    public override string StateName => "AIStateIdle";

    public float WaitTime;
    
    public override AI_State GetInitialAIState() => new([], loop: true);

    public override void OnAIStateIn() => WaitTime = Room.Time + 2f;
}
