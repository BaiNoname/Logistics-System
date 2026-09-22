using Microsoft.EntityFrameworkCore;
using TruckingSystem.Application.Common.Interfaces;
using TruckingSystem.Domain.Entities;

namespace TruckingSystem.Application.Fuel;

public class FuelService : IFuelService
{
    private readonly IApplicationDbContext _db;

    public FuelService(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<FuelProviderDto>> GetProvidersAsync()
    {
        return await _db.FuelProviders
            .Select(p => new FuelProviderDto(p.Id, p.Name, p.ContactPhone, p.Address))
            .ToListAsync();
    }

    public async Task<FuelProviderDto> CreateProviderAsync(CreateFuelProviderRequest request)
    {
        var provider = new FuelProvider
        {
            Name = request.Name,
            ContactPhone = request.ContactPhone,
            Address = request.Address,
        };
        _db.FuelProviders.Add(provider);
        await _db.SaveChangesAsync();

        return new FuelProviderDto(provider.Id, provider.Name, provider.ContactPhone, provider.Address);
    }

    public async Task<FuelActionResult> DeleteProviderAsync(int id)
    {
        var hasPriceHistory = await _db.FuelPriceHistories.AnyAsync(p => p.FuelProviderId == id);
        if (hasPriceHistory)
            return new FuelActionResult(false, "Hãng xăng này đã có lịch sử giá — không thể xóa. Cần xóa hết lịch sử giá trước.");

        var provider = await _db.FuelProviders.FindAsync(id);
        if (provider is null) return new FuelActionResult(false, "Không tìm thấy hãng xăng.");

        _db.FuelProviders.Remove(provider);
        await _db.SaveChangesAsync();
        return new FuelActionResult(true, null);
    }

    public async Task<List<FuelPriceHistoryDto>> GetPriceHistoryAsync(int? providerId)
    {
        var query = _db.FuelPriceHistories.Include(p => p.FuelProvider).AsQueryable();
        if (providerId.HasValue) query = query.Where(p => p.FuelProviderId == providerId.Value);

        return await query
            .OrderByDescending(p => p.EffectiveDate)
            .Select(p => new FuelPriceHistoryDto(p.Id, p.FuelType, p.Price, p.EffectiveDate, p.FuelProviderId, p.FuelProvider != null ? p.FuelProvider.Name : "(Giá thị trường chung)"))
            .ToListAsync();
    }

    public async Task<FuelPriceHistoryDto> CreatePriceAsync(CreateFuelPriceRequest request)
    {
        var price = new FuelPriceHistory
        {
            FuelType = request.FuelType,
            Price = request.Price,
            EffectiveDate = request.EffectiveDate,
            FuelProviderId = request.FuelProviderId,
        };
        _db.FuelPriceHistories.Add(price);
        await _db.SaveChangesAsync();

        string? providerName = null;
        if (request.FuelProviderId.HasValue)
        {
            var provider = await _db.FuelProviders.FindAsync(request.FuelProviderId.Value);
            providerName = provider?.Name;
        }

        return new FuelPriceHistoryDto(price.Id, price.FuelType, price.Price, price.EffectiveDate, price.FuelProviderId, providerName ?? "(Giá thị trường chung)");
    }
}