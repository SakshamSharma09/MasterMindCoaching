using MasterMind.API.Utilities;
using Xunit;

namespace MasterMind.API.Tests;

public class MonthlyFeeScheduleTests
{
    [Fact]
    public void GeneratesOnlyDueMonthsOnTheFirst()
    {
        var dates = MonthlyFeeSchedule.DueDates(
            new DateOnly(2026, 4, 1),
            new DateOnly(2027, 3, 31),
            new DateOnly(2026, 7, 26));

        Assert.Equal(
            new[]
            {
                new DateOnly(2026, 4, 1),
                new DateOnly(2026, 5, 1),
                new DateOnly(2026, 6, 1),
                new DateOnly(2026, 7, 1)
            },
            dates);
    }

    [Fact]
    public void InactiveDateShortensScheduleAndDueDateIsOverdueOnTheFirst()
    {
        var endDate = MonthlyFeeSchedule.EffectiveEndDate(
            new DateOnly(2027, 3, 31),
            new DateOnly(2026, 6, 15));

        Assert.Equal(new DateOnly(2026, 6, 15), endDate);
        Assert.True(MonthlyFeeSchedule.IsOverdue(
            new DateOnly(2026, 7, 1),
            new DateOnly(2026, 7, 1)));
    }
}
