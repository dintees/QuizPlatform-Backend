using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTestSessioncolumntoUserAnswersentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TestSessionId",
                table: "UserAnswers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswers_TestSessionId",
                table: "UserAnswers",
                column: "TestSessionId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserAnswers_TestSessions_TestSessionId",
                table: "UserAnswers",
                column: "TestSessionId",
                principalTable: "TestSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserAnswers_TestSessions_TestSessionId",
                table: "UserAnswers");

            migrationBuilder.DropIndex(
                name: "IX_UserAnswers_TestSessionId",
                table: "UserAnswers");

            migrationBuilder.DropColumn(
                name: "TestSessionId",
                table: "UserAnswers");
        }
    }
}
