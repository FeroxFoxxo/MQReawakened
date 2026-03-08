using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Server.Base.Database.Abstractions;
using Web.Apps.Leaderboards.Data;
using Web.Apps.Leaderboards.Database.Scores;

namespace Web.Apps.Leaderboards.Database;
public class LeaderboardDatabase(DbContextOptions<LeaderboardDatabase> options) : DataContext<LeaderboardDatabase>(options), IDataContextCreate
{
    public DbSet<TopScoresDbEntry> TopScores { get; set; }

    public static void AddContextToServiceProvider(IServiceCollection serviceCollection) =>
        serviceCollection.AddDbContext<LeaderboardDatabase>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // TOP SCORES
        modelBuilder.Entity<TopScoresDbEntry>().Property(e => e.GameId);
        modelBuilder.Entity<TopScoresDbEntry>().Property(e => e.Scores).HasConversion(new CustomConverter<List<TopScore>>());
    }
}
