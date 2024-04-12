using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Controller;
using Server.Reawakened.Entities.DataComponentAccessors.Hampster;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.HampsterController;
public class HamsterControllerComp : BaseAIStateMachine<HamsterControllerMQR>
{
    /* 
     * -- AI STATES --
     * AIStateHamsterAttack
     * 
     * AIStateIdle
     * AIStateMove
     * AIStatePatrol
     * AIStateStunned
     * AIStateWait
     * AIStateWait2
    */

    public bool StartIdle => ComponentData.StartIdle;
    public float TimeToDirtFXInStunOut => ComponentData.TimeToDirtFXInStunOut;
}
