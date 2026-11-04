using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Apps.Migrations
{
    /// <inheritdoc />
    public partial class Refactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Scores",
                table: "TopScores");

            migrationBuilder.AddColumn<int>(
                name: "CharacterId",
                table: "TopScores",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<short>(
                name: "Rank",
                table: "TopScores",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<int>(
                name: "Score",
                table: "TopScores",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ScoreType",
                table: "TopScores",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "Time",
                table: "TopScores",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_TopScores_GameId_CharacterId_ScoreType",
                table: "TopScores",
                columns: new[] { "GameId", "CharacterId", "ScoreType" });

            migrationBuilder.CreateIndex(
                name: "IX_TopScores_GameId_ScoreType_Score",
                table: "TopScores",
                columns: new[] { "GameId", "ScoreType", "Score" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TopScores_GameId_CharacterId_ScoreType",
                table: "TopScores");

            migrationBuilder.DropIndex(
                name: "IX_TopScores_GameId_ScoreType_Score",
                table: "TopScores");

            migrationBuilder.DropColumn(
                name: "CharacterId",
                table: "TopScores");

            migrationBuilder.DropColumn(
                name: "Rank",
                table: "TopScores");

            migrationBuilder.DropColumn(
                name: "Score",
                table: "TopScores");

            migrationBuilder.DropColumn(
                name: "ScoreType",
                table: "TopScores");

            migrationBuilder.DropColumn(
                name: "Time",
                table: "TopScores");

            migrationBuilder.AddColumn<string>(
                name: "Scores",
                table: "TopScores",
                type: "TEXT",
                nullable: true);
        }
    }
}
