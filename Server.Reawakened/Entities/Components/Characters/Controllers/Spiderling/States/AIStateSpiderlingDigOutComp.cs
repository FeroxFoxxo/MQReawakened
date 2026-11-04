using static A2m.Server.ExtLevelEditor;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using Server.Reawakened.Entities.DataComponentAccessors.Spiderling.States;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Spiderling.States;
public class AIStateSpiderlingDigOutComp : BaseAIState<AIStateSpiderlingDigOutMQR, AI_State>
{
    public override string StateName => "AIStateSpiderlingDigOut";

    public bool DigOutOnSpawn => ComponentData.DigOutOnSpawn;
    public float AnimationDuration => ComponentData.AnimationDuration;

    public float WaitTime = 0f;
    
    public override AI_State GetInitialAIState() => new(
        [
            new ((!DigOutOnSpawn) ? 0f : AnimationDuration, "DigOut")
        ], loop: false);

    public override void OnAIStateIn() => WaitTime = Room.Time + AnimationDuration;
}
