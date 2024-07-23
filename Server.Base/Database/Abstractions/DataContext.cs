using Microsoft.EntityFrameworkCore;
using Server.Base.Core.Extensions;
namespace Server.Base.Database.Abstractions;

public abstract class DataContext<TContext> : BaseDataContext where TContext : DbContext
{
    public string DbPath { get; }

    public DataContext(DbContextOptions<TContext> options) : base(options)
    {
        try
        {
            var path = InternalDirectory.GetDirectory("Saves");
            DbPath = Path.Join(path, $"{typeof(TContext).Name}.db");
        }
        catch (UnauthorizedAccessException)
        {
            // Likely due to migrations being run, safely discard and let configs throw error
        }
    }

    // The following configures EF to create a Sqlite database file in the
    // special "local" folder for your platform.
    protected override void OnConfiguring(DbContextOptionsBuilder options) =>
        options.UseSqlite($"Data Source={DbPath}")
            .EnableDetailedErrors()
            .EnableSensitiveDataLogging();
}
