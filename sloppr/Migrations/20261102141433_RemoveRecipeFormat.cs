using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sloppr.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRecipeFormat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Recipes_RecipeFormats_FormatKey_FormatVersion",
                table: "Recipes");

            migrationBuilder.DropIndex(
                name: "IX_Recipes_FormatKey_FormatVersion",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "FormatKey",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "FormatVersion",
                table: "Recipes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.CreateIndex(
                name: "IX_Recipes_FormatKey_FormatVersion",
                table: "Recipes",
                columns: new[] { "FormatKey", "FormatVersion" });

            migrationBuilder.AddForeignKey(
                name: "FK_Recipes_RecipeFormats_FormatKey_FormatVersion",
                table: "Recipes",
                columns: new[] { "FormatKey", "FormatVersion" },
                principalTable: "RecipeFormats",
                principalColumns: new[] { "Key", "Version" },
                onDelete: ReferentialAction.Restrict);
        }
    }
}
