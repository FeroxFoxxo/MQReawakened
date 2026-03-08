using Web.Apps.Leaderboards.Data;

namespace Web.Apps.Leaderboards.Database.Scores;
public class TopScoresModel(TopScoresDbEntry entry)
{
    public TopScoresDbEntry Write => entry;

    public int GameId => Write.GameId;

    public List<TopScore> Scores => Write.Scores;
}
