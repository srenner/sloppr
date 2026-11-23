using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sloppr.Migrations
{
    /// <inheritdoc />
    public partial class AddOcrModelSetting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OCRModelId",
                table: "ApplicationSettings",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "ApplicationSettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "OCRModelId",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OCRModelId",
                table: "ApplicationSettings");
        }
    }
}
