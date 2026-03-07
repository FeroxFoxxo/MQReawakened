using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Reawakened.Migrations
{
    /// <inheritdoc />
    public partial class MinigameTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Tokens",
                table: "Characters",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Tokens",
                table: "Characters");
        }
    }
}
