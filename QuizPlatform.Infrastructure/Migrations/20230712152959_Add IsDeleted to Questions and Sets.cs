using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsDeletedtoQuestionsandSets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Sets",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Questions",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Sets");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Questions");
        }
    }
}
