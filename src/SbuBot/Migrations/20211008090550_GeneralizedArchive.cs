using Microsoft.EntityFrameworkCore.Migrations;

namespace SbuBot.Migrations
{
    public partial class GeneralizedArchive : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<ulong>(
                name: "ArchiveId",
                table: "Guilds",
                type: "numeric(20,0)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArchiveId",
                table: "Guilds");
        }
    }
}
