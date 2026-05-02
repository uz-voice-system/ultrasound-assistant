using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UltrasoundAssistant.AggregationService.Migrations
{
    /// <inheritdoc />
    public partial class SyncAggregationModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "processed_commands");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "events",
                newName: "EventId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EventId",
                table: "events",
                newName: "Id");

            migrationBuilder.CreateTable(
                name: "processed_commands",
                columns: table => new
                {
                    CommandId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcessedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_processed_commands", x => x.CommandId);
                });
        }
    }
}
