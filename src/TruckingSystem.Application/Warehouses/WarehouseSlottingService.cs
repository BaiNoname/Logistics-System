using Microsoft.EntityFrameworkCore;
using TruckingSystem.Application.Common.Interfaces;
using TruckingSystem.Domain.Entities;
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
            .Select(z => new WarehouseZoneDto(z.Id, z.Name, z.ZoneType.ToString(), z.StorageBins.Count, z.PosX, z.PosY, z.WidthMeters, z.HeightMeters))
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
            .Select(w => new WarehouseListItemDto(w.Id, w.Name, w.Address, w.CapacityM3, w.LengthMeters, w.WidthMeters))
            .ToListAsync();
    }

    public async Task<WarehouseListItemDto> CreateWarehouseAsync(CreateWarehouseRequest request)
    {
        var warehouse = new Warehouse
        {
            Name = request.Name,
            Address = request.Address,
            CapacityM3 = request.CapacityM3,
            LengthMeters = request.LengthMeters,
            WidthMeters = request.WidthMeters,
        };
        _db.Warehouses.Add(warehouse);
        await _db.SaveChangesAsync();

        return new WarehouseListItemDto(warehouse.Id, warehouse.Name, warehouse.Address, warehouse.CapacityM3, warehouse.LengthMeters, warehouse.WidthMeters);
    }

    public async Task<WarehouseZoneDto> CreateZoneAsync(int warehouseId, CreateZoneRequest request)
    {
        var zoneType = Enum.TryParse<ZoneType>(request.ZoneType, out var parsed) ? parsed : ZoneType.Normal;

        var zone = new WarehouseZone
        {
            WarehouseId = warehouseId,
            Name = request.Name,
            ZoneType = zoneType,
            PosX = request.PosX,
            PosY = request.PosY,
            WidthMeters = request.WidthMeters,
            HeightMeters = request.HeightMeters,
        };
        _db.WarehouseZones.Add(zone);
        await _db.SaveChangesAsync();

        return new WarehouseZoneDto(zone.Id, zone.Name, zone.ZoneType.ToString(), 0, zone.PosX, zone.PosY, zone.WidthMeters, zone.HeightMeters);
    }

    public async Task<StorageBinDto> CreateBinAsync(int zoneId, CreateBinRequest request)
    {
        var bin = new StorageBin
        {
            WarehouseZoneId = zoneId,
            Code = request.Code,
            CapacityM3 = request.CapacityM3,
            MaxWeightKg = request.MaxWeightKg,
            Row = request.Row,
            Column = request.Column,
            CurrentUsedM3 = 0,
        };
        _db.StorageBins.Add(bin);
        await _db.SaveChangesAsync();

        return new StorageBinDto(bin.Id, bin.Code, bin.CapacityM3, bin.CurrentUsedM3, bin.MaxWeightKg, bin.Row, bin.Column);
    }

    public async Task<WarehouseActionResult> DeleteWarehouseAsync(int warehouseId)
    {
        var hasStock = await _db.WarehouseStocks.AnyAsync(s => s.WarehouseId == warehouseId && s.QtyOnHand > 0);
        if (hasStock)
            return new WarehouseActionResult(false, "Kho này đang có tồn kho thật — không thể xóa. Cần xuất hết hàng trước.");

        var zones = await _db.WarehouseZones.Include(z => z.StorageBins).Where(z => z.WarehouseId == warehouseId).ToListAsync();
        foreach (var zone in zones)
        {
            _db.StorageBins.RemoveRange(zone.StorageBins);
        }
        _db.WarehouseZones.RemoveRange(zones);

        var warehouse = await _db.Warehouses.FindAsync(warehouseId);
        if (warehouse is null) return new WarehouseActionResult(false, "Không tìm thấy kho.");
        _db.Warehouses.Remove(warehouse);

        await _db.SaveChangesAsync();
        return new WarehouseActionResult(true, null);
    }

    public async Task<WarehouseActionResult> DeleteZoneAsync(int zoneId)
    {
        var zone = await _db.WarehouseZones.Include(z => z.StorageBins).FirstOrDefaultAsync(z => z.Id == zoneId);
        if (zone is null) return new WarehouseActionResult(false, "Không tìm thấy khu vực.");

        if (zone.StorageBins.Any(b => b.CurrentUsedM3 > 0))
            return new WarehouseActionResult(false, "Khu vực này còn vị trí đang chứa hàng — không thể xóa. Cần xuất hết hàng trước.");

        _db.StorageBins.RemoveRange(zone.StorageBins);
        _db.WarehouseZones.Remove(zone);

        await _db.SaveChangesAsync();
        return new WarehouseActionResult(true, null);
    }

    public async Task<WarehouseActionResult> DeleteBinAsync(int binId)
    {
        var bin = await _db.StorageBins.FindAsync(binId);
        if (bin is null) return new WarehouseActionResult(false, "Không tìm thấy vị trí.");

        if (bin.CurrentUsedM3 > 0)
            return new WarehouseActionResult(false, "Vị trí này đang chứa hàng — không thể xóa. Cần xuất hết hàng trước.");

        _db.StorageBins.Remove(bin);
        await _db.SaveChangesAsync();
        return new WarehouseActionResult(true, null);
    }

    public async Task<ReceiveGoodsResult> ReceiveGoodsAsync(int warehouseId, int transportOrderId, int cargoLineId,
        decimal qty, int receivedByEmployeeId, int? tripId)
    {
        var receipt = new GoodsReceipt
        {
            TransportOrderId = transportOrderId,
            CargoLineId = cargoLineId,
            WarehouseId = warehouseId,
            TripId = tripId,
            ReceivedByEmployeeId = receivedByEmployeeId,
            QtyReceived = qty,
        };
        _db.GoodsReceipts.Add(receipt);

        // Cộng dồn tồn kho tổng hợp (Warehouse, CargoLine) — tạo mới nếu chưa có dòng nào
        var stock = await _db.WarehouseStocks.FirstOrDefaultAsync(s => s.WarehouseId == warehouseId && s.CargoLineId == cargoLineId);
        if (stock is null)
        {
            stock = new WarehouseStock { WarehouseId = warehouseId, CargoLineId = cargoLineId, QtyOnHand = 0 };
            _db.WarehouseStocks.Add(stock);
        }
        stock.QtyOnHand += qty;
        stock.LastUpdatedAt = DateTime.UtcNow;

        // Chạy Best-Fit tự động chọn vị trí — nếu kho hết chỗ phù hợp, vẫn ghi
        // nhận hàng đã về (không chặn nghiệp vụ), chỉ báo thiếu chỗ xếp.
        var suggestion = await SuggestBinAsync(warehouseId, new SuggestBinRequest(cargoLineId, qty));

        if (!suggestion.Found)
        {
            await _db.SaveChangesAsync(); // vẫn lưu GoodsReceipt + WarehouseStock dù chưa xếp được vị trí
            return new ReceiveGoodsResult(true, "⚠️ Hàng đã ghi nhận về kho, nhưng KHÔNG tìm được vị trí trống phù hợp — cần xếp thủ công sau: " + suggestion.Message, null, null);
        }

        await _db.SaveChangesAsync(); // cần Id thật của GoodsReceipt trước khi tạo StorageAssignment

        var bin = await _db.StorageBins.FindAsync(suggestion.StorageBinId!.Value);
        bin!.CurrentUsedM3 += qty;

        _db.StorageAssignments.Add(new StorageAssignment
        {
            GoodsReceiptId = receipt.Id,
            StorageBinId = bin.Id,
            QtyStored = qty,
        });

        await _db.SaveChangesAsync();
        return new ReceiveGoodsResult(true, null, bin.Id, suggestion.BinCode);
    }

    public async Task<IssueGoodsResult> IssueGoodsAsync(int warehouseId, int transportOrderId, int cargoLineId,
        decimal qty, int issuedByEmployeeId, int tripId)
    {
        var stock = await _db.WarehouseStocks.FirstOrDefaultAsync(s => s.WarehouseId == warehouseId && s.CargoLineId == cargoLineId);
        if (stock is null || stock.QtyOnHand < qty)
            return new IssueGoodsResult(false, $"Không đủ tồn kho để xuất — hiện có {stock?.QtyOnHand ?? 0}, cần xuất {qty}.");

        _db.GoodsIssues.Add(new GoodsIssue
        {
            TransportOrderId = transportOrderId,
            CargoLineId = cargoLineId,
            WarehouseId = warehouseId,
            TripId = tripId,
            IssuedByEmployeeId = issuedByEmployeeId,
            QtyIssued = qty,
        });

        stock.QtyOnHand -= qty;
        stock.LastUpdatedAt = DateTime.UtcNow;

        // Trừ dần StorageAssignment theo FIFO (lô nhập trước xuất trước) —
        // giải phóng lại đúng khoảng trống trong StorageBin tương ứng.
        var assignments = await _db.StorageAssignments
            .Include(a => a.GoodsReceipt)
            .Include(a => a.StorageBin)
            .Where(a => a.GoodsReceipt.WarehouseId == warehouseId && a.GoodsReceipt.CargoLineId == cargoLineId && a.QtyStored > 0)
            .OrderBy(a => a.AssignedAt)
            .ToListAsync();

        var remainingToIssue = qty;
        foreach (var assignment in assignments)
        {
            if (remainingToIssue <= 0) break;
            var take = Math.Min(remainingToIssue, assignment.QtyStored);

            assignment.QtyStored -= take;
            assignment.StorageBin.CurrentUsedM3 -= take;
            remainingToIssue -= take;

            if (assignment.QtyStored <= 0) _db.StorageAssignments.Remove(assignment);
        }
        // Nếu remainingToIssue > 0 sau vòng lặp (dữ liệu StorageAssignment bị
        // lệch so với WarehouseStock vì lý do nào đó) — vẫn cho xuất kho bình
        // thường (ưu tiên không chặn nghiệp vụ), chỉ là không giải phóng được
        // hết StorageBin tương ứng. Trường hợp hiếm, chấp nhận được.

        await _db.SaveChangesAsync();
        return new IssueGoodsResult(true, null);
    }
}