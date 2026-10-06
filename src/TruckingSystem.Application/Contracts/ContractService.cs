using Microsoft.EntityFrameworkCore;
using TruckingSystem.Application.Common.Interfaces;

namespace TruckingSystem.Application.Contracts;

public class ContractService(IApplicationDbContext db) : IContractService
{
    public async Task<ContractLookupDto> GetLookupsAsync() => new()
    {
        Customers = await db.Customers.AsNoTracking().OrderBy(c => c.Name)
            .Select(c => new ContractOptionDto { Id = c.Id, Name = c.Name }).ToListAsync(),
        PriceLists = await db.PriceLists.AsNoTracking().OrderBy(p => p.Name)
            .Select(p => new ContractOptionDto { Id = p.Id, Name = p.Name }).ToListAsync(),
    };

    public async Task<ContractListItemDto> CreateAsync(SaveContractRequest request)
    {
        Validate(request);
        if (!await db.Customers.AnyAsync(c => c.Id == request.CustomerId))
            throw new ArgumentException("Khách hàng không tồn tại.");
        if (request.PriceListId.HasValue && !await db.PriceLists.AnyAsync(p => p.Id == request.PriceListId))
            throw new ArgumentException("Bảng giá không tồn tại.");
        if (await db.Contracts.AnyAsync(c => c.Code == request.Code.Trim()))
            throw new ArgumentException("Mã hợp đồng đã tồn tại.");

        var contract = new TruckingSystem.Domain.Entities.Contract();
        Apply(contract, request);
        db.Contracts.Add(contract);
        await db.SaveChangesAsync();
        return (await GetAllAsync()).First(c => c.Id == contract.Id);
    }

    public async Task<bool> UpdateAsync(int id, SaveContractRequest request)
    {
        Validate(request);
        var contract = await db.Contracts.FirstOrDefaultAsync(c => c.Id == id);
        if (contract is null) return false;
        if (!await db.Customers.AnyAsync(c => c.Id == request.CustomerId))
            throw new ArgumentException("Khách hàng không tồn tại.");
        if (request.PriceListId.HasValue && !await db.PriceLists.AnyAsync(p => p.Id == request.PriceListId))
            throw new ArgumentException("Bảng giá không tồn tại.");
        if (await db.Contracts.AnyAsync(c => c.Id != id && c.Code == request.Code.Trim()))
            throw new ArgumentException("Mã hợp đồng đã tồn tại.");

        Apply(contract, request);
        await db.SaveChangesAsync();
        return true;
    }

    private static void Apply(TruckingSystem.Domain.Entities.Contract contract, SaveContractRequest request)
    {
        contract.CustomerId = request.CustomerId;
        contract.Code = request.Code.Trim();
        contract.StartDate = request.StartDate;
        contract.EndDate = request.EndDate;
        contract.Terms = string.IsNullOrWhiteSpace(request.Terms) ? null : request.Terms.Trim();
        contract.PriceListId = request.PriceListId;
    }

    private static void Validate(SaveContractRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
            throw new ArgumentException("Mã hợp đồng không được để trống.");
        if (request.EndDate.HasValue && request.EndDate.Value.Date < request.StartDate.Date)
            throw new ArgumentException("Ngày kết thúc phải sau ngày bắt đầu.");
    }

    public Task<List<ContractListItemDto>> GetAllAsync()
        => db.Contracts.AsNoTracking()
            .OrderByDescending(c => c.StartDate)
            .Select(c => new ContractListItemDto
            {
                Id = c.Id,
                CustomerId = c.CustomerId,
                Code = c.Code,
                CustomerName = c.Customer.Name,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                Terms = c.Terms,
                PriceListName = c.PriceList == null ? null : c.PriceList.Name,
                PriceListId = c.PriceListId,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
            })
            .ToListAsync();
}
