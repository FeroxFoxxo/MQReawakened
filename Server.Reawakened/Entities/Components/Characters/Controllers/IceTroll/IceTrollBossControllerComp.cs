using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Controller;
using Server.Reawakened.Entities.Components.Characters.Controllers.IceTroll.States;
using Server.Reawakened.Entities.Components.GameObjects.InterObjs.Interfaces;
using Server.Reawakened.Entities.Components.GameObjects.Trigger;
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
    * AIStateTrollIdle [DONE]
    * AIStateTrollTaunt
    * AIStateTrollRetreat [DONE]
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
        if (Room == null)
            return;

        GoToNextState(new GameObjectComponents() {
            {"AIStateTrollIdle", new ComponentSettings() {"ST", "0"}}
        });
    }

    public void Destroy(Player player, Room room, string id)
    {
        if (Room == null)
            return;

        var retreat = room.GetEntityFromId<AIStateTrollRetreatComp>(Id);

        if (retreat == null)
            return;

        var delay = retreat.TalkDuration + retreat.DieDuration + retreat.TransTime;

        GoToNextState(new GameObjectComponents()
        {
            { "AIStateTrollRetreat", new ComponentSettings() { "ST", "0" }}
        });

        if (retreat.DoorToOpenID > 0)
            TimerThread.DelayCall(OpenDoor, retreat.DoorToOpenID, TimeSpan.FromSeconds(delay), TimeSpan.Zero, 1);
    }

    private void OpenDoor(object door)
    {
        if (Room == null)
            return;

        var doorId = (int) door;

        foreach (var trigReceiver in Room.GetEntitiesFromId<TriggerReceiverComp>(doorId.ToString()))
            trigReceiver.Trigger(true);
    }
}
