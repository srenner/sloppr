using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sloppr.Migrations
{
    /// <inheritdoc />
    public partial class AiProvidersModelDiscoveryPath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ModelDiscoveryPath",
                table: "AiProviders",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ModelDiscoveryPath",
                table: "AiProviders");
        }
    }
}
