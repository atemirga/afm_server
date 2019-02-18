using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RobiGroup.AskMeFootball.Data.Migrations
{
    public partial class SeperateGamerCard : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GamerCards_AspNetUsers_GamerId",
                table: "GamerCards");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_GamerCards_CardId_GamerId",
                table: "GamerCards");

            migrationBuilder.AlterColumn<string>(
                name: "GamerId",
                table: "GamerCards",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AddColumn<DateTime>(
                name: "EndTime",
                table: "GamerCards",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "GamerCards",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_GamerCards_CardId",
                table: "GamerCards",
                column: "CardId");

            migrationBuilder.AddForeignKey(
                name: "FK_GamerCards_AspNetUsers_GamerId",
                table: "GamerCards",
                column: "GamerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GamerCards_AspNetUsers_GamerId",
                table: "GamerCards");

            migrationBuilder.DropIndex(
                name: "IX_GamerCards_CardId",
                table: "GamerCards");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "GamerCards");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "GamerCards");

            migrationBuilder.AlterColumn<string>(
                name: "GamerId",
                table: "GamerCards",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_GamerCards_CardId_GamerId",
                table: "GamerCards",
                columns: new[] { "CardId", "GamerId" });

            migrationBuilder.AddForeignKey(
                name: "FK_GamerCards_AspNetUsers_GamerId",
                table: "GamerCards",
                column: "GamerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
