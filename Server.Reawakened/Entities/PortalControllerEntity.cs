using Microsoft.Extensions.Logging;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.Rooms.Services;
using Server.Reawakened.XMLs.Bundles;

namespace Server.Reawakened.Entities;

public class PortalControllerEntity : SyncedEntity<PortalController>
{
    public string OverrideCondition => EntityData.OverrideCondition;
    public bool LaunchMinigame => EntityData.LaunchMinigame;
    public string TimedEventId => EntityData.timedEventId;
    public string TimedEventPortalObjectName => EntityData.TimedEventPortalObjectName;
    public string TimedEventPortalOnAnim => EntityData.TimedEventPortalOnAnim;
    public string TimedEventPortalOffAnim => EntityData.TimedEventPortalOffAnim;

    public WorldGraph WorldGraph { get; set; }
    public WorldHandler WorldHandler { get; set; }
    public ILogger<PortalControllerEntity> Logger { get; set; }

    public override object[] GetInitData(Player player) =>
        new object[] { string.Empty };

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

        var newLevelId = WorldGraph.GetLevelFromPortal(character.LevelData.LevelId, portalId);
        var node = WorldGraph.GetDestNodeFromPortal(character.LevelData.LevelId, portalId);

        if (node != null)
        {
            Logger.LogDebug("Node Found: Portal ID '{Portal}', Spawn ID '{Spawn}'.", node.PortalID, node.ToSpawnID);
            character.SetLevel(newLevelId, node.PortalID, node.ToSpawnID, Logger);
        }
        else
        {
            Logger.LogError("Could not find node for '{Old}' -> '{New}'.", character.LevelData.LevelId, newLevelId);
            character.SetLevel(newLevelId, portalId,
                portal.EventDataList.Count < 4 ? 0 : int.Parse(portal.SpawnPointID), Logger);
        }

        var levelInfo = WorldGraph.GetInfoLevel(newLevelId);

        Logger.LogInformation(
            "Teleporting {CharacterName} ({CharacterId}) to {LevelName} ({LevelId}) " +
            "using portals {PortalId} -> {NewPortalId}", character.Data.CharacterName,
            character.Data.CharacterId, levelInfo.InGameName, levelInfo.LevelId, portalId,
            character.LevelData.PortalId
        );

        player.SendLevelChange(WorldHandler, WorldGraph);
    }
}
