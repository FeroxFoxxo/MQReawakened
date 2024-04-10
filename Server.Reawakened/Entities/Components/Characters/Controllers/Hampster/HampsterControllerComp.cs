using Server.Reawakened.Entities.DataComponentAccessors.Hampster;
using Server.Reawakened.Rooms.Models.Entities;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.HampsterController;
public class HampsterControllerComp : Component<HampsterControllerMQR>
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
