using Server.Reawakened.Entities.DataComponentAccessors.Spiderling;
using Server.Reawakened.Rooms.Models.Entities;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Spiderling;
public class SpiderlingControllerComp : Component<SpiderlingControllerMQR>
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
