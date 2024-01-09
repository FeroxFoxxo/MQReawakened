using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Entities.AIStates.SyncEvents;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using static A2m.Server.ExtLevelEditor;

namespace Server.Reawakened.Entities.AIStates;

public class AIStateSpiderTeaserEntranceComp : Component<AIStateSpiderTeaserEntrance>, IRecieverTriggered
{
    public float DelayBeforeEntranceDuration => ComponentData.DelayBeforeEntranceDuration;
    public float EntranceDuration => ComponentData.EntranceDuration;
    public float IntroDuration => ComponentData.IntroDuration;
    public float MinimumLifeRatioAccepted => ComponentData.MinimumLifeRatioAccepted;
    public float LifeRatioAtHeal => ComponentData.LifeRatioAtHeal;

    public TimerThread TimerThread { get; set; }

    public override object[] GetInitData(Player player) => ["ST", "0"];

    public void RecievedTrigger(bool triggered)
    {
        if (triggered)
        {
            var syncEvent = new AiStateSyncEvent()
            {
                InStates = {
                },
                GoToStates = {
                    {"AIStateSpiderTeaserEntrance", new ComponentSettings() {"ST", "0"}}
                }
            };

            Room.SendSyncEvent(syncEvent.GetSyncEvent(Id, Room));

            TimerThread.DelayCall(RunDrop, null, TimeSpan.FromSeconds(16 + 1), TimeSpan.Zero, 1);
        }
    }

    public void RunDrop(object _)
    {
        var drop = Room.Entities[Id].First(x => x is AIStateSpiderDropComp) as AIStateSpiderDropComp;

        var syncEvent2 = new AiStateSyncEvent()
        {
            InStates = {
                    {"AIStateSpiderTeaserEntrance", new ComponentSettings() {"ST", "0"}}
                },
            GoToStates = {
                    {"AIStateSpiderDrop", new ComponentSettings() {Position.X.ToString(), drop.FloorY.ToString(), Position.Z.ToString()}}
                }
        };

        Room.SendSyncEvent(syncEvent2.GetSyncEvent(Id, Room));
    }
}
