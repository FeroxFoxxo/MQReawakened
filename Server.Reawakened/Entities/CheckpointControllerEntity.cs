using Server.Base.Network;
using Server.Reawakened.Levels.Models.Entities;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;

namespace Server.Reawakened.Entities;

public class CheckpointControllerEntity : SyncedEntity<CheckpointController>
{
    public int SpawnPoint => EntityData.SpawnpointID;
    
    public override void RunSyncedEvent(SyncEvent syncEvent, NetState netState)
    {
        var player = netState.Get<Player>();
        var character = player.GetCurrentCharacter();

        if (character.SpawnPoint == SpawnPoint)
            return;

        var checkpoints = Level.LevelEntityHandler.GetEntities<CheckpointControllerEntity>().Values;
        var possibleLastCheckpoint = checkpoints.FirstOrDefault(c => c.SpawnPoint == character.SpawnPoint);
        possibleLastCheckpoint?.TriggerCheckpoint(false, netState, player);

        character.SpawnPoint = SpawnPoint;
        
        TriggerCheckpoint(true, netState, player);
    }

    public void TriggerCheckpoint(bool active, NetState netState, Player player)
    {
        var trigger = new Trigger_SyncEvent(Id.ToString(), Level.Time, true,
            player.PlayerId.ToString(), active);

        netState.SendSyncEventToPlayer(trigger);
    }
}
