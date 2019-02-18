using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RobiGroup.AskMeFootball.Data.Migrations
{
    public partial class AddCardWinners : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CardWinners",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Prize = table.Column<string>(nullable: true),
                    CardStartTime = table.Column<DateTime>(nullable: false),
                    CardEndTime = table.Column<DateTime>(nullable: false),
                    GamerCardScore = table.Column<int>(nullable: false),
                    CardId = table.Column<int>(nullable: false),
                    GamerId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardWinners", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CardWinners_Cards_CardId",
                        column: x => x.CardId,
                        principalTable: "Cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CardWinners_AspNetUsers_GamerId",
                        column: x => x.GamerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CardWinners_CardId",
                table: "CardWinners",
                column: "CardId");

            migrationBuilder.CreateIndex(
                name: "IX_CardWinners_GamerId",
                table: "CardWinners",
                column: "GamerId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CardWinners");
        }
    }
}
