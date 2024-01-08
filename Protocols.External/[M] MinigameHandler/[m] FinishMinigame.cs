﻿using Microsoft.Extensions.Logging;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.XMLs.BundlesInternal;

namespace Protocols.External._M__MinigameHandler;
public class FinishedMinigame : ExternalProtocol
{
    public override string ProtocolName => "Mm";

    public LootCatalogInt LootCatalog { get; set; }

    public PlayerHandler PlayerHandler { get; set; }

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

        if (Player.TempData.ArenaModel.BestTimeForLevel.ContainsKey(Player.Room.LevelInfo.Name))
            if (finishedRaceTime < Player.TempData.ArenaModel.BestTimeForLevel[Player.Room.LevelInfo.Name])
            {
                Player.TempData.ArenaModel.BestTimeForLevel[Player.Room.LevelInfo.Name] = finishedRaceTime;
                Player.SendXt("Ms", Player.Room.LevelInfo.InGameName);
            }

        Player.TempData.ArenaModel.StartArena = false;
        Player.TempData.ArenaModel.HasStarted = false;

        var playersInRoom = PlayerHandler.GetAllPlayers();

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

        var dataList = new List<string>();

        var rdmBananaReward = new Random().Next(7, 11 * Player.Character.Data.GlobalLevel);
        var xpReward = Player.Character.Data.ReputationForNextLevel / 11;

        var lootedItems = Player.TempData.ArenaModel.GrantLootedItems(LootCatalog, minigameId);
        var lootableItems = Player.TempData.ArenaModel.GrantLootableItems(LootCatalog, minigameId);

        dataList.Add(membersInGroup.ToString());
        dataList.Add(rdmBananaReward.ToString());
        dataList.Add(xpReward.ToString());
        dataList.Add(lootedItems.ToString());
        dataList.Add(lootableItems.ToString());
        SendXt("Mp", minigameId, SplitRewardData(dataList));

        Player.SendCashUpdate();
    }

    private string SplitRewardData(List<string> dataList)
    {
        var sb = new SeparatedStringBuilder('<');

        foreach (var data in dataList)
            sb.Append(data.ToString());

        return sb.ToString();
    }

}