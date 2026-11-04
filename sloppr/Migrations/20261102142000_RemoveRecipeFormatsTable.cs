using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sloppr.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRecipeFormatsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecipeFormats");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RecipeFormats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsCurrent = table.Column<bool>(type: "INTEGER", nullable: false),
                    Key = table.Column<string>(type: "TEXT", nullable: false),
                    Rules = table.Column<string>(type: "TEXT", nullable: false),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeFormats", x => x.Id);
                    table.UniqueConstraint("AK_RecipeFormats_Key_Version", x => new { x.Key, x.Version });
                });

            migrationBuilder.InsertData(
                table: "RecipeFormats",
                columns: new[] { "Id", "CreatedBy", "DateCreated", "IsCurrent", "Key", "Rules", "Version" },
                values: new object[] { 1, "system", new DateTime(2026, 10, 31, 0, 0, 0, 0, DateTimeKind.Utc), true, "standard", "{\n  \"info\": {\n    \"name\": { \"required\": true, \"maxLength\": 120 },\n    \"description\": { \"required\": true, \"maxLength\": 500 },\n    \"servingCount\": { \"min\": 1 },\n    \"prepTimeMinutes\": { \"min\": 0 },\n    \"cookTimeMinutes\": { \"min\": 0 },\n    \"difficulty\": { \"min\": 1, \"max\": 3 },\n    \"notes\": { \"maxLength\": 1000 }\n  },\n  \"ingredients\": {\n    \"minCount\": 1,\n    \"fields\": {\n      \"order\": { \"min\": 1, \"unique\": true },\n      \"quantity\": { \"minExclusive\": 0 },\n      \"unit\": { \"maxLength\": 40 },\n      \"name\": { \"required\": true, \"maxLength\": 120 },\n      \"group\": { \"maxLength\": 50 },\n      \"note\": { \"maxLength\": 200 }\n    }\n  },\n  \"steps\": {\n    \"minCount\": 1,\n    \"fields\": {\n      \"order\": { \"min\": 1, \"unique\": true },\n      \"instruction\": { \"required\": true, \"maxLength\": 1000 },\n      \"group\": { \"maxLength\": 50 }\n    }\n  }\n}", 1 });
        }
    }
}
