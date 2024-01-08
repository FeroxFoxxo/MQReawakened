using Microsoft.Extensions.Logging;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Players.Models.Minigames;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;

namespace Server.Reawakened.Entities.Components;

public class TriggerCoopArenaSwitchControllerComp : Component<TriggerCoopArenaSwitchController>
{
    public string ArenaObjectId => ComponentData.ArenaObjectID;
    public DatabaseContainer DatabaseContainer { get; set; }
    public ILogger<TriggerCoopArenaSwitchControllerComp> Logger { get; set; }

    public override object[] GetInitData(Player player) => base.GetInitData(player);

    public override void RunSyncedEvent(SyncEvent syncEvent, Player player)
    {
        if (player.TempData.ArenaModel.HasStarted)
        {
            Logger.LogInformation("Arena has already started, stopping syncEvent.");
            player.TempData.ArenaModel.ShouldStartArena = true;
            return;
        }

        player.Room.SendSyncEvent(syncEvent);

        var playersInRoom = DatabaseContainer.GetAllPlayers();
        var arenaActivation = Convert.ToInt32(syncEvent.EventDataList[2]);

        //Method to determine if arena trigger event is minigame. if minigame, proceed with code below.

        player.TempData.ArenaModel.ShouldStartArena = arenaActivation > 0;

        if (playersInRoom.Count > 1)
        {
            if (playersInRoom.All(p => p.TempData.ArenaModel.ShouldStartArena))
            {
                foreach (var member in playersInRoom)
                {
                    ArenaModel.SetCharacterIds(member, playersInRoom);
                    member.TempData.ArenaModel.HasStarted = true;
                    StartMinigame(member);
                }
            }
        }
        else
        {
            if (player.TempData.ArenaModel.ShouldStartArena)
            {
                StartMinigame(player);
                ArenaModel.SetCharacterIds(player, new List<Player> { player });
            }
        }
    }

    public void StartMinigame(Player player)
    {
        var startRace = new Trigger_SyncEvent(ArenaObjectId, Room.Time, true, player.GameObjectId.ToString(), Room.LevelInfo.LevelId, true, true);
        player.SendSyncEventToPlayer(startRace);
    }
}
