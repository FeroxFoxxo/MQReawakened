using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Controller;
using Server.Reawakened.Entities.Components.Characters.Controllers.SpiderBoss.States;
using Server.Reawakened.Entities.Components.GameObjects.InterObjs.Interfaces;
using Server.Reawakened.Entities.Components.GameObjects.Trigger;
using Server.Reawakened.Entities.Components.GameObjects.Trigger.Interfaces;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms;
using static A2m.Server.ExtLevelEditor;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.SpiderBoss;

public class SpiderBossControllerComp : BaseAIStateMachine<SpiderBossController>, IRecieverTriggered, IDestructible
{
    /* 
     * -- AI STATES --
     * AIStateSpiderBase
     * AIStateSpiderDeactivated
     * AIStateSpiderDrop [DONE]
     * AIStateSpiderIdle [DONE]
     * AIStateSpiderMove
     * AIStateSpiderPatrol
     * AIStateSpiderPhase1
     * AIStateSpiderPhase2
     * AIStateSpiderPhase3
     * AIStateSpiderPhaseTeaser
     * AIStateSpiderPhaseTrans
     * AIStateSpiderRetreat [DONE]
     * AIStateSpiderVenom
     * AIStateSpiderVineThrow
     * AIStateSpiderWebs
     * 
     * -- BOSS ONLY --
     * AIStateSpiderEntrance [DONE]
     * AIStateSpiderSwichSide
     * 
     * -- TEASER ONLY --
     * AIStateSpiderTeaserEntrance [DONE]
     * AIStateSpiderTeaserRetreat [DONE]
    */

    public bool Teaser => ComponentData.Teaser;
    public string NPCId => ComponentData.NPCId;
    public string NPCTriggerId => ComponentData.NPCTriggerId;

    public TimerThread TimerThread { get; set; }

    public void RecievedTrigger(bool triggered)
    {
        if (Room == null)
            return;

        if (triggered)
        {
            var delay = Teaser ?
                Room.GetEntityFromId<AIStateSpiderTeaserEntranceComp>(Id)?.DelayBeforeEntranceDuration :
                Room.GetEntityFromId<AIStateSpiderEntranceComp>(Id)?.DelayBeforeEntranceDuration;

            if (delay.HasValue)
                TimerThread.DelayCall(RunEntrance, null, TimeSpan.FromSeconds(delay.Value), TimeSpan.Zero, 1);
        }
    }

    public void RunEntrance(object _)
    {
        if (Room == null)
            return;

        var delay = Teaser ?
            Room.GetEntityFromId<AIStateSpiderTeaserEntranceComp>(Id)?.IntroDuration :
            Room.GetEntityFromId<AIStateSpiderEntranceComp>(Id)?.IntroDuration;

        if (delay.HasValue)
        {
            GoToNextState(new GameObjectComponents() {
                {Teaser ? "AIStateSpiderTeaserEntrance" : "AIStateSpiderEntrance", new ComponentSettings() {"ST", "0"}}
            });

            TimerThread.DelayCall(RunExitEntrance, null, TimeSpan.FromSeconds(delay.Value), TimeSpan.Zero, 1);
        }
    }

    private void RunExitEntrance(object _)
    {
        if (Room == null)
            return;

        var drop = Room.GetEntityFromId<AIStateSpiderDropComp>(Id);

        if (drop == null)
            return;

        Position.SetPosition(Position.X, drop.FloorY, Position.Z);

        GoToNextState(new GameObjectComponents() {
            {"AIStateSpiderDrop", new ComponentSettings() {Position.X.ToString(), Position.Y.ToString(), Position.Z.ToString()}},
            {"AIStateSpiderIdle", new ComponentSettings() {"ST", "0"}}
        });
    }

    public void Destroy(Player player, Room room, string id)
    {
        if (Room == null)
            return;

        var delay = 0f;
        var doorId = 0;
        var state = string.Empty;

        if (Teaser)
        {
            var retreat = room.GetEntityFromId<AIStateSpiderTeaserRetreatComp>(Id);

            if (retreat == null)
                return;

            delay = retreat.TalkDuration + retreat.DieDuration + retreat.TransTime;
            doorId = retreat.DoorToOpenID;
        }
        else
        {
            var retreat = Room.GetEntityFromId<AIStateSpiderRetreatComp>(Id);

            if (retreat == null)
                return;

            delay = retreat.TalkDuration + retreat.DieDuration + retreat.TransTime;
            doorId = retreat.DoorToOpenID;
        }

        GoToNextState(new GameObjectComponents() {
            {Teaser ? "AIStateSpiderTeaserRetreat" : "AIStateSpiderRetreat", new ComponentSettings() {"ST", "0"}}
        });

        if (doorId > 0)
            TimerThread.DelayCall(OpenDoor, doorId, TimeSpan.FromSeconds(delay), TimeSpan.Zero, 1);
    }

    private void OpenDoor(object door)
    {
        if (Room == null)
            return;

        var doorId = (int)door;

        foreach (var trigReceiver in Room.GetEntitiesFromId<TriggerReceiverComp>(doorId.ToString()))
            trigReceiver.Trigger(true);
    }
}
