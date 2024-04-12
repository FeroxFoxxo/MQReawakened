using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using Server.Reawakened.Entities.DataComponentAccessors.Hampster.States;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Hamster.States;
public class AIStateHamsterAttackComp : BaseAIState<AIStateHamsterAttackMQR>
{
    public float InTime => ComponentData.InTime;
    public float LoopTime => ComponentData.LoopTime;
    public float OutTime => ComponentData.OutTime;
    public float JumpHeight => ComponentData.JumpHeight;
}
