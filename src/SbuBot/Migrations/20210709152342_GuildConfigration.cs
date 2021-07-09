using Microsoft.EntityFrameworkCore.Migrations;

namespace SbuBot.Migrations
{
    public partial class GuildConfigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "Config",
                table: "Guilds",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Config",
                table: "Guilds");
        }
    }
}
