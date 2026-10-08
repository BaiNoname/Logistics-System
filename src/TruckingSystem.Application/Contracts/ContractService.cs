using Microsoft.EntityFrameworkCore;
using TruckingSystem.Application.Common.Interfaces;
using TruckingSystem.Domain.Entities;

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
        // ponytail: Kiểm tra mã tại ứng dụng; thêm unique index khi cần chống tạo trùng đồng thời.
        if (await db.Contracts.AnyAsync(c => c.Code == request.Code.Trim()))
            throw new ArgumentException("Mã hợp đồng đã tồn tại.");

        var contract = new TruckingSystem.Domain.Entities.Contract();
        Apply(contract, request);
        db.Contracts.Add(contract);
        await db.SaveChangesAsync();
        return (await GetByIdAsync(contract.Id))!;
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
        if (contract.CustomerId != request.CustomerId && await db.SalesOrders.AnyAsync(o => o.ContractId == id))
            throw new ArgumentException("Không thể đổi khách hàng của hợp đồng đã được đơn hàng sử dụng.");

        Apply(contract, request);
        contract.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return true;
    }

    private static void Apply(TruckingSystem.Domain.Entities.Contract contract, SaveContractRequest request)
    {
        contract.CustomerId = request.CustomerId;
        contract.Code = request.Code.Trim();
        contract.StartDate = request.StartDate.Date;
        contract.EndDate = request.EndDate?.Date;
        contract.Terms = string.IsNullOrWhiteSpace(request.Terms) ? null : request.Terms.Trim();
        contract.PriceListId = request.PriceListId;
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
        if (await db.SalesOrders.AnyAsync(o => o.ContractId == id))
            throw new InvalidOperationException("Không thể xóa hợp đồng đã được đơn hàng sử dụng.");

        db.Contracts.Remove(contract);
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException("Không thể xóa hợp đồng. Vui lòng tải lại dữ liệu và thử lại.", ex);
        }
        return true;
    }

    private static IQueryable<ContractListItemDto> ProjectContracts(IQueryable<Contract> contracts)
        => contracts
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
            });
}
