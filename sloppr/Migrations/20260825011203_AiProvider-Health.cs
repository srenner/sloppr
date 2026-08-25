using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sloppr.Migrations
{
    /// <inheritdoc />
    public partial class AiProviderHealth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateHealthChecked",
                table: "AiProviders",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HealthCheckPath",
                table: "AiProviders",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsHealthy",
                table: "AiProviders",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastHealthResponse",
                table: "AiProviders",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateHealthChecked",
                table: "AiProviders");

            migrationBuilder.DropColumn(
                name: "HealthCheckPath",
                table: "AiProviders");

            migrationBuilder.DropColumn(
                name: "IsHealthy",
                table: "AiProviders");

            migrationBuilder.DropColumn(
                name: "LastHealthResponse",
                table: "AiProviders");
        }
    }
}
