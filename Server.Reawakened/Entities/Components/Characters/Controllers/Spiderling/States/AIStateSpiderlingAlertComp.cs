using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using Server.Reawakened.Entities.DataComponentAccessors.Spiderling.States;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Spiderling.States;
public class AIStateSpiderlingAlertComp : BaseAIState<AIStateSpiderlingAlertMQR>
{
    public float AlertTime => ComponentData.AlertTime;
}
