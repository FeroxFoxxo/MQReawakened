using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using Server.Reawakened.Entities.Components.Characters.Controllers.IceTroll.States;
using Server.Reawakened.Entities.Components.GameObjects.InterObjs.Interfaces;
using Server.Reawakened.Entities.Components.GameObjects.Trigger.Interfaces;
using Server.Reawakened.Rooms;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.IceTroll;
public class IceTrollBossControllerComp : BaseAIStateMachine<IceTrollBossController>, IRecieverTriggered, IDestructible
{
    /* 
    * -- AI STATES --
    * AIStateTrollArmorBreak
    * AIStateTrollBase
    * AIStateTrollBreath
    * AIStateTrollDeactivated
    * [DONE] AIStateTrollEntrance
    * AIStateTrollIdle
    * AIStateTrollPhase1
    * AIStateTrollPhase2
    * AIStateTrollPhase3
    * AIStateTrollPhaseTrans
    * [DONE] AIStateTrollRetreat
    * AIStateTrollSmash
    * AIStateTrollTaunt
    * AIStateTrollVacuum
    */

    public int CurrentPhase = 0;
    public bool Broken = false;

    public void RecievedTrigger(bool triggered)
    {
        if (Room == null)
            return;

        if (triggered)
        {
            AddNextState<AIStateTrollEntranceComp>();
            GoToNextState();
        }
    }

    public void Destroy(Room room, string id)
    {
        if (Room == null)
            return;

        AddNextState<AIStateTrollRetreatComp>();
        GoToNextState();
    }
}
