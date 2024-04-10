using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Controller;
using Server.Reawakened.Entities.DataComponentAccessors.Hampster;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.HampsterController;
public class HamsterControllerComp : AiStateMachineInternalComponent<HamsterControllerMQR>
{
    /* 
     * -- AI STATES --
     * AIStatePatrol
     * AIStateHamsterAttack
     * AIStateIdle
     * AIStateWait
     * AIStateStunned
     * AIStateMove
     * AIStateWait2
    */

    public bool StartIdle => ComponentData.StartIdle;
    public float TimeToDirtFXInStunOut => ComponentData.TimeToDirtFXInStunOut;
}
