using A2m.Server;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using Server.Reawakened.Entities.DataComponentAccessors.SpiderBoss.States;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.SpiderBoss.States;
public class AIStateSpiderMoveComp : BaseAIState<AIStateSpiderMoveMQR>
{
    public override string StateName => "AIStateSpiderMove";

    public float[] MovementSpeed => ComponentData.MovementSpeed;
    public float CeilingY => ComponentData.CeilingY;
    public float PatrolFromY => ComponentData.PatrolFromY;

    // Provide Initial And Target Position
    public override ExtLevelEditor.ComponentSettings GetSettings() => throw new NotImplementedException();
}
