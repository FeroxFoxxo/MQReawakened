using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Reawakened.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Characters",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                RecipeList = table.Column<string>(type: "TEXT", nullable: true),
                InternalDamageResistPointsStandard = table.Column<int>(type: "INTEGER", nullable: false),
                InternalDamageResistPointsFire = table.Column<int>(type: "INTEGER", nullable: false),
                InternalDamageResistPointsIce = table.Column<int>(type: "INTEGER", nullable: false),
                InternalDamageResistPointsPoison = table.Column<int>(type: "INTEGER", nullable: false),
                InternalDamageResistPointsLightning = table.Column<int>(type: "INTEGER", nullable: false),
                ExternalDamageResistPointsStandard = table.Column<int>(type: "INTEGER", nullable: false),
                ExternalDamageResistPointsFire = table.Column<int>(type: "INTEGER", nullable: false),
                ExternalDamageResistPointsIce = table.Column<int>(type: "INTEGER", nullable: false),
                ExternalDamageResistPointsPoison = table.Column<int>(type: "INTEGER", nullable: false),
                ExternalDamageResistPointsLightning = table.Column<int>(type: "INTEGER", nullable: false),
                ExternalStatusEffectResistSecondsStun = table.Column<int>(type: "INTEGER", nullable: false),
                ExternalStatusEffectResistSecondsSlow = table.Column<int>(type: "INTEGER", nullable: false),
                ExternalStatusEffectResistSecondsFreeze = table.Column<int>(type: "INTEGER", nullable: false),
                LevelId = table.Column<int>(type: "INTEGER", nullable: false),
                SpawnPointId = table.Column<string>(type: "TEXT", nullable: true),
                Items = table.Column<string>(type: "TEXT", nullable: true),
                HotbarButtons = table.Column<string>(type: "TEXT", nullable: true),
                Properties = table.Column<string>(type: "TEXT", nullable: true),
                Colors = table.Column<string>(type: "TEXT", nullable: true),
                EquippedItems = table.Column<string>(type: "TEXT", nullable: true),
                EquippedBinding = table.Column<string>(type: "TEXT", nullable: true),
                PetItemId = table.Column<int>(type: "INTEGER", nullable: false),
                Registered = table.Column<bool>(type: "INTEGER", nullable: false),
                CharacterName = table.Column<string>(type: "TEXT", nullable: true),
                UserUuid = table.Column<int>(type: "INTEGER", nullable: false),
                Gender = table.Column<int>(type: "INTEGER", nullable: false),
                MaxLife = table.Column<int>(type: "INTEGER", nullable: false),
                CurrentLife = table.Column<int>(type: "INTEGER", nullable: false),
                GlobalLevel = table.Column<int>(type: "INTEGER", nullable: false),
                InteractionStatus = table.Column<int>(type: "INTEGER", nullable: false),
                Allegiance = table.Column<int>(type: "INTEGER", nullable: false),
                ForceTribeSelection = table.Column<bool>(type: "INTEGER", nullable: false),
                DiscoveredStats = table.Column<string>(type: "TEXT", nullable: true),
                QuestLog = table.Column<string>(type: "TEXT", nullable: true),
                CompletedQuests = table.Column<string>(type: "TEXT", nullable: true),
                PetAutonomous = table.Column<bool>(type: "INTEGER", nullable: false),
                GuestPassExpiry = table.Column<long>(type: "INTEGER", nullable: false),
                ShouldExpireGuestPass = table.Column<bool>(type: "INTEGER", nullable: false),
                TribesDiscovered = table.Column<string>(type: "TEXT", nullable: true),
                TribesProgression = table.Column<string>(type: "TEXT", nullable: true),
                Friends = table.Column<string>(type: "TEXT", nullable: true),
                Blocked = table.Column<string>(type: "TEXT", nullable: true),
                Muted = table.Column<string>(type: "TEXT", nullable: true),
                Cash = table.Column<int>(type: "INTEGER", nullable: false),
                NCash = table.Column<int>(type: "INTEGER", nullable: false),
                ActiveQuestId = table.Column<int>(type: "INTEGER", nullable: false),
                Reputation = table.Column<int>(type: "INTEGER", nullable: false),
                ReputationForCurrentLevel = table.Column<int>(type: "INTEGER", nullable: false),
                ReputationForNextLevel = table.Column<int>(type: "INTEGER", nullable: false),
                SpawnPositionX = table.Column<float>(type: "REAL", nullable: false),
                SpawnPositionY = table.Column<float>(type: "REAL", nullable: false),
                SpawnOnBackPlane = table.Column<bool>(type: "INTEGER", nullable: false),
                BadgePoints = table.Column<int>(type: "INTEGER", nullable: false),
                AbilityPower = table.Column<int>(type: "INTEGER", nullable: false),
                CollectedIdols = table.Column<string>(type: "TEXT", nullable: true),
                Emails = table.Column<string>(type: "TEXT", nullable: true),
                EmailMessages = table.Column<string>(type: "TEXT", nullable: true),
                Events = table.Column<string>(type: "TEXT", nullable: true),
                AchievementObjectives = table.Column<string>(type: "TEXT", nullable: true),
                BestMinigameTimes = table.Column<string>(type: "TEXT", nullable: true),
                CurrentCollectedDailies = table.Column<string>(type: "TEXT", nullable: true),
                CurrentQuestDailies = table.Column<string>(type: "TEXT", nullable: true),
                Pets = table.Column<string>(type: "TEXT", nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_Characters", x => x.Id));

        migrationBuilder.CreateTable(
            name: "UserInfos",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                CharacterIds = table.Column<string>(type: "TEXT", nullable: true),
                Mail = table.Column<string>(type: "TEXT", nullable: true),
                LastCharacterSelected = table.Column<string>(type: "TEXT", nullable: true),
                AuthToken = table.Column<string>(type: "TEXT", nullable: true),
                Gender = table.Column<int>(type: "INTEGER", nullable: false),
                DateOfBirth = table.Column<DateTime>(type: "TEXT", nullable: false),
                Member = table.Column<bool>(type: "INTEGER", nullable: false),
                SignUpExperience = table.Column<string>(type: "TEXT", nullable: true),
                Region = table.Column<string>(type: "TEXT", nullable: true),
                TrackingShortId = table.Column<string>(type: "TEXT", nullable: true),
                ChatLevel = table.Column<int>(type: "INTEGER", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_UserInfos", x => x.Id));
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Characters");

        migrationBuilder.DropTable(
            name: "UserInfos");
    }
}
