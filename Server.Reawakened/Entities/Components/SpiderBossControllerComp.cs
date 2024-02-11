using Microsoft.Extensions.Logging;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Entities.AIStates;
using Server.Reawakened.Entities.AIStates.SyncEvents;
using Server.Reawakened.Entities.Interfaces;
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
        if (triggered)
        {
            var delay = 0f;

            if (Teaser)
            {
                var entrance = Room.Entities[Id].First(x => x is AIStateSpiderTeaserEntranceComp) as AIStateSpiderTeaserEntranceComp;
                delay = entrance.IntroDuration + entrance.DelayBeforeEntranceDuration;

                GoToNextState(new GameObjectComponents() {
                    {"AIStateSpiderTeaserEntrance", new ComponentSettings() {"ST", "0"}}
                });
            }
            else
            {
                var entrance = Room.Entities[Id].First(x => x is AIStateSpiderEntranceComp) as AIStateSpiderEntranceComp;
                delay = entrance.IntroDuration + entrance.DelayBeforeEntranceDuration;

                GoToNextState(new GameObjectComponents() {
                    {"AIStateSpiderEntrance", new ComponentSettings() {"ST", "0"}}
                });
            }

            TimerThread.DelayCall(RunDrop, null, TimeSpan.FromSeconds(delay), TimeSpan.Zero, 1);
        }
    }

    public void RunDrop(object _)
    {
        var drop = Room.Entities[Id].First(x => x is AIStateSpiderDropComp) as AIStateSpiderDropComp;

        Position.Y = drop.FloorY;

        GoToNextState(new GameObjectComponents() {
            {"AIStateSpiderDrop", new ComponentSettings() {Position.X.ToString(), Position.Y.ToString(), Position.Z.ToString()}},
            {"AIStateSpiderIdle", new ComponentSettings() {"ST", "0"}}
        });
    }

    public void Destroy(Room room, string id)
    {
        var delay = 0f;
        var doorId = 0;

        if (Teaser)
        {
            var retreat = Room.Entities[Id].First(x => x is AIStateSpiderTeaserRetreatComp) as AIStateSpiderTeaserRetreatComp;
            delay = retreat.TalkDuration + retreat.DieDuration + retreat.TransTime;
            doorId = retreat.DoorToOpenID;

            GoToNextState(new GameObjectComponents() {
                {"AIStateSpiderTeaserRetreat", new ComponentSettings() {"ST", "0"}}
            });
        }
        else
        {
            var retreat = Room.Entities[Id].First(x => x is AIStateSpiderRetreatComp) as AIStateSpiderRetreatComp;
            delay = retreat.TalkDuration + retreat.DieDuration + retreat.TransTime;
            doorId = retreat.DoorToOpenID;

            GoToNextState(new GameObjectComponents() {
                {"AIStateSpiderRetreat", new ComponentSettings() {"ST", "0"}}
            });
        }

        if (doorId > 0)
            TimerThread.DelayCall(OpenDoor, doorId, TimeSpan.FromSeconds(delay), TimeSpan.Zero, 1);
    }

    public void OpenDoor(object door)
    {
        var doorId = (int)door;

        if (Room.Entities.TryGetValue(doorId.ToString(), out var foundTrigger))
            foreach (var comp in foundTrigger)
                if (comp is TriggerReceiverComp trigReciev)
                    trigReciev.Trigger(true);
    }

    public void GoToNextState(GameObjectComponents NewState)
    {
        var syncEvent2 = new AiStateSyncEvent()
        {
            InStates = PreviousState,
            GoToStates = NewState
        };

        PreviousState = NewState;

        Room.SendSyncEvent(syncEvent2.GetSyncEvent(Id, Room));
    }
}
