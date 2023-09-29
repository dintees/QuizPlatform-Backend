using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeUserAnswersentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserAnswersId",
                table: "Answers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserAnswers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuestionId = table.Column<int>(type: "int", nullable: false),
                    TextAnswerValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShortAnswerValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserAnswers_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Answers_UserAnswersId",
                table: "Answers",
                column: "UserAnswersId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswers_QuestionId",
                table: "UserAnswers",
                column: "QuestionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Answers_UserAnswers_UserAnswersId",
                table: "Answers",
                column: "UserAnswersId",
                principalTable: "UserAnswers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Answers_UserAnswers_UserAnswersId",
                table: "Answers");

            migrationBuilder.DropTable(
                name: "UserAnswers");

            migrationBuilder.DropIndex(
                name: "IX_Answers_UserAnswersId",
                table: "Answers");

            migrationBuilder.DropColumn(
                name: "UserAnswersId",
                table: "Answers");
        }
    }
}
