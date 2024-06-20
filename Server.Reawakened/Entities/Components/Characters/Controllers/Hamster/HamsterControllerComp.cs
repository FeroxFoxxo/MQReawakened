using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using Server.Reawakened.Entities.DataComponentAccessors.Hampster;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Hamster;
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
