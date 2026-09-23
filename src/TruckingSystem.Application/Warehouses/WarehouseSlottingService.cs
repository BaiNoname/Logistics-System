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
        var cargo = await _db.CargoLines.Include(c => c.TransportOrder).FirstOrDefaultAsync(c => c.Id == request.CargoLineId);
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

        // Bước 3: TỐI ƯU THEO ĐỘ KHẨN CẤP (Turnover-based Slotting) + Best-Fit —
        // Đơn hàng SẮP tới hạn giao (trong vòng 3 ngày) → ưu tiên GẦN CỬA
        // (DistanceFromDoorMeters nhỏ nhất) trước, Best-Fit chỉ là tiêu chí phụ.
        // Đơn hàng CHƯA gấp (hoặc không có hẹn giao) → vẫn ưu tiên Best-Fit như
        // cũ (khoảng trống dư ra nhỏ nhất), nhưng nếu 2 vị trí dư ra bằng nhau
        // thì cố tình chọn vị trí XA CỬA hơn — để dành chỗ gần cửa cho đơn nào
        // sau này thật sự gấp.
        var committedDate = cargo.TransportOrder.CommittedDeliveryDate;
        var isUrgent = committedDate.HasValue && (committedDate.Value - DateTime.UtcNow).TotalDays <= 3;

        var scored = candidateBins.Select(b => new
        {
            Bin = b,
            RemainingAfter = (b.CapacityM3 - b.CurrentUsedM3) - request.QtyToStore,
        });

        var best = isUrgent
            ? scored.OrderBy(x => x.Bin.DistanceFromDoorMeters).ThenBy(x => x.RemainingAfter).First()
            : scored.OrderBy(x => x.RemainingAfter).ThenByDescending(x => x.Bin.DistanceFromDoorMeters).First();

        var urgencyNote = isUrgent ? " (ưu tiên GẦN CỬA vì đơn sắp tới hạn giao)" : "";

        return new SuggestBinResponse(
            Found: true,
            StorageBinId: best.Bin.Id,
            BinCode: best.Bin.Code,
            ZoneName: best.Bin.WarehouseZone.Name,
            RemainingAfterM3: best.RemainingAfter,
            Message: isUrgent ? $"Đã chọn vị trí{urgencyNote}." : null
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
        var bins = await _db.StorageBins
            .Include(b => b.Assignments.Where(a => a.QtyStored > 0))
                .ThenInclude(a => a.GoodsReceipt).ThenInclude(r => r.TransportOrder).ThenInclude(o => o.Customer)
            .Include(b => b.Assignments)
                .ThenInclude(a => a.GoodsReceipt).ThenInclude(r => r.CargoLine)
            .Where(b => b.WarehouseZoneId == zoneId)
            .ToListAsync();

        return bins.Select(b => new StorageBinDto(
            b.Id, b.Code, b.CapacityM3, b.CurrentUsedM3, b.MaxWeightKg, b.PosX, b.PosY, b.WidthMeters, b.HeightMeters, b.DistanceFromDoorMeters,
            b.Assignments.Where(a => a.QtyStored > 0).Select(a => new BinContentItemDto(
                a.GoodsReceipt.TransportOrder.Code,
                a.GoodsReceipt.TransportOrder.Customer.Name,
                a.GoodsReceipt.CargoLine.CargoType,
                a.QtyStored,
                a.GoodsReceipt.ReceivedAt)).ToList()
        )).ToList();
    }

    public async Task<List<WarehouseListItemDto>> GetWarehousesAsync()
    {
        return await _db.Warehouses
            .Select(w => new WarehouseListItemDto(w.Id, w.Name, w.Address, w.CapacityM3, w.LengthMeters, w.WidthMeters, w.Lat, w.Lng))
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
            Lat = request.Lat,
            Lng = request.Lng,
        };
        _db.Warehouses.Add(warehouse);
        await _db.SaveChangesAsync();

        return new WarehouseListItemDto(warehouse.Id, warehouse.Name, warehouse.Address, warehouse.CapacityM3, warehouse.LengthMeters, warehouse.WidthMeters, warehouse.Lat, warehouse.Lng);
    }

    public async Task<WarehouseActionResult> UpdateWarehouseAsync(int warehouseId, UpdateWarehouseRequest request)
    {
        var warehouse = await _db.Warehouses.FindAsync(warehouseId);
        if (warehouse is null) return new WarehouseActionResult(false, "Không tìm thấy kho.");

        warehouse.Name = request.Name;
        warehouse.Address = request.Address;
        warehouse.CapacityM3 = request.CapacityM3;
        warehouse.LengthMeters = request.LengthMeters;
        warehouse.WidthMeters = request.WidthMeters;
        warehouse.Lat = request.Lat;
        warehouse.Lng = request.Lng;

        await _db.SaveChangesAsync();
        return new WarehouseActionResult(true, null);
    }

    public async Task<WarehouseZoneDto> CreateZoneAsync(int warehouseId, CreateZoneRequest request)
    {
        var warehouseForBounds = await _db.Warehouses.FindAsync(warehouseId);

        // CHẶN khu vực v nằm NGOÀI ranh giới kho — trước đây chỉ kiểm tra
        // chồng lấn với khu KHÁC, chưa kiểm tra có vượt quá kích thước THẬT
        // của kho hay không (VD: kho rộng 30m nhưng tạo khu ở PosX=40 vẫn cho
        // qua) — chỉ áp dụng khi kho ĐÃ khai báo kích thước thật (nullable).
        if (warehouseForBounds?.LengthMeters is decimal whLength && warehouseForBounds.WidthMeters is decimal whWidth)
        {
            if (request.PosX < 0 || request.PosY < 0 || request.PosX + request.WidthMeters > whLength || request.PosY + request.HeightMeters > whWidth)
                throw new InvalidOperationException($"Khu vực này vượt ra NGOÀI ranh giới kho (kho rộng {whLength}m x {whWidth}m) — chỉnh lại vị trí/kích thước cho vừa trong kho.");
        }

        // CHẶN chồng lấn vị trí với khu vực KHÁC đã có trong cùng kho — trước
        // đây không kiểm tra gì cả, chỉ có 1 dòng CHỮ GỢI Ý ở giao diện nhắc
        // người dùng tự tính toán, không hề có kiểm tra thật — 2 khu vực có
        // thể đè hẳn lên nhau trên sơ đồ mà hệ thống vẫn cho tạo bình thường.
        var existingZones = await _db.WarehouseZones.Where(z => z.WarehouseId == warehouseId).ToListAsync();
        var overlapping = existingZones.FirstOrDefault(z => RectanglesOverlap(
            request.PosX, request.PosY, request.WidthMeters, request.HeightMeters,
            z.PosX, z.PosY, z.WidthMeters, z.HeightMeters));
        if (overlapping is not null)
            throw new InvalidOperationException($"Vị trí/kích thước này chồng lấn với khu vực \"{overlapping.Name}\" đã có (X={overlapping.PosX}, Y={overlapping.PosY}, Rộng={overlapping.WidthMeters}, Cao={overlapping.HeightMeters}) — chọn tọa độ khác.");

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

    public async Task<WarehouseActionResult> UpdateZoneAsync(int zoneId, CreateZoneRequest request)
    {
        var zone = await _db.WarehouseZones.FindAsync(zoneId);
        if (zone is null) return new WarehouseActionResult(false, "Không tìm thấy khu vực.");

        var warehouseForBounds = await _db.Warehouses.FindAsync(zone.WarehouseId);
        if (warehouseForBounds?.LengthMeters is decimal whLength && warehouseForBounds.WidthMeters is decimal whWidth)
        {
            if (request.PosX < 0 || request.PosY < 0 || request.PosX + request.WidthMeters > whLength || request.PosY + request.HeightMeters > whWidth)
                return new WarehouseActionResult(false, $"Khu vực này vượt ra NGOÀI ranh giới kho (kho rộng {whLength}m x {whWidth}m) — chỉnh lại vị trí/kích thước cho vừa trong kho.");
        }

        var otherZones = await _db.WarehouseZones.Where(z => z.WarehouseId == zone.WarehouseId && z.Id != zoneId).ToListAsync();
        var overlapping = otherZones.FirstOrDefault(z => RectanglesOverlap(
            request.PosX, request.PosY, request.WidthMeters, request.HeightMeters,
            z.PosX, z.PosY, z.WidthMeters, z.HeightMeters));
        if (overlapping is not null)
            return new WarehouseActionResult(false, $"Vị trí/kích thước này chồng lấn với khu vực \"{overlapping.Name}\" đã có — chọn tọa độ khác.");

        var zoneType = Enum.TryParse<ZoneType>(request.ZoneType, out var parsed) ? parsed : ZoneType.Normal;

        zone.Name = request.Name;
        zone.ZoneType = zoneType;
        zone.PosX = request.PosX;
        zone.PosY = request.PosY;
        zone.WidthMeters = request.WidthMeters;
        zone.HeightMeters = request.HeightMeters;

        await _db.SaveChangesAsync();
        return new WarehouseActionResult(true, null);
    }

    public async Task<StorageBinDto> CreateBinAsync(int zoneId, CreateBinRequest request)
    {
        var zone = await _db.WarehouseZones.FindAsync(zoneId);
        if (zone is null) throw new InvalidOperationException("Không tìm thấy khu vực.");

        // CHẶN vị trí vượt ra NGOÀI ranh giới khu vực chứa nó — đúng thực tế
        // vật lý: 1 vị trí lưu trữ không thể to hơn hoặc tràn ra khỏi khu vực.
        if (request.PosX < 0 || request.PosY < 0 || request.PosX + request.WidthMeters > zone.WidthMeters || request.PosY + request.HeightMeters > zone.HeightMeters)
            throw new InvalidOperationException($"Vị trí này vượt ra NGOÀI ranh giới khu vực \"{zone.Name}\" (khu vực rộng {zone.WidthMeters}m x {zone.HeightMeters}m) — chỉnh lại tọa độ/kích thước cho vừa trong khu vực.");

        // CHẶN đè lên vị trí KHÁC đã có trong cùng khu vực — trước đây chỉ so
        // Row/Column (chỉ số trừu tượng), giờ so ĐÚNG diện tích thật, phát
        // hiện đè dù chỉ chồng lấn 1 phần nhỏ.
        var existingBins = await _db.StorageBins.Where(b => b.WarehouseZoneId == zoneId).ToListAsync();
        var overlappingBin = existingBins.FirstOrDefault(b => RectanglesOverlap(
            request.PosX, request.PosY, request.WidthMeters, request.HeightMeters,
            b.PosX, b.PosY, b.WidthMeters, b.HeightMeters));
        if (overlappingBin is not null)
            throw new InvalidOperationException($"Vị trí này đè lên vị trí \"{overlappingBin.Code}\" đã có (X={overlappingBin.PosX}, Y={overlappingBin.PosY}, Rộng={overlappingBin.WidthMeters}, Cao={overlappingBin.HeightMeters}) — chọn tọa độ khác.");

        var bin = new StorageBin
        {
            WarehouseZoneId = zoneId,
            Code = request.Code,
            CapacityM3 = request.CapacityM3,
            MaxWeightKg = request.MaxWeightKg,
            PosX = request.PosX,
            PosY = request.PosY,
            WidthMeters = request.WidthMeters,
            HeightMeters = request.HeightMeters,
            DistanceFromDoorMeters = request.DistanceFromDoorMeters,
            CurrentUsedM3 = 0,
        };
        _db.StorageBins.Add(bin);
        await _db.SaveChangesAsync();

        return new StorageBinDto(bin.Id, bin.Code, bin.CapacityM3, bin.CurrentUsedM3, bin.MaxWeightKg, bin.PosX, bin.PosY, bin.WidthMeters, bin.HeightMeters, bin.DistanceFromDoorMeters, new List<BinContentItemDto>());
    }

    public async Task<WarehouseActionResult> UpdateBinAsync(int binId, CreateBinRequest request)
    {
        var bin = await _db.StorageBins.Include(b => b.WarehouseZone).FirstOrDefaultAsync(b => b.Id == binId);
        if (bin is null) return new WarehouseActionResult(false, "Không tìm thấy vị trí.");

        if (request.CapacityM3 < bin.CurrentUsedM3)
            return new WarehouseActionResult(false, $"Sức chứa mới ({request.CapacityM3} m³) nhỏ hơn lượng hàng đang chứa ({bin.CurrentUsedM3} m³) — không thể sửa.");

        var zone = bin.WarehouseZone;
        if (request.PosX < 0 || request.PosY < 0 || request.PosX + request.WidthMeters > zone.WidthMeters || request.PosY + request.HeightMeters > zone.HeightMeters)
            return new WarehouseActionResult(false, $"Vị trí này vượt ra NGOÀI ranh giới khu vực \"{zone.Name}\" (khu vực rộng {zone.WidthMeters}m x {zone.HeightMeters}m) — chỉnh lại tọa độ/kích thước cho vừa trong khu vực.");

        var otherBins = await _db.StorageBins.Where(b => b.Id != binId && b.WarehouseZoneId == bin.WarehouseZoneId).ToListAsync();
        var overlappingBin = otherBins.FirstOrDefault(b => RectanglesOverlap(
            request.PosX, request.PosY, request.WidthMeters, request.HeightMeters,
            b.PosX, b.PosY, b.WidthMeters, b.HeightMeters));
        if (overlappingBin is not null)
            return new WarehouseActionResult(false, $"Vị trí này đè lên vị trí \"{overlappingBin.Code}\" đã có — chọn tọa độ khác.");

        bin.Code = request.Code;
        bin.CapacityM3 = request.CapacityM3;
        bin.MaxWeightKg = request.MaxWeightKg;
        bin.PosX = request.PosX;
        bin.PosY = request.PosY;
        bin.WidthMeters = request.WidthMeters;
        bin.HeightMeters = request.HeightMeters;
        bin.DistanceFromDoorMeters = request.DistanceFromDoorMeters;

        await _db.SaveChangesAsync();
        return new WarehouseActionResult(true, null);
    }

    public async Task<WarehouseActionResult> DeleteWarehouseAsync(int warehouseId)
    {
        var hasStock = await _db.WarehouseStocks.AnyAsync(s => s.WarehouseId == warehouseId && s.QtyOnHand > 0);
        if (hasStock)
            return new WarehouseActionResult(false, "Kho này đang có tồn kho thật — không thể xóa. Cần xuất hết hàng trước.");

        // CHẶN xóa nếu có Trip (chuyến) đang tham chiếu tới kho này — trước
        // đây không kiểm tra, xóa xong sẽ để lại Trip.WarehouseId "treo" trỏ
        // tới 1 kho không còn tồn tại, gây lỗi khi tải lại màn Chuyến/lộ trình
        // sau này (NullReferenceException khi cố đọc Trip.Warehouse.Lat...).
        var hasTrips = await _db.Trips.AnyAsync(t => t.WarehouseId == warehouseId);
        if (hasTrips)
            return new WarehouseActionResult(false, "Kho này đang được 1 hoặc nhiều Chuyến (Trip) tham chiếu tới — không thể xóa. Chỉ xóa được kho chưa từng dùng trong bất kỳ chuyến nào.");

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
        decimal qty, int receivedByEmployeeId, int? tripId, int? chosenBinId = null)
    {
        // CHẶN nhập khống — không cho nhập vượt quá số lượng ĐÃ KHAI BÁO trong
        // đơn vận chuyển (CargoLine.QtyTotal), tính cả những lần đã nhập trước
        // đó cộng dồn lại. VD: đơn khai 1, đã nhập 1 rồi thì không cho nhập
        // thêm 3 nữa — dù là tự động (qua Trip) hay nhập tay đều bị chặn như nhau.
        var cargoLine = await _db.CargoLines.FindAsync(cargoLineId);
        if (cargoLine is null)
            return new ReceiveGoodsResult(false, "Không tìm thấy dòng hàng (CargoLine).", null, null);

        var alreadyReceived = await _db.GoodsReceipts
            .Where(r => r.CargoLineId == cargoLineId)
            .SumAsync(r => r.QtyReceived);

        if (alreadyReceived + qty > cargoLine.QtyTotal)
        {
            var remaining = cargoLine.QtyTotal - alreadyReceived;
            return new ReceiveGoodsResult(false,
                $"Vượt quá số lượng đã khai báo trong đơn — dòng hàng \"{cargoLine.CargoType}\" chỉ khai {cargoLine.QtyTotal}, đã nhập {alreadyReceived}, chỉ còn được nhập tối đa {remaining} nữa.",
                null, null);
        }

        // XÁC ĐỊNH + KIỂM TRA VỊ TRÍ ĐÍCH TRƯỚC — KHÔNG tạo GoodsReceipt/cộng
        // tồn kho trước khi chắc chắn có 1 vị trí ĐÚNG KHU VỰC (theo tính chất
        // hàng: thường/dễ vỡ/nguy hiểm) và ĐỦ CHỖ. Trước đây làm ngược lại (ghi
        // nhận tồn kho trước, tìm vị trí sau) khiến hàng "biến mất" khỏi sơ đồ
        // nếu không tìm được chỗ — số liệu tồn kho tổng có nhưng không thấy ở
        // đâu trên sơ đồ trực quan. Giờ: KHÔNG có vị trí hợp lệ = KHÔNG cho
        // nhập luôn, chặn từ đầu, không để lại "hàng ma".
        var requiredZoneType = cargoLine.IsHazmat ? ZoneType.Hazmat : cargoLine.IsFragile ? ZoneType.Fragile : ZoneType.Normal;
        StorageBin targetBin;

        if (chosenBinId.HasValue)
        {
            // NGƯỜI DÙNG TỰ CHỌN VỊ TRÍ — vẫn phải kiểm tra ĐÚNG KHU VỰC, không
            // được nhét hàng nguy hiểm vào khu thường chỉ vì người dùng tự chọn tay.
            var chosenBin = await _db.StorageBins.Include(b => b.WarehouseZone).FirstOrDefaultAsync(b => b.Id == chosenBinId.Value);
            if (chosenBin is null)
                return new ReceiveGoodsResult(false, "Vị trí đã chọn không tồn tại.", null, null);

            if (chosenBin.WarehouseZone.ZoneType != requiredZoneType)
                return new ReceiveGoodsResult(false,
                    $"Vị trí {chosenBin.Code} thuộc khu '{TranslateZoneType(chosenBin.WarehouseZone.ZoneType)}', không phù hợp với hàng hóa loại '{TranslateZoneType(requiredZoneType)}' — cần chọn đúng vị trí trong khu vực tương ứng.",
                    null, null);

            if (chosenBin.CapacityM3 - chosenBin.CurrentUsedM3 < qty)
                return new ReceiveGoodsResult(false,
                    $"Vị trí {chosenBin.Code} không đủ chỗ trống ({chosenBin.CapacityM3 - chosenBin.CurrentUsedM3:0.00} m³ còn lại, cần {qty} m³).",
                    null, null);

            targetBin = chosenBin;
        }
        else
        {
            // Chạy Best-Fit tự động chọn vị trí — nếu KHÔNG tìm được vị trí phù
            // hợp (VD: chưa từng tạo khu Hazmat) -> CHẶN LUÔN, không cho nhập,
            // báo rõ lý do để Dispatcher đi tạo đúng khu vực trước.
            var suggestion = await SuggestBinAsync(warehouseId, new SuggestBinRequest(cargoLineId, qty));
            if (!suggestion.Found)
                return new ReceiveGoodsResult(false,
                    $"Không thể nhập kho — kho này CHƯA CÓ khu vực '{TranslateZoneType(requiredZoneType)}' phù hợp (hoặc đã hết chỗ trống). Cần tạo khu vực đúng loại trước khi nhập loại hàng này. Chi tiết: {suggestion.Message}",
                    null, null);

            targetBin = (await _db.StorageBins.FindAsync(suggestion.StorageBinId!.Value))!;
        }

        // Đến đây CHẮC CHẮN đã có targetBin hợp lệ, đúng khu, đủ chỗ — giờ mới
        // thật sự ghi nhận: tạo GoodsReceipt, cộng tồn kho, gán vào vị trí.
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

        var stock = await _db.WarehouseStocks.FirstOrDefaultAsync(s => s.WarehouseId == warehouseId && s.CargoLineId == cargoLineId);
        if (stock is null)
        {
            stock = new WarehouseStock { WarehouseId = warehouseId, CargoLineId = cargoLineId, QtyOnHand = 0 };
            _db.WarehouseStocks.Add(stock);
        }
        stock.QtyOnHand += qty;
        stock.LastUpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(); // cần Id thật của GoodsReceipt trước khi tạo StorageAssignment

        targetBin.CurrentUsedM3 += qty;
        _db.StorageAssignments.Add(new StorageAssignment
        {
            GoodsReceiptId = receipt.Id,
            StorageBinId = targetBin.Id,
            QtyStored = qty,
        });
        await _db.SaveChangesAsync();

        return new ReceiveGoodsResult(true, null, targetBin.Id, targetBin.Code);
    }

    private static string TranslateZoneType(ZoneType z) => z switch
    {
        ZoneType.Hazmat => "Hàng nguy hiểm",
        ZoneType.Fragile => "Hàng dễ vỡ",
        _ => "Hàng thường",
    };

    /// <summary>Kiểm tra 2 hình chữ nhật (khu vực trên sơ đồ) có chồng lấn
    /// nhau không — dùng thuật toán chuẩn: KHÔNG chồng lấn khi 1 trong 4 điều
    /// kiện đúng (nằm hẳn bên trái/phải/trên/dưới nhau); ngược lại là có
    /// chồng lấn dù chỉ 1 phần nhỏ.</summary>
    private static bool RectanglesOverlap(decimal x1, decimal y1, decimal w1, decimal h1, decimal x2, decimal y2, decimal w2, decimal h2)
    {
        bool noOverlap = x1 + w1 <= x2 || x2 + w2 <= x1 || y1 + h1 <= y2 || y2 + h2 <= y1;
        return !noOverlap;
    }

    public async Task<IssueGoodsResult> IssueGoodsAsync(int warehouseId, int transportOrderId, int cargoLineId,
        decimal qty, int issuedByEmployeeId, int? tripId)
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

        await _db.SaveChangesAsync();
        return new IssueGoodsResult(true, null);
    }

    public async Task<List<StorageBinDto>> GetAvailableBinsForCargoAsync(int warehouseId, int cargoLineId)
    {
        var cargo = await _db.CargoLines.FindAsync(cargoLineId);
        if (cargo is null) return new List<StorageBinDto>();

        var requiredZoneType = cargo.IsHazmat ? ZoneType.Hazmat
                              : cargo.IsFragile ? ZoneType.Fragile
                              : ZoneType.Normal;

        return await _db.StorageBins
            .Include(b => b.WarehouseZone)
            .Where(b => b.WarehouseZone.WarehouseId == warehouseId && b.WarehouseZone.ZoneType == requiredZoneType)
            .OrderBy(b => b.CurrentUsedM3) // ô còn trống nhiều nhất lên đầu, dễ chọn
            .Select(b => new StorageBinDto(b.Id, b.Code, b.CapacityM3, b.CurrentUsedM3, b.MaxWeightKg, b.PosX, b.PosY, b.WidthMeters, b.HeightMeters, b.DistanceFromDoorMeters, new List<BinContentItemDto>()))
            .ToListAsync();
    }

    public async Task<List<CargoLineOptionDto>> GetCargoLinesForOrderAsync(int transportOrderId)
    {
        return await _db.CargoLines
            .Include(c => c.TransportOrder)
            .Where(c => c.TransportOrderId == transportOrderId)
            .Select(c => new CargoLineOptionDto(c.Id, c.CargoType, c.QtyTotal, c.TransportOrder.Code))
            .ToListAsync();
    }

    public async Task<List<WarehouseTransactionDto>> GetTransactionHistoryAsync(int warehouseId)
    {
        var receipts = await _db.GoodsReceipts
            .Include(r => r.TransportOrder).Include(r => r.CargoLine).Include(r => r.ReceivedByEmployee)
            .Where(r => r.WarehouseId == warehouseId)
            .Select(r => new WarehouseTransactionDto("Nhập", r.ReceivedAt, r.TransportOrder.Code, r.CargoLine.CargoType, r.QtyReceived, r.ReceivedByEmployee.FullName))
            .ToListAsync();

        var issues = await _db.GoodsIssues
            .Include(i => i.TransportOrder).Include(i => i.CargoLine).Include(i => i.IssuedByEmployee)
            .Where(i => i.WarehouseId == warehouseId)
            .Select(i => new WarehouseTransactionDto("Xuất", i.IssuedAt, i.TransportOrder.Code, i.CargoLine.CargoType, i.QtyIssued, i.IssuedByEmployee.FullName))
            .ToListAsync();

        return receipts.Concat(issues).OrderByDescending(t => t.OccurredAt).ToList();
    }
}