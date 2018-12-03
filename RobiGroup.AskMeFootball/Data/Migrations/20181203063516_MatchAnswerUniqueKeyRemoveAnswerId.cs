using Microsoft.EntityFrameworkCore.Migrations;

namespace RobiGroup.AskMeFootball.Data.Migrations
{
    public partial class MatchAnswerUniqueKeyRemoveAnswerId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MatchAnswers_QuestionAnswers_AnswerId",
                table: "MatchAnswers");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_MatchAnswers_MatchGamerId_QuestionId_AnswerId",
                table: "MatchAnswers");

            migrationBuilder.AlterColumn<int>(
                name: "AnswerId",
                table: "MatchAnswers",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddUniqueConstraint(
                name: "AK_MatchAnswers_MatchGamerId_QuestionId",
                table: "MatchAnswers",
                columns: new[] { "MatchGamerId", "QuestionId" });

            migrationBuilder.AddForeignKey(
                name: "FK_MatchAnswers_QuestionAnswers_AnswerId",
                table: "MatchAnswers",
                column: "AnswerId",
                principalTable: "QuestionAnswers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MatchAnswers_QuestionAnswers_AnswerId",
                table: "MatchAnswers");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_MatchAnswers_MatchGamerId_QuestionId",
                table: "MatchAnswers");

            migrationBuilder.AlterColumn<int>(
                name: "AnswerId",
                table: "MatchAnswers",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_MatchAnswers_MatchGamerId_QuestionId_AnswerId",
                table: "MatchAnswers",
                columns: new[] { "MatchGamerId", "QuestionId", "AnswerId" });

            migrationBuilder.AddForeignKey(
                name: "FK_MatchAnswers_QuestionAnswers_AnswerId",
                table: "MatchAnswers",
                column: "AnswerId",
                principalTable: "QuestionAnswers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
