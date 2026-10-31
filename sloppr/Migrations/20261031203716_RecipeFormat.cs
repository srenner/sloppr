using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sloppr.Migrations
{
    /// <inheritdoc />
    public partial class RecipeFormat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FormatKey",
                table: "Recipes",
                type: "TEXT",
                nullable: false,
                defaultValue: "standard");

            migrationBuilder.AddColumn<int>(
                name: "FormatVersion",
                table: "Recipes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Recipes",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RecipeFormats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<string>(type: "TEXT", nullable: false),
                    Version = table.Column<int>(type: "INTEGER", nullable: false),
                    Rules = table.Column<string>(type: "TEXT", nullable: false),
                    IsCurrent = table.Column<bool>(type: "INTEGER", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeFormats", x => x.Id);
                    table.UniqueConstraint("AK_RecipeFormats_Key_Version", x => new { x.Key, x.Version });
                });

            migrationBuilder.CreateTable(
                name: "RecipeIngredients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RecipeId = table.Column<int>(type: "INTEGER", nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    Quantity = table.Column<double>(type: "REAL", nullable: true),
                    Unit = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Group = table.Column<string>(type: "TEXT", nullable: true),
                    Note = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeIngredients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecipeIngredients_Recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "Recipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecipeSteps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RecipeId = table.Column<int>(type: "INTEGER", nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    Instruction = table.Column<string>(type: "TEXT", nullable: false),
                    Group = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecipeSteps_Recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "Recipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "RecipeFormats",
                columns: new[] { "Id", "CreatedBy", "DateCreated", "IsCurrent", "Key", "Rules", "Version" },
                values: new object[] { 1, "system", new DateTime(2026, 10, 31, 0, 0, 0, 0, DateTimeKind.Utc), true, "standard", "{\n  \"info\": {\n    \"name\": { \"required\": true, \"maxLength\": 120 },\n    \"description\": { \"required\": true, \"maxLength\": 500 },\n    \"servingCount\": { \"min\": 1 },\n    \"prepTimeMinutes\": { \"min\": 0 },\n    \"cookTimeMinutes\": { \"min\": 0 },\n    \"difficulty\": { \"min\": 1, \"max\": 3 },\n    \"notes\": { \"maxLength\": 1000 }\n  },\n  \"ingredients\": {\n    \"minCount\": 1,\n    \"fields\": {\n      \"order\": { \"min\": 1, \"unique\": true },\n      \"quantity\": { \"minExclusive\": 0 },\n      \"unit\": { \"maxLength\": 40 },\n      \"name\": { \"required\": true, \"maxLength\": 120 },\n      \"group\": { \"maxLength\": 50 },\n      \"note\": { \"maxLength\": 200 }\n    }\n  },\n  \"steps\": {\n    \"minCount\": 1,\n    \"fields\": {\n      \"order\": { \"min\": 1, \"unique\": true },\n      \"instruction\": { \"required\": true, \"maxLength\": 1000 },\n      \"group\": { \"maxLength\": 50 }\n    }\n  }\n}", 1 });

            migrationBuilder.CreateIndex(
                name: "IX_Recipes_FormatKey_FormatVersion",
                table: "Recipes",
                columns: new[] { "FormatKey", "FormatVersion" });

            migrationBuilder.CreateIndex(
                name: "IX_RecipeIngredients_RecipeId",
                table: "RecipeIngredients",
                column: "RecipeId");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeSteps_RecipeId",
                table: "RecipeSteps",
                column: "RecipeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Recipes_RecipeFormats_FormatKey_FormatVersion",
                table: "Recipes",
                columns: new[] { "FormatKey", "FormatVersion" },
                principalTable: "RecipeFormats",
                principalColumns: new[] { "Key", "Version" },
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Recipes_RecipeFormats_FormatKey_FormatVersion",
                table: "Recipes");

            migrationBuilder.DropTable(
                name: "RecipeFormats");

            migrationBuilder.DropTable(
                name: "RecipeIngredients");

            migrationBuilder.DropTable(
                name: "RecipeSteps");

            migrationBuilder.DropIndex(
                name: "IX_Recipes_FormatKey_FormatVersion",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "FormatKey",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "FormatVersion",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Recipes");
        }
    }
}
