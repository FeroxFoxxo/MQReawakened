using Server.Base.Database.Abstractions;
using Web.Apps.Leaderboards.Data;

namespace Web.Apps.Leaderboards.Database.Scores;
public class TopScoresHandler(IServiceProvider services, LeaderboardLock dbLock) :
    DataHandler<TopScoresDbEntry, LeaderboardDatabase, LeaderboardLock>(services, dbLock)
{
    public override bool HasDefault => false;

    public override TopScoresDbEntry CreateDefault() => null;

    public TopScoresDbEntry Create(int gameId, List<TopScore> scores)
    {
        var user = new TopScoresDbEntry(gameId, scores);

        Add(user, gameId);

        return user;
    }

    public TopScoresModel GetScoresFromId(int id) =>
        GetScoresFromData(Get(id));

    public TopScoresModel GetScoresFromData(TopScoresDbEntry characterScores) =>
        characterScores != null ? new(characterScores) : null;
}
