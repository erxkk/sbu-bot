using Microsoft.EntityFrameworkCore.Migrations;

namespace SbuBot.Migrations
{
    public partial class CascadeDeleteGuilds : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ColorRoles_Guilds_GuildId",
                table: "ColorRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_Members_Guilds_GuildId",
                table: "Members");

            migrationBuilder.DropForeignKey(
                name: "FK_Tags_Guilds_GuildId",
                table: "Tags");

            migrationBuilder.AddForeignKey(
                name: "FK_ColorRoles_Guilds_GuildId",
                table: "ColorRoles",
                column: "GuildId",
                principalTable: "Guilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Members_Guilds_GuildId",
                table: "Members",
                column: "GuildId",
                principalTable: "Guilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tags_Guilds_GuildId",
                table: "Tags",
                column: "GuildId",
                principalTable: "Guilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ColorRoles_Guilds_GuildId",
                table: "ColorRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_Members_Guilds_GuildId",
                table: "Members");

            migrationBuilder.DropForeignKey(
                name: "FK_Tags_Guilds_GuildId",
                table: "Tags");

            migrationBuilder.AddForeignKey(
                name: "FK_ColorRoles_Guilds_GuildId",
                table: "ColorRoles",
                column: "GuildId",
                principalTable: "Guilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Members_Guilds_GuildId",
                table: "Members",
                column: "GuildId",
                principalTable: "Guilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Tags_Guilds_GuildId",
                table: "Tags",
                column: "GuildId",
                principalTable: "Guilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
