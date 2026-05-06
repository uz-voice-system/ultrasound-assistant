using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UltrasoundAssistant.ProjectionService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateReadModelsUsersPatientsSchedulesAppointmentsReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_template_fields_BlockId_Position",
                table: "template_fields");

            migrationBuilder.DropIndex(
                name: "IX_template_blocks_TemplateId_Position",
                table: "template_blocks");

            migrationBuilder.DropIndex(
                name: "IX_reports_PatientId",
                table: "reports");

            migrationBuilder.DropIndex(
                name: "IX_reports_TemplateId",
                table: "reports");

            migrationBuilder.DropColumn(
                name: "DoctorId",
                table: "reports");

            migrationBuilder.DropColumn(
                name: "PatientId",
                table: "reports");

            migrationBuilder.RenameColumn(
                name: "TemplateId",
                table: "reports",
                newName: "AppointmentId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "templates",
                type: "character varying(300)",
                maxLength: 300,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<int>(
                name: "DefaultAppointmentDurationMinutes",
                table: "templates",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "template_fields",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<decimal>(
                name: "NormMin",
                table: "template_fields",
                type: "numeric(18,4)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "NormMax",
                table: "template_fields",
                type: "numeric(18,4)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DisplayName",
                table: "template_fields",
                type: "character varying(300)",
                maxLength: 300,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "template_fields",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "template_blocks",
                type: "character varying(300)",
                maxLength: 300,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "reports",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldMaxLength: 50);

            migrationBuilder.Sql("""
                UPDATE reports
                SET "ContentJson" = '{}'
                WHERE "ContentJson" IS NULL OR btrim("ContentJson") = '';
                """);

            migrationBuilder.Sql("""
                ALTER TABLE reports
                ALTER COLUMN "ContentJson" TYPE jsonb
                USING "ContentJson"::jsonb;
                """);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "patients",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "patients",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "patient_documents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Series = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IssuedBy = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IssueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DepartmentCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Organization = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_patient_documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_patient_documents_patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Login = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FullName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "appointments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Comment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_appointments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_appointments_patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_appointments_templates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "templates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_appointments_users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_appointments_users_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "doctor_profiles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Specialization = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Cabinet = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PhoneExtension = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_doctor_profiles", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_doctor_profiles_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_schedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DayOfWeek = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_schedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_schedules_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_templates_IsDeleted",
                table: "templates",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_template_fields_BlockId_Position",
                table: "template_fields",
                columns: new[] { "BlockId", "Position" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_template_fields_Role",
                table: "template_fields",
                column: "Role");

            migrationBuilder.CreateIndex(
                name: "IX_template_fields_Type",
                table: "template_fields",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_template_field_phrases_Phrase",
                table: "template_field_phrases",
                column: "Phrase");

            migrationBuilder.CreateIndex(
                name: "IX_template_blocks_TemplateId_Position",
                table: "template_blocks",
                columns: new[] { "TemplateId", "Position" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_template_block_phrases_Phrase",
                table: "template_block_phrases",
                column: "Phrase");

            migrationBuilder.CreateIndex(
                name: "IX_reports_AppointmentId",
                table: "reports",
                column: "AppointmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_reports_CreatedAtUtc",
                table: "reports",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_reports_IsDeleted",
                table: "reports",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_reports_Status",
                table: "reports",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_patients_BirthDate",
                table: "patients",
                column: "BirthDate");

            migrationBuilder.CreateIndex(
                name: "IX_patients_FullName",
                table: "patients",
                column: "FullName");

            migrationBuilder.CreateIndex(
                name: "IX_patients_PhoneNumber",
                table: "patients",
                column: "PhoneNumber");

            migrationBuilder.CreateIndex(
                name: "IX_appointments_CreatedByUserId",
                table: "appointments",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_appointments_DoctorId_StartAtUtc_EndAtUtc",
                table: "appointments",
                columns: new[] { "DoctorId", "StartAtUtc", "EndAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_appointments_IsDeleted",
                table: "appointments",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_appointments_PatientId_StartAtUtc",
                table: "appointments",
                columns: new[] { "PatientId", "StartAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_appointments_Status",
                table: "appointments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_appointments_TemplateId",
                table: "appointments",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_patient_documents_DocumentType_Series_Number",
                table: "patient_documents",
                columns: new[] { "DocumentType", "Series", "Number" });

            migrationBuilder.CreateIndex(
                name: "IX_patient_documents_Number",
                table: "patient_documents",
                column: "Number");

            migrationBuilder.CreateIndex(
                name: "IX_patient_documents_PatientId_DocumentType",
                table: "patient_documents",
                columns: new[] { "PatientId", "DocumentType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_schedules_IsDeleted",
                table: "user_schedules",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_user_schedules_UserId_DayOfWeek",
                table: "user_schedules",
                columns: new[] { "UserId", "DayOfWeek" });

            migrationBuilder.CreateIndex(
                name: "IX_users_FullName",
                table: "users",
                column: "FullName");

            migrationBuilder.CreateIndex(
                name: "IX_users_IsActive",
                table: "users",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_users_Login",
                table: "users",
                column: "Login",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_Role",
                table: "users",
                column: "Role");

            migrationBuilder.AddForeignKey(
                name: "FK_reports_appointments_AppointmentId",
                table: "reports",
                column: "AppointmentId",
                principalTable: "appointments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_reports_appointments_AppointmentId",
                table: "reports");

            migrationBuilder.DropTable(
                name: "appointments");

            migrationBuilder.DropTable(
                name: "doctor_profiles");

            migrationBuilder.DropTable(
                name: "patient_documents");

            migrationBuilder.DropTable(
                name: "user_schedules");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropIndex(
                name: "IX_templates_IsDeleted",
                table: "templates");

            migrationBuilder.DropIndex(
                name: "IX_template_fields_BlockId_Position",
                table: "template_fields");

            migrationBuilder.DropIndex(
                name: "IX_template_fields_Role",
                table: "template_fields");

            migrationBuilder.DropIndex(
                name: "IX_template_fields_Type",
                table: "template_fields");

            migrationBuilder.DropIndex(
                name: "IX_template_field_phrases_Phrase",
                table: "template_field_phrases");

            migrationBuilder.DropIndex(
                name: "IX_template_blocks_TemplateId_Position",
                table: "template_blocks");

            migrationBuilder.DropIndex(
                name: "IX_template_block_phrases_Phrase",
                table: "template_block_phrases");

            migrationBuilder.DropIndex(
                name: "IX_reports_AppointmentId",
                table: "reports");

            migrationBuilder.DropIndex(
                name: "IX_reports_CreatedAtUtc",
                table: "reports");

            migrationBuilder.DropIndex(
                name: "IX_reports_IsDeleted",
                table: "reports");

            migrationBuilder.DropIndex(
                name: "IX_reports_Status",
                table: "reports");

            migrationBuilder.DropIndex(
                name: "IX_patients_BirthDate",
                table: "patients");

            migrationBuilder.DropIndex(
                name: "IX_patients_FullName",
                table: "patients");

            migrationBuilder.DropIndex(
                name: "IX_patients_PhoneNumber",
                table: "patients");

            migrationBuilder.DropColumn(
                name: "DefaultAppointmentDurationMinutes",
                table: "templates");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "template_fields");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "patients");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "patients");

            migrationBuilder.RenameColumn(
                name: "AppointmentId",
                table: "reports",
                newName: "TemplateId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "templates",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(300)",
                oldMaxLength: 300);

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "template_fields",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<decimal>(
                name: "NormMin",
                table: "template_fields",
                type: "numeric(10,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "NormMax",
                table: "template_fields",
                type: "numeric(10,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DisplayName",
                table: "template_fields",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(300)",
                oldMaxLength: 300);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "template_blocks",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(300)",
                oldMaxLength: 300);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "reports",
                type: "integer",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.Sql("""
                ALTER TABLE reports
                ALTER COLUMN "ContentJson" TYPE text
                USING "ContentJson"::text;
                """);

            migrationBuilder.AddColumn<Guid>(
                name: "DoctorId",
                table: "reports",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "PatientId",
                table: "reports",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_template_fields_BlockId_Position",
                table: "template_fields",
                columns: new[] { "BlockId", "Position" });

            migrationBuilder.CreateIndex(
                name: "IX_template_blocks_TemplateId_Position",
                table: "template_blocks",
                columns: new[] { "TemplateId", "Position" });

            migrationBuilder.CreateIndex(
                name: "IX_reports_PatientId",
                table: "reports",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_reports_TemplateId",
                table: "reports",
                column: "TemplateId");
        }
    }
}
