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
        var options = new DbContextOptionsBuilder<MasterMindDbContext>()
            .UseInMemoryDatabase($"salary-tests-{Guid.NewGuid()}")
            .Options;
        await using var context = new MasterMindDbContext(options);

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
}
