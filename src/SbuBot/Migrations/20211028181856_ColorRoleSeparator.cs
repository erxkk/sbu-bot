using Microsoft.EntityFrameworkCore.Migrations;

namespace SbuBot.Migrations
{
    public partial class ColorRoleSeparator : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<ulong>(
                name: "ColorRoleBottomId",
                table: "Guilds",
                type: "numeric(20,0)",
                nullable: true);

            migrationBuilder.AddColumn<ulong>(
                name: "ColorRoleTopId",
                table: "Guilds",
                type: "numeric(20,0)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ColorRoleBottomId",
                table: "Guilds");

            migrationBuilder.DropColumn(
                name: "ColorRoleTopId",
                table: "Guilds");
        }
    }
}
