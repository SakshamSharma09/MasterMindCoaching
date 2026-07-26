using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MasterMind.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentInactiveDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH('dbo.Students', 'InactiveDate') IS NULL
                    ALTER TABLE dbo.Students ADD InactiveDate datetime2 NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH('dbo.Students', 'InactiveDate') IS NOT NULL
                    ALTER TABLE dbo.Students DROP COLUMN InactiveDate;
                """);
        }
    }
}
