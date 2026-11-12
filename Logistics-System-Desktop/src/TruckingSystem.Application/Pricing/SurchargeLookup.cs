using Microsoft.EntityFrameworkCore;
using TruckingSystem.Application.Common.Interfaces;
using TruckingSystem.Domain.Entities;

namespace TruckingSystem.Application.Pricing;

/// <summary>Tra bảng phụ phí đã công bố hiệu lực tại một ngày; dùng chung cho hóa đơn và gói chatbot (Logistics-System-ChatBot).</summary>
public static class SurchargeLookup
{
    public static async Task<SurchargePolicy?> AtAsync(IApplicationDbContext db, DateTime date, CancellationToken ct = default)
    {
        var day = DateOnly.FromDateTime(date);
        var list = await db.SurchargeLists.AsNoTracking().Include(p => p.Lines).Where(p => p.Published && p.EffectiveDate <= day)
            .OrderByDescending(p => p.Id).FirstOrDefaultAsync(ct);
        return list?.Rates();
    }
}
