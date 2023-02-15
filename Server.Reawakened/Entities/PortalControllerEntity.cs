using Microsoft.Extensions.Logging;
using Server.Base.Network;
using Server.Reawakened.Levels.Models.Entities;
using Server.Reawakened.Levels.Services;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.XMLs.Bundles;

namespace Server.Reawakened.Entities;

public class PortalControllerEntity : SyncedEntity<PortalController>
{
    public WorldGraph WorldGraph { get; set; }
    public LevelHandler LevelHandler { get; set; }
    public ILogger<PortalControllerEntity> Logger { get; set; }

    public override object[] GetInitData(NetState netState) =>
        new object[] { string.Empty };

    public override void RunSyncedEvent(SyncEvent syncEvent, NetState netState)
    {
        var player = netState.Get<Player>();

        var portal = new Portal_SyncEvent(syncEvent);

        if (portal.IsAllowedEntry == false)
            return;

        var currentLevel = player.GetLevelId();

        if (portal.EventDataList[0] is not int)
            throw new InvalidDataException($"Portal with id '{portal.EventDataList[0]}' could not be cast to int.");

        var portalId = (int)portal.EventDataList[0];

        if (portalId == 0)
            portalId = Id;

        var newLevelId = WorldGraph.GetDestinationLevelID(currentLevel, portalId);

        if (newLevelId > 0)
        {
            var character = player.GetCurrentCharacter();

            DestNode node = null;

            var nodes = WorldGraph.GetLevelWorldGraphNodes(newLevelId);

            if (nodes != null)
                node = nodes.FirstOrDefault(a => a.ToLevelID == character.Level);

            if (node != null)
            {
                Logger.LogDebug("Node Found: Portal ID '{Portal}', Spawn ID '{Spawn}'.", node.PortalID, node.ToSpawnID);
                character.SetCharacterSpawn(node.PortalID, node.ToSpawnID, Logger);
            }
            else
            {
                Logger.LogError("Could not find node for '{Old}' -> '{New}'.", currentLevel, newLevelId);
                character.SetCharacterSpawn(portalId,
                    portal.EventDataList.Count < 4 ? 0 : int.Parse(portal.SpawnPointID), Logger);
            }

            character.Level = newLevelId;

            Logger.LogInformation("Teleporting {CharacterName} ({CharacterId}) to {LevelName} ({LevelId}) " +
                                  "using portals {PortalId} to {NewPortalId}", character.Data.CharacterName,
                character.Data.CharacterId, Level.LevelInfo.LevelId, Level.LevelInfo.InGameName, portalId, character.PortalId);

            player.SendLevelChange(netState, LevelHandler, WorldGraph);
        }
        else
        {
            throw new InvalidDataException($"Portal '{portalId}' is null for world {currentLevel}!");
        }
    }
}
