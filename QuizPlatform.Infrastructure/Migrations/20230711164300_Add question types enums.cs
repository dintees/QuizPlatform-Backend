using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Addquestiontypesenums : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IdType",
                table: "Questions",
                newName: "QuestionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_QuestionTypeId",
                table: "Questions",
                column: "QuestionTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_QuestionTypes_QuestionTypeId",
                table: "Questions",
                column: "QuestionTypeId",
                principalTable: "QuestionTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Questions_QuestionTypes_QuestionTypeId",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_Questions_QuestionTypeId",
                table: "Questions");

            migrationBuilder.RenameColumn(
                name: "QuestionTypeId",
                table: "Questions",
                newName: "IdType");
        }
    }
}
