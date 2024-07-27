using A2m.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Server.Base.Database.Abstractions;
using Server.Reawakened.Database.Characters;
using Server.Reawakened.Database.Users;
using Server.Reawakened.Players.Models.Character;
using Server.Reawakened.Players.Models.Misc;
using Server.Reawakened.Players.Models.Pets;
using Server.Reawakened.Players.Models.System;

namespace Server.Reawakened.Database;
public class ReawakenedDatabase(DbContextOptions<ReawakenedDatabase> options) : DataContext<ReawakenedDatabase>(options), IDataContextCreate
{
    public DbSet<UserInfoDbEntry> UserInfos { get; set; }
    public DbSet<CharacterDbEntry> Characters { get; set; }

    public static void AddContextToServiceProvider(IServiceCollection serviceCollection) =>
        serviceCollection.AddDbContext<ReawakenedDatabase>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // USER
        modelBuilder.Entity<UserInfoDbEntry>().Property(e => e.CharacterIds).HasConversion(new CustomConverter<List<int>>());
        modelBuilder.Entity<UserInfoDbEntry>().Property(e => e.Mail).HasConversion(new CustomConverter<Dictionary<int, SystemMailModel>>());

        // CHARACTER
        modelBuilder.Entity<CharacterDbEntry>().Property(e => e.RecipeList).HasConversion(new CustomConverter<List<RecipeModel>>());
        modelBuilder.Entity<CharacterDbEntry>().Property(e => e.Items).HasConversion(new CustomConverter<Dictionary<int, ItemModel>>());
        modelBuilder.Entity<CharacterDbEntry>().Property(e => e.HotbarButtons).HasConversion(new CustomConverter<Dictionary<int, ItemModel>>());
        modelBuilder.Entity<CharacterDbEntry>().Property(e => e.StatusEffects).HasConversion(new CustomConverter<Dictionary<ItemEffectType, StatusEffectModel>>());
        modelBuilder.Entity<CharacterDbEntry>().Property(e => e.Properties).HasConversion(new CustomConverter<Dictionary<CustomDataProperties, int>>());
        modelBuilder.Entity<CharacterDbEntry>().Property(e => e.Colors).HasConversion(new CustomConverter<Dictionary<CustomDataProperties, ColorModel>>());
        modelBuilder.Entity<CharacterDbEntry>().Property(e => e.EquippedItems).HasConversion(new CustomConverter<Dictionary<ItemSubCategory, int>>());
        modelBuilder.Entity<CharacterDbEntry>().Property(e => e.EquippedBinding).HasConversion(new CustomConverter<List<ItemSubCategory>>());
        modelBuilder.Entity<CharacterDbEntry>().Property(e => e.DiscoveredStats).HasConversion(new CustomConverter<List<int>>());
        modelBuilder.Entity<CharacterDbEntry>().Property(e => e.QuestLog).HasConversion(new CustomConverter<List<QuestStatusModel>>());
        modelBuilder.Entity<CharacterDbEntry>().Property(e => e.CompletedQuests).HasConversion(new CustomConverter<List<int>>());
        modelBuilder.Entity<CharacterDbEntry>().Property(e => e.TribesDiscovered).HasConversion(new CustomConverter<Dictionary<TribeType, bool>>());
        modelBuilder.Entity<CharacterDbEntry>().Property(e => e.TribesProgression).HasConversion(new CustomConverter<Dictionary<TribeType, TribeDataModel>>());
        modelBuilder.Entity<CharacterDbEntry>().Property(e => e.Friends).HasConversion(new CustomConverter<List<int>>());
        modelBuilder.Entity<CharacterDbEntry>().Property(e => e.Blocked).HasConversion(new CustomConverter<List<int>>());
        modelBuilder.Entity<CharacterDbEntry>().Property(e => e.Muted).HasConversion(new CustomConverter<List<int>>());
        modelBuilder.Entity<CharacterDbEntry>().Property(e => e.Reports).HasConversion(new CustomConverter<Dictionary<string, ReportModel>>());
        modelBuilder.Entity<CharacterDbEntry>().Property(e => e.CollectedIdols).HasConversion(new CustomConverter<Dictionary<int, List<int>>>());
        modelBuilder.Entity<CharacterDbEntry>().Property(e => e.Emails).HasConversion(new CustomConverter<List<EmailHeaderModel>>());
        modelBuilder.Entity<CharacterDbEntry>().Property(e => e.EmailMessages).HasConversion(new CustomConverter<List<EmailMessageModel>>());
        modelBuilder.Entity<CharacterDbEntry>().Property(e => e.Events).HasConversion(new CustomConverter<List<int>>());
        modelBuilder.Entity<CharacterDbEntry>().Property(e => e.AchievementObjectives).HasConversion(new CustomConverter<Dictionary<int, Dictionary<string, int>>>());
        modelBuilder.Entity<CharacterDbEntry>().Property(e => e.BestMinigameTimes).HasConversion(new CustomConverter<Dictionary<string, float>>());
        modelBuilder.Entity<CharacterDbEntry>().Property(e => e.CurrentCollectedDailies).HasConversion(new CustomConverter<Dictionary<string, DailiesModel>>());
        modelBuilder.Entity<CharacterDbEntry>().Property(e => e.CurrentQuestDailies).HasConversion(new CustomConverter<Dictionary<string, DailiesModel>>());
        modelBuilder.Entity<CharacterDbEntry>().Property(e => e.Pets).HasConversion(new CustomConverter<Dictionary<string, PetModel>>());
    }
}
