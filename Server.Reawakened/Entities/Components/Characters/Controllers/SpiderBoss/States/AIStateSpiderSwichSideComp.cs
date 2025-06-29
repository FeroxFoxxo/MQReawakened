using A2m.Server;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.SpiderBoss.States;
public class AIStateSpiderSwichSideComp : BaseAIState<AIStateSpiderSwichSide, AI_State>
{
    public override string StateName => "AIStateSpiderSwichSide";

    public bool StartRight => ComponentData.StartRight;
    public float XLeft => ComponentData.XLeft;
    public float XRight => ComponentData.XRight;
    public float[] WaitTime => ComponentData.WaitTime;

    public override AI_State GetInitialAIState() => new([], false);

    public override ExtLevelEditor.ComponentSettings GetSettings() => [StateMachine.GetForceDirectionX().ToString()];

    public override void StateIn() => StateMachine.SetForceDirectionX((!StartRight) ? 1 : (-1));
}
