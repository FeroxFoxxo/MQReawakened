using Microsoft.Extensions.Logging;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Players.Models.Minigames;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.XMLs.BundlesInternal;

namespace Server.Reawakened.Entities.Components;

public class TriggerCoopArenaSwitchControllerComp : Component<TriggerCoopArenaSwitchController>
{
    public string ArenaObjectId => ComponentData.ArenaObjectID;
    public DatabaseContainer DatabaseContainer { get; set; }
    public ArenaCatalogInt ArenaCatalogInt { get; set; }
    public ILogger<TriggerCoopArenaSwitchControllerComp> Logger { get; set; }

    public override object[] GetInitData(Player player) => base.GetInitData(player);

    public override void RunSyncedEvent(SyncEvent syncEvent, Player player)
    {
        var playersInRoom = DatabaseContainer.GetAllPlayers();
        var arenaActivation = Convert.ToInt32(syncEvent.EventDataList[2]);

        if (ArenaModel.ArenaActivated)
        {
            Logger.LogInformation("Arena has already started, stopping syncEvent.");
            return;
        }

        player.Room.SendSyncEvent(syncEvent);

        player.TempData.ArenaModel.ActivatedSwitch = arenaActivation > 0;

        if (player.TempData.ArenaModel.ActivatedSwitch)
        {
            if (!ArenaModel.Participants.Contains(player))
                ArenaModel.Participants.Add(player);

            player.Room.SendSyncEvent(new TriggerUpdate_SyncEvent(ArenaObjectId, player.Room.Time, ArenaModel.Participants.Count));
        }


        if (playersInRoom.All(p => p.TempData.ArenaModel.ActivatedSwitch))
        {
            foreach (var member in playersInRoom)
            {
                ArenaModel.ArenaActivated = true;
                ArenaModel.SetCharacterIds(playersInRoom);
                StartArenaActivation(member);
            }
        }
    }

    public void StartArenaActivation(Player player) =>
        player.SendSyncEventToPlayer(new Trigger_SyncEvent(ArenaObjectId, Room.Time,
            true, player.GameObjectId.ToString(), Room.LevelInfo.LevelId, true, true));
}
