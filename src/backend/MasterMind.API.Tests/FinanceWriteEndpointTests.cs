using System.Security.Claims;
using MasterMind.API.Controllers;
using MasterMind.API.Data;
using MasterMind.API.Models.Entities;
using MasterMind.API.Services.Implementations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace MasterMind.API.Tests;

public class FinanceWriteEndpointTests
{
    [Fact]
    public async Task FinancialSummaryReportsMonthlyRecurringRevenueAndUnassignedStudents()
    {
        await using var context = NewContext();
        var (session, student) = await SeedSessionAndStudent(context);
        var unassigned = new Student
        {
            FirstName = "test2",
            LastName = "finance",
            ParentMobile = "7000000002",
            IsActive = true,
            SessionId = session.Id
        };
        var plan = new FeeStructure
        {
            Name = "Quarterly test plan",
            Type = FeeType.Tuition,
            Category = FeeCategory.Monthly,
            Frequency = FeeFrequency.Quarterly,
            Amount = 3600,
            AcademicYear = session.AcademicYear,
            IsActive = true
        };
        context.AddRange(unassigned, plan);
        await context.SaveChangesAsync();
        context.StudentFees.Add(new StudentFee
        {
            StudentId = student.Id,
            FeeStructureId = plan.Id,
            Amount = 3600,
            FinalAmount = 3600,
            DueDate = DateOnly.FromDateTime(DateTime.Today).AddMonths(3),
            AcademicYear = session.AcademicYear,
            FeeCategory = FeeCategory.Monthly,
            Frequency = FeeFrequency.Quarterly,
            RecurrenceIntervalMonths = 3,
            IsRecurring = true,
            Status = FeeStatus.Pending
        });
        await context.SaveChangesAsync();

        var controller = WithAdmin(new FinanceController(
            context,
            NullLogger<FinanceController>.Instance,
            new TeacherSalaryService(context),
            new RecurringObligationService(context)));
        var response = await controller.GetFinancialSummary(session.Id);
        var ok = Assert.IsType<OkObjectResult>(response.Result);
        var payload = Assert.IsType<ApiResponse<FinancialSummary>>(ok.Value);

        Assert.Equal(1200m, payload.Data!.MonthlyRecurringRevenue);
        Assert.Equal(1, payload.Data.UnassignedStudents);
        Assert.Equal(2, payload.Data.ActiveHouseholds);
    }

    [Fact]
    public async Task CollectPaymentSucceedsWhenParentEmailWasNotYetProvided()
    {
        var options = new DbContextOptionsBuilder<MasterMindDbContext>()
            .UseInMemoryDatabase($"fee-payment-{Guid.NewGuid()}")
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        await using var context = new MasterMindDbContext(options);

        var (session, student) = await SeedSessionAndStudent(context);
        student.MotherName = "Test Mother";
        student.ParentEmail = null;
        var admin = new User
        {
            Email = "fee-admin@example.invalid",
            Mobile = "9887258679",
            FirstName = "Fee",
            LastName = "Admin",
            PasswordHash = "test",
            IsActive = true
        };
        var structure = new FeeStructure
        {
            Name = "Monthly test fee",
            Type = FeeType.Tuition,
            Category = FeeCategory.Monthly,
            Frequency = FeeFrequency.Monthly,
            Amount = 1200,
            AcademicYear = session.AcademicYear,
            IsActive = true
        };
        context.AddRange(admin, structure);
        await context.SaveChangesAsync();
        var fee = new StudentFee
        {
            StudentId = student.Id,
            FeeStructureId = structure.Id,
            Amount = 1200,
            FinalAmount = 1200,
            DueDate = DateOnly.FromDateTime(DateTime.Today),
            AcademicYear = session.AcademicYear,
            FeeCategory = FeeCategory.Monthly,
            Frequency = FeeFrequency.Monthly,
            Status = FeeStatus.Pending
        };
        context.StudentFees.Add(fee);
        await context.SaveChangesAsync();

        var controller = WithUser(new FeeCollectionController(
            context,
            NullLogger<FeeCollectionController>.Instance,
            new NoOpEmailService()), admin.Id);
        var response = await controller.CollectPayment(new CollectPaymentRequest
        {
            StudentId = student.Id,
            PaymentMethod = PaymentMethod.UPI,
            FeeItems = new List<PaymentFeeItemDto>
            {
                new() { StudentFeeId = fee.Id, Amount = 1200, Description = "August fee", Period = "August" }
            }
        });

        Assert.IsType<CreatedAtActionResult>(response.Result);
        Assert.Equal(FeeStatus.Paid, (await context.StudentFees.FindAsync(fee.Id))!.Status);
        var receipt = Assert.Single(await context.FeeReceipts.ToListAsync());
        var payment = Assert.Single(await context.Payments.ToListAsync());
        Assert.Equal(string.Empty, receipt.ParentEmail);
        Assert.Equal("Test Mother", receipt.ParentName);
        Assert.Equal($"MM-PAY-{payment.Id:D8}", payment.TransactionId);
    }

    [Fact]
    public async Task FeeDetailsExcludeDeletedSchedulesAndInternalControlRows()
    {
        await using var context = NewContext();
        var (session, student) = await SeedSessionAndStudent(context);
        var plan = new FeeStructure
        {
            Name = "Monthly tuition",
            Type = FeeType.Tuition,
            Category = FeeCategory.Monthly,
            Frequency = FeeFrequency.Monthly,
            Amount = 3000,
            AcademicYear = session.AcademicYear,
            IsActive = true
        };
        context.FeeStructures.Add(plan);
        await context.SaveChangesAsync();

        var deletedSchedule = new StudentFee
        {
            StudentId = student.Id, FeeStructureId = plan.Id, Amount = 3000,
            FinalAmount = 3000, DueDate = DateOnly.FromDateTime(DateTime.Today),
            AcademicYear = session.AcademicYear, FeeCategory = FeeCategory.Monthly,
            IsRecurring = true, IsDeleted = true, Status = FeeStatus.Pending
        };
        context.StudentFees.Add(deletedSchedule);
        await context.SaveChangesAsync();
        context.StudentFees.AddRange(
            new StudentFee
            {
                StudentId = student.Id, FeeStructureId = plan.Id, Amount = 3000,
                FinalAmount = 3000, DueDate = DateOnly.FromDateTime(DateTime.Today),
                AcademicYear = session.AcademicYear, FeeCategory = FeeCategory.Monthly,
                ParentFeeId = deletedSchedule.Id, Status = FeeStatus.Pending
            },
            new StudentFee
            {
                StudentId = student.Id, FeeStructureId = plan.Id, Amount = 3000,
                FinalAmount = 3000, DueDate = DateOnly.FromDateTime(DateTime.Today).AddMonths(1),
                AcademicYear = session.AcademicYear, FeeCategory = FeeCategory.Monthly,
                Status = FeeStatus.Pending
            },
            new StudentFee
            {
                StudentId = student.Id, FeeStructureId = plan.Id, Amount = 3000,
                FinalAmount = 3000, DueDate = DateOnly.FromDateTime(DateTime.Today).AddMonths(2),
                AcademicYear = session.AcademicYear, FeeCategory = FeeCategory.Monthly,
                IsDeleted = true, Status = FeeStatus.Pending
            });
        await context.SaveChangesAsync();

        var controller = WithAdmin(new FeeCollectionController(
            context, NullLogger<FeeCollectionController>.Instance, new NoOpEmailService()));
        var response = await controller.GetStudentFeeDetails(student.Id);
        var ok = Assert.IsType<OkObjectResult>(response.Result);
        var payload = Assert.IsType<ApiResponse<StudentFeeDetailsDto>>(ok.Value);

        var visible = Assert.Single(payload.Data!.PendingFees);
        Assert.Equal(3000m, visible.BalanceAmount);
    }

    [Fact]
    public async Task CreateFeeCreatesScheduleAndCurrentInstallment()
    {
        await using var context = NewContext();
        var (session, student) = await SeedSessionAndStudent(context);
        var plan = new FeeStructure
        {
            Name = "Test monthly plan",
            Type = FeeType.Tuition,
            Category = FeeCategory.Monthly,
            Frequency = FeeFrequency.Monthly,
            Amount = 1500,
            AcademicYear = session.AcademicYear,
            IsActive = true
        };
        context.FeeStructures.Add(plan);
        await context.SaveChangesAsync();

        var recurringService = new RecurringObligationService(context);
        var controller = WithAdmin(new FinanceController(
            context,
            NullLogger<FinanceController>.Instance,
            new TeacherSalaryService(context),
            recurringService));
        var start = new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1);

        var response = await controller.CreateFee(new CreateFeeRequest
        {
            StudentId = student.Id,
            FeeStructureId = plan.Id,
            Amount = 1500,
            FeeCategory = FeeCategory.Monthly,
            Frequency = FeeFrequency.Monthly,
            StartDate = start,
            FirstDueDate = start.AddMonths(1),
            ScheduleEndDate = DateOnly.FromDateTime(session.EndDate),
            AcademicYear = session.AcademicYear
        });

        Assert.IsType<CreatedAtActionResult>(response.Result);
        Assert.Single(await context.StudentFees.Where(f => f.IsRecurring).ToListAsync());
        Assert.Single(await context.StudentFees.Where(f => f.OccurrenceKey != null).ToListAsync());
    }

    [Fact]
    public async Task OneTimeFrequencyWithMonthlyPlanCreatesSingleFee()
    {
        await using var context = NewContext();
        var (session, student) = await SeedSessionAndStudent(context);
        var plan = new FeeStructure
        {
            Name = "Test reusable plan",
            Type = FeeType.Tuition,
            Category = FeeCategory.Monthly,
            Frequency = FeeFrequency.Monthly,
            Amount = 100,
            AcademicYear = session.AcademicYear,
            IsActive = true
        };
        context.FeeStructures.Add(plan);
        await context.SaveChangesAsync();

        var controller = WithAdmin(new FinanceController(
            context,
            NullLogger<FinanceController>.Instance,
            new TeacherSalaryService(context),
            new RecurringObligationService(context)));
        var today = DateOnly.FromDateTime(DateTime.Today);

        var response = await controller.CreateFee(new CreateFeeRequest
        {
            StudentId = student.Id,
            FeeStructureId = plan.Id,
            Amount = 100,
            FeeCategory = FeeCategory.Monthly,
            Frequency = FeeFrequency.OneTime,
            StartDate = today,
            FirstDueDate = today,
            ScheduleEndDate = today,
            AcademicYear = session.AcademicYear
        });

        Assert.IsType<CreatedAtActionResult>(response.Result);
        var fee = Assert.Single(await context.StudentFees.ToListAsync());
        Assert.False(fee.IsRecurring);
        Assert.Equal(FeeFrequency.OneTime, fee.Frequency);
        Assert.Equal(student.Id, fee.StudentId);
    }

    [Fact]
    public async Task CreateAndListExpensesIncludesGeneralExpenseAndTeacherSalary()
    {
        await using var context = NewContext();
        var (session, _) = await SeedSessionAndStudent(context);
        context.Teachers.Add(new Teacher
        {
            FirstName = "Test",
            LastName = "Teacher",
            Email = "finance-teacher@example.invalid",
            Mobile = "7000000001",
            JoiningDate = DateTime.Today,
            MonthlySalary = 18000,
            SessionId = session.Id,
            IsActive = true
        });
        await context.SaveChangesAsync();

        var controller = WithAdmin(new ExpensesController(
            context,
            NullLogger<ExpensesController>.Instance,
            new TeacherSalaryService(context),
            new RecurringObligationService(context)));

        var createResponse = await controller.CreateExpense(new CreateExpenseRequest
        {
            Category = "Utilities",
            Description = "Test electricity expense",
            Amount = 500,
            PaidTo = "Test vendor",
            Date = DateTime.Today.ToString("yyyy-MM-dd"),
            DueDate = DateTime.Today,
            PayNow = false
        });
        Assert.IsType<CreatedAtActionResult>(createResponse.Result);

        var listResponse = await controller.GetExpenses(null, null, null, session.Id);
        var ok = Assert.IsType<OkObjectResult>(listResponse.Result);
        var payload = Assert.IsType<ApiResponse<IEnumerable<ExpenseDto>>>(ok.Value);
        var expenses = payload.Data!.ToList();
        Assert.Contains(expenses, e => e.Source == "General" && e.Category == "Utilities");
        Assert.Contains(expenses, e => e.Source == "TeacherSalary" && e.Amount == 18000);
    }

    private static MasterMindDbContext NewContext() =>
        new(new DbContextOptionsBuilder<MasterMindDbContext>()
            .UseInMemoryDatabase($"finance-write-{Guid.NewGuid()}")
            .Options);

    private static async Task<(Session Session, Student Student)> SeedSessionAndStudent(MasterMindDbContext context)
    {
        var today = DateTime.Today;
        var session = new Session
        {
            Name = "finance-test",
            DisplayName = "Finance test session",
            AcademicYear = "test",
            StartDate = new DateTime(today.Year, 1, 1),
            EndDate = new DateTime(today.Year, 12, 31),
            IsActive = true,
            Status = SessionStatus.Active
        };
        var student = new Student
        {
            FirstName = "test",
            LastName = "finance",
            ParentMobile = "7627053236",
            IsActive = true,
            Session = session
        };
        context.AddRange(session, student);
        await context.SaveChangesAsync();
        return (session, student);
    }

    private static T WithAdmin<T>(T controller) where T : ControllerBase
    {
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    new[] { new Claim(ClaimTypes.NameIdentifier, "1") },
                    "Test"))
            }
        };
        return controller;
    }

    private static T WithUser<T>(T controller, int userId) where T : ControllerBase
    {
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) }, "Test"))
            }
        };
        return controller;
    }

    private sealed class NoOpEmailService : MasterMind.API.Services.Interfaces.IEmailService
    {
        public Task<bool> SendOtpEmailAsync(string email, string otp) => Task.FromResult(true);
        public Task<bool> SendEmailAsync(string to, string subject, string body) => Task.FromResult(true);
        public bool IsValidEmail(string email) => true;
    }
}
