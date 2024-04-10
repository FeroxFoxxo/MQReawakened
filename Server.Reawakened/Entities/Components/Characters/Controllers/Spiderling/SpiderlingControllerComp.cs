using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Controller;
using Server.Reawakened.Entities.DataComponentAccessors.Spiderling;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Spiderling;
public class SpiderlingControllerComp : AiStateMachineInternalComponent<SpiderlingControllerMQR>
{
    /* 
     * -- AI STATES --
     * AIStatePatrol
     * AIStateSpiderlingAttack
     * AIStateSpiderlingAlert
     * AIStateStunned
     * AIStateSpiderlingDigOut
    */

    public bool StartIdle => ComponentData.StartIdle;
    public float TimeToDirtFXInTaunt => ComponentData.TimeToDirtFXInTaunt;
}
