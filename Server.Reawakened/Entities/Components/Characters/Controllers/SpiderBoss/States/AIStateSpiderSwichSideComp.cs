using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.SpiderBoss.States;
public class AIStateSpiderSwichSideComp : BaseAIState<AIStateSpiderSwichSide>
{
    public bool StartRight => ComponentData.StartRight;
    public float XLeft => ComponentData.XLeft;
    public float XRight => ComponentData.XRight;
    public float[] WaitTime => ComponentData.WaitTime;
}
