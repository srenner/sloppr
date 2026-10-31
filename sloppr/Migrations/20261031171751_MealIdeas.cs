using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sloppr.Migrations
{
    /// <inheritdoc />
    public partial class MealIdeas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KeyIngredients_MealIdea_MealIdeaId",
                table: "KeyIngredients");

            migrationBuilder.DropForeignKey(
                name: "FK_Recipes_MealIdea_MealIdeaId",
                table: "Recipes");

            migrationBuilder.DropIndex(
                name: "IX_KeyIngredients_MealIdeaId",
                table: "KeyIngredients");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MealIdea",
                table: "MealIdea");

            migrationBuilder.DropColumn(
                name: "MealIdeaId",
                table: "KeyIngredients");

            migrationBuilder.RenameTable(
                name: "MealIdea",
                newName: "MealIdeas");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MealIdeas",
                table: "MealIdeas",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "MealIdeaKeyIngredients",
                columns: table => new
                {
                    MealIdeaId = table.Column<int>(type: "INTEGER", nullable: false),
                    KeyIngredientId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MealIdeaKeyIngredients", x => new { x.MealIdeaId, x.KeyIngredientId });
                    table.ForeignKey(
                        name: "FK_MealIdeaKeyIngredients_KeyIngredients_KeyIngredientId",
                        column: x => x.KeyIngredientId,
                        principalTable: "KeyIngredients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MealIdeaKeyIngredients_MealIdeas_MealIdeaId",
                        column: x => x.MealIdeaId,
                        principalTable: "MealIdeas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MealIdeaKeyIngredients_KeyIngredientId",
                table: "MealIdeaKeyIngredients",
                column: "KeyIngredientId");

            migrationBuilder.AddForeignKey(
                name: "FK_Recipes_MealIdeas_MealIdeaId",
                table: "Recipes",
                column: "MealIdeaId",
                principalTable: "MealIdeas",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Recipes_MealIdeas_MealIdeaId",
                table: "Recipes");

            migrationBuilder.DropTable(
                name: "MealIdeaKeyIngredients");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MealIdeas",
                table: "MealIdeas");

            migrationBuilder.RenameTable(
                name: "MealIdeas",
                newName: "MealIdea");

            migrationBuilder.AddColumn<int>(
                name: "MealIdeaId",
                table: "KeyIngredients",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_MealIdea",
                table: "MealIdea",
                column: "Id");

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
    }
}
