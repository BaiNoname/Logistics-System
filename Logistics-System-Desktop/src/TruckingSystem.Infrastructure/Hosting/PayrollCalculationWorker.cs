using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TruckingSystem.Application.Common;
using TruckingSystem.Application.Payroll;

namespace TruckingSystem.Infrastructure.Hosting;

public sealed class PayrollCalculationWorker(IServiceScopeFactory scopes, ILogger<PayrollCalculationWorker> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var scheduledAtUtc = LatestScheduledRunUtc(DateTime.UtcNow);
            var retry = true;
            try
            {
                retry = !await CalculateOpenMonthsAsync(scheduledAtUtc, stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                logger.LogWarning(ex, "Could not load open payroll periods. Retrying in one minute.");
            }

            var delay = retry ? TimeSpan.FromMinutes(1) : scheduledAtUtc.AddDays(1) - DateTime.UtcNow;
            if (delay > TimeSpan.Zero) await Task.Delay(delay, stoppingToken);
        }
    }

    internal static DateTime LatestScheduledRunUtc(DateTime utcNow)
    {
        var localNow = VnClock.ToVnWallClock(utcNow)!.Value;
        var runAt = localNow.AddHours(-6).Date.AddHours(6);
        return DateTime.SpecifyKind(VnClock.VnToUtc(runAt), DateTimeKind.Utc);
    }

    private async Task<bool> CalculateOpenMonthsAsync(DateTime scheduledAtUtc, CancellationToken cancellationToken)
    {
        var scheduledVn = VnClock.ToVnWallClock(scheduledAtUtc)!.Value;
        List<(int Year, int Month)> months;
        await using (var scope = scopes.CreateAsyncScope())
        {
            var service = scope.ServiceProvider.GetRequiredService<IPayrollService>();
            months = await service.GetOpenMonthsBeforeAsync(scheduledVn.Year, scheduledVn.Month, cancellationToken);
        }
        months.Insert(0, (scheduledVn.Year, scheduledVn.Month));

        var completed = true;
        foreach (var (year, month) in months)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                await using var scope = scopes.CreateAsyncScope();
                var service = scope.ServiceProvider.GetRequiredService<IPayrollService>();
                await service.RunAutomaticCalculationAsync(year, month, scheduledAtUtc, cancellationToken);
            }
            catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
            {
                completed = false;
                logger.LogWarning(ex, "Automatic payroll calculation failed for {Year}-{Month:00}. Retrying in one minute.", year, month);
            }
        }
        return completed;
    }
}
