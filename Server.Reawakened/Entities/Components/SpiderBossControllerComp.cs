using Microsoft.Extensions.Logging;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Entities.Enemies.AIStateEnemies.Rachnok.AIStates;
using Server.Reawakened.Entities.Enemies.AIStateEnemies.SyncEvents;
using Server.Reawakened.Entities.Interfaces;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using static A2m.Server.ExtLevelEditor;

namespace Server.Reawakened.Entities.Components;

public class SpiderBossControllerComp : Component<SpiderBossController>, IRecieverTriggered, IDestructible
{
    public bool Teaser => ComponentData.Teaser;
    public string NPCId => ComponentData.NPCId;
    public string NPCTriggerId => ComponentData.NPCTriggerId;

    public ILogger<SpiderBossControllerComp> Logger { get; set; }
    public TimerThread TimerThread { get; set; }

    public GameObjectComponents PreviousState = [];

    public void RecievedTrigger(bool triggered)
    {
        if (Room == null || Room.IsObjectKilled(Id))
            return;

        if (triggered)
        {
            var delay = 0f;

            if (Teaser)
            {
                var entrance = Room.GetEntityFromId<AIStateSpiderTeaserEntranceComp>(Id);

                if (entrance == null)
                    return;

                delay = entrance.IntroDuration + entrance.DelayBeforeEntranceDuration;

                GoToNextState(new GameObjectComponents() {
                    {"AIStateSpiderTeaserEntrance", new ComponentSettings() {"ST", "0"}}
                });
            }
            else
            {
                var entrance = Room.GetEntityFromId<AIStateSpiderEntranceComp>(Id);

                if (entrance == null)
                    return;

                delay = entrance.IntroDuration + entrance.DelayBeforeEntranceDuration;

                GoToNextState(new GameObjectComponents() {
                    {"AIStateSpiderEntrance", new ComponentSettings() {"ST", "0"}}
                });
            }

            TimerThread.DelayCall(RunDrop, null, TimeSpan.FromSeconds(delay), TimeSpan.Zero, 1);
        }
    }

    private void RunDrop(object _)
    {
        if (Room == null || Room.IsObjectKilled(Id))
            return;

        var drop = Room.GetEntityFromId<AIStateSpiderDropComp>(Id);

        if (drop == null)
            return;

        Position.Y = drop.FloorY;

        GoToNextState(new GameObjectComponents() {
            {"AIStateSpiderDrop", new ComponentSettings() {Position.X.ToString(), Position.Y.ToString(), Position.Z.ToString()}},
            {"AIStateSpiderIdle", new ComponentSettings() {"ST", "0"}}
        });
    }

    public void Destroy(Player player, Room room, string id)
    {
        if (room.IsObjectKilled(Id))
            return;

        var delay = 0f;
        var doorId = 0;

        if (Teaser)
        {
            var retreat = room.GetEntityFromId<AIStateSpiderTeaserRetreatComp>(Id);

            if (retreat == null)
                return;

            delay = retreat.TalkDuration + retreat.DieDuration + retreat.TransTime;
            doorId = retreat.DoorToOpenID;

            GoToNextState(new GameObjectComponents() {
                {"AIStateSpiderTeaserRetreat", new ComponentSettings() {"ST", "0"}}
            });
        }
        else
        {
            var retreat = Room.GetEntityFromId<AIStateSpiderRetreatComp>(Id);

            if (retreat == null)
                return;

            delay = retreat.TalkDuration + retreat.DieDuration + retreat.TransTime;
            doorId = retreat.DoorToOpenID;

            GoToNextState(new GameObjectComponents() {
                {"AIStateSpiderRetreat", new ComponentSettings() {"ST", "0"}}
            });
        }

        if (doorId > 0)
            TimerThread.DelayCall(OpenDoor, doorId, TimeSpan.FromSeconds(delay), TimeSpan.Zero, 1);
    }

    private void OpenDoor(object door)
    {
        var doorId = (int)door;

        if (Room == null)
            return;

        foreach (var trigReceiver in Room.GetEntitiesFromId<TriggerReceiverComp>(doorId.ToString()))
            trigReceiver.Trigger(true);
    }

    public void GoToNextState(GameObjectComponents NewState)
    {
        if (Room == null || Room.IsObjectKilled(Id))
            return;

        var syncEvent2 = new AiStateSyncEvent()
        {
            InStates = PreviousState,
            GoToStates = NewState
        };

        PreviousState = NewState;
        
        Room.SendSyncEvent(syncEvent2.GetSyncEvent(Id, Room));
    }
}
