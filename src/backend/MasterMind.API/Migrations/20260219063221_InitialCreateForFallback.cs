using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MasterMind.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateForFallback : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BudgetCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    AllocatedBudget = table.Column<decimal>(type: "TEXT", nullable: false),
                    SpentAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    FiscalYear = table.Column<string>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    ParentCategoryId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BudgetCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BudgetCategories_BudgetCategories_ParentCategoryId",
                        column: x => x.ParentCategoryId,
                        principalTable: "BudgetCategories",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AcademicYear = table.Column<string>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalStudents = table.Column<int>(type: "INTEGER", nullable: false),
                    ActiveStudents = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalClasses = table.Column<int>(type: "INTEGER", nullable: false),
                    ActiveClasses = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalTeachers = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalRevenue = table.Column<decimal>(type: "TEXT", nullable: false),
                    TotalExpenses = table.Column<decimal>(type: "TEXT", nullable: false),
                    Settings = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Subjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Category = table.Column<string>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subjects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    Mobile = table.Column<string>(type: "TEXT", nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", nullable: false),
                    LastName = table.Column<string>(type: "TEXT", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsEmailVerified = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsMobileVerified = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ProfileImageUrl = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Classes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Section = table.Column<string>(type: "TEXT", nullable: true),
                    Medium = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Board = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    AcademicYear = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    MaxStudents = table.Column<int>(type: "INTEGER", nullable: true),
                    StartTime = table.Column<TimeOnly>(type: "TEXT", nullable: true),
                    EndTime = table.Column<TimeOnly>(type: "TEXT", nullable: true),
                    DaysOfWeek = table.Column<string>(type: "TEXT", nullable: true),
                    MonthlyFee = table.Column<decimal>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    SessionId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Classes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Classes_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Expenses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Category = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    PaidTo = table.Column<string>(type: "TEXT", nullable: false),
                    ExpenseDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ReceiptNumber = table.Column<string>(type: "TEXT", nullable: true),
                    InvoiceNumber = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    PaymentMethod = table.Column<int>(type: "INTEGER", nullable: true),
                    TransactionId = table.Column<string>(type: "TEXT", nullable: true),
                    Remarks = table.Column<string>(type: "TEXT", nullable: true),
                    ProcessedByUserId = table.Column<int>(type: "INTEGER", nullable: true),
                    VendorName = table.Column<string>(type: "TEXT", nullable: true),
                    VendorContact = table.Column<string>(type: "TEXT", nullable: true),
                    IsRecurring = table.Column<bool>(type: "INTEGER", nullable: false),
                    RecurrencePattern = table.Column<string>(type: "TEXT", nullable: true),
                    NextDueDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    BudgetCategoryId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Expenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Expenses_BudgetCategories_BudgetCategoryId",
                        column: x => x.BudgetCategoryId,
                        principalTable: "BudgetCategories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Expenses_Users_ProcessedByUserId",
                        column: x => x.ProcessedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "OtpRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Identifier = table.Column<string>(type: "TEXT", nullable: false),
                    OtpCode = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Purpose = table.Column<int>(type: "INTEGER", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsUsed = table.Column<bool>(type: "INTEGER", nullable: false),
                    AttemptCount = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OtpRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OtpRecords_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Token = table.Column<string>(type: "TEXT", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsRevoked = table.Column<bool>(type: "INTEGER", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ReplacedByToken = table.Column<string>(type: "TEXT", nullable: true),
                    ReasonRevoked = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedByIp = table.Column<string>(type: "TEXT", nullable: false),
                    RevokedByIp = table.Column<string>(type: "TEXT", nullable: true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
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
                name: "Students",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "date", nullable: false),
                    Gender = table.Column<int>(type: "INTEGER", nullable: false),
                    Address = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    City = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    State = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    PinCode = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    StudentMobile = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    StudentEmail = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    ProfileImageUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    AdmissionNumber = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    AdmissionDate = table.Column<DateTime>(type: "date", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    ParentName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    ParentMobile = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ParentEmail = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    ParentOccupation = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    ParentUserId = table.Column<int>(type: "INTEGER", nullable: true),
                    SessionId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Students_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Students_Users_ParentUserId",
                        column: x => x.ParentUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Teachers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Mobile = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Gender = table.Column<int>(type: "INTEGER", nullable: false),
                    Address = table.Column<string>(type: "TEXT", nullable: true),
                    City = table.Column<string>(type: "TEXT", nullable: true),
                    State = table.Column<string>(type: "TEXT", nullable: true),
                    PinCode = table.Column<string>(type: "TEXT", nullable: true),
                    Qualification = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Specialization = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Subjects = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    ExperienceYears = table.Column<int>(type: "INTEGER", nullable: true),
                    JoiningDate = table.Column<DateTime>(type: "date", nullable: false),
                    LeavingDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    MonthlySalary = table.Column<decimal>(type: "TEXT", nullable: true),
                    ProfileImageUrl = table.Column<string>(type: "TEXT", nullable: true),
                    EmployeeId = table.Column<string>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: true),
                    SessionId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teachers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Teachers_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Teachers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserDevices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    DeviceId = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    DeviceName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    DeviceType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    BrowserInfo = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    IpAddress = table.Column<string>(type: "TEXT", maxLength: 45, nullable: false),
                    Location = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    IsTrusted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastUsedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDevices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserDevices_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    RoleId = table.Column<int>(type: "INTEGER", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClassSubjects",
                columns: table => new
                {
                    ClassId = table.Column<int>(type: "INTEGER", nullable: false),
                    SubjectId = table.Column<int>(type: "INTEGER", nullable: false),
                    TeacherAssigned = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    MaxStudents = table.Column<int>(type: "INTEGER", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassSubjects", x => new { x.ClassId, x.SubjectId });
                    table.ForeignKey(
                        name: "FK_ClassSubjects_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassSubjects_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FeeStructures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Category = table.Column<int>(type: "INTEGER", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    Frequency = table.Column<int>(type: "INTEGER", nullable: false),
                    ClassId = table.Column<int>(type: "INTEGER", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    AcademicYear = table.Column<string>(type: "TEXT", nullable: false),
                    DurationMonths = table.Column<int>(type: "INTEGER", nullable: false),
                    LateFeePerDay = table.Column<decimal>(type: "TEXT", nullable: true),
                    DiscountAmount = table.Column<decimal>(type: "TEXT", nullable: true),
                    DiscountDaysBeforeDue = table.Column<int>(type: "INTEGER", nullable: true),
                    IsRefundable = table.Column<bool>(type: "INTEGER", nullable: false),
                    RefundPercentage = table.Column<decimal>(type: "TEXT", nullable: true),
                    TermsAndConditions = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeeStructures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeeStructures_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Attendances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StudentId = table.Column<int>(type: "INTEGER", nullable: false),
                    ClassId = table.Column<int>(type: "INTEGER", nullable: false),
                    Date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CheckInTime = table.Column<TimeOnly>(type: "TEXT", nullable: true),
                    CheckOutTime = table.Column<TimeOnly>(type: "TEXT", nullable: true),
                    Remarks = table.Column<string>(type: "TEXT", nullable: true),
                    MarkedByUserId = table.Column<int>(type: "INTEGER", nullable: true),
                    MarkedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attendances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Attendances_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Attendances_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Attendances_Users_MarkedByUserId",
                        column: x => x.MarkedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Leads",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Mobile = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: true),
                    ParentName = table.Column<string>(type: "TEXT", nullable: true),
                    ParentMobile = table.Column<string>(type: "TEXT", nullable: true),
                    Address = table.Column<string>(type: "TEXT", nullable: true),
                    City = table.Column<string>(type: "TEXT", nullable: true),
                    Source = table.Column<int>(type: "INTEGER", nullable: false),
                    SourceDetails = table.Column<string>(type: "TEXT", nullable: true),
                    InterestedClass = table.Column<string>(type: "TEXT", nullable: true),
                    InterestedSubject = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false),
                    NextFollowupDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Remarks = table.Column<string>(type: "TEXT", nullable: true),
                    AssignedToUserId = table.Column<int>(type: "INTEGER", nullable: true),
                    ConvertedStudentId = table.Column<int>(type: "INTEGER", nullable: true),
                    ConvertedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Leads_Students_ConvertedStudentId",
                        column: x => x.ConvertedStudentId,
                        principalTable: "Students",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Leads_Users_AssignedToUserId",
                        column: x => x.AssignedToUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StudentClasses",
                columns: table => new
                {
                    StudentId = table.Column<int>(type: "INTEGER", nullable: false),
                    ClassId = table.Column<int>(type: "INTEGER", nullable: false),
                    EnrollmentDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    Remarks = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentClasses", x => new { x.StudentId, x.ClassId });
                    table.ForeignKey(
                        name: "FK_StudentClasses_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentClasses_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentPerformances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StudentId = table.Column<int>(type: "INTEGER", nullable: false),
                    ClassId = table.Column<int>(type: "INTEGER", nullable: true),
                    Subject = table.Column<string>(type: "TEXT", nullable: true),
                    TestName = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Score = table.Column<decimal>(type: "TEXT", nullable: false),
                    MaxScore = table.Column<decimal>(type: "TEXT", nullable: false),
                    Percentage = table.Column<decimal>(type: "TEXT", nullable: true),
                    Grade = table.Column<string>(type: "TEXT", nullable: true),
                    Rank = table.Column<int>(type: "INTEGER", nullable: true),
                    TestDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Remarks = table.Column<string>(type: "TEXT", nullable: true),
                    EvaluatedByTeacherId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentPerformances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentPerformances_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StudentPerformances_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentPerformances_Teachers_EvaluatedByTeacherId",
                        column: x => x.EvaluatedByTeacherId,
                        principalTable: "Teachers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StudentRemarks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StudentId = table.Column<int>(type: "INTEGER", nullable: false),
                    TeacherId = table.Column<int>(type: "INTEGER", nullable: false),
                    ClassId = table.Column<int>(type: "INTEGER", nullable: true),
                    Subject = table.Column<string>(type: "TEXT", nullable: true),
                    ChapterName = table.Column<string>(type: "TEXT", nullable: true),
                    TopicName = table.Column<string>(type: "TEXT", nullable: true),
                    Remarks = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Rating = table.Column<int>(type: "INTEGER", nullable: true),
                    RemarkDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsVisibleToParent = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentRemarks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentRemarks_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StudentRemarks_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentRemarks_Teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teachers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeacherAttendances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TeacherId = table.Column<int>(type: "INTEGER", nullable: false),
                    Date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CheckInTime = table.Column<TimeOnly>(type: "TEXT", nullable: true),
                    CheckOutTime = table.Column<TimeOnly>(type: "TEXT", nullable: true),
                    Remarks = table.Column<string>(type: "TEXT", nullable: true),
                    MarkedByUserId = table.Column<int>(type: "INTEGER", nullable: true),
                    MarkedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherAttendances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeacherAttendances_Teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teachers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeacherAttendances_Users_MarkedByUserId",
                        column: x => x.MarkedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TeacherClasses",
                columns: table => new
                {
                    TeacherId = table.Column<int>(type: "INTEGER", nullable: false),
                    ClassId = table.Column<int>(type: "INTEGER", nullable: false),
                    AssignedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsPrimary = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherClasses", x => new { x.TeacherId, x.ClassId });
                    table.ForeignKey(
                        name: "FK_TeacherClasses_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeacherClasses_Teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teachers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeacherSalaries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TeacherId = table.Column<int>(type: "INTEGER", nullable: false),
                    BasicSalary = table.Column<decimal>(type: "TEXT", nullable: false),
                    Allowances = table.Column<decimal>(type: "TEXT", nullable: true),
                    Deductions = table.Column<decimal>(type: "TEXT", nullable: true),
                    NetSalary = table.Column<decimal>(type: "TEXT", nullable: false),
                    Month = table.Column<string>(type: "TEXT", nullable: false),
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PaymentMethod = table.Column<int>(type: "INTEGER", nullable: true),
                    TransactionId = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Remarks = table.Column<string>(type: "TEXT", nullable: true),
                    ProcessedByUserId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherSalaries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeacherSalaries_Teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teachers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeacherSalaries_Users_ProcessedByUserId",
                        column: x => x.ProcessedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserSession",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserDeviceId = table.Column<int>(type: "INTEGER", nullable: false),
                    Token = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastActivityAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSession", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSession_UserDevices_UserDeviceId",
                        column: x => x.UserDeviceId,
                        principalTable: "UserDevices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FeePaymentSchedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StudentId = table.Column<int>(type: "INTEGER", nullable: false),
                    FeeStructureId = table.Column<int>(type: "INTEGER", nullable: false),
                    ScheduleType = table.Column<string>(type: "TEXT", nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    MonthlyAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    TotalInstallments = table.Column<int>(type: "INTEGER", nullable: false),
                    PaidInstallments = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    AcademicYear = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeePaymentSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeePaymentSchedules_FeeStructures_FeeStructureId",
                        column: x => x.FeeStructureId,
                        principalTable: "FeeStructures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FeePaymentSchedules_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentFees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StudentId = table.Column<int>(type: "INTEGER", nullable: false),
                    FeeStructureId = table.Column<int>(type: "INTEGER", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "TEXT", nullable: true),
                    DiscountReason = table.Column<string>(type: "TEXT", nullable: true),
                    FinalAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    PaidAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    DueDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Remarks = table.Column<string>(type: "TEXT", nullable: true),
                    Month = table.Column<string>(type: "TEXT", nullable: true),
                    AcademicYear = table.Column<string>(type: "TEXT", nullable: false),
                    FeeCategory = table.Column<int>(type: "INTEGER", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    EndDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    RecurringDayOfMonth = table.Column<int>(type: "INTEGER", nullable: true),
                    IsRecurring = table.Column<bool>(type: "INTEGER", nullable: false),
                    ParentFeeId = table.Column<int>(type: "INTEGER", nullable: true),
                    LateFeePerDay = table.Column<decimal>(type: "TEXT", nullable: true),
                    GracePeriodDays = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentFees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentFees_FeeStructures_FeeStructureId",
                        column: x => x.FeeStructureId,
                        principalTable: "FeeStructures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentFees_StudentFees_ParentFeeId",
                        column: x => x.ParentFeeId,
                        principalTable: "StudentFees",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StudentFees_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LeadFollowups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LeadId = table.Column<int>(type: "INTEGER", nullable: false),
                    FollowupDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: false),
                    StatusAfter = table.Column<int>(type: "INTEGER", nullable: true),
                    NextFollowupDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    NextFollowupNotes = table.Column<string>(type: "TEXT", nullable: true),
                    FollowedByUserId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeadFollowups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeadFollowups_Leads_LeadId",
                        column: x => x.LeadId,
                        principalTable: "Leads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LeadFollowups_Users_FollowedByUserId",
                        column: x => x.FollowedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FeeInstallments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FeePaymentScheduleId = table.Column<int>(type: "INTEGER", nullable: false),
                    InstallmentNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    DueDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PaidDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PaidAmount = table.Column<decimal>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    LateFee = table.Column<decimal>(type: "TEXT", nullable: true),
                    DiscountApplied = table.Column<decimal>(type: "TEXT", nullable: true),
                    Remarks = table.Column<string>(type: "TEXT", nullable: true),
                    StudentFeeId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeeInstallments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeeInstallments_FeePaymentSchedules_FeePaymentScheduleId",
                        column: x => x.FeePaymentScheduleId,
                        principalTable: "FeePaymentSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FeeInstallments_StudentFees_StudentFeeId",
                        column: x => x.StudentFeeId,
                        principalTable: "StudentFees",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StudentFeeId = table.Column<int>(type: "INTEGER", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Method = table.Column<int>(type: "INTEGER", nullable: false),
                    TransactionId = table.Column<string>(type: "TEXT", nullable: true),
                    ReceiptNumber = table.Column<string>(type: "TEXT", nullable: true),
                    Remarks = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ReceivedByUserId = table.Column<int>(type: "INTEGER", nullable: true),
                    FeeInstallmentId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_FeeInstallments_FeeInstallmentId",
                        column: x => x.FeeInstallmentId,
                        principalTable: "FeeInstallments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Payments_StudentFees_StudentFeeId",
                        column: x => x.StudentFeeId,
                        principalTable: "StudentFees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Payments_Users_ReceivedByUserId",
                        column: x => x.ReceivedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FeeReceipts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ReceiptNumber = table.Column<string>(type: "TEXT", nullable: false),
                    StudentId = table.Column<int>(type: "INTEGER", nullable: false),
                    PaymentId = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    PaidAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    BalanceAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    PaymentMethod = table.Column<string>(type: "TEXT", nullable: false),
                    ReceiptDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ReceiptStatus = table.Column<string>(type: "TEXT", nullable: false),
                    StudentName = table.Column<string>(type: "TEXT", nullable: false),
                    StudentClass = table.Column<string>(type: "TEXT", nullable: false),
                    FeeDescription = table.Column<string>(type: "TEXT", nullable: false),
                    FeePeriod = table.Column<string>(type: "TEXT", nullable: false),
                    ParentName = table.Column<string>(type: "TEXT", nullable: false),
                    ParentEmail = table.Column<string>(type: "TEXT", nullable: false),
                    ParentMobile = table.Column<string>(type: "TEXT", nullable: false),
                    IsEmailSent = table.Column<bool>(type: "INTEGER", nullable: false),
                    EmailSentAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsSmsSent = table.Column<bool>(type: "INTEGER", nullable: false),
                    SmsSentAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Remarks = table.Column<string>(type: "TEXT", nullable: true),
                    PaymentNotes = table.Column<string>(type: "TEXT", nullable: true),
                    GeneratedByUserId = table.Column<int>(type: "INTEGER", nullable: true),
                    InstitutionName = table.Column<string>(type: "TEXT", nullable: true),
                    InstitutionAddress = table.Column<string>(type: "TEXT", nullable: true),
                    InstitutionContact = table.Column<string>(type: "TEXT", nullable: true),
                    InstitutionLogo = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeeReceipts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeeReceipts_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FeeReceipts_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FeeReceipts_Users_GeneratedByUserId",
                        column: x => x.GeneratedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FeeReceiptItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FeeReceiptId = table.Column<int>(type: "INTEGER", nullable: false),
                    ItemDescription = table.Column<string>(type: "TEXT", nullable: false),
                    ItemAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "TEXT", nullable: true),
                    FinalAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    Period = table.Column<string>(type: "TEXT", nullable: true),
                    StudentFeeId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeeReceiptItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeeReceiptItems_FeeReceipts_FeeReceiptId",
                        column: x => x.FeeReceiptId,
                        principalTable: "FeeReceipts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FeeReceiptItems_StudentFees_StudentFeeId",
                        column: x => x.StudentFeeId,
                        principalTable: "StudentFees",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_ClassId",
                table: "Attendances",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_MarkedByUserId",
                table: "Attendances",
                column: "MarkedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Attendances_StudentId",
                table: "Attendances",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetCategories_ParentCategoryId",
                table: "BudgetCategories",
                column: "ParentCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Classes_SessionId",
                table: "Classes",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassSubjects_SubjectId",
                table: "ClassSubjects",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_BudgetCategoryId",
                table: "Expenses",
                column: "BudgetCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_ProcessedByUserId",
                table: "Expenses",
                column: "ProcessedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_FeeInstallments_FeePaymentScheduleId",
                table: "FeeInstallments",
                column: "FeePaymentScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_FeeInstallments_StudentFeeId",
                table: "FeeInstallments",
                column: "StudentFeeId");

            migrationBuilder.CreateIndex(
                name: "IX_FeePaymentSchedules_FeeStructureId",
                table: "FeePaymentSchedules",
                column: "FeeStructureId");

            migrationBuilder.CreateIndex(
                name: "IX_FeePaymentSchedules_StudentId",
                table: "FeePaymentSchedules",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_FeeReceiptItems_FeeReceiptId",
                table: "FeeReceiptItems",
                column: "FeeReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_FeeReceiptItems_StudentFeeId",
                table: "FeeReceiptItems",
                column: "StudentFeeId");

            migrationBuilder.CreateIndex(
                name: "IX_FeeReceipts_GeneratedByUserId",
                table: "FeeReceipts",
                column: "GeneratedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_FeeReceipts_PaymentId",
                table: "FeeReceipts",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_FeeReceipts_StudentId",
                table: "FeeReceipts",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_FeeStructures_ClassId",
                table: "FeeStructures",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_LeadFollowups_FollowedByUserId",
                table: "LeadFollowups",
                column: "FollowedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LeadFollowups_LeadId",
                table: "LeadFollowups",
                column: "LeadId");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_AssignedToUserId",
                table: "Leads",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_ConvertedStudentId",
                table: "Leads",
                column: "ConvertedStudentId");

            migrationBuilder.CreateIndex(
                name: "IX_OtpRecords_UserId",
                table: "OtpRecords",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_FeeInstallmentId",
                table: "Payments",
                column: "FeeInstallmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ReceivedByUserId",
                table: "Payments",
                column: "ReceivedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_StudentFeeId",
                table: "Payments",
                column: "StudentFeeId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentClasses_ClassId",
                table: "StudentClasses",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentFees_FeeStructureId",
                table: "StudentFees",
                column: "FeeStructureId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentFees_ParentFeeId",
                table: "StudentFees",
                column: "ParentFeeId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentFees_StudentId",
                table: "StudentFees",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentPerformances_ClassId",
                table: "StudentPerformances",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentPerformances_EvaluatedByTeacherId",
                table: "StudentPerformances",
                column: "EvaluatedByTeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentPerformances_StudentId",
                table: "StudentPerformances",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentRemarks_ClassId",
                table: "StudentRemarks",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentRemarks_StudentId",
                table: "StudentRemarks",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentRemarks_TeacherId",
                table: "StudentRemarks",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_ParentUserId",
                table: "Students",
                column: "ParentUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_SessionId",
                table: "Students",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherAttendances_MarkedByUserId",
                table: "TeacherAttendances",
                column: "MarkedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherAttendances_TeacherId",
                table: "TeacherAttendances",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherClasses_ClassId",
                table: "TeacherClasses",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_Teachers_SessionId",
                table: "Teachers",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Teachers_UserId",
                table: "Teachers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherSalaries_ProcessedByUserId",
                table: "TeacherSalaries",
                column: "ProcessedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherSalaries_TeacherId",
                table: "TeacherSalaries",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDevices_UserId",
                table: "UserDevices",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSession_UserDeviceId",
                table: "UserSession",
                column: "UserDeviceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Attendances");

            migrationBuilder.DropTable(
                name: "ClassSubjects");

            migrationBuilder.DropTable(
                name: "Expenses");

            migrationBuilder.DropTable(
                name: "FeeReceiptItems");

            migrationBuilder.DropTable(
                name: "LeadFollowups");

            migrationBuilder.DropTable(
                name: "OtpRecords");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "StudentClasses");

            migrationBuilder.DropTable(
                name: "StudentPerformances");

            migrationBuilder.DropTable(
                name: "StudentRemarks");

            migrationBuilder.DropTable(
                name: "TeacherAttendances");

            migrationBuilder.DropTable(
                name: "TeacherClasses");

            migrationBuilder.DropTable(
                name: "TeacherSalaries");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "UserSession");

            migrationBuilder.DropTable(
                name: "Subjects");

            migrationBuilder.DropTable(
                name: "BudgetCategories");

            migrationBuilder.DropTable(
                name: "FeeReceipts");

            migrationBuilder.DropTable(
                name: "Leads");

            migrationBuilder.DropTable(
                name: "Teachers");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "UserDevices");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "FeeInstallments");

            migrationBuilder.DropTable(
                name: "FeePaymentSchedules");

            migrationBuilder.DropTable(
                name: "StudentFees");

            migrationBuilder.DropTable(
                name: "FeeStructures");

            migrationBuilder.DropTable(
                name: "Students");

            migrationBuilder.DropTable(
                name: "Classes");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Sessions");
        }
    }
}
