using Microsoft.EntityFrameworkCore;
using TruckingSystem.Application.Common;
using TruckingSystem.Domain.Entities;
using TruckingSystem.Domain.Enums;
using TruckingSystem.Domain.Payroll;

namespace TruckingSystem.Application.Payroll;

public partial class PayrollService
{
    public async Task<List<DriverKpiRateDto>> GetKpiRatesAsync()
    {
        var rates = await db.DriverKpiRates.AsNoTracking().ToListAsync();
        var types = await db.Vehicles.Select(v => v.VehicleType).Distinct().ToListAsync();
        return types.Concat(rates.Select(r => r.VehicleType)).Distinct().OrderBy(t => t).Select(type =>
        {
            var rate = rates.SingleOrDefault(r => r.VehicleType == type);
            return new DriverKpiRateDto { VehicleType = type, VehicleFactor = rate?.VehicleFactor ?? 0,
                AmountPerPoint = rate?.AmountPerPoint ?? 0, KmPerPoint = rate?.KmPerPoint ?? 200 };
        }).ToList();
    }

    public Task<PayrollResult> SaveKpiRatesAsync(List<DriverKpiRateDto> request, string actor)
        => Task.FromResult(Fail("Cấu hình đồng/điểm đã ngừng sử dụng. Dùng cấu hình K, điểm chuẩn và H theo ngày hiệu lực."));
}
