using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMathModetoQuestion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ExpirationTIme",
                table: "UserTokens",
                newName: "ExpirationTime");

            migrationBuilder.AddColumn<bool>(
                name: "MathMode",
                table: "Questions",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MathMode",
                table: "Questions");

            migrationBuilder.RenameColumn(
                name: "ExpirationTime",
                table: "UserTokens",
                newName: "ExpirationTIme");
        }
    }
}
