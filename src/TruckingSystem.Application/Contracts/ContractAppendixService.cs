using Microsoft.EntityFrameworkCore;
using TruckingSystem.Application.Common.Interfaces;
using TruckingSystem.Domain.Entities;

namespace TruckingSystem.Application.Contracts;

public class ContractAppendixService(IApplicationDbContext db) : IContractAppendixService
{
    public Task<List<ContractAppendixDto>> GetAllAsync()
        => Project(db.ContractAppendices.AsNoTracking().OrderByDescending(a => a.IssueDate).ThenByDescending(a => a.Id)).ToListAsync();

    public async Task<ContractAppendixLookupsDto> GetLookupsAsync() => new(
        await db.Contracts.AsNoTracking().OrderBy(c => c.Code)
            .Select(c => new ContractOptionDto { Id = c.Id, Name = c.Code }).ToListAsync(),
        await db.TariffRules.AsNoTracking().Where(r => r.PriceList.Status == PriceListStatus.Active)
            .OrderBy(r => r.PriceList.Name).ThenBy(r => r.VehicleType).ThenBy(r => r.Id)
            .Select(r => new AppendixTariffOptionDto(r.Id, r.PriceList.Name, r.VehicleType,
                r.PricePerKm, r.PricePerTon, r.SurchargeFragile, r.SurchargeHazmat, r.PriceListId)).ToListAsync())
    {
        PriceLists = await db.PriceLists.AsNoTracking().Where(p => p.Status == PriceListStatus.Active)
            .OrderBy(p => p.Name).ThenBy(p => p.Id)
            .Select(p => new ContractOptionDto { Id = p.Id, Name = p.Name }).ToListAsync(),
    };

    public async Task<ContractAppendixDto> CreateAsync(SaveContractAppendixRequest request)
    {
        await ValidateAsync(request);
        var appendix = new ContractAppendix();
        Apply(appendix, request);
        db.ContractAppendices.Add(appendix);
        await db.SaveChangesAsync();
        return await Project(db.ContractAppendices.AsNoTracking().Where(a => a.Id == appendix.Id)).SingleAsync();
    }

    public async Task<bool> UpdateAsync(int id, SaveContractAppendixRequest request)
    {
        await ValidateAsync(request, id);
        var appendix = await db.ContractAppendices.FindAsync(id);
        if (appendix is null) return false;
        Apply(appendix, request);
        appendix.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var appendix = await db.ContractAppendices.FindAsync(id);
        if (appendix is null) return false;
        db.ContractAppendices.Remove(appendix);
        await db.SaveChangesAsync();
        return true;
    }

    private async Task ValidateAsync(SaveContractAppendixRequest request, int? id = null)
    {
        ContractAppendix.Validate(request.Code, request.ContractId, request.IssueDate, request.EffectiveDate, request.TariffRuleId);
        if (!await db.Contracts.AnyAsync(c => c.Id == request.ContractId))
            throw new ArgumentException("Hợp đồng không tồn tại. Vui lòng làm mới dữ liệu.");
        var tariff = await db.TariffRules.AsNoTracking().Where(r => r.Id == request.TariffRuleId)
            .Select(r => new { r.PriceList.Status }).FirstOrDefaultAsync();
        if (tariff is null)
            throw new ArgumentException("Chi tiết bảng giá không tồn tại. Vui lòng làm mới dữ liệu.");
        if (tariff.Status != PriceListStatus.Active &&
            !await db.ContractAppendices.AnyAsync(a => a.Id == id && a.TariffRuleId == request.TariffRuleId))
            throw new ArgumentException("Chỉ được chọn chi tiết thuộc bảng giá đang áp dụng.");
        if (await db.ContractAppendices.AnyAsync(a => a.Id != id && a.Code == request.Code.Trim()))
            throw new ArgumentException("Mã phụ lục đã tồn tại.");
    }

    private static void Apply(ContractAppendix appendix, SaveContractAppendixRequest request)
    {
        appendix.Code = request.Code.Trim();
        appendix.ContractId = request.ContractId;
        appendix.IssueDate = request.IssueDate.Date;
        appendix.EffectiveDate = request.EffectiveDate.Date;
        appendix.TariffRuleId = request.TariffRuleId;
    }

    private static IQueryable<ContractAppendixDto> Project(IQueryable<ContractAppendix> appendices)
        => appendices.Select(a => new ContractAppendixDto(a.Id, a.Code, a.ContractId, a.Contract.Code,
            a.IssueDate, a.EffectiveDate, new AppendixTariffOptionDto(a.TariffRuleId, a.TariffRule.PriceList.Name,
                a.TariffRule.VehicleType, a.TariffRule.PricePerKm, a.TariffRule.PricePerTon,
                a.TariffRule.SurchargeFragile, a.TariffRule.SurchargeHazmat, a.TariffRule.PriceListId)));
}
