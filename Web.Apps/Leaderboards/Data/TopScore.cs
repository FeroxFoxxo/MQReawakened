using Web.Apps.Leaderboards.Enums;

namespace Web.Apps.Leaderboards.Data;
public class TopScore
{
    public int Score { get; set; }

    public short Rank { get; set; }

    public string Time { get; set; }

    public int CharacterId { get; set; }

    public ScoreType ScoreType { get; set; }

    public TopScore() { }

    public TopScore (int score, short rank, string time, int id, ScoreType type = ScoreType.AllTime)
    {
        Score = score;
        Rank = rank;
        Time = time;
        CharacterId = id;
        ScoreType = type;
    }

    public TopScore (TopScore other, ScoreType type)
    {
        Score = other.Score;
        Rank = other.Rank;
        Time = other.Time;
        CharacterId = other.CharacterId;
        ScoreType = type;
    }
}
