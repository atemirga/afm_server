using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RobiGroup.AskMeFootball.Data.Migrations
{
    public partial class MultiplierAndHints : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Multiplier",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "HintHistories",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    GamerId = table.Column<string>(nullable: true),
                    MatchId = table.Column<int>(nullable: false),
                    QuestionId = table.Column<int>(nullable: false),
                    UsedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HintHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MultiplierHistories",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    GamerId = table.Column<string>(nullable: true),
                    MatchId = table.Column<int>(nullable: false),
                    QuestionId = table.Column<int>(nullable: false),
                    UsedAt = table.Column<DateTime>(nullable: false),
                    IsMultiplied = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MultiplierHistories", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HintHistories");

            migrationBuilder.DropTable(
                name: "MultiplierHistories");

            migrationBuilder.DropColumn(
                name: "Multiplier",
                table: "AspNetUsers");
        }
    }
}
