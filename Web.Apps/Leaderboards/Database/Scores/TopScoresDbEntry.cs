using Server.Base.Core.Models;
using Web.Apps.Leaderboards.Data;

namespace Web.Apps.Leaderboards.Database.Scores;
public class TopScoresDbEntry : PersistantData
{
    public int GameId { get; set; }

    public List<TopScore> Scores { get; set; }

    public TopScoresDbEntry() { }

    public TopScoresDbEntry(int gameId, List<TopScore> scores)
    {
        GameId = gameId;
        Scores = scores;
    }
}
