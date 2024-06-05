using A2m.Server;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.SpiderBoss.States;
public class AIStateSpiderSwichSideComp : BaseAIState<AIStateSpiderSwichSide>
{
    public override string StateName => "AIStateSpiderSwichSide";

    public bool StartRight => ComponentData.StartRight;
    public float XLeft => ComponentData.XLeft;
    public float XRight => ComponentData.XRight;
    public float[] WaitTime => ComponentData.WaitTime;

    // Provide Direction
    public override ExtLevelEditor.ComponentSettings GetSettings() => throw new NotImplementedException();
}
