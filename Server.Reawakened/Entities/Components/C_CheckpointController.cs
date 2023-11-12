using Microsoft.Extensions.Logging;
using Server.Base.Logging;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms.Extensions;
using System.Text;

namespace Server.Reawakened.Entities.Components;

public class C_CheckpointController : AC_TriggerCoopController<CheckpointController>
{
    public int SpawnPoint => ComponentData.SpawnpointID;

    public new ILogger<C_CheckpointController> Logger { get; set; }

    public override void Triggered(Player player, bool isSuccess, bool isActive)
    {
        if (!isActive)
            return;

        Logger.LogInformation("Checkpoint 1: {SpawnPoint}", SpawnPoint);

        if (Room.CheckpointId == Id)
        {
            Logger.LogTrace("Skipped checkpoint: {SpawnPoint}", SpawnPoint);
            return;
        }

        var spawns = Room.GetComponentsOfType<C_SpawnPoint>()
            .Values.OrderBy(s => s.Index).ToArray();

        var spawnPoint = spawns.FirstOrDefault(s => s.Index == SpawnPoint);

        if (spawnPoint != null)
            Room.CheckpointSpawn = spawnPoint;
        else
        {
            var sb = new StringBuilder();

            sb.AppendLine($"SpawnPoint Index: {SpawnPoint}")
                .Append($"Possibilities: {string.Join(", ", spawns.Select(s => $"{s.Index} (ID: {s.Id})"))}");

            FileLogger.WriteGenericLog<CheckpointController>("checkpoints-errors", "Checkpoint Spawn Failed",
                sb.ToString(), LoggerType.Warning);
        }

        var checkpoints = Room.GetComponentsOfType<C_CheckpointController>().Values;
        var possibleLastCheckpoint = checkpoints.FirstOrDefault(c => c.Id == Room.CheckpointId);

        possibleLastCheckpoint?.Trigger(player, false);

        Room.CheckpointId = Id;
    }
}
