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
            return new SuggestBinResponse(false, new List<SuggestedAllocationDto>(), "Không tìm thấy dòng hàng (CargoLine).");

        // Bước 1: Xác định khu vực phù hợp theo tính chất hàng hóa
        var requiredZoneType = cargo.IsHazmat ? ZoneType.Hazmat
                              : cargo.IsFragile ? ZoneType.Fragile
                              : ZoneType.Normal;

        // QUY ĐỔI ĐƠN VỊ: QtyToStore là SỐ LƯỢNG (cái/thùng), còn ô kho đo bằng
        // m³ và kg. Trước đây cộng THẲNG số lượng vào m³ (100 thùng tổng 2 m³ bị
        // tính là chiếm 100 m³) — sai, khác hẳn cách Chia xe (Greedy) đã quy đổi
        // đúng: 1 đơn vị = tổng m³ (kg) của dòng hàng / tổng số lượng.
        var unitVolume = UnitVolumeM3(cargo);
        var unitWeight = UnitWeightKg(cargo);
        var neededM3 = request.QtyToStore * unitVolume;

        // Bước 2: Lấy các ô đúng khu, đúng kho — kèm hàng đang chứa để tính được
        // khối lượng hiện có (kiểm tra tải trọng tối đa của ô).
        var bins = await _db.StorageBins
            .Include(b => b.WarehouseZone)
            .Include(b => b.Assignments.Where(a => a.QtyStored > 0))
                .ThenInclude(a => a.GoodsReceipt).ThenInclude(r => r.CargoLine)
            .Where(b => b.WarehouseZone.WarehouseId == warehouseId && b.WarehouseZone.ZoneType == requiredZoneType)
            .ToListAsync();

        // Khoảng cách tới cửa của từng ô: TỰ TÍNH từ vị trí cửa kho + tọa độ khu
        // vực + tọa độ ô (không còn dùng con số nhập tay, dễ nhập khống/sai).
        var warehouse = await _db.Warehouses.FindAsync(warehouseId);
        var hasDoor = warehouse is not null && HasDoor(warehouse);
        var distanceOf = bins.ToDictionary(b => b.Id, b => hasDoor ? DoorDistanceMeters(warehouse!, b.WarehouseZone, b) ?? 0 : 0);
        var doorNote = hasDoor ? null
            : "⚠️ Kho chưa khai báo vị trí cửa — chưa ưu tiên gần/xa cửa được, đang xếp theo Best-Fit. Vào Sửa kho để khai báo cửa.";

        // Mỗi ô: còn bao nhiêu m³, và xếp thêm được TỐI ĐA bao nhiêu đơn vị hàng
        // này (giới hạn bởi CẢ thể tích lẫn tải trọng, cái nào chặt hơn tính cái đó).
        var candidates = bins
            .Select(b => new BinCandidate(b, b.CapacityM3 - b.CurrentUsedM3, MaxQtyFitting(b, unitVolume, unitWeight), distanceOf[b.Id]))
            .Where(x => x.MaxQty > 0)
            .ToList();

        if (!candidates.Any())
        {
            return new SuggestBinResponse(false, new List<SuggestedAllocationDto>(),
                $"Không còn vị trí trống nào (theo thể tích và tải trọng) trong khu vực '{TranslateZoneType(requiredZoneType)}' của kho này.");
        }

        // Bước 3: VÙNG GẦN CỬA = 1/3 số ô gần cửa nhất (trong đúng loại khu vực
        // này). Tính trên TẤT CẢ ô, kể cả ô đã đầy, để ranh giới vùng cố định,
        // không xê dịch theo mức lấp đầy. Vùng này DÀNH RIÊNG cho hàng gấp —
        // giống cách kho thực tế xếp hàng theo tốc độ luân chuyển (hàng ra nhanh
        // sát cửa xuất, hàng ra chậm vào sâu bên trong). Kho chưa khai báo cửa
        // thì không có vùng gần cửa (mọi ô coi như "bên trong").
        var nearDoorCount = (int)Math.Ceiling(bins.Count / 3.0);
        var nearDoorIds = hasDoor
            ? bins.OrderBy(b => distanceOf[b.Id]).Take(nearDoorCount).Select(b => b.Id).ToHashSet()
            : new HashSet<int>();

        var committedDate = cargo.TransportOrder.CommittedDeliveryDate;
        var isUrgent = committedDate.HasValue && (committedDate.Value - DateTime.UtcNow).TotalDays <= 3;
        var qty = request.QtyToStore;

        SuggestBinResponse Found(List<SuggestedAllocationDto> allocations, string? note)
        {
            var message = string.Join("\n", new[] { note, doorNote }.Where(m => !string.IsNullOrEmpty(m)));
            return new SuggestBinResponse(true, allocations, message.Length == 0 ? null : message);
        }

        if (isUrgent && hasDoor)
        {
            // HÀNG GẤP (hạn giao ≤ 3 ngày): ô GẦN CỬA nhất trước.
            var single = PickSingleFit(candidates, qty, neededM3, nearestFirst: true);
            if (single is not null)
                return Found(new List<SuggestedAllocationDto> { single }, "Đã chọn vị trí (ưu tiên GẦN CỬA vì đơn sắp tới hạn giao).");

            var split = FillInOrder(candidates.OrderBy(x => x.Distance), qty, unitVolume);
            if (split is not null) return Found(split, SplitNote(split));
        }
        else
        {
            // HÀNG KHÔNG GẤP (hoặc kho chưa khai báo cửa): ưu tiên VÙNG BÊN TRONG
            // (chừa chỗ gần cửa cho hàng gấp), trong vùng vẫn chọn ô vừa khít nhất
            // (Best-Fit) để kho không bị phân mảnh. CHỈ lấn sang vùng gần cửa khi
            // bên trong hết chỗ — không bao giờ báo "kho đầy" oan.
            var inner = candidates.Where(x => !nearDoorIds.Contains(x.Bin.Id)).ToList();
            var near = candidates.Where(x => nearDoorIds.Contains(x.Bin.Id)).ToList();
            const string fallbackNote = "⚠️ Vùng bên trong đã hết chỗ phù hợp — tạm xếp vào vùng gần cửa.";

            // 1) Vừa trọn 1 ô ở vùng bên trong
            var singleInner = PickSingleFit(inner, qty, neededM3, nearestFirst: false);
            if (singleInner is not null) return Found(new List<SuggestedAllocationDto> { singleInner }, null);

            // 2) Chia nhiều ô nhưng VẪN trong vùng bên trong (ô còn ít chỗ trước)
            var splitInner = FillInOrder(inner.OrderBy(x => x.FreeM3), qty, unitVolume);
            if (splitInner is not null) return Found(splitInner, SplitNote(splitInner));

            // 3) Vừa trọn 1 ô ở vùng gần cửa
            var singleNear = PickSingleFit(near, qty, neededM3, nearestFirst: false);
            if (singleNear is not null) return Found(new List<SuggestedAllocationDto> { singleNear }, fallbackNote);

            // 4) Chia gộp tất cả: hết vùng bên trong rồi mới tới gần cửa (xa cửa trước)
            var splitAll = FillInOrder(inner.OrderBy(x => x.FreeM3).Concat(near.OrderByDescending(x => x.Distance)), qty, unitVolume);
            if (splitAll is not null) return Found(splitAll, SplitNote(splitAll) + "\n" + fallbackNote);
        }

        // Cộng dồn TẤT CẢ ô vẫn không đủ (đến đây mọi MaxQty đều hữu hạn).
        var shortfall = qty - candidates.Sum(x => x.MaxQty);
        return new SuggestBinResponse(false, new List<SuggestedAllocationDto>(),
            $"Không đủ chỗ trong khu vực '{TranslateZoneType(requiredZoneType)}' — dù cộng dồn TẤT CẢ vị trí còn trống vẫn thiếu chỗ cho {shortfall:0.##} đơn vị (≈ {shortfall * unitVolume:0.00} m³). Cần tạo thêm vị trí/khu vực mới.");
    }

    /// <summary>1 ô còn trống của đúng loại khu vực: m³ còn trống, số lượng tối
    /// đa loại hàng đang xét còn xếp được (theo cả thể tích lẫn tải trọng), và
    /// khoảng cách TỰ TÍNH tới cửa kho.</summary>
    private sealed record BinCandidate(StorageBin Bin, decimal FreeM3, decimal MaxQty, decimal Distance);

    /// <summary>Chọn 1 ô DUY NHẤT chứa vừa trọn lô hàng. nearestFirst=true (hàng
    /// gấp): gần cửa nhất trước. false: Best-Fit — m³ dư ra nhỏ nhất, nếu bằng
    /// nhau thì ô xa cửa hơn. Trả về null nếu không ô nào chứa trọn.</summary>
    private static SuggestedAllocationDto? PickSingleFit(IEnumerable<BinCandidate> pool, decimal qty, decimal neededM3, bool nearestFirst)
    {
        var fits = pool.Where(x => x.MaxQty >= qty).ToList();
        if (!fits.Any()) return null;

        var best = nearestFirst
            ? fits.OrderBy(x => x.Distance).ThenBy(x => x.FreeM3 - neededM3).First()
            : fits.OrderBy(x => x.FreeM3 - neededM3).ThenByDescending(x => x.Distance).First();
        return new SuggestedAllocationDto(best.Bin.Id, best.Bin.Code, best.Bin.WarehouseZone.Name, qty, best.FreeM3 - neededM3);
    }

    /// <summary>Lấp đầy lần lượt các ô theo đúng thứ tự truyền vào cho tới khi đủ
    /// số lượng. Trả về null nếu cộng dồn hết vẫn không đủ.</summary>
    private static List<SuggestedAllocationDto>? FillInOrder(IEnumerable<BinCandidate> ordered, decimal qty, decimal unitVolume)
    {
        var allocations = new List<SuggestedAllocationDto>();
        var remaining = qty;
        foreach (var x in ordered)
        {
            if (remaining <= 0) break;
            var take = Math.Min(x.MaxQty, remaining);
            allocations.Add(new SuggestedAllocationDto(x.Bin.Id, x.Bin.Code, x.Bin.WarehouseZone.Name, take, x.FreeM3 - take * unitVolume));
            remaining -= take;
        }
        return remaining > 0 ? null : allocations;
    }

    private static string SplitNote(List<SuggestedAllocationDto> allocations)
    {
        if (allocations.Count == 1) return "";
        var zonesUsed = allocations.Select(a => a.ZoneName).Distinct().Count();
        return zonesUsed > 1
            ? $"⚠️ Số lượng quá lớn, phải CHIA RA {allocations.Count} vị trí thuộc {zonesUsed} khu vực khác nhau mới đủ chỗ."
            : $"⚠️ Số lượng quá lớn cho 1 vị trí, đã chia ra {allocations.Count} vị trí trong cùng khu vực.";
    }

    // ================= Vị trí cửa kho & khoảng cách tới cửa =================

    private static bool HasDoor(Warehouse w) =>
        w.DoorSide.HasValue && w.DoorOffsetMeters.HasValue && w.LengthMeters.HasValue && w.WidthMeters.HasValue;

    /// <summary>Tọa độ (mét) của cửa trên sơ đồ kho. X theo chiều DÀI, Y theo
    /// chiều RỘNG, gốc (0,0) ở góc trên-trái — cửa luôn nằm đúng trên tường.</summary>
    private static (decimal X, decimal Y) DoorPoint(Warehouse w)
    {
        var offset = w.DoorOffsetMeters!.Value;
        return w.DoorSide!.Value switch
        {
            WallSide.Top => (offset, 0m),
            WallSide.Bottom => (offset, w.WidthMeters!.Value),
            WallSide.Left => (0m, offset),
            _ => (w.LengthMeters!.Value, offset), // Right
        };
    }

    /// <summary>Khoảng cách (mét) từ cửa tới TÂM ô, đo theo lối đi VUÔNG GÓC
    /// (đi dọc rồi rẽ ngang, |dx| + |dy|) — sát thực tế hơn đường chim bay vì
    /// xe nâng chạy theo lối đi giữa các kệ. Tọa độ ô là tương đối trong khu
    /// vực, khu vực là tương đối trong kho. Null nếu kho chưa khai báo cửa.</summary>
    private static decimal? DoorDistanceMeters(Warehouse w, WarehouseZone zone, StorageBin bin)
    {
        if (!HasDoor(w)) return null;
        var (doorX, doorY) = DoorPoint(w);
        var centerX = zone.PosX + bin.PosX + bin.WidthMeters / 2;
        var centerY = zone.PosY + bin.PosY + bin.HeightMeters / 2;
        return Math.Round(Math.Abs(centerX - doorX) + Math.Abs(centerY - doorY), 2);
    }

    /// <summary>Kiểm tra vị trí cửa: CHỈ chọn tường + khoảng cách dọc tường, nên
    /// cửa không bao giờ lọt vào giữa kho. Khoảng cách phải nằm trong chiều dài
    /// đúng bức tường đó. Không chọn tường = kho chưa khai báo cửa (cho phép).
    /// Trả về thông báo lỗi, hoặc null nếu hợp lệ.</summary>
    private static string? ValidateDoor(string? side, decimal? offset, decimal? length, decimal? width, out WallSide? parsedSide)
    {
        parsedSide = null;
        if (string.IsNullOrWhiteSpace(side)) return null;
        if (!Enum.TryParse<WallSide>(side, out var wall)) return "Vị trí cửa không hợp lệ.";
        if (length is not > 0m || width is not > 0m)
            return "Cần khai báo chiều dài và chiều rộng kho trước khi khai báo vị trí cửa.";

        var wallLength = wall is WallSide.Top or WallSide.Bottom ? length!.Value : width!.Value;
        if (offset is null || offset < 0 || offset > wallLength)
            return $"Cửa phải nằm trên tường {TranslateWall(wall)} — khoảng cách từ góc phải từ 0 đến {wallLength:0.##} m (đúng chiều dài bức tường này).";

        parsedSide = wall;
        return null;
    }

    private static string TranslateWall(WallSide w) => w switch
    {
        WallSide.Top => "trên",
        WallSide.Bottom => "dưới",
        WallSide.Left => "trái",
        _ => "phải",
    };

    // ================= Quy đổi đơn vị & sức chứa ô kho =================

    /// <summary>m³ của 1 đơn vị hàng = tổng m³ dòng hàng / tổng số lượng (giống Greedy).</summary>
    private static decimal UnitVolumeM3(CargoLine c) => c.QtyTotal > 0 ? c.VolumeM3 / c.QtyTotal : 0;

    /// <summary>kg của 1 đơn vị hàng = tổng kg dòng hàng / tổng số lượng.</summary>
    private static decimal UnitWeightKg(CargoLine c) => c.QtyTotal > 0 ? c.WeightKg / c.QtyTotal : 0;

    /// <summary>Tổng khối lượng (kg) hàng ĐANG nằm trong ô — tính từ các lô đang
    /// chứa, không cần thêm cột mới. Yêu cầu đã Include Assignments → GoodsReceipt → CargoLine.</summary>
    private static decimal CurrentWeightKg(StorageBin bin) =>
        bin.Assignments.Where(a => a.QtyStored > 0).Sum(a => a.QtyStored * UnitWeightKg(a.GoodsReceipt.CargoLine));

    /// <summary>Số lượng TỐI ĐA của loại hàng này còn xếp được vào ô — giới hạn
    /// bởi CẢ thể tích còn trống LẪN tải trọng còn lại. Ràng buộc nào không áp
    /// dụng được (đơn vị = 0, hoặc ô chưa khai báo tải trọng tối đa) thì bỏ qua.</summary>
    private static decimal MaxQtyFitting(StorageBin bin, decimal unitVolume, decimal unitWeight)
    {
        var freeM3 = bin.CapacityM3 - bin.CurrentUsedM3;
        if (freeM3 <= 0) return 0;

        var maxQty = decimal.MaxValue;
        if (unitVolume > 0) maxQty = Math.Min(maxQty, freeM3 / unitVolume);

        if (bin.MaxWeightKg > 0 && unitWeight > 0)
        {
            var freeKg = bin.MaxWeightKg - CurrentWeightKg(bin);
            if (freeKg <= 0) return 0;
            maxQty = Math.Min(maxQty, freeKg / unitWeight);
        }
        return maxQty;
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

        // Khoảng cách tới cửa luôn TÍNH LẠI theo vị trí cửa/khu vực/ô hiện tại —
        // không dùng số đã lưu, tránh lệch khi vừa sửa cửa hoặc di chuyển khu vực.
        var zone = await _db.WarehouseZones.Include(z => z.Warehouse).FirstOrDefaultAsync(z => z.Id == zoneId);

        return bins.Select(b => new StorageBinDto(
            b.Id, b.Code, b.CapacityM3, b.CurrentUsedM3, b.MaxWeightKg, b.PosX, b.PosY, b.WidthMeters, b.HeightMeters,
            zone is null ? b.DistanceFromDoorMeters : DoorDistanceMeters(zone.Warehouse, zone, b) ?? 0,
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
            .Select(w => new WarehouseListItemDto(w.Id, w.Name, w.Address, w.CapacityM3, w.LengthMeters, w.WidthMeters, w.Lat, w.Lng,
                w.DoorSide.HasValue ? w.DoorSide.Value.ToString() : null, w.DoorOffsetMeters))
            .ToListAsync();
    }

    public async Task<WarehouseListItemDto> CreateWarehouseAsync(CreateWarehouseRequest request)
    {
        var doorError = ValidateDoor(request.DoorSide, request.DoorOffsetMeters, request.LengthMeters, request.WidthMeters, out var doorSide);
        if (doorError is not null) throw new InvalidOperationException(doorError);

        var warehouse = new Warehouse
        {
            Name = request.Name,
            Address = request.Address,
            CapacityM3 = request.CapacityM3,
            LengthMeters = request.LengthMeters,
            WidthMeters = request.WidthMeters,
            Lat = request.Lat,
            Lng = request.Lng,
            DoorSide = doorSide,
            DoorOffsetMeters = doorSide.HasValue ? request.DoorOffsetMeters : null,
        };
        _db.Warehouses.Add(warehouse);
        await _db.SaveChangesAsync();

        return new WarehouseListItemDto(warehouse.Id, warehouse.Name, warehouse.Address, warehouse.CapacityM3, warehouse.LengthMeters, warehouse.WidthMeters, warehouse.Lat, warehouse.Lng,
            warehouse.DoorSide?.ToString(), warehouse.DoorOffsetMeters);
    }

    public async Task<WarehouseActionResult> UpdateWarehouseAsync(int warehouseId, UpdateWarehouseRequest request)
    {
        var warehouse = await _db.Warehouses.FindAsync(warehouseId);
        if (warehouse is null) return new WarehouseActionResult(false, "Không tìm thấy kho.");

        // CHẶN thu nhỏ kho khiến các KHU VỰC đã có bên trong bị TRÀN RA NGOÀI
        // ranh giới mới — trước đây không kiểm tra, đổi kích thước kho nhỏ lại
        // vẫn cho qua dù khu vực bên trong giờ "lòi ra ngoài" trên sơ đồ.
        if (request.LengthMeters is decimal newLength && request.WidthMeters is decimal newWidth)
        {
            var zones = await _db.WarehouseZones.Where(z => z.WarehouseId == warehouseId).ToListAsync();
            var overflowingZone = zones.FirstOrDefault(z => z.PosX + z.WidthMeters > newLength || z.PosY + z.HeightMeters > newWidth);
            if (overflowingZone is not null)
                return new WarehouseActionResult(false,
                    $"Không thể thu nhỏ kho xuống {newLength}m x {newWidth}m — khu vực \"{overflowingZone.Name}\" (X={overflowingZone.PosX}, Y={overflowingZone.PosY}, Rộng={overflowingZone.WidthMeters}, Cao={overflowingZone.HeightMeters}) sẽ bị tràn ra ngoài. Cần sửa/xóa khu vực đó trước.");
        }

        // Kiểm tra cửa theo kích thước MỚI — thu nhỏ kho mà cửa nằm ngoài bức
        // tường mới cũng bị chặn.
        var doorError = ValidateDoor(request.DoorSide, request.DoorOffsetMeters, request.LengthMeters, request.WidthMeters, out var doorSide);
        if (doorError is not null) return new WarehouseActionResult(false, doorError);

        warehouse.Name = request.Name;
        warehouse.Address = request.Address;
        warehouse.CapacityM3 = request.CapacityM3;
        warehouse.LengthMeters = request.LengthMeters;
        warehouse.WidthMeters = request.WidthMeters;
        warehouse.Lat = request.Lat;
        warehouse.Lng = request.Lng;
        warehouse.DoorSide = doorSide;
        warehouse.DoorOffsetMeters = doorSide.HasValue ? request.DoorOffsetMeters : null;

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

        // CHẶN thu nhỏ khu vực khiến các VỊ TRÍ (StorageBin) đã có bên trong
        // bị TRÀN RA NGOÀI ranh giới mới — cùng logic với Kho→Khu vực ở trên,
        // áp dụng thêm 1 cấp xuống Khu vực→Vị trí.
        var binsInZone = await _db.StorageBins.Where(b => b.WarehouseZoneId == zoneId).ToListAsync();
        var overflowingBin = binsInZone.FirstOrDefault(b => b.PosX + b.WidthMeters > request.WidthMeters || b.PosY + b.HeightMeters > request.HeightMeters);
        if (overflowingBin is not null)
            return new WarehouseActionResult(false,
                $"Không thể thu nhỏ khu vực xuống {request.WidthMeters}m x {request.HeightMeters}m — vị trí \"{overflowingBin.Code}\" (X={overflowingBin.PosX}, Y={overflowingBin.PosY}, Rộng={overflowingBin.WidthMeters}, Cao={overflowingBin.HeightMeters}) sẽ bị tràn ra ngoài. Cần sửa/xóa vị trí đó trước.");

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
            CurrentUsedM3 = 0,
        };
        // Khoảng cách tới cửa TỰ TÍNH (bỏ qua giá trị nhập tay cũ trong request).
        var warehouseOfZone = await _db.Warehouses.FindAsync(zone.WarehouseId);
        bin.DistanceFromDoorMeters = warehouseOfZone is null ? 0 : DoorDistanceMeters(warehouseOfZone, zone, bin) ?? 0;
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
        var warehouseOfBin = await _db.Warehouses.FindAsync(zone.WarehouseId);
        bin.DistanceFromDoorMeters = warehouseOfBin is null ? 0 : DoorDistanceMeters(warehouseOfBin, zone, bin) ?? 0; // tự tính

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
        var cargoLine = await _db.CargoLines.Include(c => c.TransportOrder).FirstOrDefaultAsync(c => c.Id == cargoLineId);
        if (cargoLine is null)
            return new ReceiveGoodsResult(false, "Không tìm thấy dòng hàng (CargoLine).", new List<string>());

        // CHẶN nhập kho THỦ CÔNG khi đơn CompanyPickup nhưng chặng Lấy hàng
        // CHƯA hoàn tất — về mặt vật lý, hàng KHÔNG THỂ đã có mặt tại kho khi
        // xe vẫn còn đang trên đường đi lấy/chưa hoàn tất chuyến. Chỉ áp dụng
        // khi tripId == null (nhập THỦ CÔNG, không qua completing 1 Trip) —
        // luồng TỰ ĐỘNG (tripId có giá trị) không cần kiểm tra vì bản thân nó
        // CHỈ được gọi đúng lúc chặng Pickup vừa Complete xong (đã đảm bảo).
        if (!tripId.HasValue && cargoLine.TransportOrder.PickupMethod == PickupMethod.CompanyPickup)
        {
            var pickupCompleted = await _db.Trips.AnyAsync(t =>
                t.TransportOrderId == cargoLine.TransportOrderId && t.LegType == TripLegType.Pickup && t.Status == TripStatus.Completed);
            if (!pickupCompleted)
                return new ReceiveGoodsResult(false,
                    "Đơn này là \"Công ty cử xe đến lấy\" — chặng Lấy hàng CHƯA hoàn tất, hàng chưa thể có mặt tại kho. Chờ tài xế hoàn tất chuyến Lấy hàng trước khi nhập kho.",
                    new List<string>());
        }

        var alreadyReceived = await _db.GoodsReceipts
            .Where(r => r.CargoLineId == cargoLineId)
            .SumAsync(r => r.QtyReceived);

        if (alreadyReceived + qty > cargoLine.QtyTotal)
        {
            var remaining = cargoLine.QtyTotal - alreadyReceived;
            return new ReceiveGoodsResult(false,
                $"Vượt quá số lượng đã khai báo trong đơn — dòng hàng \"{cargoLine.CargoType}\" chỉ khai {cargoLine.QtyTotal}, đã nhập {alreadyReceived}, chỉ còn được nhập tối đa {remaining} nữa.",
                new List<string>());
        }

        // XÁC ĐỊNH + KIỂM TRA (các) VỊ TRÍ ĐÍCH TRƯỚC — KHÔNG tạo GoodsReceipt/
        // cộng tồn kho trước khi chắc chắn có đủ vị trí hợp lệ. Có thể là 1 vị
        // trí (trường hợp thường gặp) hoặc NHIỀU vị trí/khu vực nếu số lượng
        // quá lớn cho 1 chỗ — targetAllocations gom (Bin, Số lượng xếp vào bin đó).
        var requiredZoneType = cargoLine.IsHazmat ? ZoneType.Hazmat : cargoLine.IsFragile ? ZoneType.Fragile : ZoneType.Normal;
        var targetAllocations = new List<(StorageBin Bin, decimal Qty)>();

        if (chosenBinId.HasValue)
        {
            // NGƯỜI DÙNG TỰ CHỌN VỊ TRÍ — CHỈ 1 vị trí duy nhất (chọn tay thì
            // không tự động chia nhỏ hộ — nếu không đủ chỗ, báo lỗi để người
            // dùng tự chọn vị trí khác hoặc chuyển sang chế độ Tự động).
            var chosenBin = await _db.StorageBins
                .Include(b => b.WarehouseZone)
                .Include(b => b.Assignments.Where(a => a.QtyStored > 0))
                    .ThenInclude(a => a.GoodsReceipt).ThenInclude(r => r.CargoLine)
                .FirstOrDefaultAsync(b => b.Id == chosenBinId.Value);
            if (chosenBin is null)
                return new ReceiveGoodsResult(false, "Vị trí đã chọn không tồn tại.", new List<string>());

            if (chosenBin.WarehouseZone.ZoneType != requiredZoneType)
                return new ReceiveGoodsResult(false,
                    $"Vị trí {chosenBin.Code} thuộc khu '{TranslateZoneType(chosenBin.WarehouseZone.ZoneType)}', không phù hợp với hàng hóa loại '{TranslateZoneType(requiredZoneType)}' — cần chọn đúng vị trí trong khu vực tương ứng.",
                    new List<string>());

            var maxQtyInChosen = MaxQtyFitting(chosenBin, UnitVolumeM3(cargoLine), UnitWeightKg(cargoLine));
            if (maxQtyInChosen < qty)
                return new ReceiveGoodsResult(false,
                    $"Vị trí {chosenBin.Code} không đủ chỗ (còn {chosenBin.CapacityM3 - chosenBin.CurrentUsedM3:0.00} m³, tải trọng tối đa {chosenBin.MaxWeightKg:0.##} kg) — chỉ xếp thêm được tối đa {maxQtyInChosen:0.##} đơn vị, cần {qty} (≈ {qty * UnitVolumeM3(cargoLine):0.00} m³). Thử chọn vị trí khác hoặc chuyển sang \"Tự động (Best-Fit)\" để hệ thống tự chia ra nhiều vị trí.",
                    new List<string>());

            targetAllocations.Add((chosenBin, qty));
        }
        else
        {
            // Chạy Best-Fit tự động — CÓ THỂ trả về NHIỀU vị trí nếu 1 chỗ
            // không đủ chứa hết (SuggestBinAsync tự chia nhỏ khi cần). Nếu
            // hoàn toàn KHÔNG tìm được đủ chỗ (kể cả cộng dồn) -> CHẶN LUÔN.
            var suggestion = await SuggestBinAsync(warehouseId, new SuggestBinRequest(cargoLineId, qty));
            if (!suggestion.Found)
                return new ReceiveGoodsResult(false,
                    $"Không thể nhập kho — kho này CHƯA CÓ khu vực '{TranslateZoneType(requiredZoneType)}' phù hợp (hoặc đã hết chỗ trống, kể cả cộng dồn nhiều vị trí). Cần tạo khu vực/vị trí mới trước khi nhập loại hàng này. Chi tiết: {suggestion.Message}",
                    new List<string>());

            foreach (var alloc in suggestion.Allocations)
            {
                var bin = (await _db.StorageBins.FindAsync(alloc.StorageBinId))!;
                targetAllocations.Add((bin, alloc.QtyToPlace));
            }
        }

        // Đến đây CHẮC CHẮN đã có đủ vị trí hợp lệ, đúng khu, đủ chỗ — giờ mới
        // thật sự ghi nhận: tạo 1 GoodsReceipt DUY NHẤT (đại diện cho cả lô
        // hàng), nhưng có thể tạo NHIỀU StorageAssignment (mỗi vị trí 1 dòng).
        // BỌC TRANSACTION vì có 2 LẦN SaveChangesAsync riêng biệt (lần 1 tạo
        // GoodsReceipt để lấy Id thật, lần 2 tạo StorageAssignment dùng Id đó)
        // — nếu lần 2 thất bại giữa chừng (mất kết nối, lỗi DB...), lần 1 ĐÃ
        // LƯU VĨNH VIỄN sẽ để lại đúng tình huống "hàng ma" (tăng tồn kho tổng
        // nhưng KHÔNG có StorageAssignment tương ứng) — cùng loại lỗi đã sửa
        // trước đây, chỉ khác nguyên nhân kỹ thuật gây ra.
        // CHỈ tự mở transaction nếu CHƯA có transaction nào đang mở. Hàm này được
        // gọi LỒNG bên trong transaction khác (Chia xe đơn khách tự mang tới; Duyệt/
        // Hoàn tất chuyến Lấy hàng) — EF Core không cho mở 2 transaction lồng nhau,
        // trước đây gây lỗi 500. Khi bị gọi lồng: dùng chung transaction bên ngoài,
        // bên ngoài tự Commit/Rollback cho toàn bộ.
        var ownsTransaction = _db.Database.CurrentTransaction is null;
        await using var transaction = ownsTransaction ? await _db.Database.BeginTransactionAsync() : null;

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

        foreach (var (bin, allocQty) in targetAllocations)
        {
            bin.CurrentUsedM3 += allocQty * UnitVolumeM3(cargoLine); // quy đổi số lượng -> m³
            _db.StorageAssignments.Add(new StorageAssignment
            {
                GoodsReceiptId = receipt.Id,
                StorageBinId = bin.Id,
                QtyStored = allocQty,
            });
        }
        await _db.SaveChangesAsync();
        if (ownsTransaction) await transaction!.CommitAsync();

        var binCodesUsed = targetAllocations.Select(a => a.Bin.Code).ToList();
        var multiNote = binCodesUsed.Count > 1 ? $"⚠️ Đã chia ra {binCodesUsed.Count} vị trí: {string.Join(", ", binCodesUsed)}." : null;
        return new ReceiveGoodsResult(true, multiNote, binCodesUsed);
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

        var issuedCargo = await _db.CargoLines.FindAsync(cargoLineId);
        var unitVolume = issuedCargo is null ? 0 : UnitVolumeM3(issuedCargo);

        var remainingToIssue = qty;
        foreach (var assignment in assignments)
        {
            if (remainingToIssue <= 0) break;
            var take = Math.Min(remainingToIssue, assignment.QtyStored);

            assignment.QtyStored -= take;
            assignment.StorageBin.CurrentUsedM3 -= take * unitVolume; // quy đổi số lượng -> m³
            if (assignment.StorageBin.CurrentUsedM3 < 0) assignment.StorageBin.CurrentUsedM3 = 0; // chặn số âm do dữ liệu cũ (tính sai đơn vị trước đây)
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

        var bins = await _db.StorageBins
            .Include(b => b.WarehouseZone).ThenInclude(z => z.Warehouse)
            .Where(b => b.WarehouseZone.WarehouseId == warehouseId && b.WarehouseZone.ZoneType == requiredZoneType)
            .OrderBy(b => b.CurrentUsedM3) // ô còn trống nhiều nhất lên đầu, dễ chọn
            .ToListAsync();

        return bins.Select(b => new StorageBinDto(b.Id, b.Code, b.CapacityM3, b.CurrentUsedM3, b.MaxWeightKg, b.PosX, b.PosY, b.WidthMeters, b.HeightMeters,
            DoorDistanceMeters(b.WarehouseZone.Warehouse, b.WarehouseZone, b) ?? 0, new List<BinContentItemDto>())).ToList();
    }

    public async Task<List<CargoLineOptionDto>> GetCargoLinesForOrderAsync(int transportOrderId, int warehouseId)
    {
        var lines = await _db.CargoLines
            .Include(c => c.TransportOrder)
            .Where(c => c.TransportOrderId == transportOrderId)
            .ToListAsync();

        var result = new List<CargoLineOptionDto>();
        foreach (var c in lines)
        {
            var alreadyReceived = await _db.GoodsReceipts.Where(r => r.CargoLineId == c.Id).SumAsync(r => r.QtyReceived);
            var stock = await _db.WarehouseStocks.FirstOrDefaultAsync(s => s.WarehouseId == warehouseId && s.CargoLineId == c.Id);
            result.Add(new CargoLineOptionDto(c.Id, c.CargoType, c.QtyTotal, c.QtyTotal - alreadyReceived, stock?.QtyOnHand ?? 0, c.TransportOrder.Code));
        }
        return result;
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