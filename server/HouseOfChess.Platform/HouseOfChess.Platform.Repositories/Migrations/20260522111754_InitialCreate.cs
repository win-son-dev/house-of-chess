using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HouseOfChess.Platform.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Auth0Sub = table.Column<string>(type: "text", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: false),
                    EloBullet = table.Column<int>(type: "integer", nullable: false),
                    EloBlitz = table.Column<int>(type: "integer", nullable: false),
                    EloRapid = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Games",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WhiteUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    BlackUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TimeControl = table.Column<string>(type: "text", nullable: false),
                    Pgn = table.Column<string>(type: "text", nullable: false),
                    Result = table.Column<string>(type: "text", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Games", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Games_Users_BlackUserId",
                        column: x => x.BlackUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Games_Users_WhiteUserId",
                        column: x => x.WhiteUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GameMoves",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GameId = table.Column<Guid>(type: "uuid", nullable: false),
                    Ply = table.Column<int>(type: "integer", nullable: false),
                    San = table.Column<string>(type: "text", nullable: false),
                    Uci = table.Column<string>(type: "text", nullable: false),
                    PlayedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameMoves", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameMoves_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameMoves_GameId_Ply",
                table: "GameMoves",
                columns: new[] { "GameId", "Ply" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Games_BlackUserId",
                table: "Games",
                column: "BlackUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Games_WhiteUserId",
                table: "Games",
                column: "WhiteUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Auth0Sub",
                table: "Users",
                column: "Auth0Sub",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameMoves");

            migrationBuilder.DropTable(
                name: "Games");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
