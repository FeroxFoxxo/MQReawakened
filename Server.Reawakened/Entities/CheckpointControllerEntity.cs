using Server.Base.Network;
using Server.Reawakened.Levels.Models.Entities;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Models;

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
        possibleLastCheckpoint?.TriggerCheckpoint(false, netState, character);

        character.SpawnPoint = SpawnPoint;
        
        TriggerCheckpoint(true, netState, character);
    }

    public void TriggerCheckpoint(bool active, NetState netState, CharacterModel character)
    {
        var trigger = new Trigger_SyncEvent(Id.ToString(), Level.Time, true,
            character.GetCharacterObjectId().ToString(), active);

        netState.SendSyncEventToPlayer(trigger);
    }
}
