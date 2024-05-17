using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Controller;
using Server.Reawakened.Entities.Components.Characters.Controllers.IceTroll.States;
using Server.Reawakened.Entities.Components.GameObjects.InterObjs.Interfaces;
using Server.Reawakened.Entities.Components.GameObjects.Trigger.Interfaces;
using Server.Reawakened.Rooms;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.IceTrollController;
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

    public TimerThread TimerThread { get; set; }

    public void RecievedTrigger(bool triggered)
    {
        if (Room == null)
            return;

        if (triggered)
        {
            var delay = Room.GetEntityFromId<AIStateTrollEntranceComp>(Id)?.DelayBeforeEntranceDuration;

            if (delay.HasValue)
                TimerThread.DelayCall(RunEntrance, null, TimeSpan.FromSeconds(delay.Value), TimeSpan.Zero, 1);
        }
    }

    public void RunEntrance(object _)
    {
        if (Room == null)
            return;

        AddNextState<AIStateTrollEntranceComp>();
        GoToNextState();
    }

    public void Destroy(Room room, string id)
    {
        if (Room == null)
            return;

        AddNextState<AIStateTrollRetreatComp>();
        GoToNextState();
    }
}
