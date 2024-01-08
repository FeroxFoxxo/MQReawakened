using Microsoft.Extensions.Logging;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.Rooms.Services;
using Server.Reawakened.XMLs.Bundles;

namespace Server.Reawakened.Entities.Components;

public class PortalControllerComp : Component<PortalController>
{
    public string OverrideCondition => ComponentData.OverrideCondition;
    public bool LaunchMinigame => ComponentData.LaunchMinigame;
    public string TimedEventId => ComponentData.timedEventId;
    public string TimedEventPortalObjectName => ComponentData.TimedEventPortalObjectName;
    public string TimedEventPortalOnAnim => ComponentData.TimedEventPortalOnAnim;
    public string TimedEventPortalOffAnim => ComponentData.TimedEventPortalOffAnim;

    public WorldGraph WorldGraph { get; set; }
    public WorldHandler WorldHandler { get; set; }
    public ILogger<PortalControllerComp> Logger { get; set; }

    public override object[] GetInitData(Player player) =>
        [string.Empty];

    public override void RunSyncedEvent(SyncEvent syncEvent, Player player)
    {
        var character = player.Character;

        var portal = new Portal_SyncEvent(syncEvent);

        if (portal.IsAllowedEntry == false)
            return;

        if (portal.EventDataList[0] is not int)
            throw new InvalidDataException($"Portal with id '{portal.EventDataList[0]}' could not be cast to int.");

        var portalId = (int)portal.EventDataList[0];

        if (portalId == 0)
            portalId = Id;

        var roomId = player.Room.LevelInfo.LevelId;

        var newLevelId = WorldGraph.GetLevelFromPortal(roomId, portalId);

        if (newLevelId <= 0)
        {
            Logger.LogError("Could not find level for portal {PortalId} in room {RoomId}", portalId, roomId);
            return;
        }

        var node = WorldGraph.GetDestNodeFromPortal(roomId, portalId);

        int spawnId;

        if (node != null)
        {
            spawnId = node.ToSpawnID;
            Logger.LogDebug("Node Found: Portal ID '{Portal}', Spawn ID '{Spawn}'.", node.PortalID, node.ToSpawnID);
        }
        else
        {
            spawnId = portal.EventDataList.Count < 4 ? 0 : int.Parse(portal.SpawnPointID);

            Logger.LogError("Could not find node for '{Old}' -> '{New}' for portal {PortalId}.", roomId, newLevelId, portalId);
        }

        if (roomId == newLevelId && character.LevelData.SpawnPointId == spawnId)
        {
            Logger.LogError("Attempt made to teleport to the same portal! Skipping...");
            return;
        }

        character.SetLevel(newLevelId, spawnId, Logger);

        var levelInfo = WorldGraph.GetInfoLevel(newLevelId);

        Logger.LogInformation(
            "Teleporting {CharacterName} ({CharacterId}) to {LevelName} ({LevelId}) " +
            "using portal {PortalId}", character.Data.CharacterName,
            character.Id, levelInfo.InGameName, levelInfo.LevelId, portalId
        );

        player.SendLevelChange(WorldHandler, WorldGraph);
    }
}
