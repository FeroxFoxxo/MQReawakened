using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.XMLs.Bundles.Internal;
using Web.Apps.Leaderboards.Data;
using Web.Apps.Leaderboards.Database.Scores;

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
            Player.Character.Write.BestMinigameTimes.TryAdd(Player.Room.LevelInfo.Name, completedDistance);
        }

        var game = Leaderboards.Games.FirstOrDefault(x => x.name == Player.Room.LevelInfo.Name);
        
        if (game == null)
            return;
        
        var scoreTime = DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'sszzz");

        var score = new TopScore
        {
            Score = completedDistance,
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
        else
        {
            if (topScores.Scores.Any(x => x.CharacterId == Player.Character.Id))
            {
                var existingScore = topScores.Scores.FirstOrDefault(x => x.CharacterId == Player.Character.Id);

                if (score.Score < existingScore.Score)
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
    }
}
