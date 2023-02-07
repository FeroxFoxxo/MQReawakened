using Server.Base.Core.Extensions;
using Server.Base.Network;
using Server.Reawakened.Levels.Enums;
using Server.Reawakened.Levels.Models.Entities;
using Server.Reawakened.Levels.Services;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.XMLs.Bundles;

namespace Server.Reawakened.Entities;

public class PortalControllerEntity : SynchronizedEntity<PortalController>
{
    public WorldGraph WorldGraph { get; set; }
    public LevelHandler LevelHandler { get; set; }

    public override void RunSyncedEvent(SyncEvent syncEvent, NetState netState)
    {
        var player = netState.Get<Player>();

        var portal = new Portal_SyncEvent(syncEvent);

        if (portal.IsAllowedEntry == false)
            return;

        var currentLevel = player.GetLevelId();

        if (portal.EventDataList[0] is not int)
            throw new InvalidDataException($"Portal with id '{portal.EventDataList[0]}' could not be cast to int.");

        var portalId = (int) portal.EventDataList[0];

        if (portalId == 0)
            portalId = StoredEntity.Id;

        var newLevelId = WorldGraph.GetDestinationLevelID(currentLevel, portalId);

        if (newLevelId > 0)
        {
            var nextLevel = LevelHandler.GetLevelFromId(newLevelId);
            
            var character = player.GetCurrentCharacter();

            character.PortalId = portalId;
            character.SpawnPoint = portal.EventDataList.Count < 4 ? 0 : int.Parse(portal.SpawnPointID);

            player.JoinLevel(netState, nextLevel, out var reason);

            if (reason == JoinReason.Accepted)
                player.SendLevelChange(netState, LevelHandler, WorldGraph);
        }
        else
        {
            throw new InvalidDataException($"Portal '{portalId}' is null for world {currentLevel}!");
        }
    }
}
