using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Entities.Components.GameObjects.Trigger;
using Server.Reawakened.Entities.Components.GameObjects.Trigger.Interfaces;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Players.Models.Arenas;
using Server.Reawakened.XMLs.Bundles.Base;
using Server.Reawakened.XMLs.Bundles.Internal;
using Web.Apps.Leaderboards.Data;
using Web.Apps.Leaderboards.Database.Scores;

namespace Protocols.External._M__MinigameHandler;

public class FinishedMinigame : ExternalProtocol
{
    public override string ProtocolName => "Mm";

    public WorldStatistics WorldStatistics { get; set; }
    public InternalLoot LootCatalog { get; set; }
    public ILogger<FinishedMinigame> Logger { get; set; }
    public TopScoresHandler TopScoresHandler { get; set; }
    public InternalLeaderboards Leaderboards { get; set; }

    public override void Run(string[] message)
    {
        var arenaObjectId = message[5];
        var finishedRaceTime = float.Parse(message[6]);

        var correctedTime = finishedRaceTime * 100;

        Logger.LogInformation("Minigame with ID ({minigameId}) has completed.", arenaObjectId);

        var players = Player.Room.GetPlayers();

        foreach (var player in players)
            player.SendXt("Mt", arenaObjectId, Player.CharacterId, finishedRaceTime);

        if (Player.Character.BestMinigameTimes.TryGetValue(Player.Room.LevelInfo.Name, out var time))
        {
            if (finishedRaceTime < time)
                Player.Character.BestMinigameTimes[Player.Room.LevelInfo.Name] = finishedRaceTime;
        }
        else
        {
            Player.Character.BestMinigameTimes.TryAdd(Player.Room.LevelInfo.Name, finishedRaceTime);
        }

        var trigger = Player.Room.GetEntityFromId<ITriggerComp>(arenaObjectId);

        if (trigger == null)
        {
            Logger.LogError("Cannot find statue with ID: {ID}", arenaObjectId);
            return;
        }

        trigger.RemovePhysicalInteractor(Player, Player.GameObjectId);

        if (trigger.GetPhysicalInteractorCount() <= 0)
        {
            foreach (var player in players)
                FinishMinigame(player, arenaObjectId, players.Length);

            trigger.RunTrigger(Player);
            trigger.ResetTrigger();
        }

        var game = Leaderboards.Games.FirstOrDefault(x => x.name == Player.Room.LevelInfo.Name);
        
        if (game == null)
            return;

        var scoreTime = DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'sszzz");

        var score = new TopScore
        {
            Score = (int)correctedTime,
            Rank = 0,
            Time = scoreTime,
            CharacterId = Player.Character.Id
        };

        var topScores = TopScoresHandler.GetScoresFromId(game.id);

        if (topScores == null)
        {
            var scores = new List<TopScore> { score };

            TopScoresHandler.Create(game.id, scores);

            topScores = TopScoresHandler.GetScoresFromId(game.id);
        }

        if (topScores.Scores.Any(x => x.CharacterId == Player.Character.Id))
        {
            var existingScore = topScores.Scores.FirstOrDefault(x => x.CharacterId == Player.Character.Id);

            if (existingScore.Score < score.Score)
                return;

            topScores.Scores.Remove(existingScore);
            topScores.Scores.Add(score);

            TopScoresHandler.Update(topScores.Write);

            Player.SendXt("Ms", Player.Room.LevelInfo.Name);
        }
        else
        {
            topScores.Scores.Add(score);

            TopScoresHandler.Update(topScores.Write);

            Player.SendXt("Ms", Player.Room.LevelInfo.Name);
        }
    }

    public void FinishMinigame(Player player, string minigameId, int membersInRoom)
    {
        player.SendSyncEventToPlayer(new TriggerUpdate_SyncEvent(minigameId, player.Room.Time, membersInRoom));

        var bananaReward = WorldStatistics.GetValue(ItemEffectType.BananaReward, WorldStatisticsGroup.Price, player.Character.GlobalLevel);
        var xpReward = (player.Character.ReputationForNextLevel - player.Character.ReputationForCurrentLevel) *
            WorldStatistics.GlobalStats[Globals.MinigameXPMultiplier] + WorldStatistics.GetValue(ItemEffectType.IncreaseExpFromMinigameLT16,
            WorldStatisticsGroup.Player, player.Character.GlobalLevel);

        var lootedItems = ArenaModel.GrantLootedItems(LootCatalog, player.Room.LevelInfo.LevelId, minigameId);
        var lootableItems = ArenaModel.GrantLootableItems(LootCatalog, player.Room.LevelInfo.LevelId, minigameId);

        var sb = new SeparatedStringBuilder('<');

        sb.Append(membersInRoom.ToString());
        sb.Append(bananaReward.ToString());
        sb.Append(xpReward.ToString());
        sb.Append(lootedItems.ToString());
        sb.Append(lootableItems.ToString());

        player.SendXt("Mp", minigameId, sb.ToString());

        player.SendCashUpdate();
    }
}
