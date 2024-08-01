using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Base.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.CreateTable(
            name: "Accounts",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Username = table.Column<string>(type: "TEXT", nullable: true),
                Password = table.Column<string>(type: "TEXT", nullable: true),
                Email = table.Column<string>(type: "TEXT", nullable: true),
                AccessLevel = table.Column<int>(type: "INTEGER", nullable: false),
                GameMode = table.Column<int>(type: "INTEGER", nullable: false),
                Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                LastLogin = table.Column<DateTime>(type: "TEXT", nullable: false),
                IpRestrictions = table.Column<string>(type: "TEXT", nullable: true),
                LoginIPs = table.Column<string>(type: "TEXT", nullable: true),
                Tags = table.Column<string>(type: "TEXT", nullable: true),
                Flags = table.Column<int>(type: "INTEGER", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Accounts", x => x.Id));

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable(
            name: "Accounts");
}
