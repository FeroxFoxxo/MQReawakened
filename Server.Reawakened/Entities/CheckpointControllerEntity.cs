using Microsoft.Extensions.Logging;
using Server.Base.Network;
using Server.Reawakened.Entities.Abstractions;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;

namespace Server.Reawakened.Entities;

public class CheckpointControllerEntity : AbstractTriggerCoop<CheckpointController>
{
    public int SpawnPoint => EntityData.SpawnpointID;

    public new ILogger<CheckpointControllerEntity> Logger { get; set; }

    public override void RunSyncedEvent(SyncEvent syncEvent, NetState netState)
    {
        base.RunSyncedEvent(syncEvent, netState);

        var player = netState.Get<Player>();
        var character = player.GetCurrentCharacter();

        if (Room.DefaultSpawn.Index == SpawnPoint)
        {
            Logger.LogTrace("Skipped current checkpoint: {SpawnPoint}", SpawnPoint);
            return;
        }

        var spawns = Room.RoomEntities.GetEntities<SpawnPointEntity>()
            .Values.OrderBy(s => s.Index).ToArray();

        var spawnPoint = spawns.FirstOrDefault(s => s.Index == SpawnPoint);

        if (spawnPoint != null)
            Room.DefaultSpawn = spawnPoint;
        else
            Logger.LogError("Could not find spawn point for: {Index}. Possible: {Possibilities}",
                SpawnPoint, string.Join(", ", spawns.Select(s => $"{s.Index} (ID: {s.Id})")));

        //var checkpoints = Room.RoomEntities.GetEntities<CheckpointControllerEntity>().Values;
        //var possibleLastCheckpoint = checkpoints.FirstOrDefault(c => c.SpawnPoint == character.SpawnPoint);
        //possibleLastCheckpoint?.TriggerCheckpoint(false, netState, player);

        //TriggerCheckpoint(true, netState, player);
    }
}
