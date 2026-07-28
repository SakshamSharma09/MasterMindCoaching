using MasterMind.API.Data;
using MasterMind.API.Models.Entities;
using MasterMind.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MasterMind.API.Services.Implementations;

public class RecurringObligationService : IRecurringObligationService
{
    private readonly MasterMindDbContext _context;

    public RecurringObligationService(MasterMindDbContext context)
    {
        _context = context;
    }

    public int IntervalMonths(FeeFrequency frequency) => frequency switch
    {
        FeeFrequency.Monthly => 1,
        FeeFrequency.Quarterly => 3,
        FeeFrequency.HalfYearly => 6,
        FeeFrequency.Yearly => 12,
        _ => 0
    };

    public async Task EnsureFeeObligationsAsync(
        DateOnly throughDate,
        int? sessionId = null,
        CancellationToken cancellationToken = default)
    {
        var schedules = await _context.StudentFees
            .Include(f => f.Student)
            .Where(f => !f.IsDeleted && f.IsRecurring && f.OccurrenceKey == null &&
                !f.Student.IsDeleted && f.Student.IsActive &&
                (!sessionId.HasValue || f.Student.SessionId == sessionId.Value))
            .ToListAsync(cancellationToken);

        foreach (var schedule in schedules.Where(s => s.Frequency.HasValue))
        {
            var interval = schedule.RecurrenceIntervalMonths ?? IntervalMonths(schedule.Frequency!.Value);
            if (interval <= 0) continue;

            var start = schedule.StartDate ?? schedule.PeriodStart ?? schedule.DueDate;
            var end = schedule.ScheduleEndDate ?? schedule.EndDate ?? start;
            if (schedule.Student.InactiveDate.HasValue)
            {
                var inactive = DateOnly.FromDateTime(schedule.Student.InactiveDate.Value);
                if (inactive < end) end = inactive;
            }

            for (var periodStart = start;
                 periodStart <= throughDate && periodStart <= end;
                 periodStart = periodStart.AddMonths(interval))
            {
                var periodEnd = periodStart.AddMonths(interval).AddDays(-1);
                if (periodEnd > end) periodEnd = end;
                var dueDate = (schedule.FirstDueDate ?? start.AddMonths(interval))
                    .AddMonths(((periodStart.Year - start.Year) * 12) + periodStart.Month - start.Month);
                var key = $"fee:{schedule.Id}:{periodStart:yyyyMMdd}";
                var exists = await _context.StudentFees
                    .AnyAsync(f => f.OccurrenceKey == key, cancellationToken);
                if (exists) continue;

                _context.StudentFees.Add(new StudentFee
                {
                    StudentId = schedule.StudentId,
                    FeeStructureId = schedule.FeeStructureId,
                    Amount = schedule.Amount,
                    DiscountAmount = schedule.DiscountAmount,
                    DiscountReason = schedule.DiscountReason,
                    FinalAmount = schedule.FinalAmount,
                    DueDate = dueDate,
                    FeeCategory = schedule.FeeCategory,
                    Frequency = schedule.Frequency,
                    PeriodStart = periodStart,
                    PeriodEnd = periodEnd,
                    StartDate = start,
                    EndDate = end,
                    ScheduleEndDate = end,
                    RecurrenceIntervalMonths = interval,
                    OccurrenceKey = key,
                    IsRecurring = false,
                    ParentFeeId = schedule.Id,
                    LateFeePerDay = schedule.LateFeePerDay,
                    GracePeriodDays = schedule.GracePeriodDays,
                    Month = periodStart.ToString("yyyy-MM"),
                    AcademicYear = schedule.AcademicYear,
                    Remarks = schedule.Remarks,
                    Status = FeeStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task EnsureExpenseObligationsAsync(
        DateTime throughDate,
        int? sessionId = null,
        CancellationToken cancellationToken = default)
    {
        var schedules = await _context.Expenses
            .Where(e => !e.IsDeleted && e.IsRecurring && e.ParentExpenseId == null &&
                e.RecurrenceIntervalMonths > 0 &&
                (!sessionId.HasValue || e.SessionId == sessionId.Value))
            .ToListAsync(cancellationToken);

        foreach (var schedule in schedules)
        {
            var interval = schedule.RecurrenceIntervalMonths!.Value;
            var start = (schedule.PeriodStart ?? schedule.ExpenseDate).Date;
            var end = schedule.PeriodEnd?.Date;
            for (var periodStart = start;
                 periodStart <= throughDate.Date && (!end.HasValue || periodStart <= end.Value);
                 periodStart = periodStart.AddMonths(interval))
            {
                var key = $"expense:{schedule.Id}:{periodStart:yyyyMMdd}";
                var exists = await _context.Expenses.AnyAsync(e => e.OccurrenceKey == key, cancellationToken);
                if (exists) continue;
                var periodEnd = periodStart.AddMonths(interval).AddDays(-1);
                var isPaidFirstOccurrence = periodStart == start && schedule.Status == ExpenseStatus.Paid;
                _context.Expenses.Add(new Expense
                {
                    Category = schedule.Category,
                    Description = schedule.Description,
                    Amount = schedule.Amount,
                    PaidTo = schedule.PaidTo,
                    ExpenseDate = periodStart,
                    DueDate = periodStart.AddMonths(interval),
                    Status = isPaidFirstOccurrence ? ExpenseStatus.Paid : ExpenseStatus.Pending,
                    PaymentDate = isPaidFirstOccurrence ? schedule.PaymentDate : null,
                    PaymentMethod = isPaidFirstOccurrence ? schedule.PaymentMethod : null,
                    TransactionId = isPaidFirstOccurrence ? schedule.TransactionId : null,
                    ReceiptNumber = isPaidFirstOccurrence ? schedule.ReceiptNumber : null,
                    ProcessedByUserId = isPaidFirstOccurrence ? schedule.ProcessedByUserId : null,
                    Remarks = schedule.Remarks,
                    VendorName = schedule.VendorName,
                    VendorContact = schedule.VendorContact,
                    IsRecurring = false,
                    RecurrencePattern = schedule.RecurrencePattern,
                    ParentExpenseId = schedule.Id,
                    PeriodStart = periodStart,
                    PeriodEnd = periodEnd,
                    RecurrenceIntervalMonths = interval,
                    OccurrenceKey = key,
                    SessionId = schedule.SessionId,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
