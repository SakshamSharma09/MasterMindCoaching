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
            // Production originated from EnsureCreated and has known schema drift. Keep this
            // release migration idempotent so existing records are never replaced or backfilled.
            migrationBuilder.Sql("""
                IF COL_LENGTH('dbo.Students', 'CurrentSchool') IS NULL
                    ALTER TABLE dbo.Students ADD CurrentSchool nvarchar(200) NULL;
                IF COL_LENGTH('dbo.Students', 'FatherName') IS NULL
                    ALTER TABLE dbo.Students ADD FatherName nvarchar(200) NULL;
                IF COL_LENGTH('dbo.Students', 'MotherName') IS NULL
                    ALTER TABLE dbo.Students ADD MotherName nvarchar(200) NULL;

                IF OBJECT_ID('dbo.TeacherSalaries', 'U') IS NOT NULL
                   AND COL_LENGTH('dbo.TeacherSalaries', 'ObligationKey') IS NULL
                    ALTER TABLE dbo.TeacherSalaries ADD ObligationKey nvarchar(80) NULL;

                IF OBJECT_ID('dbo.AccountDeletionRequests', 'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.AccountDeletionRequests
                    (
                        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_AccountDeletionRequests PRIMARY KEY,
                        UserId int NULL,
                        EmailOrMobile nvarchar(255) NOT NULL,
                        Reason nvarchar(1000) NULL,
                        Status nvarchar(40) NOT NULL,
                        CompletedAt datetime2 NULL,
                        CreatedAt datetime2 NOT NULL,
                        UpdatedAt datetime2 NULL,
                        IsDeleted bit NOT NULL
                    );
                END

                IF OBJECT_ID('dbo.AccountInvitations', 'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.AccountInvitations
                    (
                        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_AccountInvitations PRIMARY KEY,
                        UserId int NOT NULL,
                        StudentId int NULL,
                        TokenHash nvarchar(64) NOT NULL,
                        ExpiresAt datetime2 NOT NULL,
                        UsedAt datetime2 NULL,
                        RevokedAt datetime2 NULL,
                        CreatedByUserId int NULL,
                        CreatedAt datetime2 NOT NULL,
                        UpdatedAt datetime2 NULL,
                        IsDeleted bit NOT NULL
                    );
                END

                IF OBJECT_ID('dbo.TeacherSalaries', 'U') IS NOT NULL
                   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TeacherSalaries_ObligationKey' AND object_id = OBJECT_ID('dbo.TeacherSalaries'))
                    CREATE UNIQUE INDEX IX_TeacherSalaries_ObligationKey ON dbo.TeacherSalaries(ObligationKey) WHERE ObligationKey IS NOT NULL;
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AccountDeletionRequests_Status_CreatedAt' AND object_id = OBJECT_ID('dbo.AccountDeletionRequests'))
                    CREATE INDEX IX_AccountDeletionRequests_Status_CreatedAt ON dbo.AccountDeletionRequests(Status, CreatedAt);
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AccountDeletionRequests_UserId' AND object_id = OBJECT_ID('dbo.AccountDeletionRequests'))
                    CREATE INDEX IX_AccountDeletionRequests_UserId ON dbo.AccountDeletionRequests(UserId);
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AccountInvitations_CreatedByUserId' AND object_id = OBJECT_ID('dbo.AccountInvitations'))
                    CREATE INDEX IX_AccountInvitations_CreatedByUserId ON dbo.AccountInvitations(CreatedByUserId);
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AccountInvitations_StudentId' AND object_id = OBJECT_ID('dbo.AccountInvitations'))
                    CREATE INDEX IX_AccountInvitations_StudentId ON dbo.AccountInvitations(StudentId);
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AccountInvitations_TokenHash' AND object_id = OBJECT_ID('dbo.AccountInvitations'))
                    CREATE UNIQUE INDEX IX_AccountInvitations_TokenHash ON dbo.AccountInvitations(TokenHash);
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AccountInvitations_UserId_ExpiresAt' AND object_id = OBJECT_ID('dbo.AccountInvitations'))
                    CREATE INDEX IX_AccountInvitations_UserId_ExpiresAt ON dbo.AccountInvitations(UserId, ExpiresAt);
                """);
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
