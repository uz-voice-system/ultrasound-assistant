using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UltrasoundAssistant.ProjectionService.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTemplateStructureWithBlocks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "keywords");

            migrationBuilder.DropColumn(
                name: "StructureJson",
                table: "templates");

            migrationBuilder.CreateTable(
                name: "template_blocks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Position = table.Column<int>(type: "integer", nullable: false),
                    DefaultFieldName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_template_blocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_template_blocks_templates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "template_block_phrases",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BlockId = table.Column<Guid>(type: "uuid", nullable: false),
                    Phrase = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_template_block_phrases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_template_block_phrases_template_blocks_BlockId",
                        column: x => x.BlockId,
                        principalTable: "template_blocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "template_fields",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BlockId = table.Column<Guid>(type: "uuid", nullable: false),
                    FieldName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Position = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    NormMin = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    NormMax = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    NormUnit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    NormNormalText = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_template_fields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_template_fields_template_blocks_BlockId",
                        column: x => x.BlockId,
                        principalTable: "template_blocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "template_field_phrases",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FieldId = table.Column<Guid>(type: "uuid", nullable: false),
                    Phrase = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_template_field_phrases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_template_field_phrases_template_fields_FieldId",
                        column: x => x.FieldId,
                        principalTable: "template_fields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_templates_Name",
                table: "templates",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_template_block_phrases_BlockId_Phrase",
                table: "template_block_phrases",
                columns: new[] { "BlockId", "Phrase" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_template_blocks_TemplateId_Name",
                table: "template_blocks",
                columns: new[] { "TemplateId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_template_blocks_TemplateId_Position",
                table: "template_blocks",
                columns: new[] { "TemplateId", "Position" });

            migrationBuilder.CreateIndex(
                name: "IX_template_field_phrases_FieldId_Phrase",
                table: "template_field_phrases",
                columns: new[] { "FieldId", "Phrase" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_template_fields_BlockId_FieldName",
                table: "template_fields",
                columns: new[] { "BlockId", "FieldName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_template_fields_BlockId_Position",
                table: "template_fields",
                columns: new[] { "BlockId", "Position" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "template_block_phrases");

            migrationBuilder.DropTable(
                name: "template_field_phrases");

            migrationBuilder.DropTable(
                name: "template_fields");

            migrationBuilder.DropTable(
                name: "template_blocks");

            migrationBuilder.DropIndex(
                name: "IX_templates_Name",
                table: "templates");

            migrationBuilder.AddColumn<string>(
                name: "StructureJson",
                table: "templates",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "keywords",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    Phrase = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TargetField = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_keywords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_keywords_templates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_keywords_TemplateId_Phrase",
                table: "keywords",
                columns: new[] { "TemplateId", "Phrase" },
                unique: true);
        }
    }
}
