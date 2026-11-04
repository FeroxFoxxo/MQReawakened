using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.XMLs.Bundles.Internal;
using System.Globalization;
using Web.Apps.Leaderboards.Database.Scores;
using Web.Apps.Leaderboards.Enums;

namespace Protocols.External._M__MinigameHandler;
public class CompletedDistance : ExternalProtocol
{
    public override string ProtocolName => "Md";

    public InternalLeaderboards Leaderboards { get; set; }
    public TopScoresHandler TopScoresHandler { get; set; }

    public override void Run(string[] message)
    {
        var completedDistance = int.Parse(message[5]);

        if (Player.Character.BestMinigameTimes.TryGetValue(Player.Room.LevelInfo.Name, out var distance))
        {
            if (distance < completedDistance)
                Player.Character.BestMinigameTimes[Player.Room.LevelInfo.Name] = completedDistance;
        }
        else
        {
            Player.Character.BestMinigameTimes.TryAdd(Player.Room.LevelInfo.Name, completedDistance);
        }

        var game = Leaderboards.Games.FirstOrDefault(x => x.name == Player.Room.LevelInfo.Name);
        
        if (game == null)
            return;

        var scoreTime = DateTime.UtcNow;

        var topScores = TopScoresHandler.GetScoresFromGame(game.id);

        if (topScores == null)
        {
            TopScoresHandler.Create(game.id, completedDistance, 0, scoreTime, Player.Character.Id, ScoreType.AllTime);
            TopScoresHandler.Create(game.id, completedDistance, 0, scoreTime, Player.Character.Id, ScoreType.Daily);
            TopScoresHandler.Create(game.id, completedDistance, 0, scoreTime, Player.Character.Id, ScoreType.Weekly);

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
                        if (existingScore.Score < completedDistance || scoreDate.Date < DateTime.UtcNow.Date)
                        {
                            TopScoresHandler.Remove(existingScore.Id);
                            TopScoresHandler.Create(game.id, completedDistance, 0, scoreTime, Player.Character.Id, ScoreType.Daily);
                            newHighScore = true;
                        }
                        break;
                    case ScoreType.Weekly:
                        if (existingScore.Score < completedDistance || ISOWeek.GetWeekOfYear(scoreDate) != ISOWeek.GetWeekOfYear(DateTime.UtcNow) || scoreDate.Year != DateTime.UtcNow.Year)
                        {
                            TopScoresHandler.Remove(existingScore.Id);
                            TopScoresHandler.Create(game.id, completedDistance, 0, scoreTime, Player.Character.Id, ScoreType.Weekly);
                            newHighScore = true;
                        }
                        break;
                    default:
                        if (existingScore.Score < completedDistance)
                        {
                            TopScoresHandler.Remove(existingScore.Id);
                            TopScoresHandler.Create(game.id, completedDistance, 0, scoreTime, Player.Character.Id, ScoreType.AllTime);
                            newHighScore = true;
                        }
                        break;
                }
            }

            foreach (var scoreType in Enum.GetValues<ScoreType>())
            {
                if (!existingTypes.Contains(scoreType))
                {
                    TopScoresHandler.Create(game.id, completedDistance, 0, scoreTime, Player.Character.Id, scoreType);
                    newHighScore = true;
                }
            }
        }
        else
        {
            TopScoresHandler.Create(game.id, completedDistance, 0, scoreTime, Player.Character.Id, ScoreType.AllTime);
            TopScoresHandler.Create(game.id, completedDistance, 0, scoreTime, Player.Character.Id, ScoreType.Daily);
            TopScoresHandler.Create(game.id, completedDistance, 0, scoreTime, Player.Character.Id, ScoreType.Weekly);
            
            newHighScore = true;
        }

        if (newHighScore)
            Player.SendXt("Ms", Player.Room.LevelInfo.Name);
    }
}
