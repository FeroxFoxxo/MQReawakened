using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Web.Apps.Leaderboards.Database.Scores;

namespace Protocols.External._M__MinigameHandler;
public class GetDistance : ExternalProtocol
{
    public override string ProtocolName => "MD";

    public TopScoresHandler TopScoresHandler { get; set; }

    public override void Run(string[] message)
    {
        var monkeyBlastId = 5;
        var topScores = TopScoresHandler.GetScoresFromId(monkeyBlastId);

        if (topScores != null && topScores.Scores.Count > 0)
        {
            if (topScores.Scores.Any(x => x.CharacterId == Player.Character.Id))
            {
                var score = topScores.Scores.FirstOrDefault(x => x.CharacterId == Player.Character.Id);

                Player.SendXt("MD", (long)score.Score);
            }
        }
    }
}
