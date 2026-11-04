using Server.Base.Core.Models;
using Web.Apps.Leaderboards.Enums;

namespace Web.Apps.Leaderboards.Database.Scores;

public class TopScoresDbEntry : PersistantData
{
    public int GameId { get; set; }
    public int Score { get; set; }
    public short Rank { get; set; }
    public DateTime Time { get; set; }
    public int CharacterId { get; set; }
    public ScoreType ScoreType { get; set; }
    
    public TopScoresDbEntry() { }
    
    public TopScoresDbEntry(int gameId, int score, short rank, DateTime time, int id, ScoreType type)
    {
        GameId = gameId;
        Score = score;
        Rank = rank;
        Time = time;
        CharacterId = id;
        ScoreType = type;
    }
}
