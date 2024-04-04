using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.AbstractComponents;
using Server.Reawakened.Players;

namespace Server.Reawakened.Entities.Components.GameObjects.Checkpoints;

public class CheckpointControllerComp : BaseTriggerCoopController<CheckpointController>
{
    public int SpawnPoint => ComponentData.SpawnpointID;

    public ILogger<CheckpointControllerComp> Logger { get; set; }

    public override void Triggered(Player player, bool isSuccess, bool isActive)
    {
        if (!isActive)
            return;

        Logger.LogInformation("Checkpoint 1: {SpawnPoint}", SpawnPoint);

        if (Room.LastCheckpoint != null)
            if (Room.LastCheckpoint.Id == Id)
            {
                Logger.LogTrace("Skipped checkpoint: {SpawnPoint}", SpawnPoint);
                return;
            }

        var spawnPoint = Room.GetEntitiesFromType<SpawnPointComp>().FirstOrDefault(x => x.Index == SpawnPoint);

        if (spawnPoint != null)
            player.Character.LevelData.SpawnPointId = spawnPoint.Id;

        if (player.Room.LastCheckpoint != null)
        {
            var possibleLastCheckpoint = Room.GetEntityFromId<CheckpointControllerComp>(player.Room.LastCheckpoint.Id);
            possibleLastCheckpoint?.Trigger(player, false);
        }

        player.Room.LastCheckpoint = this;
    }
}
