using Microsoft.Extensions.Logging;
using Server.Base.Logging;
using Server.Base.Network;
using Server.Reawakened.Entities.Abstractions;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms.Extensions;
using System.Text;

namespace Server.Reawakened.Entities;

public class CheckpointControllerEntity : AbstractTriggerCoop<CheckpointController>
{
    public int SpawnPoint => EntityData.SpawnpointID;

    public new ILogger<CheckpointControllerEntity> Logger { get; set; }

    public override void RunSyncedEvent(SyncEvent syncEvent, NetState netState)
    {
        base.RunSyncedEvent(syncEvent, netState);
        
        if (Room.DefaultSpawn.Index == SpawnPoint)
        {
            Logger.LogTrace("Skipped current checkpoint: {SpawnPoint}", SpawnPoint);
            return;
        }

        var spawns = Room.GetEntities<SpawnPointEntity>()
            .Values.OrderBy(s => s.Index).ToArray();

        var spawnPoint = spawns.FirstOrDefault(s => s.Index == SpawnPoint);

        if (spawnPoint != null)
        {
            Room.DefaultSpawn = spawnPoint;
        }
        else
        {
            var sb = new StringBuilder();

            sb.AppendLine($"SpawnPoint Index: {SpawnPoint}")
                .Append($"Possibilities: {string.Join(", ", spawns.Select(s => $"{s.Index} (ID: {s.Id})"))}");

            FileLogger.WriteGenericLog<CheckpointController>("failed-checkpoints", "Checkpoint Spawn Failed", sb.ToString(), LoggerType.Warning);
        }

        //var checkpoints = Room.RoomEntities.GetEntities<CheckpointControllerEntity>().Values;
        //var possibleLastCheckpoint = checkpoints.FirstOrDefault(c => c.SpawnPoint == character.SpawnPoint);
        //possibleLastCheckpoint?.TriggerCheckpoint(false, netState, player);

        //TriggerCheckpoint(true, netState, player);
    }
}
