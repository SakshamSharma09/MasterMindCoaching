using MasterMind.API.Data;
using MasterMind.API.Models.Entities;
using MasterMind.API.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MasterMind.API.Tests;

public class TeacherSalaryServiceTests
{
    [Fact]
    public async Task EnsureMonthlyObligations_IsIdempotent_AndSkipsInactiveTeachers()
    {
        await using var context = NewContext();
        context.Teachers.AddRange(
            new Teacher
            {
                FirstName = "Active",
                LastName = "Teacher",
                Email = "active@example.com",
                Mobile = "9000000001",
                JoiningDate = DateTime.Today,
                MonthlySalary = 25000m,
                IsActive = true
            },
            new Teacher
            {
                FirstName = "Inactive",
                LastName = "Teacher",
                Email = "inactive@example.com",
                Mobile = "9000000002",
                JoiningDate = DateTime.Today,
                MonthlySalary = 30000m,
                IsActive = false
            });
        await context.SaveChangesAsync();

        var service = new TeacherSalaryService(context);
        await service.EnsureMonthlyObligationsAsync();
        await service.EnsureMonthlyObligationsAsync();

        var salaries = await context.TeacherSalaries.ToListAsync();
        var salary = Assert.Single(salaries);
        Assert.Equal(25000m, salary.NetSalary);
        Assert.Equal(SalaryStatus.Pending, salary.Status);
        Assert.False(string.IsNullOrWhiteSpace(salary.ObligationKey));
    }

    [Fact]
    public async Task GeneratesEveryMissingMonthFromJoiningMonthWithoutDuplicates()
    {
        await using var context = NewContext();
        var currentMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var firstMonth = currentMonth.AddMonths(-2);
        var session = new Session
        {
            Name = "salary-test",
            DisplayName = "Salary test session",
            AcademicYear = "test",
            StartDate = firstMonth,
            EndDate = currentMonth.AddMonths(10).AddDays(-1),
            IsActive = true,
            Status = SessionStatus.Active
        };
        var teacher = new Teacher
        {
            FirstName = "Test",
            LastName = "Teacher",
            Email = "teacher-test@example.invalid",
            Mobile = "7000000000",
            JoiningDate = firstMonth.AddDays(14),
            MonthlySalary = 12000,
            IsActive = true,
            Session = session
        };
        context.AddRange(session, teacher);
        await context.SaveChangesAsync();

        context.TeacherSalaries.Add(new TeacherSalary
        {
            TeacherId = teacher.Id,
            BasicSalary = 12000,
            NetSalary = 12000,
            Month = firstMonth.ToString("MMMM", System.Globalization.CultureInfo.InvariantCulture),
            Year = firstMonth.Year,
            Status = SalaryStatus.Pending
        });
        await context.SaveChangesAsync();

        var service = new TeacherSalaryService(context);
        await service.EnsureMonthlyObligationsAsync(session.Id);
        await service.EnsureMonthlyObligationsAsync(session.Id);

        var salaries = await context.TeacherSalaries
            .Where(s => s.TeacherId == teacher.Id)
            .OrderBy(s => s.Year)
            .ThenBy(s => s.Month)
            .ToListAsync();

        Assert.Equal(3, salaries.Count);
        Assert.Equal(3, salaries.Select(s => new { s.Year, s.Month }).Distinct().Count());
        Assert.All(salaries, s => Assert.Equal(12000, s.NetSalary));
    }

    private static MasterMindDbContext NewContext() =>
        new(new DbContextOptionsBuilder<MasterMindDbContext>()
            .UseInMemoryDatabase($"teacher-salary-{Guid.NewGuid()}")
            .Options);
}
