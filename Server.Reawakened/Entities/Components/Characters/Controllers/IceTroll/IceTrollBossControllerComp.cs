using Server.Base.Core.Abstractions;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
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

    public TimerThread TimerThread { get; set; }

    public void RecievedTrigger(bool triggered)
    {
        if (Room == null)
            return;

        if (triggered)
        {
            var delay = Room.GetEntityFromId<AIStateTrollEntranceComp>(Id)?.DelayBeforeEntranceDuration;

            if (delay.HasValue)
                TimerThread.RunDelayed(RunEntrance, this, TimeSpan.FromSeconds(delay.Value));
        }
    }

    public static void RunEntrance(ITimerData data)
    {
        if (data is not IceTrollBossControllerComp troll)
            return;

        troll.AddNextState<AIStateTrollEntranceComp>();
        troll.GoToNextState();
    }

    public void Destroy(Room room, string id)
    {
        if (Room == null)
            return;

        AddNextState<AIStateTrollRetreatComp>();
        GoToNextState();
    }
}
