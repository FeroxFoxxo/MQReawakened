using Web.Apps.Leaderboards.Enums;

namespace Web.Apps.Leaderboards.Database.Scores;
public class TopScoresModel(TopScoresDbEntry entry)
{
    public TopScoresDbEntry Write => entry;
    public int Id => entry.Id;
    public int GameId => entry.GameId;
    public int Score => entry.Score;
    public short Rank => entry.Rank;
    public DateTime Time => entry.Time;
    public int CharacterId => entry.CharacterId;
    public ScoreType ScoreType => entry.ScoreType;
}
