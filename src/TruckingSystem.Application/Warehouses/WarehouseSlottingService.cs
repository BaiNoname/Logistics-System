using Microsoft.EntityFrameworkCore;
using TruckingSystem.Application.Common.Interfaces;
using TruckingSystem.Domain.Enums;

namespace TruckingSystem.Application.Warehouses;

/// <summary>
/// Cài đặt thuật toán Best-Fit: trong các StorageBin còn đủ chỗ chứa và đúng
/// khu vực (Zone) phù hợp với tính chất hàng hóa, chọn Bin có khoảng trống
/// CÒN LẠI SAU KHI XẾP là NHỎ NHẤT — tức là vừa khít nhất, tránh lãng phí
/// không gian kho. Độ phức tạp O(n) với n = số StorageBin còn trống.
/// </summary>
public class WarehouseSlottingService : IWarehouseSlottingService
{
    private readonly IApplicationDbContext _db;

    public WarehouseSlottingService(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<SuggestBinResponse> SuggestBinAsync(int warehouseId, SuggestBinRequest request)
    {
        var cargo = await _db.CargoLines.FindAsync(request.CargoLineId);
        if (cargo is null)
            return new SuggestBinResponse(false, null, null, null, null, "Không tìm thấy dòng hàng (CargoLine).");

        // Bước 1: Xác định khu vực phù hợp theo tính chất hàng hóa
        var requiredZoneType = cargo.IsHazmat ? ZoneType.Hazmat
                              : cargo.IsFragile ? ZoneType.Fragile
                              : ZoneType.Normal;

        // Bước 2: Lọc các StorageBin còn đủ chỗ, đúng Zone, thuộc đúng Warehouse
        var candidateBins = await _db.StorageBins
            .Include(b => b.WarehouseZone)
            .Where(b => b.WarehouseZone.WarehouseId == warehouseId
                     && b.WarehouseZone.ZoneType == requiredZoneType
                     && (b.CapacityM3 - b.CurrentUsedM3) >= request.QtyToStore)
            .ToListAsync();

        if (!candidateBins.Any())
        {
            return new SuggestBinResponse(false, null, null, null, null,
                $"Không còn vị trí trống phù hợp trong khu vực '{requiredZoneType}' của kho này.");
        }

        // Bước 3: BEST-FIT — chọn Bin có khoảng trống dư ra SAU KHI xếp là NHỎ NHẤT
        var best = candidateBins
            .Select(b => new { Bin = b, RemainingAfter = (b.CapacityM3 - b.CurrentUsedM3) - request.QtyToStore })
            .OrderBy(x => x.RemainingAfter)
            .First();

        return new SuggestBinResponse(
            Found: true,
            StorageBinId: best.Bin.Id,
            BinCode: best.Bin.Code,
            ZoneName: best.Bin.WarehouseZone.Name,
            RemainingAfterM3: best.RemainingAfter,
            Message: null
        );
    }

    public async Task<List<WarehouseZoneDto>> GetZonesAsync(int warehouseId)
    {
        return await _db.WarehouseZones
            .Where(z => z.WarehouseId == warehouseId)
            .Select(z => new WarehouseZoneDto(z.Id, z.Name, z.ZoneType.ToString(), z.StorageBins.Count))
            .ToListAsync();
    }

    public async Task<List<StorageBinDto>> GetBinsAsync(int zoneId)
    {
        return await _db.StorageBins
            .Where(b => b.WarehouseZoneId == zoneId)
            .Select(b => new StorageBinDto(b.Id, b.Code, b.CapacityM3, b.CurrentUsedM3, b.MaxWeightKg, b.Row, b.Column))
            .ToListAsync();
    }

    public async Task<List<WarehouseListItemDto>> GetWarehousesAsync()
    {
        return await _db.Warehouses
            .Select(w => new WarehouseListItemDto(w.Id, w.Name, w.Address, w.CapacityM3))
            .ToListAsync();
    }
}