using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sloppr.Migrations
{
    /// <inheritdoc />
    public partial class AiProvidersPathCleanup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HealthCheckPath",
                table: "AiProviders");

            migrationBuilder.DropColumn(
                name: "ModelDiscoveryPath",
                table: "AiProviders");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HealthCheckPath",
                table: "AiProviders",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModelDiscoveryPath",
                table: "AiProviders",
                type: "TEXT",
                nullable: true);
        }
    }
}
