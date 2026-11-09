using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TruckingSystem.Application.Common.Interfaces;
using TruckingSystem.Application.FreightChat;

namespace TruckingSystem.Infrastructure.ExternalServices;

public sealed class FreightIndexWorker(IServiceScopeFactory scopes, ILogger<FreightIndexWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        do
        {
            try
            {
                using var scope = scopes.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
                var rules = await db.TariffRules.AsNoTracking().Include(r => r.PriceList).ToListAsync(stoppingToken);
                await scope.ServiceProvider.GetRequiredService<IFreightVectorStore>().SynchronizeAsync(
                    rules.Select(r => FreightDocuments.Create(r, r.PriceList)).ToList(), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception ex) { logger.LogWarning("Freight index unavailable ({Type}); retry in one minute.", ex.GetType().Name); }
            try { await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); }
            catch (OperationCanceledException) { break; }
        } while (!stoppingToken.IsCancellationRequested);
    }
}
