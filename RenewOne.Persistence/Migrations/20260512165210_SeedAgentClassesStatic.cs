using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RenewOne.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedAgentClassesStatic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AgentClasses",
                columns: new[] { "Id", "ClassName", "CommSharePercentage", "CreatedAt", "IsActive", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("550e8400-e29b-41d4-a716-446655440000"), "Premium", 15m, new DateTime(2026, 5, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), true, null },
                    { new Guid("550e8400-e29b-41d4-a716-446655440001"), "Standard", 10m, new DateTime(2026, 5, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), true, null },
                    { new Guid("550e8400-e29b-41d4-a716-446655440002"), "Basic", 5m, new DateTime(2026, 5, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), true, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AgentClasses",
                keyColumn: "Id",
                keyValue: new Guid("550e8400-e29b-41d4-a716-446655440000"));

            migrationBuilder.DeleteData(
                table: "AgentClasses",
                keyColumn: "Id",
                keyValue: new Guid("550e8400-e29b-41d4-a716-446655440001"));

            migrationBuilder.DeleteData(
                table: "AgentClasses",
                keyColumn: "Id",
                keyValue: new Guid("550e8400-e29b-41d4-a716-446655440002"));
        }
    }
}
