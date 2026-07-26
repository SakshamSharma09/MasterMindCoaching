using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MasterMind.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class MobileFirstParentOnboarding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Compatibility checks run before migrations in production, so keep this
            // migration idempotent for databases whose columns were already added.
            migrationBuilder.Sql("""
                IF COL_LENGTH('dbo.Users', 'SecondaryMobile') IS NULL
                    ALTER TABLE dbo.Users ADD SecondaryMobile nvarchar(20) NULL;
                IF COL_LENGTH('dbo.Students', 'SecondaryParentMobile') IS NULL
                    ALTER TABLE dbo.Students ADD SecondaryParentMobile nvarchar(20) NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SecondaryMobile",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SecondaryParentMobile",
                table: "Students");

        }
    }
}
