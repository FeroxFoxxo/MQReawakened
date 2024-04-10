using Server.Reawakened.Entities.DataComponentAccessors.Spiker;
using Server.Reawakened.Rooms.Models.Entities;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Spiker;
public class SpikerComponentComp : Component<SpikerControllerMQR>
{
    /* 
     * -- AI STATES --
     * AIStateSpikerAttack
     * AIStatePatrol
     * AIStateIdle
     * AIStateWait
     * AIStateStunned
    */

    public bool StartIdle => ComponentData.StartIdle;
    public float TimeToDirtFXInTaunt => ComponentData.TimeToDirtFXInTaunt;
}
