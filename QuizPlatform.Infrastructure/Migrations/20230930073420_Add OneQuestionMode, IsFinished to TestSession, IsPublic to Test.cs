using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOneQuestionModeIsFinishedtoTestSessionIsPublictoTest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Tests_SetId",
                table: "Questions");

            migrationBuilder.RenameColumn(
                name: "SetId",
                table: "Questions",
                newName: "TestId");

            migrationBuilder.RenameIndex(
                name: "IX_Questions_SetId",
                table: "Questions",
                newName: "IX_Questions_TestId");

            migrationBuilder.AddColumn<bool>(
                name: "IsFinished",
                table: "TestSessions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "OneQuestionMode",
                table: "TestSessions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "Tests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Tests_TestId",
                table: "Questions",
                column: "TestId",
                principalTable: "Tests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Tests_TestId",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "IsFinished",
                table: "TestSessions");

            migrationBuilder.DropColumn(
                name: "OneQuestionMode",
                table: "TestSessions");

            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "Tests");

            migrationBuilder.RenameColumn(
                name: "TestId",
                table: "Questions",
                newName: "SetId");

            migrationBuilder.RenameIndex(
                name: "IX_Questions_TestId",
                table: "Questions",
                newName: "IX_Questions_SetId");

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Tests_SetId",
                table: "Questions",
                column: "SetId",
                principalTable: "Tests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
