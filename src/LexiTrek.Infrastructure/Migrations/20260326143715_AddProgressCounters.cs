using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LexiTrek.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProgressCounters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CorrectCount",
                table: "UserWordProgresses",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "IncorrectCount",
                table: "UserWordProgresses",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalReviews",
                table: "UserWordProgresses",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CorrectCount",
                table: "UserWordProgresses");

            migrationBuilder.DropColumn(
                name: "IncorrectCount",
                table: "UserWordProgresses");

            migrationBuilder.DropColumn(
                name: "TotalReviews",
                table: "UserWordProgresses");
        }
    }
}
