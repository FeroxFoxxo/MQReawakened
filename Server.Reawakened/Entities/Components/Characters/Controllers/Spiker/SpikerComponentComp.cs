using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using Server.Reawakened.Entities.DataComponentAccessors.Spiker;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Spiker;
public class SpikerComponentComp : BaseAIStateMachine<SpikerControllerMQR>
{
    /* 
     * -- AI STATES --
     * AIStateSpikerAttack
     * 
     * AIStateIdle
     * AIStatePatrol
     * AIStateStunned
     * AIStateWait
    */

    public bool StartIdle => ComponentData.StartIdle;
    public float TimeToDirtFXInTaunt => ComponentData.TimeToDirtFXInTaunt;
}
