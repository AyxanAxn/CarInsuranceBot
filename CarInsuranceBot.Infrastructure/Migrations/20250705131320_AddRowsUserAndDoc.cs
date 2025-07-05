using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarInsuranceBot.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRowsUserAndDoc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Documents_UserId",
                table: "Documents");

            migrationBuilder.AddColumn<int>(
                name: "UploadAttempts",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ContentHash",
                table: "Documents",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Documents_UserId_ContentHash",
                table: "Documents",
                columns: new[] { "UserId", "ContentHash" },
                unique: true,
                filter: "[ContentHash] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Documents_UserId_ContentHash",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "UploadAttempts",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ContentHash",
                table: "Documents");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_UserId",
                table: "Documents",
                column: "UserId");
        }
    }
}
