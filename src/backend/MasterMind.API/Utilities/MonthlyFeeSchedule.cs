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
        var cursor = new DateOnly(startDate.Year, startDate.Month, 1);
        while (cursor <= endDate && cursor <= throughDate)
        {
            dates.Add(cursor);
            cursor = cursor.AddMonths(1);
        }
        return dates;
    }

    public static bool IsOverdue(DateOnly dueDate, DateOnly today) => today >= dueDate;
}
