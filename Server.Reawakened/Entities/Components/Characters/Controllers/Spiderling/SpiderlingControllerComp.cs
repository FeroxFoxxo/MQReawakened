using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using Server.Reawakened.Entities.DataComponentAccessors.Spiderling;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Spiderling;
public class SpiderlingControllerComp : BaseAIStateMachine<SpiderlingControllerMQR>
{
    /* 
     * -- AI STATES --
     * AIStateSpiderlingAlert
     * AIStateSpiderlingAttack
     * AIStateSpiderlingDigOut
     * 
     * AIStatePatrol
     * AIStateStunned
    */

    public bool StartIdle => ComponentData.StartIdle;
    public float TimeToDirtFXInTaunt => ComponentData.TimeToDirtFXInTaunt;
}
