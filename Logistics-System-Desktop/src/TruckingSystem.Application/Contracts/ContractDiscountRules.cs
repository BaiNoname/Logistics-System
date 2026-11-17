using Microsoft.EntityFrameworkCore;
using TruckingSystem.Application.Common.Interfaces;
using TruckingSystem.Domain.Entities;

namespace TruckingSystem.Application.Contracts;

internal static class ContractDiscountRules
{
    public static Task<ContractDiscountLimits> LoadLimitsAsync(IApplicationDbContext db)
        => db.MaxDiscountPercents.AsNoTracking().Where(p => p.Id == 1)
            .Select(p => new ContractDiscountLimits(p.Value, p.LateDeliveryDiscountPercent)).SingleAsync();

    public static void ValidateLateDeliveryOverride(decimal? requested, decimal currentOrDefault, bool isAdmin)
    {
        if (requested is not decimal percent) return;
        Contract.ValidateLateDeliveryDiscount(percent);
        if (!isAdmin && percent != currentOrDefault)
            throw new ArgumentException("Chỉ Admin được thay đổi mức giảm giá khi giao trễ.");
    }
}
