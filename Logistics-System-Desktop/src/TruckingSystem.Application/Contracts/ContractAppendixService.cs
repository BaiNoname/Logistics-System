using Microsoft.EntityFrameworkCore;
using System.Data;
using TruckingSystem.Application.Common;
using TruckingSystem.Application.Common.Interfaces;
using TruckingSystem.Domain.Entities;
using TruckingSystem.Domain.Enums;

namespace TruckingSystem.Application.Contracts;

public class ContractAppendixService(IApplicationDbContext db) : IContractAppendixService
{
    public Task<List<ContractAppendixDto>> GetAllAsync()
        => Project(db.ContractAppendices.AsNoTracking().OrderByDescending(a => a.IssueDate).ThenByDescending(a => a.Id)).ToListAsync();

    public async Task<ContractAppendixLookupsDto> GetLookupsAsync() => new(
        await db.Contracts.AsNoTracking().Where(c => c.StartDate <= VnClock.Today && (c.EndDate == null || c.EndDate >= VnClock.Today)).OrderBy(c => c.Code)
            .Select(c => new ContractOptionDto { Id = c.Id, Name = c.Code }).ToListAsync(),
        await db.TariffRules.AsNoTracking().Where(r => r.PriceList.Status == PriceListStatus.Active)
            .OrderBy(r => r.PriceList.Name).ThenBy(r => r.VehicleType).ThenBy(r => r.Id)
            .Select(r => new AppendixTariffOptionDto(r.Id, r.PriceList.Name, r.VehicleType,
                r.PricePerKm, r.PricePerTon, r.SurchargeFragile, r.SurchargeHazmat, r.PriceListId)).ToListAsync())
    {
        PriceLists = await db.PriceLists.AsNoTracking().Where(p => p.Status == PriceListStatus.Active)
            .OrderBy(p => p.Name).ThenBy(p => p.Id)
            .Select(p => new ContractOptionDto { Id = p.Id, Name = p.Name }).ToListAsync(),
        ContractDiscounts = await db.Contracts.AsNoTracking()
            .Select(c => new AppendixContractOptionDto(c.Id, c.Type == null ? null : c.Type.ToString(), c.DiscountPercent)).ToListAsync(),
        MaxDiscountPercent = await db.MaxDiscountPercents.AsNoTracking().Where(p => p.Id == 1).Select(p => p.Value).SingleAsync(),
    };

    public async Task<ContractAppendixDto> CreateAsync(SaveContractAppendixRequest request, bool isAdmin = false)
    {
        await using var transaction = db.Database.IsRelational()
            ? await db.Database.BeginTransactionAsync(IsolationLevel.Serializable) : null;
        var contractCode = await db.Contracts.AsNoTracking().Where(c => c.Id == request.ContractId)
            .Select(c => c.Code).SingleOrDefaultAsync()
            ?? throw new ArgumentException("Hợp đồng không tồn tại. Vui lòng làm mới dữ liệu.");
        var suffix = "-" + contractCode;
        var codes = await db.ContractAppendices.AsNoTracking()
            .Where(a => a.ContractId == request.ContractId && a.Code.StartsWith("PL-") && a.Code.EndsWith(suffix))
            .Select(a => a.Code).ToListAsync();
        request = request with { Code = DocumentNumber.Next("PL-", codes.Select(code => code[..^suffix.Length])) + suffix };
        request = await ValidateAsync(request, isAdmin);
        await DeactivateOthersAsync(request.ContractId);
        var appendix = new ContractAppendix();
        Apply(appendix, request);
        db.ContractAppendices.Add(appendix);
        await db.SaveChangesAsync();
        if (transaction is not null) await transaction.CommitAsync();
        return await Project(db.ContractAppendices.AsNoTracking().Where(a => a.Id == appendix.Id)).SingleAsync();
    }

    public async Task<bool> UpdateAsync(int id, SaveContractAppendixRequest request, bool isAdmin = false)
    {
        await using var transaction = db.Database.IsRelational()
            ? await db.Database.BeginTransactionAsync(IsolationLevel.Serializable) : null;
        var appendix = await db.ContractAppendices.FindAsync(id);
        if (appendix is null) return false;
        request = await ValidateAsync(request, isAdmin, appendix);
        if (appendix.Status == ContractAppendixStatus.Active && appendix.ContractId != request.ContractId)
            await DeactivateOthersAsync(request.ContractId, appendix.Id);
        Apply(appendix, request);
        appendix.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        if (transaction is not null) await transaction.CommitAsync();
        return true;
    }

    private async Task DeactivateOthersAsync(int contractId, int? exceptId = null)
    {
        var active = await db.ContractAppendices.Where(a => a.ContractId == contractId && a.Id != exceptId
            && a.Status == ContractAppendixStatus.Active).ToListAsync();
        foreach (var previous in active)
        {
            previous.Status = ContractAppendixStatus.Inactive;
            previous.UpdatedAt = DateTime.UtcNow;
        }
        // Giải phóng unique index trước khi thêm/chuyển phụ lục trong cùng giao dịch.
        if (active.Count > 0) await db.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var appendix = await db.ContractAppendices.FindAsync(id);
        if (appendix is null) return false;
        db.ContractAppendices.Remove(appendix);
        await db.SaveChangesAsync();
        return true;
    }

    private async Task<SaveContractAppendixRequest> ValidateAsync(SaveContractAppendixRequest request, bool isAdmin, ContractAppendix? current = null)
    {
        ContractAppendix.Validate(request.Code, request.ContractId, request.IssueDate, request.EffectiveDate, request.TariffRuleId, request.PriceListId,
            VnClock.Today, current?.IssueDate, current?.EffectiveDate);
        var contract = await db.Contracts.AsNoTracking().FirstOrDefaultAsync(c => c.Id == request.ContractId)
            ?? throw new ArgumentException("Hợp đồng không tồn tại. Vui lòng làm mới dữ liệu.");
        if ((current is null || current.ContractId != request.ContractId)
            && Contract.GetStatus(contract.StartDate, contract.EndDate, VnClock.Today) != ContractStatus.Active)
            throw new ArgumentException("Chỉ được thêm phụ lục cho hợp đồng đang hiệu lực.");
        var priceListId = request.PriceListId;
        if (request.TariffRuleId.HasValue)
        {
            var tariff = await db.TariffRules.AsNoTracking().FirstOrDefaultAsync(r => r.Id == request.TariffRuleId)
                ?? throw new ArgumentException("Chi tiết bảng giá không tồn tại. Vui lòng làm mới dữ liệu.");
            if (priceListId.HasValue && priceListId != tariff.PriceListId)
                throw new ArgumentException("Chi tiết giá phải thuộc bảng giá đã chọn.");
            priceListId = tariff.PriceListId;
        }
        var priceList = await db.PriceLists.AsNoTracking().FirstOrDefaultAsync(p => p.Id == priceListId)
            ?? throw new ArgumentException("Bảng giá không tồn tại. Vui lòng làm mới dữ liệu.");
        var currentPriceListId = current?.PriceListId;
        if (current is not null && !currentPriceListId.HasValue)
            currentPriceListId = await db.TariffRules.Where(r => r.Id == current.TariffRuleId).Select(r => (int?)r.PriceListId).FirstOrDefaultAsync();
        if (priceList.Status != PriceListStatus.Active && priceListId != currentPriceListId)
            throw new ArgumentException("Chỉ được chọn bảng giá đang áp dụng.");
        var discount = request.DiscountPercent ?? (current?.ContractId == contract.Id ? current.DiscountPercent : null) ?? contract.DiscountPercent ?? 0;
        Contract.ValidateDiscount(contract.Type ?? ContractType.LongTerm, discount);
        var limit = await db.MaxDiscountPercents.AsNoTracking().Where(p => p.Id == 1).Select(p => p.Value).SingleAsync();
        if (!isAdmin && discount > limit) throw new ArgumentException($"Ưu đãi không được vượt quá {limit}%.");
        var currentId = current?.Id;
        if (await db.ContractAppendices.AnyAsync(a => a.Id != currentId && a.Code == request.Code.Trim()))
            throw new ArgumentException("Mã phụ lục đã tồn tại.");
        return request with { DiscountPercent = discount, PriceListId = priceListId };
    }

    private static void Apply(ContractAppendix appendix, SaveContractAppendixRequest request)
    {
        appendix.Code = request.Code.Trim();
        appendix.ContractId = request.ContractId;
        appendix.IssueDate = request.IssueDate.Date;
        appendix.EffectiveDate = request.EffectiveDate.Date;
        appendix.TariffRuleId = request.TariffRuleId;
        appendix.PriceListId = request.PriceListId;
        appendix.DiscountPercent = request.DiscountPercent;
    }

    private static IQueryable<ContractAppendixDto> Project(IQueryable<ContractAppendix> appendices)
        => appendices.Select(a => new ContractAppendixDto(a.Id, a.Code, a.ContractId, a.Contract.Code,
            a.IssueDate, a.EffectiveDate, a.TariffRule == null ? null : new AppendixTariffOptionDto(a.TariffRule.Id, a.TariffRule.PriceList.Name,
                a.TariffRule.VehicleType, a.TariffRule.PricePerKm, a.TariffRule.PricePerTon,
                a.TariffRule.SurchargeFragile, a.TariffRule.SurchargeHazmat, a.TariffRule.PriceListId),
                a.DiscountPercent, a.PriceListId ?? (a.TariffRule == null ? null : (int?)a.TariffRule.PriceListId),
                a.PriceList == null ? (a.TariffRule == null ? null : a.TariffRule.PriceList.Name) : a.PriceList.Name,
                a.Status.ToString(), VnClock.Today < a.Contract.StartDate ? "Upcoming"
                    : a.Contract.EndDate.HasValue && VnClock.Today > a.Contract.EndDate.Value ? "Expired" : "Active"));
}
