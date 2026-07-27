namespace MasterMind.API.Utilities;

public static class MonthlyFeeSchedule
{
    public static DateOnly EffectiveEndDate(
        DateOnly requestedEndDate,
        DateOnly? inactiveDate)
    {
        return inactiveDate.HasValue && inactiveDate.Value < requestedEndDate
            ? inactiveDate.Value
            : requestedEndDate;
    }

    public static IReadOnlyList<DateOnly> DueDates(
        DateOnly startDate,
        DateOnly endDate,
        DateOnly throughDate)
    {
        var dates = new List<DateOnly>();
        var cursor = startDate;
        while (cursor <= endDate && cursor <= throughDate)
        {
            dates.Add(cursor);
            var nextMonth = new DateOnly(cursor.Year, cursor.Month, 1).AddMonths(1);
            cursor = new DateOnly(
                nextMonth.Year,
                nextMonth.Month,
                Math.Min(startDate.Day, DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month)));
        }
        return dates;
    }

    public static bool IsOverdue(DateOnly dueDate, DateOnly today) => today >= dueDate;
}
