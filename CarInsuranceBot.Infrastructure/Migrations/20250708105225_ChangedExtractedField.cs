using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarInsuranceBot.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangedExtractedField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Confidence",
                table: "ExtractedFields");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "Confidence",
                table: "ExtractedFields",
                type: "real",
                nullable: false,
                defaultValue: 0f);
        }
    }
}
