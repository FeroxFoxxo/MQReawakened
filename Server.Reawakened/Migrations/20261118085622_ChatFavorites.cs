using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Reawakened.Migrations
{
    /// <inheritdoc />
    public partial class ChatFavorites : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ChatFavorites",
                table: "Characters",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChatFavorites",
                table: "Characters");
        }
    }
}
