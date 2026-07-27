using MasterMind.API.Utilities;
using Xunit;

namespace MasterMind.API.Tests;

public class MonthlyFeeScheduleTests
{
    [Fact]
    public void GeneratesMonthlyFeesOnTheConfiguredStartDay()
    {
        var dates = MonthlyFeeSchedule.DueDates(
            new DateOnly(2026, 4, 15),
            new DateOnly(2027, 3, 31),
            new DateOnly(2026, 7, 26));

        Assert.Equal(
            new[]
            {
                new DateOnly(2026, 4, 15),
                new DateOnly(2026, 5, 15),
                new DateOnly(2026, 6, 15),
                new DateOnly(2026, 7, 15)
            },
            dates);
    }

    [Fact]
    public void EndOfMonthStartClampsToEachMonthsLastDay()
    {
        var dates = MonthlyFeeSchedule.DueDates(
            new DateOnly(2026, 1, 31),
            new DateOnly(2026, 4, 30),
            new DateOnly(2026, 4, 30));

        Assert.Equal(
            new[]
            {
                new DateOnly(2026, 1, 31),
                new DateOnly(2026, 2, 28),
                new DateOnly(2026, 3, 31),
                new DateOnly(2026, 4, 30)
            },
            dates);
    }

    [Fact]
    public void InactiveDateShortensScheduleAndDueDateIsOverdueWhenReached()
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
