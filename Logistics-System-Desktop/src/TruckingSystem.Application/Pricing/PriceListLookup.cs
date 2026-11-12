using Microsoft.EntityFrameworkCore;
using TruckingSystem.Application.Common;
using TruckingSystem.Application.Common.Interfaces;
using TruckingSystem.Domain.Entities;

namespace TruckingSystem.Application.Pricing;

public static class PriceListLookup
{
    // Tính theo ngày Việt Nam ngay khi đọc, không cần tác vụ nền chuyển trạng thái.
    public static IQueryable<PriceList> Current(IApplicationDbContext db)
        => db.PriceLists.AsNoTracking()
            .Where(p => (p.Status == PriceListStatus.Active || p.Status == PriceListStatus.Pending)
                && p.EffectiveDate <= VnClock.Today)
            .OrderByDescending(p => p.EffectiveDate).ThenByDescending(p => p.Id).Take(1);
}
