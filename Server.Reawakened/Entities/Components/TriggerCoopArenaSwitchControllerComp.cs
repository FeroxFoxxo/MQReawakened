using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.AbstractComponents;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;

namespace Server.Reawakened.Entities.Components;

public class TriggerCoopArenaSwitchControllerComp : TriggerCoopControllerComp<TriggerCoopArenaSwitchController>
{
    public string ArenaObjectId => ComponentData.ArenaObjectID;
    public DatabaseContainer DatabaseContainer { get; set; }
    public new ILogger<TriggerCoopArenaSwitchControllerComp> Logger { get; set; }
    public TriggerArenaComp Arena = null;
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

        if (Arena == null)
        {
            Room.Entities.TryGetValue(ArenaObjectId, out var foundEntity);
            foreach (var component in foundEntity)
            {
                if (component is TriggerArenaComp arenaComponent)
                    Arena = arenaComponent;
            }
        }

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
                    player.TempData.ArenaModel.SetCharacterIds(playersInRoom);
                    member.TempData.ArenaModel.HasStarted = true;
                    StartArena(member);

                    if (Arena != null)
                        Arena.StartArena(member);
                }
                
            }
        }
        else
        {
            if (player.TempData.ArenaModel.ShouldStartArena)
            {
                StartArena(player);
                player.TempData.ArenaModel.SetCharacterIds(new List<Player> { player });

                if (Arena != null)
                    Arena.StartArena(player);
            }
        }
    }

    public void StartArena(Player player)
    {
        var startRace = new Trigger_SyncEvent(ArenaObjectId, Room.Time, true, player.GameObjectId.ToString(), Room.LevelInfo.LevelId, true, true);
        player.SendSyncEventToPlayer(startRace);
    }
}
