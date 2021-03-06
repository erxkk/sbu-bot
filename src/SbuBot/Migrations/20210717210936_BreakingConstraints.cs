using Microsoft.EntityFrameworkCore.Migrations;

namespace SbuBot.Migrations
{
    public partial class BreakingConstraints : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Guilds",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    Config = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guilds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AutoResponses",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    Trigger = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Response = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutoResponses", x => new { x.Trigger, x.GuildId });
                    table.ForeignKey(
                        name: "FK_AutoResponses_Guilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Members",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    GuildId = table.Column<ulong>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Members", x => new { x.Id, x.GuildId });
                    table.ForeignKey(
                        name: "FK_Members_Guilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ColorRoles",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    GuildId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    OwnerId = table.Column<ulong>(type: "numeric(20,0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ColorRoles", x => new { x.Id, x.GuildId });
                    table.ForeignKey(
                        name: "FK_ColorRoles_Guilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ColorRoles_Members_OwnerId_GuildId",
                        columns: x => new { x.OwnerId, x.GuildId },
                        principalTable: "Members",
                        principalColumns: new[] { "Id", "GuildId" },
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Reminders",
                columns: table => new
                {
                    MessageId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    OwnerId = table.Column<ulong>(type: "numeric(20,0)", nullable: true),
                    GuildId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    ChannelId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    Message = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    CreatedAt = table.Column<long>(type: "bigint", nullable: false),
                    DueAt = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reminders", x => x.MessageId);
                    table.ForeignKey(
                        name: "FK_Reminders_Guilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reminders_Members_OwnerId_GuildId",
                        columns: x => new { x.OwnerId, x.GuildId },
                        principalTable: "Members",
                        principalColumns: new[] { "Id", "GuildId" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(type: "numeric(20,0)", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    OwnerId = table.Column<ulong>(type: "numeric(20,0)", nullable: true),
                    Content = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => new { x.Name, x.GuildId });
                    table.ForeignKey(
                        name: "FK_Tags_Guilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Tags_Members_OwnerId_GuildId",
                        columns: x => new { x.OwnerId, x.GuildId },
                        principalTable: "Members",
                        principalColumns: new[] { "Id", "GuildId" },
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AutoResponses_GuildId",
                table: "AutoResponses",
                column: "GuildId");

            migrationBuilder.CreateIndex(
                name: "IX_AutoResponses_Trigger",
                table: "AutoResponses",
                column: "Trigger");

            migrationBuilder.CreateIndex(
                name: "IX_ColorRoles_GuildId",
                table: "ColorRoles",
                column: "GuildId");

            migrationBuilder.CreateIndex(
                name: "IX_ColorRoles_OwnerId_GuildId",
                table: "ColorRoles",
                columns: new[] { "OwnerId", "GuildId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Members_GuildId",
                table: "Members",
                column: "GuildId");

            migrationBuilder.CreateIndex(
                name: "IX_Reminders_GuildId",
                table: "Reminders",
                column: "GuildId");

            migrationBuilder.CreateIndex(
                name: "IX_Reminders_OwnerId",
                table: "Reminders",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Reminders_OwnerId_GuildId",
                table: "Reminders",
                columns: new[] { "OwnerId", "GuildId" });

            migrationBuilder.CreateIndex(
                name: "IX_Tags_GuildId",
                table: "Tags",
                column: "GuildId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_Name",
                table: "Tags",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_OwnerId",
                table: "Tags",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_OwnerId_GuildId",
                table: "Tags",
                columns: new[] { "OwnerId", "GuildId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AutoResponses");

            migrationBuilder.DropTable(
                name: "ColorRoles");

            migrationBuilder.DropTable(
                name: "Reminders");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "Members");

            migrationBuilder.DropTable(
                name: "Guilds");
        }
    }
}
