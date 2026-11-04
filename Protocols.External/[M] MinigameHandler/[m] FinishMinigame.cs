using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Entities.Components.GameObjects.Trigger;
using Server.Reawakened.Entities.Components.GameObjects.Trigger.Interfaces;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Players.Models.Arenas;
using Server.Reawakened.Players.Models.Misc;
using Server.Reawakened.XMLs.Bundles.Base;
using Server.Reawakened.XMLs.Bundles.Internal;
using System.Globalization;
using Web.Apps.Leaderboards.Database.Scores;
using Web.Apps.Leaderboards.Enums;

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

        var bestTime = finishedRaceTime * 1000;
        var leaderboardTime = finishedRaceTime * 100;

        Logger.LogInformation("Minigame with ID ({minigameId}) has completed.", arenaObjectId);

        var players = Player.Room.GetPlayers();

        foreach (var player in players)
            player.SendXt("Mt", arenaObjectId, Player.CharacterId, finishedRaceTime);

        if (Player.Character.BestMinigameTimes.TryGetValue(Player.Room.LevelInfo.Name, out var time))
        {
            if (bestTime < time)
                Player.Character.BestMinigameTimes[Player.Room.LevelInfo.Name] = bestTime;
        }
        else
        {
            Player.Character.BestMinigameTimes.TryAdd(Player.Room.LevelInfo.Name, bestTime);
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

        var scoreTime = DateTime.UtcNow;

        var topScores = TopScoresHandler.GetScoresFromGame(game.id);
        
        if (topScores == null)
        {
            TopScoresHandler.Create(game.id, (int)leaderboardTime, 0, scoreTime, Player.Character.Id, ScoreType.AllTime);
            TopScoresHandler.Create(game.id, (int)leaderboardTime, 0, scoreTime, Player.Character.Id, ScoreType.Daily);
            TopScoresHandler.Create(game.id, (int)leaderboardTime, 0, scoreTime, Player.Character.Id, ScoreType.Weekly);

            Player.SendXt("Ms", Player.Room.LevelInfo.Name);
            return;
        }

        var newHighScore = false;

        if (topScores.Any(x => x.CharacterId == Player.Character.Id))
        {
            var existingScores = topScores
                .Where(x => x.CharacterId == Player.Character.Id).ToList();
            var existingTypes = topScores
                .Where(x => x.CharacterId == Player.Character.Id)
                .Select(x => x.ScoreType).ToList();

            foreach (var existingScore in existingScores)
            {
                var scoreDate = existingScore.Time;

                switch (existingScore.ScoreType)
                {
                    case ScoreType.Daily:
                        if (existingScore.Score > leaderboardTime || scoreDate.Date < DateTime.UtcNow.Date)
                        {
                            TopScoresHandler.Remove(existingScore.Id);
                            TopScoresHandler.Create(game.id, (int)leaderboardTime, 0, scoreTime, Player.Character.Id, ScoreType.Daily);
                            newHighScore = true;
                        }
                        break;
                    case ScoreType.Weekly:
                        if (existingScore.Score > leaderboardTime || ISOWeek.GetWeekOfYear(scoreDate) != ISOWeek.GetWeekOfYear(DateTime.UtcNow) || scoreDate.Year != DateTime.UtcNow.Year)
                        {
                            TopScoresHandler.Remove(existingScore.Id);
                            TopScoresHandler.Create(game.id, (int)leaderboardTime, 0, scoreTime, Player.Character.Id, ScoreType.Weekly);
                            newHighScore = true;
                        }
                        break;
                    default:
                        if (existingScore.Score > leaderboardTime)
                        {
                            TopScoresHandler.Remove(existingScore.Id);
                            TopScoresHandler.Create(game.id, (int)leaderboardTime, 0, scoreTime, Player.Character.Id, ScoreType.AllTime);
                            newHighScore = true;
                        }
                        break;
                }
            }

            foreach (var scoreType in Enum.GetValues<ScoreType>())
            {
                if (!existingTypes.Contains(scoreType))
                {
                    TopScoresHandler.Create(game.id, (int)leaderboardTime, 0, scoreTime, Player.Character.Id, scoreType);
                    newHighScore = true;
                }
            }
        }
        else
        {
            TopScoresHandler.Create(game.id, (int)leaderboardTime, 0, scoreTime, Player.Character.Id, ScoreType.AllTime);
            TopScoresHandler.Create(game.id, (int)leaderboardTime, 0, scoreTime, Player.Character.Id, ScoreType.Daily);
            TopScoresHandler.Create(game.id, (int)leaderboardTime, 0, scoreTime, Player.Character.Id, ScoreType.Weekly);
            
            newHighScore = true;
        }

        if (newHighScore)
            Player.SendXt("Ms", Player.Room.LevelInfo.Name);
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
