using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangerelationtypeinTestSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TestSessions_UserId",
                table: "TestSessions");

            migrationBuilder.CreateIndex(
                name: "IX_TestSessions_UserId",
                table: "TestSessions",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TestSessions_UserId",
                table: "TestSessions");

            migrationBuilder.CreateIndex(
                name: "IX_TestSessions_UserId",
                table: "TestSessions",
                column: "UserId",
                unique: true);
        }
    }
}
