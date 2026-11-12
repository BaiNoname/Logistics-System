using Microsoft.EntityFrameworkCore;
using System.Data;
using TruckingSystem.Application.Common;
using TruckingSystem.Application.Common.Interfaces;
using TruckingSystem.Domain.Entities;

namespace TruckingSystem.Application.Pricing;

public class PricingService(IApplicationDbContext db) : IPricingService
{
    public async Task<List<PriceListDto>> GetPriceListsAsync()
    {
        var today = VnClock.Today;
        var activeId = await PriceListLookup.Current(db).Select(p => (int?)p.Id).FirstOrDefaultAsync();
        var lists = await db.PriceLists.AsNoTracking().OrderByDescending(p => p.EffectiveDate).ThenByDescending(p => p.Id)
            .Select(p => new { List = p, RuleCount = p.TariffRules.Count, ContractCount = db.Contracts.Count(c => c.PriceListId == p.Id) })
            .ToListAsync();
        return lists.Select(p => new PriceListDto(p.List.Id, p.List.Name, p.List.EffectiveDate, p.RuleCount,
            p.ContractCount, p.List.StatusOn(today, activeId).ToString())).ToList();
    }

    public async Task<PriceListDto> CreatePriceListAsync(SavePriceListRequest request)
    {
        await using var transaction = db.Database.IsRelational()
            ? await db.Database.BeginTransactionAsync(IsolationLevel.Serializable) : null;
        var name = await NextNameAsync();
        PriceList.Validate(name, request.EffectiveDate, VnClock.Today);
        var priceList = new PriceList { Name = name, EffectiveDate = request.EffectiveDate.Date, Status = request.EffectiveDate.Date > VnClock.Today ? PriceListStatus.Pending : PriceListStatus.Active,
            TariffRules = BuildTariffRules(request.TariffRules) };
        db.PriceLists.Add(priceList);
        await db.SaveChangesAsync();
        if (transaction is not null) await transaction.CommitAsync();
        return new PriceListDto(priceList.Id, priceList.Name, priceList.EffectiveDate, priceList.TariffRules.Count, 0, priceList.Status.ToString());
    }

    public async Task<PriceListDto?> CopyPriceListAsync(int id, CopyPriceListRequest request)
    {
        await using var transaction = db.Database.IsRelational()
            ? await db.Database.BeginTransactionAsync(IsolationLevel.Serializable) : null;
        var source = await db.PriceLists.Include(p => p.TariffRules)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (source is null) return null;
        var today = VnClock.Today;
        var effectiveDate = request.EffectiveDate?.Date ?? today;
        var name = await NextNameAsync();
        PriceList.Validate(name, effectiveDate, today);
        var copy = new PriceList
        {
            Name = name,
            EffectiveDate = effectiveDate,
            Status = effectiveDate > today ? PriceListStatus.Pending : PriceListStatus.Active,
            TariffRules = BuildTariffRules(request.TariffRules, source.TariffRules),
        };
        db.PriceLists.Add(copy);
        await db.SaveChangesAsync();
        if (transaction is not null) await transaction.CommitAsync();
        return new PriceListDto(copy.Id, copy.Name, copy.EffectiveDate, copy.TariffRules.Count, 0, copy.Status.ToString());
    }

    public async Task<bool> UpdatePriceListAsync(int id, SavePriceListRequest request)
    {
        await using var transaction = db.Database.IsRelational()
            ? await db.Database.BeginTransactionAsync(IsolationLevel.Serializable) : null;
        var priceList = await db.PriceLists.Include(p => p.TariffRules).FirstOrDefaultAsync(p => p.Id == id);
        if (priceList is null) return false;
        priceList.EnsureEditable(VnClock.Today);
        PriceList.Validate(request.Name, request.EffectiveDate, VnClock.Today);
        var rules = BuildTariffRules(request.TariffRules, priceList.TariffRules, requireInput: true);
        foreach (var value in rules)
        {
            var rule = priceList.TariffRules.FirstOrDefault(r => string.Equals(r.VehicleType.Trim(), value.VehicleType, StringComparison.OrdinalIgnoreCase));
            if (rule is null) priceList.TariffRules.Add(value);
            else
            {
                rule.PricePerKm = value.PricePerKm;
                rule.PricePerTon = value.PricePerTon;
                rule.UpdatedAt = DateTime.UtcNow;
            }
        }
        priceList.Name = request.Name.Trim();
        priceList.EffectiveDate = request.EffectiveDate.Date;
        priceList.Status = request.EffectiveDate.Date > VnClock.Today ? PriceListStatus.Pending : PriceListStatus.Active;
        priceList.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        if (transaction is not null) await transaction.CommitAsync();
        return true;
    }

    public async Task<bool> DeletePriceListAsync(int id)
    {
        await using var transaction = db.Database.IsRelational()
            ? await db.Database.BeginTransactionAsync(IsolationLevel.Serializable) : null;
        var priceList = await db.PriceLists.FindAsync(id);
        if (priceList is null) return false;
        priceList.EnsureEditable(VnClock.Today);
        priceList.Status = PriceListStatus.Cancelled;
        priceList.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        if (transaction is not null) await transaction.CommitAsync();
        return true;
    }

    public Task<List<TariffRuleDto>> GetTariffRulesAsync(int priceListId)
        => db.TariffRules.AsNoTracking().Where(r => r.PriceListId == priceListId).OrderBy(r => r.VehicleType)
            .Select(r => new TariffRuleDto(r.Id, r.PriceListId, r.VehicleType, r.PricePerKm,
                r.PricePerTon, r.SurchargeFragile, r.SurchargeHazmat)).ToListAsync();

    public async Task<TariffRuleDto> CreateTariffRuleAsync(int priceListId, SaveTariffRuleRequest request)
    {
        await using var transaction = db.Database.IsRelational()
            ? await db.Database.BeginTransactionAsync(IsolationLevel.Serializable) : null;
        Validate(request);
        var priceList = await db.PriceLists.AsNoTracking().Include(p => p.TariffRules).FirstOrDefaultAsync(p => p.Id == priceListId);
        if (priceList is null)
            throw new ArgumentException("Bảng giá không tồn tại. Vui lòng làm mới danh sách.");
        priceList.EnsureEditable(VnClock.Today);
        ValidateVehicleTypes(priceList.TariffRules.Select(r => r.VehicleType).Append(request.VehicleType));
        var rule = new TariffRule { PriceListId = priceListId };
        Apply(rule, request);
        db.TariffRules.Add(rule);
        await db.SaveChangesAsync();
        if (transaction is not null) await transaction.CommitAsync();
        return new TariffRuleDto(rule.Id, rule.PriceListId, rule.VehicleType, rule.PricePerKm,
            rule.PricePerTon, rule.SurchargeFragile, rule.SurchargeHazmat);
    }

    public async Task<bool> UpdateTariffRuleAsync(int priceListId, int id, SaveTariffRuleRequest request)
    {
        await using var transaction = db.Database.IsRelational()
            ? await db.Database.BeginTransactionAsync(IsolationLevel.Serializable) : null;
        Validate(request);
        var rule = await db.TariffRules.Include(r => r.PriceList).ThenInclude(p => p.TariffRules)
            .FirstOrDefaultAsync(r => r.Id == id && r.PriceListId == priceListId);
        if (rule is null) return false;
        rule.PriceList.EnsureEditable(VnClock.Today);
        ValidateVehicleTypes(rule.PriceList.TariffRules.Where(r => r.Id != id).Select(r => r.VehicleType).Append(request.VehicleType));
        Apply(rule, request);
        rule.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        if (transaction is not null) await transaction.CommitAsync();
        return true;
    }

    public async Task<bool> DeleteTariffRuleAsync(int priceListId, int id)
    {
        await using var transaction = db.Database.IsRelational()
            ? await db.Database.BeginTransactionAsync(IsolationLevel.Serializable) : null;
        var rule = await db.TariffRules.Include(r => r.PriceList)
            .FirstOrDefaultAsync(r => r.Id == id && r.PriceListId == priceListId);
        if (rule is null) return false;
        rule.PriceList.EnsureEditable(VnClock.Today);
        if (await db.Contracts.AnyAsync(c => c.TariffRuleId == id))
            throw new InvalidOperationException("Không thể xóa chi tiết bảng giá đang được hợp đồng sử dụng.");
        if (await db.ContractAppendices.AnyAsync(a => a.TariffRuleId == id))
            throw new InvalidOperationException("Không thể xóa chi tiết bảng giá đang được phụ lục sử dụng.");
        if (TariffRule.DefaultVehicleTypes.Contains(rule.VehicleType.Trim(), StringComparer.OrdinalIgnoreCase))
            throw new InvalidOperationException("Phải giữ đủ giá cho các loại xe mặc định. Nhập 0 nếu không tính.");
        db.TariffRules.Remove(rule);
        await db.SaveChangesAsync();
        if (transaction is not null) await transaction.CommitAsync();
        return true;
    }

    public async Task<List<TariffRuleDto>?> SaveTariffRulesAsync(int priceListId, SaveTariffRulesRequest request)
    {
        if (request.Rows is null || request.Rows.Any(r => r is null))
            throw new ArgumentException("Danh sách giá không hợp lệ.");
        foreach (var row in request.Rows)
        {
            TariffRule.Validate(row.VehicleType, row.PricePerKm, row.PricePerTon, 0, 0);
            if (row.Id <= 0) throw new ArgumentException("Mã chi tiết giá không hợp lệ.");
        }
        if (request.Rows.Select(r => r.VehicleType.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).Count() != request.Rows.Count
            || request.Rows.Where(r => r.Id.HasValue).Select(r => r.Id).Distinct().Count() != request.Rows.Count(r => r.Id.HasValue))
            throw new ArgumentException("Danh sách có dòng giá trùng nhau.");

        await using var transaction = db.Database.IsRelational()
            ? await db.Database.BeginTransactionAsync(IsolationLevel.Serializable) : null;
        var list = await db.PriceLists.Include(p => p.TariffRules).FirstOrDefaultAsync(p => p.Id == priceListId);
        if (list is null) return null;
        list.EnsureEditable(VnClock.Today);
        ValidateVehicleTypes(list.TariffRules.Select(r => r.VehicleType).Concat(request.Rows.Where(r => !r.Id.HasValue).Select(r => r.VehicleType)));
        foreach (var row in request.Rows)
        {
            if (row.Id.HasValue)
            {
                var rule = list.TariffRules.FirstOrDefault(r => r.Id == row.Id);
                if (rule is null || !string.Equals(rule.VehicleType.Trim(), row.VehicleType.Trim(), StringComparison.OrdinalIgnoreCase))
                    throw new ArgumentException("Chi tiết giá đã thay đổi hoặc không thuộc bảng giá này. Vui lòng tải lại.");
            }
            else if (list.TariffRules.Any(r => string.Equals(r.VehicleType.Trim(), row.VehicleType.Trim(), StringComparison.OrdinalIgnoreCase)))
                throw new ArgumentException($"Loại xe {row.VehicleType} đã có giá. Vui lòng tải lại.");
        }
        foreach (var row in request.Rows)
        {
            var rule = row.Id.HasValue ? list.TariffRules.Single(r => r.Id == row.Id) : new TariffRule { PriceListId = priceListId };
            Apply(rule, new SaveTariffRuleRequest(row.VehicleType, row.PricePerKm, row.PricePerTon));
            if (!row.Id.HasValue) list.TariffRules.Add(rule);
            else rule.UpdatedAt = DateTime.UtcNow;
        }
        await db.SaveChangesAsync();
        if (transaction is not null) await transaction.CommitAsync();
        return list.TariffRules.Select(r => new TariffRuleDto(r.Id, r.PriceListId, r.VehicleType, r.PricePerKm,
            r.PricePerTon, r.SurchargeFragile, r.SurchargeHazmat)).ToList();
    }

    public async Task<List<string>> GetVehicleTypesAsync()
    {
        var vehicleTypes = await db.Vehicles.Select(v => v.VehicleType).Distinct().ToListAsync();
        var ruleTypes = await db.TariffRules.Select(r => r.VehicleType).Distinct().ToListAsync();
        return vehicleTypes.Concat(ruleTypes).Select(t => t.Trim()).Where(t => t.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(t => t).ToList();
    }

    private static List<TariffRule> BuildTariffRules(List<SaveTariffRuleRequest>? input, ICollection<TariffRule>? source = null, bool requireInput = false)
    {
        if (input is null && source is not null && !requireInput)
            input = source.Select(r => new SaveTariffRuleRequest(r.VehicleType, r.PricePerKm, r.PricePerTon)).ToList();
        if (input is null || input.Any(r => r is null)) throw new ArgumentException("Vui lòng nhập đầy đủ chi tiết bảng giá.");
        foreach (var row in input) Validate(row);
        ValidateVehicleTypes(input.Select(r => r.VehicleType));
        var types = input.Select(r => r.VehicleType.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var required = source?.Select(r => r.VehicleType.Trim()) ?? Enumerable.Empty<string>();
        if (required.Any(t => !types.Contains(t))) throw new ArgumentException("Vui lòng nhập đủ giá/km và giá/tấn cho tất cả loại xe; nhập 0 nếu không tính.");
        return input.Select(row =>
        {
            var rule = new TariffRule();
            Apply(rule, row);
            return rule;
        }).ToList();
    }

    private static void ValidateVehicleTypes(IEnumerable<string> vehicleTypes)
    {
        var values = vehicleTypes.Select(t => t.Trim()).ToList();
        var types = values.ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (types.Count != values.Count) throw new ArgumentException("Chi tiết giá có loại xe trùng nhau.");
        if (TariffRule.DefaultVehicleTypes.Any(t => !types.Contains(t)))
            throw new ArgumentException("Vui lòng nhập đầy đủ chi tiết giá cho tất cả loại xe.");
    }

    private async Task<string> NextNameAsync()
    {
        var prefix = $"BG-{VnClock.Today.Year}-";
        return DocumentNumber.Next(prefix, await db.PriceLists.AsNoTracking()
            .Where(p => p.Name.StartsWith(prefix)).Select(p => p.Name).ToListAsync());
    }

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
