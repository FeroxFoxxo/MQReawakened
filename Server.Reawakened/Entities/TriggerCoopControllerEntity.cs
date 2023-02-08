using Server.Base.Network;
using Server.Reawakened.Levels.Models.Entities;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;

namespace Server.Reawakened.Entities;

internal class TriggerCoopControllerEntity : SyncedEntity<TriggerCoopController>
{
    public bool Activated = false;

    public override string[] GetInitData(NetState netState) => new[] { Activated ? "1" : "0" };

    public override void RunSyncedEvent(SyncEvent syncEvent, NetState netState)
    {
        var player = netState.Get<Player>();

        Level.SendSyncEvent(syncEvent);
        netState.SendSyncEventToPlayer(syncEvent);

        Trigger_SyncEvent trigEvent;

        if (EntityData.TargetLevelEditorID != 0 && EntityData.TargetToDeactivateLevelEditorID == 0)
        {
            trigEvent = new Trigger_SyncEvent(EntityData.TargetLevelEditorID.ToString(), Level.Time, true, player.PlayerId.ToString(), true);
            Activated = true;
        }
        else
        {
            Activated = false;
            trigEvent = new Trigger_SyncEvent(EntityData.TargetToDeactivateLevelEditorID.ToString(), Level.Time, true, player.PlayerId.ToString(), false);
        }

        Level.SendSyncEvent(trigEvent);
        netState.SendSyncEventToPlayer(trigEvent);
    }
}
