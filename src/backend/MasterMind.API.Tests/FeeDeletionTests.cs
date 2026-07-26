using MasterMind.API.Controllers;
using MasterMind.API.Data;
using MasterMind.API.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace MasterMind.API.Tests;

public class FeeDeletionTests
{
    [Fact]
    public async Task DeletingMonthlyInstallmentStopsScheduleButRetainsPaidHistory()
    {
        var options = new DbContextOptionsBuilder<MasterMindDbContext>()
            .UseInMemoryDatabase($"fee-delete-tests-{Guid.NewGuid()}")
            .Options;
        await using var context = new MasterMindDbContext(options);
        var student = new Student
        {
            FirstName = "Test",
            LastName = "Student",
            ParentMobile = "9000000000",
            DateOfBirth = new DateTime(2012, 1, 1)
        };
        var structure = new FeeStructure
        {
            Name = "Monthly Tuition",
            Category = FeeCategory.Monthly,
            Frequency = FeeFrequency.Monthly,
            Amount = 1000,
            AcademicYear = "2026-27",
            IsActive = true
        };
        context.AddRange(student, structure);
        await context.SaveChangesAsync();

        var schedule = CreateFee(student.Id, structure.Id, true, null, new DateOnly(2026, 4, 1));
        context.StudentFees.Add(schedule);
        await context.SaveChangesAsync();
        var paid = CreateFee(student.Id, structure.Id, false, schedule.Id, new DateOnly(2026, 4, 1));
        var unpaid = CreateFee(student.Id, structure.Id, false, schedule.Id, new DateOnly(2026, 5, 1));
        context.StudentFees.AddRange(paid, unpaid);
        await context.SaveChangesAsync();
        context.Payments.Add(new Payment
        {
            StudentFeeId = paid.Id,
            Amount = 1000,
            Method = PaymentMethod.Cash,
            Status = PaymentStatus.Completed
        });
        await context.SaveChangesAsync();

        var controller = new FeesController(context, NullLogger<FeesController>.Instance);
        await controller.DeleteFee(unpaid.Id);

        Assert.True((await context.StudentFees.FindAsync(schedule.Id))!.IsDeleted);
        Assert.True((await context.StudentFees.FindAsync(unpaid.Id))!.IsDeleted);
        Assert.False((await context.StudentFees.FindAsync(paid.Id))!.IsDeleted);
    }

    private static StudentFee CreateFee(
        int studentId,
        int structureId,
        bool recurring,
        int? parentFeeId,
        DateOnly dueDate) => new()
    {
        StudentId = studentId,
        FeeStructureId = structureId,
        Amount = 1000,
        FinalAmount = 1000,
        DueDate = dueDate,
        FeeCategory = FeeCategory.Monthly,
        IsRecurring = recurring,
        ParentFeeId = parentFeeId,
        StartDate = new DateOnly(2026, 4, 1),
        EndDate = new DateOnly(2027, 3, 31),
        Month = dueDate.ToString("yyyy-MM"),
        AcademicYear = "2026-27"
    };
}
