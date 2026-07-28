using MasterMind.API.Data;
using MasterMind.API.Models.Entities;
using MasterMind.API.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MasterMind.API.Tests;

public class RecurringObligationServiceTests
{
    [Theory]
    [InlineData(FeeFrequency.Monthly, 1, "2026-05-01")]
    [InlineData(FeeFrequency.Quarterly, 3, "2026-07-01")]
    [InlineData(FeeFrequency.HalfYearly, 6, "2026-10-01")]
    [InlineData(FeeFrequency.Yearly, 12, "2027-04-01")]
    public async Task GeneratesOneCurrentPeriodWithNextCycleDueDate(
        FeeFrequency frequency,
        int interval,
        string expectedDueDate)
    {
        await using var context = NewContext();
        var (student, plan) = await SeedStudentAndPlan(context, frequency);
        context.StudentFees.Add(NewSchedule(student.Id, plan.Id, frequency, interval));
        await context.SaveChangesAsync();

        var service = new RecurringObligationService(context);
        await service.EnsureFeeObligationsAsync(new DateOnly(2026, 4, 30));
        await service.EnsureFeeObligationsAsync(new DateOnly(2026, 4, 30));

        var occurrence = Assert.Single(await context.StudentFees.Where(f => f.OccurrenceKey != null).ToListAsync());
        Assert.Equal(DateOnly.Parse(expectedDueDate), occurrence.DueDate);
        Assert.Equal(new DateOnly(2026, 4, 1), occurrence.PeriodStart);
        Assert.Equal(FeeStatus.Pending, occurrence.Status);
    }

    [Fact]
    public async Task InactiveDateStopsFuturePeriodsAndKeepsEarlierOccurrence()
    {
        await using var context = NewContext();
        var (student, plan) = await SeedStudentAndPlan(context, FeeFrequency.Monthly);
        student.InactiveDate = new DateTime(2026, 5, 15);
        context.StudentFees.Add(NewSchedule(student.Id, plan.Id, FeeFrequency.Monthly, 1));
        await context.SaveChangesAsync();

        var service = new RecurringObligationService(context);
        await service.EnsureFeeObligationsAsync(new DateOnly(2026, 8, 1));

        var occurrences = await context.StudentFees
            .Where(f => f.OccurrenceKey != null)
            .OrderBy(f => f.PeriodStart)
            .ToListAsync();
        Assert.Equal(2, occurrences.Count);
        Assert.Equal(new DateOnly(2026, 5, 1), occurrences[1].PeriodStart);
    }

    [Fact]
    public async Task RecurringExpenseGenerationIsIdempotent()
    {
        await using var context = NewContext();
        context.Expenses.Add(new Expense
        {
            Category = "Rent",
            Description = "Office rent",
            Amount = 20000,
            PaidTo = "Landlord",
            ExpenseDate = new DateTime(2026, 4, 1),
            PeriodStart = new DateTime(2026, 4, 1),
            PeriodEnd = new DateTime(2026, 12, 31),
            IsRecurring = true,
            RecurrencePattern = "Monthly",
            RecurrenceIntervalMonths = 1
        });
        await context.SaveChangesAsync();
        var service = new RecurringObligationService(context);

        await service.EnsureExpenseObligationsAsync(new DateTime(2026, 5, 1));
        await service.EnsureExpenseObligationsAsync(new DateTime(2026, 5, 1));

        var occurrences = await context.Expenses.Where(e => e.OccurrenceKey != null).ToListAsync();
        Assert.Equal(2, occurrences.Count);
        Assert.All(occurrences, e => Assert.Equal(ExpenseStatus.Pending, e.Status));
    }

    private static MasterMindDbContext NewContext() =>
        new(new DbContextOptionsBuilder<MasterMindDbContext>()
            .UseInMemoryDatabase($"recurrence-{Guid.NewGuid()}")
            .Options);

    private static async Task<(Student Student, FeeStructure Plan)> SeedStudentAndPlan(
        MasterMindDbContext context,
        FeeFrequency frequency)
    {
        var student = new Student
        {
            FirstName = "Test",
            LastName = "Student",
            ParentMobile = "7627053236",
            DateOfBirth = new DateTime(2015, 1, 1),
            IsActive = true
        };
        var plan = new FeeStructure
        {
            Name = $"{frequency} tuition",
            Type = FeeType.Tuition,
            Category = FeeCategory.Monthly,
            Frequency = frequency,
            Amount = 1000,
            AcademicYear = "2026-27",
            IsActive = true
        };
        context.AddRange(student, plan);
        await context.SaveChangesAsync();
        return (student, plan);
    }

    private static StudentFee NewSchedule(
        int studentId,
        int planId,
        FeeFrequency frequency,
        int interval) => new()
    {
        StudentId = studentId,
        FeeStructureId = planId,
        Amount = 1000,
        FinalAmount = 1000,
        DueDate = new DateOnly(2026, 4, 1).AddMonths(interval),
        FeeCategory = FeeCategory.Monthly,
        Frequency = frequency,
        FirstDueDate = new DateOnly(2026, 4, 1).AddMonths(interval),
        StartDate = new DateOnly(2026, 4, 1),
        ScheduleEndDate = new DateOnly(2027, 3, 31),
        EndDate = new DateOnly(2027, 3, 31),
        RecurrenceIntervalMonths = interval,
        IsRecurring = true,
        AcademicYear = "2026-27"
    };
}
