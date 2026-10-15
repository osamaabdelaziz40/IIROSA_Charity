using IIROSA.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace IIROSA.Api.Services;

/// <summary>
/// Orphan Report Cycle status worker (UC-ORC-05, docs/Modules/26-UC-ORC) — the project's
/// first hosted service.
///
/// Advances every cycle whose stored status no longer matches its dates: NotStarted →
/// InProgress when the start arrives, → Completed when the end passes
/// (OrphanReportCycleService.RefreshDueStatusesAsync owns the rule and the single save).
/// Runs once at startup, then on a configurable interval
/// (OrphanReportCycles:StatusCheckIntervalMinutes, default 15, clamped to [1, 1440]).
///
/// Completion has no cascade side effect — reports are locked by re-deriving the cycle
/// window at read/write time (BR-ORC-11), and the chooser/coverage reads refresh lazily
/// before serving, so a delayed or crashed tick degrades to at-most-one-interval-stale
/// badges, never a wrong window decision. A failed sweep logs and waits for the next
/// tick; it never takes the host down.
/// </summary>
public class OrphanReportCycleStatusService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OrphanReportCycleStatusService> _logger;
    private readonly TimeSpan _interval;

    public OrphanReportCycleStatusService(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<OrphanReportCycleStatusService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;

        var minutes = configuration.GetValue("OrphanReportCycles:StatusCheckIntervalMinutes", 15);
        _interval = TimeSpan.FromMinutes(Math.Clamp(minutes, 1, 1440));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Orphan report cycle status service starting — sweep every {IntervalMinutes} minute(s)",
            _interval.TotalMinutes);

        // Startup sweep first (a host that was down across a boundary catches up
        // immediately), then the interval. Cancellation surfaces as
        // OperationCanceledException from WaitForNextTickAsync — the normal stop path.
        using var timer = new PeriodicTimer(_interval);
        do
        {
            await RunSweepAsync(stoppingToken);
        }
        while (!stoppingToken.IsCancellationRequested
               && await timer.WaitForNextTickAsync(stoppingToken));
    }

    /// <summary>
    /// One scoped pass — BackgroundService is a singleton and must not capture scoped
    /// services (the DbContext among them) directly.
    /// </summary>
    private async Task RunSweepAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var cycleService = scope.ServiceProvider.GetRequiredService<IOrphanReportCycleService>();

            var transitions = await cycleService.RefreshDueStatusesAsync(stoppingToken);
            if (transitions > 0)
                _logger.LogInformation(
                    "Orphan report cycle status sweep advanced {Count} cycle(s)", transitions);
        }
        catch (OperationCanceledException)
        {
            // Host shutdown — not an error
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Orphan report cycle status sweep failed — retrying on the next tick; reads stay correct via the lazy re-check");
        }
    }
}
