using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RobiGroup.AskMeFootball.Data.Migrations
{
    public partial class AddGamerCardToMatchGamer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GamerCardId",
                table: "MatchGamers",
                nullable: false,
                defaultValue: 0);
            
            migrationBuilder.Sql("UPDATE mg SET mg.GamerCardId = gc.Id " +
                                 "FROM MatchGamers mg " +
                                 "INNER join Matches m on m.Id = mg.MatchId " +
                                 "INNER JOIN Cards c ON c.Id = m.CardId " +
                                 "INNER join GamerCards gc on gc.CardId = c.Id AND gc.GamerId = mg.GamerId", true);

            migrationBuilder.CreateIndex(
                name: "IX_MatchGamers_GamerCardId",
                table: "MatchGamers",
                column: "GamerCardId");

            migrationBuilder.AddForeignKey(
                name: "FK_MatchGamers_GamerCards_GamerCardId",
                table: "MatchGamers",
                column: "GamerCardId",
                principalTable: "GamerCards",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MatchGamers_GamerCards_GamerCardId",
                table: "MatchGamers");

            migrationBuilder.DropIndex(
                name: "IX_MatchGamers_GamerCardId",
                table: "MatchGamers");

            migrationBuilder.DropColumn(
                name: "GamerCardId",
                table: "MatchGamers");
        }
    }
}
