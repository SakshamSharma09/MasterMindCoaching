using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MasterMind.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class Release_1_0_9_OperationalFixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TeacherSalaries_Teachers_TeacherId",
                table: "TeacherSalaries");

            migrationBuilder.DropForeignKey(
                name: "FK_TeacherSalaries_Users_ProcessedByUserId",
                table: "TeacherSalaries");

            migrationBuilder.AddColumn<string>(
                name: "ObligationKey",
                table: "TeacherSalaries",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CurrentSchool",
                table: "Students",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FatherName",
                table: "Students",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotherName",
                table: "Students",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AccountDeletionRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    EmailOrMobile = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountDeletionRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountDeletionRequests_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "AccountInvitations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    StudentId = table.Column<int>(type: "int", nullable: true),
                    TokenHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountInvitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountInvitations_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AccountInvitations_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AccountInvitations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TeacherSalaries_ObligationKey",
                table: "TeacherSalaries",
                column: "ObligationKey",
                unique: true,
                filter: "[ObligationKey] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AccountDeletionRequests_Status_CreatedAt",
                table: "AccountDeletionRequests",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AccountDeletionRequests_UserId",
                table: "AccountDeletionRequests",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountInvitations_CreatedByUserId",
                table: "AccountInvitations",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountInvitations_StudentId",
                table: "AccountInvitations",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountInvitations_TokenHash",
                table: "AccountInvitations",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccountInvitations_UserId_ExpiresAt",
                table: "AccountInvitations",
                columns: new[] { "UserId", "ExpiresAt" });

            migrationBuilder.AddForeignKey(
                name: "FK_TeacherSalaries_Teachers_TeacherId",
                table: "TeacherSalaries",
                column: "TeacherId",
                principalTable: "Teachers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TeacherSalaries_Users_ProcessedByUserId",
                table: "TeacherSalaries",
                column: "ProcessedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TeacherSalaries_Teachers_TeacherId",
                table: "TeacherSalaries");

            migrationBuilder.DropForeignKey(
                name: "FK_TeacherSalaries_Users_ProcessedByUserId",
                table: "TeacherSalaries");

            migrationBuilder.DropTable(
                name: "AccountDeletionRequests");

            migrationBuilder.DropTable(
                name: "AccountInvitations");

            migrationBuilder.DropIndex(
                name: "IX_TeacherSalaries_ObligationKey",
                table: "TeacherSalaries");

            migrationBuilder.DropColumn(
                name: "ObligationKey",
                table: "TeacherSalaries");

            migrationBuilder.DropColumn(
                name: "CurrentSchool",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "FatherName",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "MotherName",
                table: "Students");

            migrationBuilder.AddForeignKey(
                name: "FK_TeacherSalaries_Teachers_TeacherId",
                table: "TeacherSalaries",
                column: "TeacherId",
                principalTable: "Teachers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TeacherSalaries_Users_ProcessedByUserId",
                table: "TeacherSalaries",
                column: "ProcessedByUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
