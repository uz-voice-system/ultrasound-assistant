using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UltrasoundAssistant.ProjectionService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReportUltrasoundImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "UltrasoundImageBytes",
                table: "reports",
                type: "bytea",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UltrasoundImageContentType",
                table: "reports",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UltrasoundImageFileName",
                table: "reports",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UltrasoundImageUploadedAtUtc",
                table: "reports",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UltrasoundImageBytes",
                table: "reports");

            migrationBuilder.DropColumn(
                name: "UltrasoundImageContentType",
                table: "reports");

            migrationBuilder.DropColumn(
                name: "UltrasoundImageFileName",
                table: "reports");

            migrationBuilder.DropColumn(
                name: "UltrasoundImageUploadedAtUtc",
                table: "reports");
        }
    }
}
