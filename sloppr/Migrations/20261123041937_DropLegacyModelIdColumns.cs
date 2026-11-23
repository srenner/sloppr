using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sloppr.Migrations
{
    /// <inheritdoc />
    public partial class DropLegacyModelIdColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExtractionModelId",
                table: "ApplicationSettings");

            migrationBuilder.DropColumn(
                name: "IdeaModelId",
                table: "ApplicationSettings");

            migrationBuilder.DropColumn(
                name: "OCRModelId",
                table: "ApplicationSettings");

            migrationBuilder.DropColumn(
                name: "RecipeModelId",
                table: "ApplicationSettings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ExtractionModelId",
                table: "ApplicationSettings",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdeaModelId",
                table: "ApplicationSettings",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OCRModelId",
                table: "ApplicationSettings",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RecipeModelId",
                table: "ApplicationSettings",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "ApplicationSettings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ExtractionModelId", "IdeaModelId", "OCRModelId", "RecipeModelId" },
                values: new object[] { null, null, null, null });
        }
    }
}
