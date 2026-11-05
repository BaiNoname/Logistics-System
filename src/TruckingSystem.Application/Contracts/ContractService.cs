using Microsoft.EntityFrameworkCore;
using System.Data;
using TruckingSystem.Application.Common;
using TruckingSystem.Domain.Enums;
using TruckingSystem.Application.Common.Interfaces;
using TruckingSystem.Application.Pricing;
using TruckingSystem.Domain.Entities;

namespace TruckingSystem.Application.Contracts;

public class ContractService(IApplicationDbContext db) : IContractService
{
    public async Task<ContractLookupDto> GetLookupsAsync() => new()
    {
        MaxDiscountPercent = await GetMaxDiscountPercentAsync(),
        Customers = await db.Customers.AsNoTracking().OrderBy(c => c.Name)
            .Select(c => new ContractOptionDto { Id = c.Id, Name = c.Name }).ToListAsync(),
        PriceLists = await db.PriceLists.AsNoTracking().Where(p => p.Status == PriceListStatus.Active).OrderBy(p => p.Name)
            .Select(p => new ContractOptionDto { Id = p.Id, Name = p.Name }).ToListAsync(),
        TariffRules = await db.TariffRules.AsNoTracking().Where(r => r.PriceList.Status == PriceListStatus.Active)
            .OrderBy(r => r.VehicleType).ThenBy(r => r.Id)
            .Select(r => new TariffRuleDto(r.Id, r.PriceListId, r.VehicleType, r.PricePerKm,
                r.PricePerTon, r.SurchargeFragile, r.SurchargeHazmat)).ToListAsync(),
    };

    public async Task<ContractListItemDto> CreateAsync(SaveContractRequest request, bool isAdmin = false)
    {
        await using var transaction = db.Database.IsRelational()
            ? await db.Database.BeginTransactionAsync(IsolationLevel.Serializable) : null;
        var prefix = $"HD-{request.CustomerId}-{VnClock.Today.Year}-";
        request.Code = DocumentNumber.Next(prefix, await db.Contracts.AsNoTracking()
            .Where(c => c.Code.StartsWith(prefix)).Select(c => c.Code).ToListAsync());
        Validate(request);
        await ValidateDiscountLimitAsync(request.DiscountPercent, isAdmin);
        if (!await db.Customers.AnyAsync(c => c.Id == request.CustomerId))
            throw new ArgumentException("Khách hàng không tồn tại.");
        if (!request.PriceListId.HasValue) throw new ArgumentException("Vui lòng chọn bảng giá đang áp dụng.");
        ValidateType(request.Type, request.DiscountPercent);
        await ValidatePriceListAsync(request.PriceListId);
        await ValidateTariffRuleAsync(request.PriceListId, request.TariffRuleId);
        if (await db.Contracts.AnyAsync(c => c.Code == request.Code.Trim()))
            throw new ArgumentException("Mã hợp đồng đã tồn tại.");

        var contract = new TruckingSystem.Domain.Entities.Contract();
        Apply(contract, request, request.TariffRuleId);
        db.Contracts.Add(contract);
        await db.SaveChangesAsync();
        if (transaction is not null) await transaction.CommitAsync();
        return (await GetByIdAsync(contract.Id))!;
    }

    public async Task<bool> UpdateAsync(int id, SaveContractRequest request, bool isAdmin = false)
    {
        Validate(request);
        var contract = await db.Contracts.FirstOrDefaultAsync(c => c.Id == id);
        if (contract is null) return false;
        if (request.DiscountPercent != contract.DiscountPercent)
            await ValidateDiscountLimitAsync(request.DiscountPercent, isAdmin);
        if (!await db.Customers.AnyAsync(c => c.Id == request.CustomerId))
            throw new ArgumentException("Khách hàng không tồn tại.");
        await ValidatePriceListAsync(request.PriceListId, contract.PriceListId);
        var tariffRuleId = request.TariffRuleId ?? (request.PriceListId == contract.PriceListId ? contract.TariffRuleId : null);
        await ValidateTariffRuleAsync(request.PriceListId, tariffRuleId, contract.TariffRuleId);
        if (await db.Contracts.AnyAsync(c => c.Id != id && c.Code == request.Code.Trim()))
            throw new ArgumentException("Mã hợp đồng đã tồn tại.");
        if (contract.CustomerId != request.CustomerId && (await db.SalesOrders.AnyAsync(o => o.ContractId == id) || await db.TransportOrders.AnyAsync(o => o.ContractId == id)))
            throw new ArgumentException("Không thể đổi khách hàng của hợp đồng đã được đơn hàng sử dụng.");

        Apply(contract, request, tariffRuleId);
        contract.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<int> GetMaxDiscountPercentAsync()
        => (await db.MaxDiscountPercents.AsNoTracking().SingleAsync(p => p.Id == 1)).Value;

    public async Task SaveMaxDiscountPercentAsync(int value)
    {
        MaxDiscountPercent.Validate(value);
        var setting = await db.MaxDiscountPercents.SingleAsync(p => p.Id == 1);
        setting.Value = value;
        setting.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    private async Task ValidateDiscountLimitAsync(decimal? discount, bool isAdmin)
    {
        var limit = await GetMaxDiscountPercentAsync();
        if (!isAdmin && discount > limit)
            throw new ArgumentException($"Chỉ Admin được áp dụng ưu đãi vượt quá {limit}%.");
    }

    private async Task ValidatePriceListAsync(int? priceListId, int? currentPriceListId = null)
    {
        if (!priceListId.HasValue) return;
        var priceList = await db.PriceLists.AsNoTracking().FirstOrDefaultAsync(p => p.Id == priceListId);
        if (priceList is null) throw new ArgumentException("Bảng giá không tồn tại.");
        if (priceList.Status != PriceListStatus.Active && priceListId != currentPriceListId)
            throw new ArgumentException("Chỉ được chọn bảng giá đang áp dụng.");
    }

    private async Task ValidateTariffRuleAsync(int? priceListId, int? tariffRuleId, int? currentTariffRuleId = null)
    {
        if (!tariffRuleId.HasValue) return;
        var rule = await db.TariffRules.AsNoTracking().Where(r => r.Id == tariffRuleId)
            .Select(r => new { r.PriceListId, r.PriceList.Status }).FirstOrDefaultAsync();
        if (rule is null || rule.PriceListId != priceListId)
            throw new ArgumentException("Chi tiết giá phải thuộc bảng giá đã chọn.");
        if (rule.Status != PriceListStatus.Active && tariffRuleId != currentTariffRuleId)
            throw new ArgumentException("Chỉ được chọn chi tiết thuộc bảng giá đang áp dụng.");
    }

    private static ContractType ValidateType(string? type, decimal? discountPercent)
    {
        var value = type switch
        {
            "OneTime" => ContractType.OneTime,
            "LongTerm" => ContractType.LongTerm,
            _ => throw new ArgumentException("Vui lòng chọn loại hợp đồng một lần hoặc dài hạn."),
        };
        Contract.ValidateDiscount(value, discountPercent);
        return value;
    }

    private static void Apply(TruckingSystem.Domain.Entities.Contract contract, SaveContractRequest request, int? tariffRuleId)
    {
        if (request.Type is not null || request.DiscountPercent.HasValue)
        {
            contract.Type = ValidateType(request.Type ?? contract.Type?.ToString(), request.DiscountPercent);
            contract.DiscountPercent = contract.Type == ContractType.LongTerm ? request.DiscountPercent : null;
        }
        contract.CustomerId = request.CustomerId;
        contract.Code = request.Code.Trim();
        contract.StartDate = request.StartDate.Date;
        contract.EndDate = request.EndDate?.Date;
        contract.Terms = string.IsNullOrWhiteSpace(request.Terms) ? null : request.Terms.Trim();
        contract.PriceListId = request.PriceListId;
        contract.TariffRuleId = tariffRuleId;
    }

    private static void Validate(SaveContractRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
            throw new ArgumentException("Mã hợp đồng không được để trống.");
        if (request.CustomerId <= 0)
            throw new ArgumentException("Vui lòng chọn khách hàng.");
        if (request.PriceListId <= 0)
            throw new ArgumentException("Bảng giá không hợp lệ.");
        Contract.ValidatePeriod(request.StartDate, request.EndDate);
    }

    public Task<List<ContractListItemDto>> GetAllAsync()
        => ProjectContracts(db.Contracts.AsNoTracking()
            .OrderByDescending(c => c.StartDate)
            .ThenByDescending(c => c.Id)).ToListAsync();

    public Task<ContractListItemDto?> GetByIdAsync(int id)
        => ProjectContracts(db.Contracts.AsNoTracking().Where(c => c.Id == id)).FirstOrDefaultAsync();

    public async Task<bool> DeleteAsync(int id)
    {
        var contract = await db.Contracts.FirstOrDefaultAsync(c => c.Id == id);
        if (contract is null) return false;
        if (await db.SalesOrders.AnyAsync(o => o.ContractId == id) || await db.TransportOrders.AnyAsync(o => o.ContractId == id))
            throw new InvalidOperationException("Không thể xóa hợp đồng đã được đơn hàng sử dụng.");
        if (await db.ContractAppendices.AnyAsync(a => a.ContractId == id))
            throw new InvalidOperationException("Không thể xóa hợp đồng đang có phụ lục.");

        db.Contracts.Remove(contract);
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException("Không thể xóa hợp đồng. Vui lòng tải lại dữ liệu và thử lại.", ex);
        }
        return true;
    }

    private IQueryable<ContractListItemDto> ProjectContracts(IQueryable<Contract> contracts)
        => contracts
            .Select(c => new ContractListItemDto
            {
                Id = c.Id,
                CustomerId = c.CustomerId,
                Code = c.Code,
                Type = c.Type == null ? null : c.Type.ToString(),
                DiscountPercent = c.DiscountPercent,
                CustomerName = c.Customer.Name,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                Terms = c.Terms,
                PriceListName = c.PriceList == null ? null : c.PriceList.Name,
                PriceListId = c.PriceListId,
                TariffRule = c.TariffRule == null ? null : new TariffRuleDto(c.TariffRule.Id, c.TariffRule.PriceListId,
                    c.TariffRule.VehicleType, c.TariffRule.PricePerKm, c.TariffRule.PricePerTon,
                    c.TariffRule.SurchargeFragile, c.TariffRule.SurchargeHazmat),
                AppendixCodes = db.ContractAppendices.Where(a => a.ContractId == c.Id)
                    .OrderByDescending(a => a.IssueDate).ThenByDescending(a => a.Id)
                    .Select(a => a.Code).Take(1).ToList(),
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
            });
}
