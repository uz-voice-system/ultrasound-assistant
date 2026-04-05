using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UltrasoundAssistant.ProjectionService.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteTemplateReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "templates",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "reports",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "templates");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "reports");
        }
    }
}
