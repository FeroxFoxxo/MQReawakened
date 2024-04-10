using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Controller;
using Server.Reawakened.Entities.Components.Characters.Controllers.IceTroll.States;
using Server.Reawakened.Entities.Components.Characters.Controllers.SpiderBoss.States;
using Server.Reawakened.Entities.Components.GameObjects.InterObjs.Interfaces;
using Server.Reawakened.Entities.Components.GameObjects.Trigger.Interfaces;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms;
using static A2m.Server.ExtLevelEditor;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.IceTrollController;
public class IceTrollBossControllerComp : AiStateMachineComponent<IceTrollBossController>, IRecieverTriggered, IDestructible
{
    /* 
    * -- AI STATES --
    * AIStateTrollVacuum
    * AIStateTrollSmash
    * AIStateTrollBreath
    * AIStateTrollBase
    * AIStateTrollIdle
    * AIStateTrollTaunt
    * AIStateTrollRetreat
    * AIStateTrollPhase2
    * AIStateTrollPhase1
    * AIStateTrollPhase3
    * AIStateTrollArmorBreak
    * AIStateTrollPhaseTrans
    * AIStateTrollDeactivated
    * AIStateTrollEntrance [DONE]
    */

    public TimerThread TimerThread { get; set; }

    public void RecievedTrigger(bool triggered)
    {
        if (Room == null || Room.IsObjectKilled(Id))
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
        if (Room == null || Room.IsObjectKilled(Id))
            return;

        var delay = Room.GetEntityFromId<AIStateTrollEntranceComp>(Id)?.IntroDuration;

        if (delay.HasValue)
        {
            GoToNextState(new GameObjectComponents() {
                {"AIStateTrollEntrance", new ComponentSettings() {"ST", "0"}}
            });

            TimerThread.DelayCall(RunExitEntrance, null, TimeSpan.FromSeconds(delay.Value), TimeSpan.Zero, 1);
        }
    }

    private void RunExitEntrance(object _)
    {
        if (Room == null || Room.IsObjectKilled(Id))
            return;
    }

    public void Destroy(Player player, Room room, string id)
    { }
}
