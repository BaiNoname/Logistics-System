using Microsoft.EntityFrameworkCore;
using TruckingSystem.Application.Common.Interfaces;
using TruckingSystem.Domain.Entities;

namespace TruckingSystem.Application.Pricing;

internal static class SurchargeLookup
{
    public static async Task<SurchargePolicy?> AtAsync(IApplicationDbContext db, DateTime date, CancellationToken ct = default)
    {
        var day = DateOnly.FromDateTime(date);
        var list = await db.SurchargeLists.AsNoTracking().Include(p => p.Lines).Where(p => p.Published && p.EffectiveDate <= day)
            .OrderByDescending(p => p.Id).FirstOrDefaultAsync(ct);
        return list?.Rates();
    }
}
