using System.Security.Claims;
using MasterMind.API.Controllers;
using MasterMind.API.Data;
using MasterMind.API.Models.Entities;
using MasterMind.API.Services.Implementations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace MasterMind.API.Tests;

public class FinanceWriteEndpointTests
{
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
}
