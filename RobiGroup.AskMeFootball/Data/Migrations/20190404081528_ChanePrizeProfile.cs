using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RobiGroup.AskMeFootball.Data.Migrations
{
    public partial class ChanePrizeProfile : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PrizeProfiles");

            migrationBuilder.AlterColumn<int>(
                name: "InStock",
                table: "Prizes",
                nullable: false,
                oldClrType: typeof(bool));

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Prizes",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Date",
                table: "Prizes",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Facebook",
                table: "Prizes",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirstPhoneNumber",
                table: "Prizes",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Instagram",
                table: "Prizes",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecondPhoneNumber",
                table: "Prizes",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Twitter",
                table: "Prizes",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Vkontakte",
                table: "Prizes",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "Prizes");

            migrationBuilder.DropColumn(
                name: "Date",
                table: "Prizes");

            migrationBuilder.DropColumn(
                name: "Facebook",
                table: "Prizes");

            migrationBuilder.DropColumn(
                name: "FirstPhoneNumber",
                table: "Prizes");

            migrationBuilder.DropColumn(
                name: "Instagram",
                table: "Prizes");

            migrationBuilder.DropColumn(
                name: "SecondPhoneNumber",
                table: "Prizes");

            migrationBuilder.DropColumn(
                name: "Twitter",
                table: "Prizes");

            migrationBuilder.DropColumn(
                name: "Vkontakte",
                table: "Prizes");

            migrationBuilder.AlterColumn<bool>(
                name: "InStock",
                table: "Prizes",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.CreateTable(
                name: "PrizeProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Address = table.Column<string>(nullable: true),
                    Date = table.Column<DateTime>(nullable: false),
                    Facebook = table.Column<string>(nullable: true),
                    FirstPhoneNumber = table.Column<string>(nullable: true),
                    Instagram = table.Column<string>(nullable: true),
                    PrizeId = table.Column<int>(nullable: false),
                    SecondPhoneNumber = table.Column<string>(nullable: true),
                    Twitter = table.Column<string>(nullable: true),
                    Vkontakte = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrizeProfiles", x => x.Id);
                });
        }
    }
}
