using Microsoft.EntityFrameworkCore;
using TruckingSystem.Application.Common.Interfaces;
using TruckingSystem.Domain.Entities;

namespace TruckingSystem.Application.Pricing;

public class PricingService(IApplicationDbContext db) : IPricingService
{
    public Task<List<PriceListDto>> GetPriceListsAsync()
        => db.PriceLists.AsNoTracking().OrderByDescending(p => p.EffectiveDate).ThenBy(p => p.Name)
            .Select(p => new PriceListDto(p.Id, p.Name, p.EffectiveDate, p.TariffRules.Count,
                db.Contracts.Count(c => c.PriceListId == p.Id))).ToListAsync();

    public async Task<PriceListDto> CreatePriceListAsync(SavePriceListRequest request)
    {
        PriceList.Validate(request.Name, request.EffectiveDate);
        var priceList = new PriceList { Name = request.Name.Trim(), EffectiveDate = request.EffectiveDate.Date };
        db.PriceLists.Add(priceList);
        await db.SaveChangesAsync();
        return new PriceListDto(priceList.Id, priceList.Name, priceList.EffectiveDate, 0, 0);
    }

    public async Task<bool> UpdatePriceListAsync(int id, SavePriceListRequest request)
    {
        PriceList.Validate(request.Name, request.EffectiveDate);
        var priceList = await db.PriceLists.FindAsync(id);
        if (priceList is null) return false;
        priceList.Name = request.Name.Trim();
        priceList.EffectiveDate = request.EffectiveDate.Date;
        priceList.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeletePriceListAsync(int id)
    {
        var priceList = await db.PriceLists.FindAsync(id);
        if (priceList is null) return false;
        if (await db.Contracts.AnyAsync(c => c.PriceListId == id))
            throw new InvalidOperationException("Không thể xóa bảng giá đang được hợp đồng sử dụng.");
        db.PriceLists.Remove(priceList);
        await db.SaveChangesAsync();
        return true;
    }

    public Task<List<TariffRuleDto>> GetTariffRulesAsync(int priceListId)
        => db.TariffRules.AsNoTracking().Where(r => r.PriceListId == priceListId).OrderBy(r => r.VehicleType)
            .Select(r => new TariffRuleDto(r.Id, r.PriceListId, r.VehicleType, r.PricePerKm,
                r.PricePerTon, r.SurchargeFragile, r.SurchargeHazmat)).ToListAsync();

    public async Task<TariffRuleDto> CreateTariffRuleAsync(int priceListId, SaveTariffRuleRequest request)
    {
        Validate(request);
        if (!await db.PriceLists.AnyAsync(p => p.Id == priceListId))
            throw new ArgumentException("Bảng giá không tồn tại. Vui lòng làm mới danh sách.");
        var rule = new TariffRule { PriceListId = priceListId };
        Apply(rule, request);
        db.TariffRules.Add(rule);
        await db.SaveChangesAsync();
        return new TariffRuleDto(rule.Id, rule.PriceListId, rule.VehicleType, rule.PricePerKm,
            rule.PricePerTon, rule.SurchargeFragile, rule.SurchargeHazmat);
    }

    public async Task<bool> UpdateTariffRuleAsync(int priceListId, int id, SaveTariffRuleRequest request)
    {
        Validate(request);
        var rule = await db.TariffRules.FirstOrDefaultAsync(r => r.Id == id && r.PriceListId == priceListId);
        if (rule is null) return false;
        Apply(rule, request);
        rule.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteTariffRuleAsync(int priceListId, int id)
    {
        var rule = await db.TariffRules.FirstOrDefaultAsync(r => r.Id == id && r.PriceListId == priceListId);
        if (rule is null) return false;
        db.TariffRules.Remove(rule);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<List<string>> GetVehicleTypesAsync()
    {
        var vehicleTypes = await db.Vehicles.Select(v => v.VehicleType).Distinct().ToListAsync();
        var ruleTypes = await db.TariffRules.Select(r => r.VehicleType).Distinct().ToListAsync();
        return vehicleTypes.Concat(ruleTypes).Select(t => t.Trim()).Where(t => t.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(t => t).ToList();
    }

    private static void Validate(SaveTariffRuleRequest request)
        => TariffRule.Validate(request.VehicleType, request.PricePerKm, request.PricePerTon,
            request.SurchargeFragile, request.SurchargeHazmat);

    private static void Apply(TariffRule rule, SaveTariffRuleRequest request)
    {
        rule.VehicleType = request.VehicleType.Trim();
        rule.PricePerKm = request.PricePerKm;
        rule.PricePerTon = request.PricePerTon;
        rule.SurchargeFragile = request.SurchargeFragile;
        rule.SurchargeHazmat = request.SurchargeHazmat;
    }
}
