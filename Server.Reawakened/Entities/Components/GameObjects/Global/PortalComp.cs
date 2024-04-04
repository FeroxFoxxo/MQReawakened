using Server.Reawakened.Players;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.Rooms.Services;

namespace Server.Reawakened.Entities.Components;

public class PortalComp : Component<PortalController>
{
    public string OverrideCondition => ComponentData.OverrideCondition;
    public bool LaunchMinigame => ComponentData.LaunchMinigame;
    public string TimedEventId => ComponentData.timedEventId;
    public string TimedEventPortalObjectName => ComponentData.TimedEventPortalObjectName;
    public string TimedEventPortalOnAnim => ComponentData.TimedEventPortalOnAnim;
    public string TimedEventPortalOffAnim => ComponentData.TimedEventPortalOffAnim;

    public WorldHandler WorldHandler { get; set; }

    public override object[] GetInitData(Player player) =>
        [string.Empty];

    public override void RunSyncedEvent(SyncEvent syncEvent, Player player)
    {
        var portal = new Portal_SyncEvent(syncEvent);

        if (portal.IsAllowedEntry == false)
            return;

        if (portal.EventDataList[0] is not int)
            throw new InvalidDataException($"Portal with id '{portal.EventDataList[0]}' could not be cast to int.");

        var portalId = (int)portal.EventDataList[0];

        if (portalId == 0)
            portalId = int.Parse(Id);

        var levelId = player.Room.LevelInfo.LevelId;
        var defaultSpawn = portal.EventDataList.Count < 4 ? string.Empty : portal.SpawnPointID;

        WorldHandler.UsePortal(player, levelId, portalId, defaultSpawn);
    }
}
