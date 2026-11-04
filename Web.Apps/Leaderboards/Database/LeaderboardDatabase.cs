using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Server.Base.Database.Abstractions;
using Web.Apps.Leaderboards.Database.Scores;

namespace Web.Apps.Leaderboards.Database;

public class LeaderboardDatabase(DbContextOptions<LeaderboardDatabase> options)
    : DataContext<LeaderboardDatabase>(options), IDataContextCreate
{
    public DbSet<TopScoresDbEntry> TopScores { get; set; }

    public static void AddContextToServiceProvider(IServiceCollection serviceCollection) =>
        serviceCollection.AddDbContext<LeaderboardDatabase>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) => modelBuilder.Entity<TopScoresDbEntry>(entity =>
    {
        entity.HasKey(e => e.Id);

        entity.Property(e => e.GameId).IsRequired();
        entity.Property(e => e.CharacterId).IsRequired();
        entity.Property(e => e.Score).IsRequired();
        entity.Property(e => e.ScoreType).IsRequired();
        entity.Property(e => e.Time).IsRequired();

        entity.HasIndex(e => new { e.GameId, e.ScoreType, e.Score });
        entity.HasIndex(e => new { e.GameId, e.CharacterId, e.ScoreType });
    });
}
