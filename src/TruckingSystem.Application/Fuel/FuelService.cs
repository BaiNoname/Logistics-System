using Microsoft.EntityFrameworkCore;
using TruckingSystem.Application.Common.Interfaces;
using TruckingSystem.Domain.Entities;

namespace TruckingSystem.Application.Fuel;

public class FuelService : IFuelService
{
    /// <summary>DANH MỤC loại nhiên liệu chuẩn — nguồn DUY NHẤT cho các dropdown
    /// (thêm giá ở màn Nhiên liệu). Xe tải chủ yếu chạy Dầu DO (diesel); các loại
    /// xăng dành cho xe tải nhẹ/van chạy máy xăng. Muốn thêm loại mới chỉ cần
    /// thêm vào danh sách này.</summary>
    public static readonly string[] StandardFuelTypes =
    {
        "Dầu DO",
        "Xăng RON95",
        "Xăng E5 RON92",
        "Xăng E10 RON95",
    };

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

    public async Task<List<string>> GetFuelTypesAsync(bool pricedOnly)
    {
        var priced = await _db.FuelPriceHistories.Select(p => p.FuelType).Distinct().ToListAsync();
        // Danh mục = loại chuẩn + loại đã có trong bảng giá (dữ liệu cũ). KHÔNG lấy từ
        // xe nữa — trước đây gõ sai trên xe sẽ thành 1 "loại hợp lệ" trong danh mục.
        IEnumerable<string> all = pricedOnly ? priced : StandardFuelTypes.Concat(priced);
        return all
            .Select(t => t.Trim())
            .Where(t => t.Length > 0)
            .GroupBy(t => t, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .OrderBy(t => t)
            .ToList();
    }

    public async Task<FuelPriceHistoryDto> CreatePriceAsync(CreateFuelPriceRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FuelType))
            throw new InvalidOperationException("Vui lòng chọn loại nhiên liệu.");
        if (request.Price <= 0)
            throw new InvalidOperationException("Giá phải lớn hơn 0.");

        // Chỉ nhận loại nhiên liệu có trong danh mục (chọn từ dropdown) — tránh
        // gõ sai tên khiến xe không tìm được đơn giá ("Dầu DO" vs "dau do ").
        var allowed = await GetFuelTypesAsync(pricedOnly: false);
        var fuelType = allowed.FirstOrDefault(t => string.Equals(t, request.FuelType.Trim(), StringComparison.OrdinalIgnoreCase));
        if (fuelType is null)
            throw new InvalidOperationException($"Loại nhiên liệu \"{request.FuelType}\" không có trong danh mục.");

        if (request.FuelProviderId.HasValue && !await _db.FuelProviders.AnyAsync(p => p.Id == request.FuelProviderId.Value))
            throw new InvalidOperationException("Hãng nhiên liệu không tồn tại.");

        var effectiveDate = request.EffectiveDate.Date;
        var duplicate = await _db.FuelPriceHistories.AnyAsync(p =>
            p.FuelProviderId == request.FuelProviderId && p.FuelType == fuelType && p.EffectiveDate.Date == effectiveDate);
        if (duplicate)
            throw new InvalidOperationException($"Đã có giá \"{fuelType}\" áp dụng ngày {effectiveDate:dd/MM/yyyy} cho hãng này — xóa giá cũ trước nếu muốn nhập lại.");

        var price = new FuelPriceHistory
        {
            FuelType = fuelType,
            Price = request.Price,
            EffectiveDate = effectiveDate,
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

    public async Task<FuelActionResult> DeletePriceAsync(int priceId)
    {
        var price = await _db.FuelPriceHistories.FindAsync(priceId);
        if (price is null) return new FuelActionResult(false, "Không tìm thấy mức giá này.");

        _db.FuelPriceHistories.Remove(price);
        await _db.SaveChangesAsync();
        return new FuelActionResult(true, null);
    }
}