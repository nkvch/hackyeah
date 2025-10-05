using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UknfPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Add_Authentication_Tables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastPasswordChangeDate",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ActivationTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivationTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivationTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuthenticationAuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Success = table.Column<bool>(type: "boolean", nullable: false),
                    FailureReason = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthenticationAuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuthenticationAuditLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PasswordHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PasswordHistories_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsRevoked = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    RevokedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    FileName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FileStorageKey = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    ReportType = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    ReportingPeriod = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ValidationStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ValidationResultFileKey = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UniqueValidationId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsArchived = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsCorrectionOfReportId = table.Column<Guid>(type: "uuid", nullable: true),
                    SubmittedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ValidationStartedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ValidationCompletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ErrorDescription = table.Column<string>(type: "text", nullable: true),
                    ContestedDescription = table.Column<string>(type: "text", nullable: true),
                    ContestedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ContestedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActivationTokens_IsUsed",
                table: "ActivationTokens",
                column: "IsUsed");

            migrationBuilder.CreateIndex(
                name: "IX_ActivationTokens_Token",
                table: "ActivationTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ActivationTokens_UserId_ExpiresAt",
                table: "ActivationTokens",
                columns: new[] { "UserId", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AuthenticationAuditLogs_Email",
                table: "AuthenticationAuditLogs",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_AuthenticationAuditLogs_Email_Timestamp",
                table: "AuthenticationAuditLogs",
                columns: new[] { "Email", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_AuthenticationAuditLogs_Success",
                table: "AuthenticationAuditLogs",
                column: "Success");

            migrationBuilder.CreateIndex(
                name: "IX_AuthenticationAuditLogs_Timestamp",
                table: "AuthenticationAuditLogs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_AuthenticationAuditLogs_UserId",
                table: "AuthenticationAuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordHistory_UserId_CreatedDate",
                table: "PasswordHistories",
                columns: new[] { "UserId", "CreatedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_Token",
                table: "RefreshTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId_ExpiresAt",
                table: "RefreshTokens",
                columns: new[] { "UserId", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Reports_Entity_Period_Type",
                table: "Reports",
                columns: new[] { "EntityId", "ReportingPeriod", "ReportType" });

            migrationBuilder.CreateIndex(
                name: "IX_Reports_EntityId",
                table: "Reports",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_IsArchived",
                table: "Reports",
                column: "IsArchived");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_ReportingPeriod",
                table: "Reports",
                column: "ReportingPeriod");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_SubmittedDate",
                table: "Reports",
                column: "SubmittedDate");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_ValidationStatus",
                table: "Reports",
                column: "ValidationStatus");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivationTokens");

            migrationBuilder.DropTable(
                name: "AuthenticationAuditLogs");

            migrationBuilder.DropTable(
                name: "PasswordHistories");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "Reports");

            migrationBuilder.DropColumn(
                name: "LastPasswordChangeDate",
                table: "Users");
        }
    }
}
