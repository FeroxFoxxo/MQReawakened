using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.AbstractComponents;
using Server.Reawakened.Entities.Components;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Players.Models.Arenas;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.XMLs.BundlesInternal;

namespace Protocols.External._M__MinigameHandler;

public class FinishedMinigame : ExternalProtocol
{
    public override string ProtocolName => "Mm";

    public InternalLoot LootCatalog { get; set; }
    public DatabaseContainer DatabaseContainer { get; set; }
    public ILogger<FinishedMinigame> Logger { get; set; }

    public override void Run(string[] message)
    {
        var objectId = message[5];
        var finishedRaceTime = float.Parse(message[6]);

        Logger.LogInformation("Minigame with ID ({minigameId}) has completed.", objectId);

        SendXt("Mt", objectId, Player.GameObjectId, finishedRaceTime);
        
        if (Player.Character.BestMinigameTimes.TryGetValue(Player.Room.LevelInfo.Name, out var time))
        {
            if (finishedRaceTime < time)
            {
                Player.Character.BestMinigameTimes[Player.Room.LevelInfo.Name] = finishedRaceTime;
                Player.SendXt("Ms", Player.Room.LevelInfo.InGameName);
            }
        }
        else
            Player.Character.BestMinigameTimes.Add(Player.Room.LevelInfo.Name, finishedRaceTime);

        ITriggerComp statue = null;

        if (Player.Room.Entities.TryGetValue(objectId, out var foundEntity))
            foreach (var component in foundEntity)
                if (component is ITriggerComp statueComp)
                    statue = statueComp;

        if (statue == null)
        {
            Logger.LogError("Cannot find statue with ID: {ID}", objectId);
            return;
        }

        if (statue.CurrentPhysicalInteractors.Contains(Player.GameObjectId))
            statue.CurrentPhysicalInteractors.Remove(Player.GameObjectId);

        if (statue.CurrentPhysicalInteractors.Count == 0)
        {
            foreach (var player in Player.Room.Players)
                FinishMinigame(player.Value, objectId, Player.Room.Players.Count);

            if (Player.Room.Entities.TryGetValue(objectId, out var switchEntity))
                foreach (var component in switchEntity)
                    if (component is TriggerCoopArenaSwitchControllerComp tComp)
                    {
                        tComp.IsActive = true;
                        tComp.IsEnabled = true;
                        tComp.CurrentPhysicalInteractors.Clear();
                    }
        }
    }

    public void FinishMinigame(Player player, string minigameId, int membersInRoom)
    {
        var endRace = new TriggerUpdate_SyncEvent(minigameId, player.Room.Time, membersInRoom);

        player.SendSyncEventToPlayer(endRace);

        var rdmBananaReward = new Random().Next(7, 11 * player.Character.Data.GlobalLevel);
        var xpReward = player.Character.Data.ReputationForNextLevel / 11;

        var lootedItems = ArenaModel.GrantLootedItems(LootCatalog, minigameId);
        var lootableItems = ArenaModel.GrantLootableItems(LootCatalog, minigameId);

        var sb = new SeparatedStringBuilder('<');

        sb.Append(membersInRoom.ToString());
        sb.Append(rdmBananaReward.ToString());
        sb.Append(xpReward.ToString());
        sb.Append(lootedItems.ToString());
        sb.Append(lootableItems.ToString());

        SendXt("Mp", minigameId, sb.ToString());

        player.SendCashUpdate();
    }
}
