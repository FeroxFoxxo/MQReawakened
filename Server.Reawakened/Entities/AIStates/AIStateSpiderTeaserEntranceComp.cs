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

    public override object[] GetInitData(Player player) => ["ST", DelayBeforeEntranceDuration];

    public void RecievedTrigger(bool triggered)
    {
        if (triggered)
        {
            var syncEvent = new AiStateSyncEvent()
            {
                InStates = {
                },
                GoToStates = {
                    {"AIStateSpiderTeaserEntrance", new ComponentSettings() {"ST", DelayBeforeEntranceDuration.ToString()}}
                }
            };

            Room.SendSyncEvent(syncEvent.GetSyncEvent(Id, Room));

            Task.Delay(Convert.ToInt32(IntroDuration) * 1000);

            var drop = Room.Entities[Id].First(x => x is AIStateSpiderDropComp) as AIStateSpiderDropComp;

            var syncEvent2 = new AiStateSyncEvent()
            {
                InStates = {
                    {"AIStateSpiderTeaserEntrance", new ComponentSettings() {"ST", DelayBeforeEntranceDuration.ToString()}}
                },
                GoToStates = {
                    {"AIStateSpiderDrop", new ComponentSettings() {Position.X.ToString(), drop.FloorY.ToString(), Position.Z.ToString()}}
                }
            };

            Room.SendSyncEvent(syncEvent2.GetSyncEvent(Id, Room));
        }
    }
}
