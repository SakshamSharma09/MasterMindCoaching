using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MasterMind.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPaperGeneratorModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PaperDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionId = table.Column<int>(type: "int", nullable: true),
                    UploadedByUserId = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    BlobContainer = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BlobName = table.Column<string>(type: "nvarchar(700)", maxLength: 700, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    PageCount = table.Column<int>(type: "int", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RetainUntil = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaperDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaperDocuments_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PaperDocuments_Users_UploadedByUserId",
                        column: x => x.UploadedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PaperGenerationJobs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionId = table.Column<int>(type: "int", nullable: true),
                    RequestedByUserId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    StatusMessage = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ClassName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Chapter = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    TotalMarks = table.Column<int>(type: "int", nullable: false),
                    DurationMinutes = table.Column<int>(type: "int", nullable: false),
                    RelevancePercentage = table.Column<int>(type: "int", nullable: false),
                    SettingsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AiModelUsed = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(1500)", maxLength: 1500, nullable: true),
                    GeneratedPaperBlobContainer = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    GeneratedPaperBlobName = table.Column<string>(type: "nvarchar(700)", maxLength: 700, nullable: true),
                    AnswerKeyBlobContainer = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AnswerKeyBlobName = table.Column<string>(type: "nvarchar(700)", maxLength: 700, nullable: true),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RetainUntil = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaperGenerationJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaperGenerationJobs_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PaperGenerationJobs_Users_RequestedByUserId",
                        column: x => x.RequestedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PaperExtractedQuestions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionId = table.Column<int>(type: "int", nullable: true),
                    SourceDocumentId = table.Column<int>(type: "int", nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ClassName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Chapter = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    Marks = table.Column<int>(type: "int", nullable: false),
                    QuestionType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Difficulty = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    QuestionText = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    AnswerText = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    SourcePageNumber = table.Column<int>(type: "int", nullable: true),
                    SourceMode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaperExtractedQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaperExtractedQuestions_PaperDocuments_SourceDocumentId",
                        column: x => x.SourceDocumentId,
                        principalTable: "PaperDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PaperExtractedQuestions_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PaperJobDocuments",
                columns: table => new
                {
                    PaperGenerationJobId = table.Column<int>(type: "int", nullable: false),
                    PaperDocumentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaperJobDocuments", x => new { x.PaperGenerationJobId, x.PaperDocumentId });
                    table.ForeignKey(
                        name: "FK_PaperJobDocuments_PaperDocuments_PaperDocumentId",
                        column: x => x.PaperDocumentId,
                        principalTable: "PaperDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaperJobDocuments_PaperGenerationJobs_PaperGenerationJobId",
                        column: x => x.PaperGenerationJobId,
                        principalTable: "PaperGenerationJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaperDocuments_BlobName",
                table: "PaperDocuments",
                column: "BlobName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaperDocuments_SessionId_UploadedByUserId_CreatedAt",
                table: "PaperDocuments",
                columns: new[] { "SessionId", "UploadedByUserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PaperDocuments_UploadedByUserId",
                table: "PaperDocuments",
                column: "UploadedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PaperExtractedQuestions_SessionId_Subject_ClassName_QuestionType",
                table: "PaperExtractedQuestions",
                columns: new[] { "SessionId", "Subject", "ClassName", "QuestionType" });

            migrationBuilder.CreateIndex(
                name: "IX_PaperExtractedQuestions_SourceDocumentId",
                table: "PaperExtractedQuestions",
                column: "SourceDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_PaperGenerationJobs_RequestedByUserId",
                table: "PaperGenerationJobs",
                column: "RequestedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PaperGenerationJobs_SessionId_RequestedByUserId_CreatedAt",
                table: "PaperGenerationJobs",
                columns: new[] { "SessionId", "RequestedByUserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PaperJobDocuments_PaperDocumentId",
                table: "PaperJobDocuments",
                column: "PaperDocumentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "PaperExtractedQuestions");
            migrationBuilder.DropTable(name: "PaperJobDocuments");
            migrationBuilder.DropTable(name: "PaperDocuments");
            migrationBuilder.DropTable(name: "PaperGenerationJobs");
        }
    }
}
