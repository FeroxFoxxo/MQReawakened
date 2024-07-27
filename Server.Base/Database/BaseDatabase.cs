using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Server.Base.Accounts.Models;
using Server.Base.Database.Abstractions;
using Server.Base.Database.Accounts;

namespace Server.Base.Database;
public class BaseDatabase(DbContextOptions<BaseDatabase> options) : DataContext<BaseDatabase>(options), IDataContextCreate
{
    public DbSet<AccountDbEntry> Accounts { get; set; }

    public static void AddContextToServiceProvider(IServiceCollection serviceCollection) =>
        serviceCollection.AddDbContext<BaseDatabase>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Accounts
        modelBuilder.Entity<AccountDbEntry>().Property(e => e.Tags).HasConversion(new CustomConverter<List<AccountTag>>());
        modelBuilder.Entity<AccountDbEntry>().Property(e => e.IpRestrictions).HasConversion(new CustomConverter<string[]>());
        modelBuilder.Entity<AccountDbEntry>().Property(e => e.LoginIPs).HasConversion(new CustomConverter<string[]>());
    }
}
