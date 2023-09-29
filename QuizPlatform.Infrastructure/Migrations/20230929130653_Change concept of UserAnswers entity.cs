using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeconceptofUserAnswersentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Answers_UserAnswers_UserAnswersId",
                table: "Answers");

            migrationBuilder.DropIndex(
                name: "IX_Answers_UserAnswersId",
                table: "Answers");

            migrationBuilder.DropColumn(
                name: "UserAnswersId",
                table: "Answers");

            migrationBuilder.AddColumn<int>(
                name: "QuestionAnswerId",
                table: "UserAnswers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswers_QuestionAnswerId",
                table: "UserAnswers",
                column: "QuestionAnswerId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserAnswers_Answers_QuestionAnswerId",
                table: "UserAnswers",
                column: "QuestionAnswerId",
                principalTable: "Answers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserAnswers_Answers_QuestionAnswerId",
                table: "UserAnswers");

            migrationBuilder.DropIndex(
                name: "IX_UserAnswers_QuestionAnswerId",
                table: "UserAnswers");

            migrationBuilder.DropColumn(
                name: "QuestionAnswerId",
                table: "UserAnswers");

            migrationBuilder.AddColumn<int>(
                name: "UserAnswersId",
                table: "Answers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Answers_UserAnswersId",
                table: "Answers",
                column: "UserAnswersId");

            migrationBuilder.AddForeignKey(
                name: "FK_Answers_UserAnswers_UserAnswersId",
                table: "Answers",
                column: "UserAnswersId",
                principalTable: "UserAnswers",
                principalColumn: "Id");
        }
    }
}
