using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UknfPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    [Migration("20251005014153")]
    public partial class AddUserProfileManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add PendingEmail column to Users table
            migrationBuilder.AddColumn<string>(
                name: "PendingEmail",
                table: "Users",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            // Create EmailChangeTokens table
            migrationBuilder.CreateTable(
                name: "EmailChangeTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    NewEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailChangeTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailChangeTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create indexes for EmailChangeTokens
            migrationBuilder.CreateIndex(
                name: "IX_EmailChangeTokens_Token",
                table: "EmailChangeTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmailChangeTokens_UserId_ExpiresAt",
                table: "EmailChangeTokens",
                columns: new[] { "UserId", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_EmailChangeTokens_IsUsed",
                table: "EmailChangeTokens",
                column: "IsUsed");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop EmailChangeTokens table
            migrationBuilder.DropTable(
                name: "EmailChangeTokens");

            // Remove PendingEmail column from Users table
            migrationBuilder.DropColumn(
                name: "PendingEmail",
                table: "Users");
        }
    }
}

