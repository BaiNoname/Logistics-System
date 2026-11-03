using Microsoft.EntityFrameworkCore;
using TruckingSystem.Application.Common;
using TruckingSystem.Application.Common.Interfaces;
using TruckingSystem.Domain.Entities;

namespace TruckingSystem.Application.Pricing;

public class PricingService(IApplicationDbContext db) : IPricingService
{
    public Task<List<PriceListDto>> GetPriceListsAsync()
        => db.PriceLists.AsNoTracking().OrderByDescending(p => p.EffectiveDate).ThenBy(p => p.Name)
            .Select(p => new PriceListDto(p.Id, p.Name, p.EffectiveDate, p.TariffRules.Count,
                db.Contracts.Count(c => c.PriceListId == p.Id), p.Status == PriceListStatus.Active ? "Active" : "Inactive")).ToListAsync();

    public async Task<PriceListDto> CreatePriceListAsync(SavePriceListRequest request)
    {
        PriceList.Validate(request.Name, request.EffectiveDate, VnClock.Today);
        var status = ParseStatus(request.Status) ?? PriceListStatus.Active;
        var priceList = new PriceList { Name = request.Name.Trim(), EffectiveDate = request.EffectiveDate.Date, Status = status };
        db.PriceLists.Add(priceList);
        await db.SaveChangesAsync();
        return new PriceListDto(priceList.Id, priceList.Name, priceList.EffectiveDate, 0, 0, priceList.Status.ToString());
    }

    public async Task<PriceListDto?> CopyPriceListAsync(int id, CopyPriceListRequest request)
    {
        var source = await db.PriceLists.Include(p => p.TariffRules)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (source is null) return null;
        var today = VnClock.Today;
        PriceList.Validate(request.Name, today, today);
        var copy = new PriceList
        {
            Name = request.Name.Trim(),
            EffectiveDate = today,
            Status = PriceListStatus.Active,
            TariffRules = source.TariffRules.Select(r => new TariffRule
            {
                VehicleType = r.VehicleType,
                PricePerKm = r.PricePerKm,
                PricePerTon = r.PricePerTon,
            }).ToList(),
        };
        source.Status = PriceListStatus.Inactive;
        source.UpdatedAt = DateTime.UtcNow;
        db.PriceLists.Add(copy);
        await db.SaveChangesAsync();
        return new PriceListDto(copy.Id, copy.Name, copy.EffectiveDate, copy.TariffRules.Count, 0, copy.Status.ToString());
    }

    public async Task<bool> UpdatePriceListAsync(int id, SavePriceListRequest request)
    {
        var priceList = await db.PriceLists.FindAsync(id);
        if (priceList is null) return false;
        priceList.EnsureEditable();
        PriceList.Validate(request.Name, request.EffectiveDate, VnClock.Today);
        var status = ParseStatus(request.Status);
        priceList.Name = request.Name.Trim();
        priceList.EffectiveDate = request.EffectiveDate.Date;
        priceList.Status = status ?? (priceList.Status == PriceListStatus.Active ? PriceListStatus.Active : PriceListStatus.Inactive);
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
        if (await db.ContractAppendices.AnyAsync(a => a.TariffRule.PriceListId == id))
            throw new InvalidOperationException("Không thể xóa bảng giá có chi tiết đang được phụ lục sử dụng.");
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
        var priceList = await db.PriceLists.AsNoTracking().FirstOrDefaultAsync(p => p.Id == priceListId);
        if (priceList is null)
            throw new ArgumentException("Bảng giá không tồn tại. Vui lòng làm mới danh sách.");
        priceList.EnsureEditable();
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
        var rule = await db.TariffRules.Include(r => r.PriceList)
            .FirstOrDefaultAsync(r => r.Id == id && r.PriceListId == priceListId);
        if (rule is null) return false;
        rule.PriceList.EnsureEditable();
        Apply(rule, request);
        rule.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteTariffRuleAsync(int priceListId, int id)
    {
        var rule = await db.TariffRules.Include(r => r.PriceList)
            .FirstOrDefaultAsync(r => r.Id == id && r.PriceListId == priceListId);
        if (rule is null) return false;
        rule.PriceList.EnsureEditable();
        if (await db.Contracts.AnyAsync(c => c.TariffRuleId == id))
            throw new InvalidOperationException("Không thể xóa chi tiết bảng giá đang được hợp đồng sử dụng.");
        if (await db.ContractAppendices.AnyAsync(a => a.TariffRuleId == id))
            throw new InvalidOperationException("Không thể xóa chi tiết bảng giá đang được phụ lục sử dụng.");
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

    private static PriceListStatus? ParseStatus(string? status) => status switch
    {
        null => null,
        "Active" => PriceListStatus.Active,
        // Older clients may still send the removed status.
        "Inactive" or "Expired" => PriceListStatus.Inactive,
        _ => throw new ArgumentException("Trạng thái bảng giá không hợp lệ."),
    };

    private static void Validate(SaveTariffRuleRequest request)
        => TariffRule.Validate(request.VehicleType, request.PricePerKm, request.PricePerTon,
            request.SurchargeFragile ?? 0, request.SurchargeHazmat ?? 0);

    private static void Apply(TariffRule rule, SaveTariffRuleRequest request)
    {
        if ((request.SurchargeFragile.HasValue && request.SurchargeFragile != rule.SurchargeFragile) ||
            (request.SurchargeHazmat.HasValue && request.SurchargeHazmat != rule.SurchargeHazmat))
            throw new InvalidOperationException("Phụ phí chỉ được quản lý trong mục Phụ phí.");
        rule.VehicleType = request.VehicleType.Trim();
        rule.PricePerKm = request.PricePerKm;
        rule.PricePerTon = request.PricePerTon;
    }
}
