using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Base.Logging;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Players.Models.Character;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.XMLs.BundlesInternal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Analytics;

namespace Server.Reawakened.Entities.Components;
public class TriggerCoopArenaSwitchControllerComp : Component<TriggerCoopArenaSwitchController>
{
    public string ArenaObjectId => ComponentData.ArenaObjectID;
    public MinigameCatalogInt MinigameCatalogInt { get; set; }
    public ILogger<TriggerCoopArenaSwitchControllerComp> Logger { get; set; }

    public override object[] GetInitData(Player player) => base.GetInitData(player);

    public override void RunSyncedEvent(SyncEvent syncEvent, Player player)
    {
        if (player.TempData.ArenaModel.HasStarted)
        {
            Logger.LogInformation("Arena has already started, stopping syncEvent.");
            player.TempData.ArenaModel.StartArena = true;
            return;
        }

        player.Room.SendSyncEvent(syncEvent);

        var playersInRoom = Convert.ToInt32(syncEvent.EventDataList[1]);
        var arenaActivation = Convert.ToInt32(syncEvent.EventDataList[2]);

        //Method to determine if arena trigger event is minigame. if minigame, proceed with code below.

        player.TempData.ArenaModel.ArenaObjectId = int.Parse(ArenaObjectId);
        player.TempData.ArenaModel.StartArena = arenaActivation > 0;

        if (player.TempData.Group != null)
        {
            var playersInGroup = player.TempData.Group.GetMembers();
            if (playersInGroup.All(p => p.TempData.ArenaModel.StartArena))
            {
                foreach (var member in playersInGroup)
                {
                    member.TempData.ArenaModel.SetCharacterIds(member, playersInGroup);
                    member.TempData.ArenaModel.HasStarted = true;
                }
                var startRace = new Trigger_SyncEvent(ArenaObjectId, Room.Time, true, player.GameObjectId.ToString(), Room.LevelInfo.LevelId, true, true);
                Room.SendSyncEvent(startRace);
            }
        }
    }
}
