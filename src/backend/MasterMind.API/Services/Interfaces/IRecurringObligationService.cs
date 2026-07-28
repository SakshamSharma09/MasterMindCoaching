using MasterMind.API.Models.Entities;

namespace MasterMind.API.Services.Interfaces;

public interface IRecurringObligationService
{
    Task EnsureFeeObligationsAsync(
        DateOnly throughDate,
        int? sessionId = null,
        CancellationToken cancellationToken = default);

    Task EnsureExpenseObligationsAsync(
        DateTime throughDate,
        int? sessionId = null,
        CancellationToken cancellationToken = default);

    int IntervalMonths(FeeFrequency frequency);
}
