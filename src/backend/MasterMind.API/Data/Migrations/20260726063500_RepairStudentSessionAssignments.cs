using MasterMind.API.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MasterMind.API.Data.Migrations;

[DbContext(typeof(MasterMindDbContext))]
[Migration("20260726063500_RepairStudentSessionAssignments")]
public partial class RepairStudentSessionAssignments : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            ;WITH RankedStudentSessions AS
            (
                SELECT
                    sc.StudentId,
                    c.SessionId,
                    ROW_NUMBER() OVER
                    (
                        PARTITION BY sc.StudentId
                        ORDER BY
                            CASE WHEN sc.IsActive = 1 THEN 0 ELSE 1 END,
                            sc.EnrollmentDate DESC,
                            c.SessionId DESC
                    ) AS RowNumber
                FROM dbo.StudentClasses sc
                INNER JOIN dbo.Classes c ON c.Id = sc.ClassId
                WHERE c.IsDeleted = 0
                  AND c.SessionId IS NOT NULL
            )
            UPDATE students
            SET SessionId = ranked.SessionId
            FROM dbo.Students students
            INNER JOIN RankedStudentSessions ranked
                ON ranked.StudentId = students.Id
               AND ranked.RowNumber = 1
            WHERE students.IsDeleted = 0
              AND (students.SessionId IS NULL OR students.SessionId <> ranked.SessionId);

            UPDATE students
            SET SessionId = NULL
            FROM dbo.Students students
            WHERE students.IsDeleted = 0
              AND NOT EXISTS
              (
                  SELECT 1
                  FROM dbo.StudentClasses sc
                  INNER JOIN dbo.Classes c ON c.Id = sc.ClassId
                  WHERE sc.StudentId = students.Id
                    AND c.IsDeleted = 0
                    AND c.SessionId IS NOT NULL
              );
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Session reconstruction is a metadata repair derived from existing class mappings.
        // Reversing it would reintroduce the incorrect all-students-in-active-session state.
    }
}
