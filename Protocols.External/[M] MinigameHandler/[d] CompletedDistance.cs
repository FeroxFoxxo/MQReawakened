using Server.Base.Core.Extensions;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.XMLs.Bundles.Internal;
using System.Globalization;
using Web.Apps.Leaderboards.Data;
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

        var scoreTime = DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'sszzz");

        var score = new TopScore
        (
            (int)completedDistance,
            0,
            scoreTime,
            Player.Character.Id
        );

        var topScores = TopScoresHandler.GetScoresFromId(game.id);

        if (topScores == null)
        {
            var scoreDaily = new TopScore(score, ScoreType.Daily);
            var scoreWeekly = new TopScore(score, ScoreType.Weekly);

            var scores = new List<TopScore> { score, scoreDaily, scoreWeekly };

            TopScoresHandler.Create(game.id, scores);

            Player.SendXt("Ms", Player.Room.LevelInfo.Name);
            return;
        }

        var newHighScore = false;

        if (topScores.Scores.Any(x => x.CharacterId == Player.Character.Id))
        {
            var existingScores = topScores.Scores.FindAll(x => x.CharacterId == Player.Character.Id).DeepCopy();
            var existingTypes = existingScores.Select(x => x.ScoreType).ToHashSet();

            foreach (var existingScore in existingScores)
            {
                var scoreDate = DateTime.Parse(existingScore.Time);

                switch (existingScore.ScoreType)
                {
                    case ScoreType.Daily:
                        if (existingScore.Score < score.Score || scoreDate.Date < DateTime.Now.Date)
                        {
                            topScores.Scores.Remove(existingScore);
                            topScores.Scores.Add(new TopScore(score, ScoreType.Daily));
                            newHighScore = true;
                        }
                        break;
                    case ScoreType.Weekly:
                        if (existingScore.Score < score.Score || ISOWeek.GetWeekOfYear(scoreDate) != ISOWeek.GetWeekOfYear(DateTime.Now) || scoreDate.Year != DateTime.Now.Year)
                        {
                            topScores.Scores.Remove(existingScore);
                            topScores.Scores.Add(new TopScore(score, ScoreType.Weekly));
                            newHighScore = true;
                        }
                        break;
                    default:
                        if (existingScore.Score < score.Score)
                        {
                            topScores.Scores.Remove(existingScore);
                            topScores.Scores.Add(score);
                            newHighScore = true;
                        }
                        break;
                }
            }

            foreach (var scoreType in Enum.GetValues<ScoreType>())
            {
                if (!existingTypes.Contains(scoreType))
                {
                    topScores.Scores.Add(new TopScore(score, scoreType));
                    newHighScore = true;
                }
            }
        }
        else
        {
            topScores.Scores.Add(score);
            topScores.Scores.Add(new TopScore(score, ScoreType.Daily));
            topScores.Scores.Add(new TopScore(score, ScoreType.Weekly));

            newHighScore = true;
        }

        if (newHighScore)
        {
            TopScoresHandler.Update(topScores.Write);

            Player.SendXt("Ms", Player.Room.LevelInfo.Name);
        }
    }
}
