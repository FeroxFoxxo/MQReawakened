using Microsoft.Extensions.Logging;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Players.Models.Minigames;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.XMLs.BundlesInternal;

namespace Protocols.External._M__MinigameHandler;

public class FinishedMinigame : ExternalProtocol
{
    public override string ProtocolName => "Mm";

    public LootCatalogInt LootCatalog { get; set; }
    public DatabaseContainer DatabaseContainer { get; set; }

    public ILogger<FinishedMinigame> Logger { get; set; }

    public override void Run(string[] message)
    {
        var minigameId = int.Parse(message[5]);
        var finishedRaceTime = float.Parse(message[6]);

        Logger.LogInformation("Minigame with ID ({minigameId}) has completed.", minigameId);

        SendXt("Mt", minigameId, Player.GameObjectId, finishedRaceTime);

        if (Player.TempData.ArenaModel.BestTimeForLevel == null)
        {
            Player.TempData.ArenaModel.BestTimeForLevel = [];
            Player.TempData.ArenaModel.BestTimeForLevel.Add(Player.Room.LevelInfo.Name, finishedRaceTime);
        }

        if (Player.TempData.ArenaModel.BestTimeForLevel.TryGetValue(Player.Room.LevelInfo.Name, out var time))
            if (finishedRaceTime < time)
            {
                Player.TempData.ArenaModel.BestTimeForLevel[Player.Room.LevelInfo.Name] = finishedRaceTime;
                Player.SendXt("Ms", Player.Room.LevelInfo.InGameName);
            }

        Player.TempData.ArenaModel.ShouldStartArena = false;
        Player.TempData.ArenaModel.HasStarted = false;

        var playersInRoom = DatabaseContainer.GetAllPlayers();

        if (playersInRoom.Count > 1)
        {
            if (playersInRoom.All(p => !p.TempData.ArenaModel.HasStarted))
                foreach (var player in playersInRoom)
                    FinishMinigame(minigameId, playersInRoom.Count);
        }

        else
            FinishMinigame(minigameId, 1);
    }

    public void FinishMinigame(int minigameId, int membersInGroup)
    {
        var endRace = new TriggerUpdate_SyncEvent(minigameId.ToString(), Player.Room.Time, membersInGroup);

        Player.Room.SendSyncEvent(endRace);

        var rdmBananaReward = new Random().Next(7, 11 * Player.Character.Data.GlobalLevel);
        var xpReward = Player.Character.Data.ReputationForNextLevel / 11;

        var lootedItems = ArenaModel.GrantLootedItems(LootCatalog, minigameId);
        var lootableItems = ArenaModel.GrantLootableItems(LootCatalog, minigameId);

        var sb = new SeparatedStringBuilder('<');

        sb.Append(membersInGroup.ToString());
        sb.Append(rdmBananaReward.ToString());
        sb.Append(xpReward.ToString());
        sb.Append(lootedItems.ToString());
        sb.Append(lootableItems.ToString());

        SendXt("Mp", minigameId, sb.ToString());

        Player.SendCashUpdate();
    }
}
