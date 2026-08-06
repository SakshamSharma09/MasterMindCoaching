using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MasterMind.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserLoginIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF NOT EXISTS
                (
                    SELECT 1 FROM sys.indexes
                    WHERE name = 'IX_Users_Email'
                      AND object_id = OBJECT_ID('dbo.Users')
                )
                    CREATE INDEX IX_Users_Email ON dbo.Users(Email);

                IF NOT EXISTS
                (
                    SELECT 1 FROM sys.indexes
                    WHERE name = 'IX_Users_Mobile'
                      AND object_id = OBJECT_ID('dbo.Users')
                )
                    CREATE INDEX IX_Users_Mobile ON dbo.Users(Mobile);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS
                (
                    SELECT 1 FROM sys.indexes
                    WHERE name = 'IX_Users_Email'
                      AND object_id = OBJECT_ID('dbo.Users')
                )
                    DROP INDEX IX_Users_Email ON dbo.Users;

                IF EXISTS
                (
                    SELECT 1 FROM sys.indexes
                    WHERE name = 'IX_Users_Mobile'
                      AND object_id = OBJECT_ID('dbo.Users')
                )
                    DROP INDEX IX_Users_Mobile ON dbo.Users;
                """);
        }
    }
}
