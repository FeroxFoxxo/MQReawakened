using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Server.Base.Database.Abstractions;
using Web.Apps.Leaderboards.Enums;

namespace Web.Apps.Leaderboards.Database.Scores;
public class TopScoresHandler(IServiceProvider services, LeaderboardLock dbLock) :
    DataHandler<TopScoresDbEntry, LeaderboardDatabase, LeaderboardLock>(services, dbLock)
{
    public override bool HasDefault => false;

    public override TopScoresDbEntry CreateDefault() => null;

    public TopScoresDbEntry Create(int gameId, int score, short rank, DateTime time, int id, ScoreType type)
    {
        var scoreEntry = new TopScoresDbEntry(gameId, score, rank, time, id, type);

        Add(scoreEntry);

        return scoreEntry;
    }

    public List<TopScoresModel> GetScoresFromGame(int gameId)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LeaderboardDatabase>();

        lock (DbLock.Lock)
        {
            return [.. db.TopScores.AsNoTracking()
                .Where(t => t.GameId == gameId).AsEnumerable().Select(GetScoreFromData)];
        }
    }

    public List<TopScoresModel> GetScoresFromCharacter(int characterId)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LeaderboardDatabase>();

        lock (DbLock.Lock)
        {
            return [.. db.TopScores.AsNoTracking()
                .Where(t => t.CharacterId == characterId).AsEnumerable().Select(GetScoreFromData)];
        }
    }

    public TopScoresModel GetScoreFromData(TopScoresDbEntry characterScores) =>
        characterScores != null ? new(characterScores) : null;
}
