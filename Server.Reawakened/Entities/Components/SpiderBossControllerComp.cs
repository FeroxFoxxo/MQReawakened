using Server.Base.Timers.Services;
using Server.Reawakened.Entities.AIStates.SyncEvents;
using Server.Reawakened.Entities.AIStates;
using static A2m.Server.ExtLevelEditor;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.Rooms.Extensions;
using Server.Base.Timers.Extensions;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.Interfaces;
using Server.Reawakened.Rooms;

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
            if (Teaser)
                RunTeaserEntrance();
            else
                Logger.LogCritical("Default spider boss not implemented yet! Only teaser.");
    }

    public void RunTeaserEntrance()
    {
        var entrance = Room.Entities[Id].First(x => x is AIStateSpiderTeaserEntranceComp) as AIStateSpiderTeaserEntranceComp;

        var nextState = new GameObjectComponents() {
            {"AIStateSpiderTeaserEntrance", new ComponentSettings() {"ST", "0"}}
        };

        GoToNextState(nextState);

        TimerThread.DelayCall(RunTeaserDrop, null, TimeSpan.FromSeconds(entrance.IntroDuration + entrance.DelayBeforeEntranceDuration), TimeSpan.Zero, 1);
    }

    public void RunTeaserDrop(object _)
    {
        var drop = Room.Entities[Id].First(x => x is AIStateSpiderDropComp) as AIStateSpiderDropComp;

        Position.Y = drop.FloorY;

        var nextState = new GameObjectComponents() {
            {"AIStateSpiderDrop", new ComponentSettings() {Position.X.ToString(), Position.Y.ToString(), Position.Z.ToString()}}
        };

        GoToNextState(nextState);
    }

    public void Destroy(Room room, string id)
    {
        if (Teaser)
        {
            var nextState = new GameObjectComponents() {
                {"AIStateSpiderTeaserRetreat", new ComponentSettings() {"ST", "0"}}
            };

            GoToNextState(nextState);
        }
        else
            Logger.LogCritical("Default spider boss not implemented yet! Only teaser.");
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
