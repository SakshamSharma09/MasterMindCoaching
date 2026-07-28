using MasterMind.API.Services.Interfaces;

namespace MasterMind.API.Services.Implementations;

public sealed class RecurringObligationWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RecurringObligationWorker> _logger;

    public RecurringObligationWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<RecurringObligationWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var schedules = scope.ServiceProvider.GetRequiredService<IRecurringObligationService>();
                var salaries = scope.ServiceProvider.GetRequiredService<ITeacherSalaryService>();
                await schedules.EnsureFeeObligationsAsync(
                    DateOnly.FromDateTime(DateTime.Today), cancellationToken: stoppingToken);
                await schedules.EnsureExpenseObligationsAsync(
                    DateTime.Today, cancellationToken: stoppingToken);
                await salaries.EnsureMonthlyObligationsAsync(cancellationToken: stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Recurring obligation generation failed; the defensive API checks will retry.");
            }

            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }
}
