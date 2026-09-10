using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sloppr.Migrations
{
    /// <inheritdoc />
    public partial class ApplicationSetting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TextFormat",
                table: "Recipes",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "PrepTime",
                table: "Recipes",
                newName: "ServingCount");

            migrationBuilder.RenameColumn(
                name: "FullText",
                table: "Recipes",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "CookTime",
                table: "Recipes",
                newName: "PrepTimeMinutes");

            migrationBuilder.AddColumn<int>(
                name: "CookTimeMinutes",
                table: "Recipes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Difficulty",
                table: "Recipes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MealIdeaId",
                table: "Recipes",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MealIdeaId",
                table: "KeyIngredients",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ApplicationSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    ApplicationName = table.Column<string>(type: "TEXT", nullable: false),
                    ExtractionModelId = table.Column<int>(type: "INTEGER", nullable: true),
                    IdeaModelId = table.Column<int>(type: "INTEGER", nullable: true),
                    RecipeModelId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationSettings", x => x.Id);
                    table.CheckConstraint("CK_Settings_SingleRow", "[Id] = 1");
                });

            migrationBuilder.CreateTable(
                name: "MealIdea",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    IsUserApproved = table.Column<bool>(type: "INTEGER", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MealIdea", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "ApplicationSettings",
                columns: new[] { "Id", "ApplicationName", "ExtractionModelId", "IdeaModelId", "RecipeModelId" },
                values: new object[] { 1, "sloppr", null, null, null });

            migrationBuilder.CreateIndex(
                name: "IX_Recipes_MealIdeaId",
                table: "Recipes",
                column: "MealIdeaId");

            migrationBuilder.CreateIndex(
                name: "IX_KeyIngredients_MealIdeaId",
                table: "KeyIngredients",
                column: "MealIdeaId");

            migrationBuilder.AddForeignKey(
                name: "FK_KeyIngredients_MealIdea_MealIdeaId",
                table: "KeyIngredients",
                column: "MealIdeaId",
                principalTable: "MealIdea",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Recipes_MealIdea_MealIdeaId",
                table: "Recipes",
                column: "MealIdeaId",
                principalTable: "MealIdea",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KeyIngredients_MealIdea_MealIdeaId",
                table: "KeyIngredients");

            migrationBuilder.DropForeignKey(
                name: "FK_Recipes_MealIdea_MealIdeaId",
                table: "Recipes");

            migrationBuilder.DropTable(
                name: "ApplicationSettings");

            migrationBuilder.DropTable(
                name: "MealIdea");

            migrationBuilder.DropIndex(
                name: "IX_Recipes_MealIdeaId",
                table: "Recipes");

            migrationBuilder.DropIndex(
                name: "IX_KeyIngredients_MealIdeaId",
                table: "KeyIngredients");

            migrationBuilder.DropColumn(
                name: "CookTimeMinutes",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "Difficulty",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "MealIdeaId",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "MealIdeaId",
                table: "KeyIngredients");

            migrationBuilder.RenameColumn(
                name: "ServingCount",
                table: "Recipes",
                newName: "PrepTime");

            migrationBuilder.RenameColumn(
                name: "PrepTimeMinutes",
                table: "Recipes",
                newName: "CookTime");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Recipes",
                newName: "TextFormat");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Recipes",
                newName: "FullText");
        }
    }
}
