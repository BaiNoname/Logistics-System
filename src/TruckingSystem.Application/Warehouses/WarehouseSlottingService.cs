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

    /// <summary>Gợi ý vị trí cho màn "Kiểm tra Best-Fit" — áp ĐÚNG quy tắc số
    /// lượng như khi nhập kho thật: phải > 0 và không vượt phần CÒN ĐƯỢC NHẬP
    /// của dòng hàng (khai báo − đã nhập). Chỉ gợi ý, KHÔNG ghi gì vào kho.</summary>
    public async Task<SuggestBinResponse> PreviewSuggestBinAsync(int warehouseId, SuggestBinRequest request)
    {
        var cargo = await _db.CargoLines.FirstOrDefaultAsync(c => c.Id == request.CargoLineId);
        if (cargo is null)
            return new SuggestBinResponse(false, new List<SuggestedAllocationDto>(), "Không tìm thấy dòng hàng.");
        if (request.QtyToStore <= 0)
            return new SuggestBinResponse(false, new List<SuggestedAllocationDto>(), "Số lượng phải lớn hơn 0.");

        var alreadyReceived = await _db.GoodsReceipts.Where(r => r.CargoLineId == cargo.Id).SumAsync(r => r.QtyReceived);
        var remaining = cargo.QtyTotal - alreadyReceived;
        if (remaining <= 0)
            return new SuggestBinResponse(false, new List<SuggestedAllocationDto>(),
                $"Dòng hàng \"{cargo.CargoType}\" đã nhập đủ {cargo.QtyTotal:0.##}/{cargo.QtyTotal:0.##} — không còn gì để xếp vị trí.");
        if (request.QtyToStore > remaining)
            return new SuggestBinResponse(false, new List<SuggestedAllocationDto>(),
                $"Vượt số lượng — dòng hàng \"{cargo.CargoType}\" khai {cargo.QtyTotal:0.##}, đã nhập {alreadyReceived:0.##}, chỉ còn được nhập tối đa {remaining:0.##}.");

        return await SuggestBinAsync(warehouseId, request);
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
        // Hàng đếm theo ĐƠN VỊ NGUYÊN (10 máy, 50 thùng): mỗi ô chỉ nhận số NGUYÊN đơn vị —
        // trước đây có thể chia 2,37 máy vào ô A và 7,63 máy vào ô B (Chia xe thì luôn
        // làm tròn xuống đơn vị nguyên).
        var wholeUnits = cargo.QtyTotal == Math.Floor(cargo.QtyTotal) && request.QtyToStore == Math.Floor(request.QtyToStore);
        var candidates = bins
            .Select(b =>
            {
                var maxQty = MaxQtyFitting(b, unitVolume, unitWeight);
                // +0.0001: thể tích đã lưu bị làm tròn -> 3,9999… vẫn tính là 4 đơn vị.
                if (wholeUnits && maxQty != decimal.MaxValue) maxQty = Math.Floor(maxQty + 0.0001m);
                // Hàng số lượng lẻ: cắt về 2 chữ số thập phân (độ chính xác cột số lượng trong
                // DB) — tránh phần lẻ 0,0033 bị DB làm tròn, để lại 0,01 "ma" trong ô mãi.
                else if (maxQty != decimal.MaxValue) maxQty = Math.Floor(maxQty * 100m) / 100m;
                return new BinCandidate(b, b.CapacityM3 - b.CurrentUsedM3, maxQty, distanceOf[b.Id]);
            })
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
        // Hạn giao cam kết lưu theo GIỜ VIỆT NAM -> so với giờ VN hiện tại (trước đây so với UTC, lệch 7 tiếng).
        var isUrgent = committedDate.HasValue && (committedDate.Value - DateTime.UtcNow.AddHours(7)).TotalDays <= 3;
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
        var shapeError = ValidateWarehouseShape(request.CapacityM3, request.LengthMeters, request.WidthMeters, request.Lat, request.Lng);
        if (shapeError is not null) throw new InvalidOperationException(shapeError);
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
        var shapeError = ValidateWarehouseShape(request.CapacityM3, request.LengthMeters, request.WidthMeters, request.Lat, request.Lng);
        if (shapeError is not null) return new WarehouseActionResult(false, shapeError);

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

        // DỜI TỌA ĐỘ kho khi đang có chuyến CHẠY qua kho: xe đang trên đường sẽ bị điều hướng về
        // chỗ mới, số kế hoạch (km, số ngày) của chuyến lệch hẳn -> chặn. Chuyến CHƯA CHẠY thì
        // được — controller tính lại ước tính cho các chuyến đó sau khi lưu.
        var coordsChanged = warehouse.Lat != request.Lat || warehouse.Lng != request.Lng;
        if (coordsChanged)
        {
            var runningIds = await _db.Trips
                .Where(t => t.WarehouseId == warehouseId && (t.Status == TripStatus.InProgress || t.Status == TripStatus.PendingConfirmation))
                .OrderBy(t => t.Id).Select(t => t.Id).Take(5).ToListAsync();
            if (runningIds.Count > 0)
                return new WarehouseActionResult(false,
                    $"Không thể dời tọa độ kho — đang có chuyến chạy qua kho này (#{string.Join(", #", runningIds)}). " +
                    "Chờ các chuyến đó hoàn tất rồi đổi địa chỉ / tọa độ kho.");
        }

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
        var rectError = ValidateRect(request.PosX, request.PosY, request.WidthMeters, request.HeightMeters, "Khu vực");
        if (rectError is not null) throw new InvalidOperationException(rectError);
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
        var rectError = ValidateRect(request.PosX, request.PosY, request.WidthMeters, request.HeightMeters, "Khu vực");
        if (rectError is not null) return new WarehouseActionResult(false, rectError);

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

        // Đổi LOẠI khu (Thường / Dễ vỡ / Nguy hiểm) khi khu đang chứa hàng -> hàng nằm
        // sai loại khu (VD hóa chất trong khu thường). Phải xuất/chuyển hết hàng trước.
        // Xét cả SỐ LƯỢNG hàng thật (giống khi xóa khu) — m³ có thể làm tròn về 0 dù vẫn còn hàng.
        // Theo SỐ LƯỢNG hàng thật (m³ có thể còn số lẻ dù đã xuất hết — xem DeleteZoneAsync).
        if (zoneType != zone.ZoneType &&
            await _db.StorageAssignments.AnyAsync(a => a.StorageBin.WarehouseZoneId == zoneId && a.QtyStored > 0))
            return new WarehouseActionResult(false, "Khu vực đang chứa hàng — không thể đổi loại khu. Xuất hết hàng trong khu trước.");
        // Đổi loại khu không được làm kho MẤT loại khu cuối cùng mà đơn đang xử lý còn cần nhập.
        if (zoneType != zone.ZoneType)
        {
            var retypeBlock = await WarehouseZoneRules.BlockIfRemovesNeededAsync(_db, zone.WarehouseId, "đổi loại khu", retype: (zone.Id, zoneType));
            if (retypeBlock is not null) return new WarehouseActionResult(false, retypeBlock);
        }

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
        var binError = ValidateBinNumbers(request);
        if (binError is not null) throw new InvalidOperationException(binError);

        var codeError = await ValidateBinCodeAsync(zone.WarehouseId, request.Code, excludeBinId: null);
        if (codeError is not null) throw new InvalidOperationException(codeError);

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
            Code = (request.Code ?? "").Trim(),
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

    /// <summary>Mã vị trí phải có và KHÔNG trùng trong cùng 1 kho — mã dùng để
    /// thủ kho tìm hàng, trùng mã là lấy nhầm hàng.</summary>
    private async Task<string?> ValidateBinCodeAsync(int warehouseId, string? code, int? excludeBinId)
    {
        var normalized = (code ?? "").Trim().ToUpper();
        if (normalized.Length == 0) return "Mã vị trí không được để trống.";
        var duplicate = await _db.StorageBins.AnyAsync(b =>
            b.WarehouseZone.WarehouseId == warehouseId && b.Code.Trim().ToUpper() == normalized &&
            (excludeBinId == null || b.Id != excludeBinId));
        return duplicate ? $"Mã vị trí \"{code!.Trim()}\" đã có trong kho này — chọn mã khác." : null;
    }

    public async Task<WarehouseActionResult> UpdateBinAsync(int binId, CreateBinRequest request)
    {
        var bin = await _db.StorageBins
            .Include(b => b.WarehouseZone)
            .Include(b => b.Assignments.Where(a => a.QtyStored > 0))
                .ThenInclude(a => a.GoodsReceipt).ThenInclude(r => r.CargoLine)
            .FirstOrDefaultAsync(b => b.Id == binId);
        if (bin is null) return new WarehouseActionResult(false, "Không tìm thấy vị trí.");
        var binError = ValidateBinNumbers(request);
        if (binError is not null) return new WarehouseActionResult(false, binError);

        if (request.CapacityM3 < bin.CurrentUsedM3)
            return new WarehouseActionResult(false, $"Sức chứa mới ({request.CapacityM3} m³) nhỏ hơn lượng hàng đang chứa ({bin.CurrentUsedM3} m³) — không thể sửa.");

        // Tải trọng tối đa mới cũng không được nhỏ hơn khối lượng hàng đang nằm trong ô
        // (0 = không giới hạn tải trọng, giống quy ước khi xếp hàng).
        var currentKg = CurrentWeightKg(bin);
        if (request.MaxWeightKg > 0 && request.MaxWeightKg < currentKg)
            return new WarehouseActionResult(false, $"Tải trọng tối đa mới ({request.MaxWeightKg:0.##} kg) nhỏ hơn khối lượng hàng đang chứa ({currentKg:0.##} kg) — không thể sửa.");

        var zone = bin.WarehouseZone;
        var codeError = await ValidateBinCodeAsync(zone.WarehouseId, request.Code, excludeBinId: bin.Id);
        if (codeError is not null) return new WarehouseActionResult(false, codeError);

        if (request.PosX < 0 || request.PosY < 0 || request.PosX + request.WidthMeters > zone.WidthMeters || request.PosY + request.HeightMeters > zone.HeightMeters)
            return new WarehouseActionResult(false, $"Vị trí này vượt ra NGOÀI ranh giới khu vực \"{zone.Name}\" (khu vực rộng {zone.WidthMeters}m x {zone.HeightMeters}m) — chỉnh lại tọa độ/kích thước cho vừa trong khu vực.");

        var otherBins = await _db.StorageBins.Where(b => b.Id != binId && b.WarehouseZoneId == bin.WarehouseZoneId).ToListAsync();
        var overlappingBin = otherBins.FirstOrDefault(b => RectanglesOverlap(
            request.PosX, request.PosY, request.WidthMeters, request.HeightMeters,
            b.PosX, b.PosY, b.WidthMeters, b.HeightMeters));
        if (overlappingBin is not null)
            return new WarehouseActionResult(false, $"Vị trí này đè lên vị trí \"{overlappingBin.Code}\" đã có — chọn tọa độ khác.");

        bin.Code = (request.Code ?? "").Trim();
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

        // CHẶN xóa kho còn được ĐƠN HÀNG / XE / TÀI XẾ tham chiếu, hoặc đã có lịch sử
        // nhập/xuất — xóa kho sẽ XÓA DÂY CHUYỀN (cascade) luôn các đơn hàng (kể cả đơn
        // Nháp chưa chia xe), hóa đơn và lịch sử kho của đơn đó; còn xe/tài xế thì làm
        // lỗi khóa ngoại. Chỉ xóa được kho chưa từng dùng.
        var orderCount = await _db.TransportOrders.CountAsync(o => o.WarehouseId == warehouseId);
        if (orderCount > 0)
            return new WarehouseActionResult(false, $"Kho này là kho trung chuyển của {orderCount} đơn hàng — không thể xóa (xóa sẽ mất luôn các đơn và hóa đơn liên quan).");
        var vehicleCount = await _db.Vehicles.CountAsync(v => v.WarehouseId == warehouseId);
        var driverCount = await _db.DriverProfiles.CountAsync(d => d.WarehouseId == warehouseId);
        if (vehicleCount + driverCount > 0)
            return new WarehouseActionResult(false, $"Còn {vehicleCount} xe và {driverCount} tài xế đóng quân ở kho này — hãy chuyển họ sang kho khác trước khi xóa.");
        var hasHistory = await _db.GoodsReceipts.AnyAsync(r => r.WarehouseId == warehouseId)
                         || await _db.GoodsIssues.AnyAsync(i => i.WarehouseId == warehouseId);
        if (hasHistory)
            return new WarehouseActionResult(false, "Kho này đã có lịch sử nhập/xuất hàng — không thể xóa (cần giữ lịch sử để đối chiếu).");

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

        // Kiểm tra theo SỐ LƯỢNG hàng thật, KHÔNG theo m³: m³ là số lẻ tích lũy qua nhiều
        // lần nhập/xuất (VD còn 0,0001 m³ dù đã xuất hết) -> xét m³ sẽ chặn xóa oan mãi.
        var binIds = zone.StorageBins.Select(b => b.Id).ToList();
        if (await _db.StorageAssignments.AnyAsync(a => binIds.Contains(a.StorageBinId) && a.QtyStored > 0))
            return new WarehouseActionResult(false, "Khu vực này còn hàng trong vị trí — không thể xóa. Cần xuất hết hàng trước.");
        // Khu TRỐNG nhưng là chỗ cuối cùng cho loại hàng của đơn đang chờ nhập kho -> chặn.
        var zoneBlock = await WarehouseZoneRules.BlockIfRemovesNeededAsync(_db, zone.WarehouseId, "xóa khu vực", excludeZoneId: zone.Id);
        if (zoneBlock is not null) return new WarehouseActionResult(false, zoneBlock);

        // Lịch sử xếp hàng đã hết (số lượng = 0) thì xóa cùng vị trí.
        _db.StorageAssignments.RemoveRange(await _db.StorageAssignments.Where(a => binIds.Contains(a.StorageBinId)).ToListAsync());
        _db.StorageBins.RemoveRange(zone.StorageBins);
        _db.WarehouseZones.Remove(zone);

        await _db.SaveChangesAsync();
        return new WarehouseActionResult(true, null);
    }

    public async Task<WarehouseActionResult> DeleteBinAsync(int binId)
    {
        var bin = await _db.StorageBins.FindAsync(binId);
        if (bin is null) return new WarehouseActionResult(false, "Không tìm thấy vị trí.");

        // Kiểm tra theo SỐ LƯỢNG hàng thật, KHÔNG theo m³ (m³ có thể còn số lẻ 0,0001 dù
        // đã xuất hết, hoặc làm tròn về 0 dù vẫn còn hàng).
        var assignments = await _db.StorageAssignments.Where(a => a.StorageBinId == binId).ToListAsync();
        if (assignments.Any(a => a.QtyStored > 0))
            return new WarehouseActionResult(false, "Vị trí này còn hàng — không thể xóa. Cần xuất hết hàng trước.");
        // Vị trí TRỐNG nhưng là vị trí cuối cùng của loại khu mà đơn đang xử lý còn cần -> chặn.
        var binWarehouseId = await _db.WarehouseZones.Where(z => z.Id == bin.WarehouseZoneId).Select(z => z.WarehouseId).FirstAsync();
        var binBlock = await WarehouseZoneRules.BlockIfRemovesNeededAsync(_db, binWarehouseId, "xóa vị trí", excludeBinId: bin.Id);
        if (binBlock is not null) return new WarehouseActionResult(false, binBlock);

        _db.StorageAssignments.RemoveRange(assignments); // lịch sử xếp hàng đã hết (số lượng = 0)
        _db.StorageBins.Remove(bin);
        await _db.SaveChangesAsync();
        return new WarehouseActionResult(true, null);
    }

    public async Task<ReceiveGoodsResult> ReceiveGoodsAsync(int warehouseId, int transportOrderId, int cargoLineId,
        decimal qty, int? receivedByEmployeeId, int? tripId, int? chosenBinId = null, string? performedByUserName = null,
        bool isManual = false)
    {
        // NHẬP TAY (ngoài chuyến): KHÓA dòng đơn (SELECT ... FOR UPDATE) trước khi kiểm tra số
        // lượng — Hủy đơn cũng khóa dòng này: nhập tay đúng lúc hủy thì 1 bên phải chờ, bên sau
        // đọc trạng thái mới (không còn hàng nhập vào đơn đã hủy mà không ai xuất trả). 2 lần
        // nhập tay cùng lúc cho cùng đơn cũng nối tiếp nhau -> không vượt số khai báo.
        if (isManual && _db.Database.CurrentTransaction is null)
        {
            await using var manualTx = await _db.Database.BeginTransactionAsync();
            await _db.Database.ExecuteSqlInterpolatedAsync($"SELECT 1 FROM \"TransportOrders\" WHERE \"Id\" = {transportOrderId} FOR UPDATE");
            var manualResult = await ReceiveGoodsCoreAsync(warehouseId, transportOrderId, cargoLineId, qty, receivedByEmployeeId,
                tripId, chosenBinId, performedByUserName, isManual);
            if (manualResult.Success) await manualTx.CommitAsync();
            return manualResult;
        }
        return await ReceiveGoodsCoreAsync(warehouseId, transportOrderId, cargoLineId, qty, receivedByEmployeeId,
            tripId, chosenBinId, performedByUserName, isManual);
    }

    private async Task<ReceiveGoodsResult> ReceiveGoodsCoreAsync(int warehouseId, int transportOrderId, int cargoLineId,
        decimal qty, int? receivedByEmployeeId, int? tripId, int? chosenBinId, string? performedByUserName, bool isManual)
    {
        // CHẶN nhập khống — không cho nhập vượt quá số lượng ĐÃ KHAI BÁO trong
        // đơn vận chuyển (CargoLine.QtyTotal), tính cả những lần đã nhập trước
        // đó cộng dồn lại. VD: đơn khai 1, đã nhập 1 rồi thì không cho nhập
        // thêm 3 nữa — dù là tự động (qua Trip) hay nhập tay đều bị chặn như nhau.
        var cargoLine = await _db.CargoLines.Include(c => c.TransportOrder).FirstOrDefaultAsync(c => c.Id == cargoLineId);
        if (cargoLine is null)
            return new ReceiveGoodsResult(false, "Không tìm thấy dòng hàng (CargoLine).", new List<string>());
        // Dòng hàng phải thuộc ĐÚNG đơn ghi trên phiếu — tránh phiếu nhập ghi nhầm đơn.
        if (cargoLine.TransportOrderId != transportOrderId)
            return new ReceiveGoodsResult(false, "Dòng hàng không thuộc đơn hàng đã chọn.", new List<string>());

        // CHẶN nhập kho THỦ CÔNG khi đơn CompanyPickup nhưng chặng Lấy hàng
        // CHƯA hoàn tất — về mặt vật lý, hàng KHÔNG THỂ đã có mặt tại kho khi
        // xe vẫn còn đang trên đường đi lấy/chưa hoàn tất chuyến. Chỉ áp dụng
        // khi tripId == null (nhập THỦ CÔNG ngoài chuyến) — nhập THEO CHUYẾN
        // (tripId có giá trị) do PendingReceiptService gọi, nơi đó đã kiểm tra
        // chuyến Lấy hàng đã Hoàn tất + còn đúng phần chờ nhập.
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

        // NHẬP TAY: không cho nhập cho đơn đã Hủy / đã Hoàn tất, và phải CHỪA LẠI
        // phần hàng mà các chuyến Lấy hàng CHƯA xong (chưa hủy) sẽ mang về —
        // trước đây chỉ cần 1 chuyến lấy hàng xong là nhập tay được tới đủ QtyTotal,
        // khiến chuyến lấy hàng còn lại bị kẹt "Chờ xác nhận" (duyệt sẽ vượt số lượng).
        // (isManual = true: thủ kho nhập tay trên màn Kho; Chia xe đơn "khách tự mang tới"
        // cũng gọi hàm này với tripId = null nhưng KHÔNG phải nhập tay.)
        if (isManual)
        {
            var unitError = ValidateManualQty(cargoLine, qty);
            if (unitError is not null) return new ReceiveGoodsResult(false, unitError, new List<string>());

            // CÙNG 1 quy tắc với màn Nhập/Xuất tay (GetOrderStockFlowAsync trả về để bật/tắt nút).
            var (canReceive, blockReason) = await GetManualReceiveLimitAsync(cargoLine, cargoLine.TransportOrder, warehouseId);
            if (qty > canReceive)
                return new ReceiveGoodsResult(false,
                    blockReason ?? $"Dòng hàng \"{cargoLine.CargoType}\": chỉ nhập tay thêm được tối đa {canReceive:0.##}.",
                    new List<string>());
        }

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
            // Vị trí phải nằm trong ĐÚNG kho đang nhập — trước đây chỉ kiểm tra loại khu,
            // gọi API với vị trí của kho khác vẫn được (tồn kho ghi kho A, hàng nằm kho B).
            if (chosenBin.WarehouseZone.WarehouseId != warehouseId)
                return new ReceiveGoodsResult(false, $"Vị trí {chosenBin.Code} không thuộc kho đang nhập.", new List<string>());

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
            PerformedByUserName = performedByUserName,
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

    // ---- Kiểm tra số liệu kho / khu / vị trí ở SERVER (trước đây chỉ Desktop kiểm tra:
    // gọi thẳng API được kích thước 0/âm, số quá lớn làm tràn cột -> lỗi 500) ----
    private const decimal MaxMeters = 100_000m;

    private static string? ValidateWarehouseShape(decimal? capacityM3, decimal? length, decimal? width, double? lat, double? lng)
    {
        if (capacityM3 is { } c && (c <= 0 || c > 100_000_000m)) return "Sức chứa kho phải lớn hơn 0 (tối đa 100.000.000 m³).";
        if (length is { } l && (l <= 0 || l > MaxMeters)) return "Chiều dài kho phải lớn hơn 0 (tối đa 100.000 m).";
        if (width is { } w && (w <= 0 || w > MaxMeters)) return "Chiều rộng kho phải lớn hơn 0 (tối đa 100.000 m).";
        if (lat.HasValue != lng.HasValue) return "Tọa độ kho phải có đủ cả vĩ độ lẫn kinh độ.";
        if (lat is { } la && (la < -90 || la > 90)) return "Vĩ độ kho không hợp lệ (-90 … 90).";
        if (lng is { } lo && (lo < -180 || lo > 180)) return "Kinh độ kho không hợp lệ (-180 … 180).";
        // (0, 0) = ô tọa độ để mặc định / chưa chọn địa chỉ bản đồ -> lộ trình qua kho sai hẳn.
        if (lat == 0 && lng == 0) return "Tọa độ kho (0, 0) không hợp lệ — hãy chọn địa chỉ từ ô tìm kiếm bản đồ.";
        return null;
    }

    private static string? ValidateRect(decimal posX, decimal posY, decimal width, decimal height, string what)
    {
        if (posX < 0 || posY < 0) return $"{what}: tọa độ X, Y không được âm.";
        if (width <= 0 || height <= 0) return $"{what}: kích thước rộng / cao phải lớn hơn 0.";
        if (posX + width > MaxMeters || posY + height > MaxMeters) return $"{what}: kích thước quá lớn.";
        return null;
    }

    /// <summary>Số lượng nhập/xuất tay: hàng đếm theo đơn vị NGUYÊN (số lượng khai là số
    /// nguyên, VD 10 máy) thì không nhận số lẻ (2,5 máy) — cùng quy tắc với Chia xe / xếp kho;
    /// hàng số lượng lẻ tối đa 2 chữ số thập phân (độ chính xác cột trong DB).</summary>
    private static string? ValidateManualQty(CargoLine cargoLine, decimal qty)
    {
        if (cargoLine.QtyTotal == Math.Floor(cargoLine.QtyTotal) && qty != Math.Floor(qty))
            return $"Dòng hàng \"{cargoLine.CargoType}\" đếm theo đơn vị nguyên — số lượng phải là số nguyên.";
        if (qty != Math.Round(qty, 2))
            return "Số lượng tối đa 2 chữ số thập phân.";
        return null;
    }

    private static string? ValidateBinNumbers(CreateBinRequest r)
    {
        var rect = ValidateRect(r.PosX, r.PosY, r.WidthMeters, r.HeightMeters, "Vị trí");
        if (rect is not null) return rect;
        if (r.CapacityM3 <= 0 || r.CapacityM3 > 1_000_000m) return "Sức chứa vị trí phải lớn hơn 0 (tối đa 1.000.000 m³).";
        if (r.MaxWeightKg < 0 || r.MaxWeightKg > 100_000_000m) return "Tải trọng tối đa không hợp lệ (0 = không giới hạn).";
        return null;
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
        decimal qty, int? issuedByEmployeeId, int? tripId, string? performedByUserName = null)
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
            PerformedByUserName = performedByUserName,
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

        // Vị trí đã HẾT hàng (không còn lô nào số lượng > 0) mà m³ đã dùng còn số lẻ do làm
        // tròn (VD 1 m³ chia 3 đơn vị, xuất từng cái -> còn 0,0001) -> đưa về đúng 0.
        var touchedBinIds = assignments.Select(a => a.StorageBinId).Distinct().ToList();
        var emptiedBins = await _db.StorageBins
            .Where(b => touchedBinIds.Contains(b.Id) && b.CurrentUsedM3 != 0
                        && !b.Assignments.Any(a => a.QtyStored > 0))
            .ToListAsync();
        if (emptiedBins.Count > 0)
        {
            foreach (var b in emptiedBins) b.CurrentUsedM3 = 0;
            await _db.SaveChangesAsync();
        }
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
        // Lấy dữ liệu thô rồi mới ghép chữ trong bộ nhớ (EF không dịch được
        // switch/ToString của enum sang SQL).
        var receipts = await _db.GoodsReceipts
            .Where(r => r.WarehouseId == warehouseId)
            .Select(r => new
            {
                r.Id,
                r.ReceivedAt,
                r.TransportOrderId,
                OrderCode = r.TransportOrder.Code,
                PickupMethod = r.TransportOrder.PickupMethod,
                r.CargoLine.CargoType,
                r.QtyReceived,
                r.TripId,
                TripLeg = r.Trip != null ? (TripLegType?)r.Trip.LegType : null,
                EmployeeName = r.ReceivedByEmployee != null ? r.ReceivedByEmployee.FullName : null,
                r.PerformedByUserName,
            })
            .ToListAsync();

        var issues = await _db.GoodsIssues
            .Where(i => i.WarehouseId == warehouseId)
            .Select(i => new
            {
                i.Id,
                i.IssuedAt,
                i.TransportOrderId,
                OrderCode = i.TransportOrder.Code,
                i.CargoLine.CargoType,
                i.QtyIssued,
                i.TripId,
                TripLeg = i.Trip != null ? (TripLegType?)i.Trip.LegType : null,
                EmployeeName = i.IssuedByEmployee != null ? i.IssuedByEmployee.FullName : null,
                i.PerformedByUserName,
                i.ReversedAt,
                i.ReversedByUserName,
                OrderStatus = i.TransportOrder.Status,
            })
            .ToListAsync();

        var result = new List<WarehouseTransactionDto>();
        foreach (var r in receipts)
        {
            // Nhập tay = không gắn chuyến VÀ có tài khoản thực hiện (màn Nhập/Xuất tay luôn
            // ghi tài khoản). Nhập tự động lúc Chia xe (đơn khách tự mang tới) không có tài
            // khoản. Dữ liệu cũ: đơn công ty đi lấy mà không gắn chuyến cũng là nhập tay.
            var manual = r.TripId is null && (r.PerformedByUserName != null || r.PickupMethod == PickupMethod.CompanyPickup);
            var source = r.TripId is not null ? $"Chuyến #{r.TripId} · {LegText(r.TripLeg)}"
                       : manual ? "✋ Thủ công"
                       : "Khách tự mang tới kho (nhập khi Chia xe)";
            var performer = r.TripId is null && !manual && r.PerformedByUserName is null
                ? "Tự động khi Chia xe"
                : PerformerText(r.TripId, r.EmployeeName, r.PerformedByUserName);
            result.Add(new WarehouseTransactionDto("Nhập", r.ReceivedAt, r.OrderCode, r.CargoType, r.QtyReceived,
                performer,
                r.Id, r.TransportOrderId, r.TripId, source, manual, false, null));
        }
        foreach (var i in issues)
        {
            var manual = i.TripId is null;
            var isCancelReturn = IsCancelReturnIssue(i.PerformedByUserName);
            var source = isCancelReturn ? "↩️ Trả khách tại kho (hủy đơn)"
                       : manual ? "✋ Thủ công" : $"Chuyến #{i.TripId} · {LegText(i.TripLeg)}";
            var note = i.ReversedAt is null ? null
                : $"↩️ Đã hoàn tác lúc {i.ReversedAt.Value.AddHours(7):dd/MM HH:mm}" + (i.ReversedByUserName is null ? "" : $" bởi {i.ReversedByUserName}") + " — hàng đã trả lại kho";
            // Hoàn tác được: phiếu xuất tay, chưa hoàn tác, KHÔNG phải phiếu tự động trả khách
            // lúc hủy đơn (cùng điều kiện ReverseManualIssueAsync kiểm tra).
            var canReverse = manual && !isCancelReturn && i.ReversedAt is null;
            result.Add(new WarehouseTransactionDto("Xuất", i.IssuedAt, i.OrderCode, i.CargoType, i.QtyIssued,
                PerformerText(i.TripId, i.EmployeeName, i.PerformedByUserName),
                i.Id, i.TransportOrderId, i.TripId, source, manual, i.ReversedAt is not null, note, canReverse));
        }
        return result.OrderByDescending(t => t.OccurredAt).ToList();
    }

    private static string LegText(TripLegType? leg) => leg switch
    {
        TripLegType.Pickup => "Lấy hàng",
        TripLegType.Delivery => "Giao hàng",
        TripLegType.Return => "Trả hàng",
        _ => "",
    };

    /// <summary>Người thực hiện hiển thị: theo chuyến = tài xế; thủ công = tài
    /// khoản đăng nhập (kèm tên nhân viên nếu tài khoản có gắn nhân viên).
    /// Dữ liệu thủ công CŨ (trước bản sửa) từng bị gán cứng nhân viên số 1 —
    /// không còn tin được nên ghi rõ là không xác định.</summary>
    private static string PerformerText(int? tripId, string? employeeName, string? userName)
    {
        // Có tài khoản thực hiện (nhập kho hàng chờ nhập của chuyến, trả khách tại kho...) -> ghi
        // đúng người bấm; phiếu theo chuyến không có tài khoản (dữ liệu cũ, xuất khi chạy) = tài xế.
        if (userName is not null) return employeeName is null ? userName : $"{employeeName} ({userName})";
        if (tripId is not null) return employeeName is null ? "—" : $"{employeeName} (tài xế)";
        return "Không xác định (dữ liệu cũ)";
    }

    // ================= Tình trạng hàng theo đơn =================

    /// <summary>Số lượng đang GIỮ cho các chuyến sắp lấy hàng ra khỏi kho: chuyến
    /// Giao chưa bắt đầu (bắt đầu mới xuất kho) và chuyến Trả hàng chưa hoàn tất
    /// (hoàn tất mới xuất kho) — trừ phần chuyến đó đã xuất rồi.</summary>
    private async Task<decimal> GetReservedQtyAsync(int cargoLineId)
    {
        var lines = await _db.TripLines
            .Where(tl => tl.CargoLineId == cargoLineId
                && ((tl.Trip.LegType == TripLegType.Delivery && tl.Trip.Status == TripStatus.Planned)
                    || (tl.Trip.LegType == TripLegType.Return
                        && tl.Trip.Status != TripStatus.Completed && tl.Trip.Status != TripStatus.Cancelled)))
            .Select(tl => new { tl.TripId, tl.QtyPlanned })
            .ToListAsync();
        if (lines.Count == 0) return 0;

        var tripIds = lines.Select(l => l.TripId).Distinct().ToList();
        var issued = await _db.GoodsIssues
            .Where(g => g.CargoLineId == cargoLineId && g.TripId != null && tripIds.Contains(g.TripId.Value) && g.ReversedAt == null)
            .GroupBy(g => g.TripId!.Value)
            .Select(g => new { TripId = g.Key, Qty = g.Sum(x => x.QtyIssued) })
            .ToDictionaryAsync(x => x.TripId, x => x.Qty);

        return lines.Sum(l => Math.Max(0, l.QtyPlanned - (issued.TryGetValue(l.TripId, out var q) ? q : 0)));
    }

    /// <summary>Tiền tố "người thực hiện" của phiếu xuất TỰ ĐỘNG trả khách tại kho khi hủy
    /// đơn (TransportOrderService.CancelAsync) — để nhận diện, hiển thị và không cho hoàn tác.</summary>
    public const string CancelReturnPerformerPrefix = "Hủy đơn ";

    private static bool IsCancelReturnIssue(string? performedByUserName)
        => performedByUserName is not null && performedByUserName.StartsWith(CancelReturnPerformerPrefix);

    /// <summary>Phần hàng của dòng này THUỘC các chuyến Lấy hàng mà chưa vào kho: chuyến chưa
    /// xong (chưa hủy) sẽ mang về + chuyến đã về kho nhưng còn CHỜ NHẬP (màn Hàng chờ nhập).
    /// Phần này nhập theo chuyến, không nhập tay ngoài chuyến (tránh nhập trùng).</summary>
    private async Task<decimal> GetPendingPickupQtyAsync(int cargoLineId)
    {
        var onTheWay = await _db.TripLines
            .Where(l => l.CargoLineId == cargoLineId
                        && l.Trip.LegType == TripLegType.Pickup
                        && l.Trip.Status != TripStatus.Cancelled
                        && l.Trip.Status != TripStatus.Completed)
            .SumAsync(l => (decimal?)l.QtyPlanned) ?? 0m;
        var waitingReceipt = (await PendingReceiptRules.LoadAsync(_db, l => l.CargoLineId == cargoLineId)).Sum(p => p.QtyPending);
        return onTheWay + waitingReceipt;
    }

    /// <summary>NHẬP TAY được tối đa bao nhiêu + lý do nếu không được — 1 quy tắc DUY
    /// NHẤT dùng cho cả lúc nhập (ReceiveGoodsAsync) lẫn màn Nhập/Xuất tay (bật/tắt nút).
    /// - Chỉ đơn ĐÃ XÁC NHẬN / ĐANG CHẠY (đơn Nháp còn sửa được, đơn Hủy/Hoàn tất thì thôi).
    /// - Đúng kho trung chuyển của đơn.
    /// - Đơn công ty đi lấy: phải có chặng Lấy hàng hoàn tất (hàng mới có mặt ở kho).
    /// - Chừa phần các chuyến Lấy hàng chưa xong sẽ mang về / đã về kho còn chờ nhập.</summary>
    private async Task<(decimal Max, string? Reason)> GetManualReceiveLimitAsync(CargoLine cargoLine, TransportOrder order, int warehouseId)
    {
        if (order.Status is not (TransportOrderStatus.Confirmed or TransportOrderStatus.InProgress))
            return (0, order.Status switch
            {
                TransportOrderStatus.Draft => $"Đơn {order.Code} còn Nháp — hãy Xác nhận đơn trước khi nhập kho.",
                TransportOrderStatus.Cancelled => $"Đơn {order.Code} đã HỦY — không nhập kho thêm cho đơn này.",
                _ => $"Đơn {order.Code} đã HOÀN TẤT — không nhập kho thêm cho đơn này.",
            });

        if (warehouseId != order.WarehouseId)
            return (0, $"Đơn {order.Code} dùng kho trung chuyển khác — chỉ nhập hàng của đơn này vào đúng kho đã chọn trên đơn.");

        if (order.PickupMethod == PickupMethod.CompanyPickup)
        {
            var pickupCompleted = await _db.Trips.AnyAsync(t =>
                t.TransportOrderId == order.Id && t.LegType == TripLegType.Pickup && t.Status == TripStatus.Completed);
            if (!pickupCompleted)
                return (0, "Đơn này là \"Công ty cử xe đến lấy\" — chặng Lấy hàng CHƯA hoàn tất, hàng chưa thể có mặt tại kho.");
        }

        var alreadyReceived = await _db.GoodsReceipts.Where(r => r.CargoLineId == cargoLine.Id).SumAsync(r => (decimal?)r.QtyReceived) ?? 0m;
        var pendingPickups = await GetPendingPickupQtyAsync(cargoLine.Id);
        var max = Math.Max(0, cargoLine.QtyTotal - alreadyReceived - pendingPickups);
        if (max > 0) return (max, null);
        return (0, pendingPickups > 0
            ? $"Dòng hàng \"{cargoLine.CargoType}\": khai {cargoLine.QtyTotal:0.##}, đã nhập {alreadyReceived:0.##}, còn {pendingPickups:0.##} thuộc các chuyến Lấy hàng (đang chạy, hoặc đã về kho chờ nhập) — hàng của chuyến nhập ở nút 📦 Nhập hàng từ chuyến, không nhập tay ở đây."
            : $"Dòng hàng \"{cargoLine.CargoType}\" đã nhập đủ số lượng khai báo ({cargoLine.QtyTotal:0.##}).");
    }

    /// <summary>XUẤT TAY được tối đa bao nhiêu + lý do nếu không được.
    /// Đơn ĐANG XỬ LÝ (Nháp / Đã xác nhận / Đang chạy): KHÔNG xuất tay — hàng của đơn sẽ
    /// tự xuất kho khi chuyến giao/trả chạy; xuất tay lúc này làm Chia xe / chuyến giao
    /// sau đó thiếu hàng ("Không đủ tồn kho"). Muốn trả hàng cho khách thì Hủy đơn (hàng
    /// chưa có chuyến trả sẽ được xuất trả tại kho). Đơn đã Hoàn tất / Đã hủy: xuất phần
    /// còn sót trong kho, trừ phần đang giữ cho chuyến Trả hàng chưa xong.</summary>
    private async Task<(decimal Max, string? Reason)> GetManualIssueLimitAsync(CargoLine cargoLine, TransportOrder order, int warehouseId)
    {
        if (order.Status is TransportOrderStatus.Draft or TransportOrderStatus.Confirmed or TransportOrderStatus.InProgress)
            return (0, $"Đơn {order.Code} đang xử lý — hàng sẽ TỰ xuất kho khi chuyến giao chạy, không xuất tay. " +
                       "Muốn trả hàng cho khách: Hủy đơn (hàng còn trong kho chưa có chuyến trả sẽ được xuất trả tại kho).");

        var onHand = await _db.WarehouseStocks
            .Where(s => s.WarehouseId == warehouseId && s.CargoLineId == cargoLine.Id)
            .SumAsync(s => (decimal?)s.QtyOnHand) ?? 0m;
        var reserved = await GetReservedQtyAsync(cargoLine.Id);
        var free = Math.Max(0, onHand - reserved);
        if (free > 0) return (free, null);
        return (0, reserved > 0
            ? $"Trong kho có {onHand:0.##} nhưng {reserved:0.##} đang được GIỮ cho chuyến trả hàng sắp chạy — không xuất tay được."
            : "Không còn hàng của dòng này trong kho.");
    }

    public async Task<int?> ResolveEmployeeIdAsync(string? userId)
    {
        if (string.IsNullOrEmpty(userId)) return null;
        return await _db.Employees.Where(e => e.UserId == userId).Select(e => (int?)e.Id).FirstOrDefaultAsync();
    }

    /// <summary>Xuất kho THỦ CÔNG — chỉ được xuất phần hàng KHÔNG thuộc chuyến
    /// nào (hàng dư, điều chỉnh...). Hàng đã dành cho chuyến Giao/Trả sắp chạy
    /// thì CHẶN, tránh đúng lỗi: xuất tay mất hàng, tới lúc chuyến giao bắt đầu
    /// thì báo "không đủ tồn kho".</summary>
    public async Task<IssueGoodsResult> IssueManualAsync(int warehouseId, int transportOrderId, int cargoLineId, decimal qty, StockPerformer performer)
    {
        // Dòng hàng phải thuộc ĐÚNG đơn ghi trên phiếu xuất.
        var cargoLine = await _db.CargoLines.Include(c => c.TransportOrder)
            .FirstOrDefaultAsync(c => c.Id == cargoLineId && c.TransportOrderId == transportOrderId);
        if (cargoLine is null)
            return new IssueGoodsResult(false, "Dòng hàng không thuộc đơn hàng đã chọn.");
        if (qty <= 0)
            return new IssueGoodsResult(false, "Số lượng xuất phải lớn hơn 0.");
        var unitError = ValidateManualQty(cargoLine, qty);
        if (unitError is not null) return new IssueGoodsResult(false, unitError);

        // CÙNG 1 quy tắc với màn Nhập/Xuất tay (nút Xuất bật/tắt theo đúng kết quả này).
        var (canIssue, blockReason) = await GetManualIssueLimitAsync(cargoLine, cargoLine.TransportOrder, warehouseId);
        if (qty > canIssue)
            return new IssueGoodsResult(false, blockReason ?? $"Chỉ có thể xuất tay tối đa {canIssue:0.##}.");
        return await IssueGoodsAsync(warehouseId, transportOrderId, cargoLineId, qty, performer.EmployeeId, tripId: null, performer.UserName);
    }

    /// <summary>Hoàn tác 1 phiếu xuất THỦ CÔNG bị làm nhầm: cộng lại tồn kho và
    /// xếp hàng trở lại ô kệ (Best-Fit, gắn vào phiếu nhập gốc của dòng hàng).
    /// Phiếu xuất được GIỮ LẠI và đánh dấu "đã hoàn tác" để lịch sử minh bạch.</summary>
    public async Task<WarehouseActionResult> ReverseManualIssueAsync(int issueId, string? userName)
    {
        var issue = await _db.GoodsIssues.Include(i => i.CargoLine).FirstOrDefaultAsync(i => i.Id == issueId);
        if (issue is null) return new WarehouseActionResult(false, "Không tìm thấy phiếu xuất.");
        if (issue.TripId is not null)
            return new WarehouseActionResult(false, "Chỉ hoàn tác được phiếu xuất THỦ CÔNG — phiếu này do chuyến #" + issue.TripId + " tạo ra.");
        if (issue.ReversedAt is not null) return new WarehouseActionResult(false, "Phiếu xuất này đã được hoàn tác trước đó.");

        // Phiếu xuất TỰ ĐỘNG trả khách lúc hủy đơn: hàng đã giao lại cho khách -> không hoàn tác.
        // (Phiếu xuất tay của đơn Hoàn tất / Đã hủy vẫn hoàn tác được: hàng trả lại kho thì
        // vẫn xuất tay lại được — xuất tay chỉ áp dụng cho đơn đã Hoàn tất / Đã hủy.)
        if (IsCancelReturnIssue(issue.PerformedByUserName))
            return new WarehouseActionResult(false, "Phiếu này là hàng TRẢ KHÁCH tại kho khi hủy đơn — không hoàn tác được.");

        var originReceipt = await _db.GoodsReceipts
            .Where(r => r.WarehouseId == issue.WarehouseId && r.CargoLineId == issue.CargoLineId)
            .OrderByDescending(r => r.ReceivedAt)
            .FirstOrDefaultAsync();
        if (originReceipt is null)
            return new WarehouseActionResult(false, "Không tìm thấy phiếu nhập gốc của dòng hàng này — không thể xếp hàng trở lại kho.");

        var suggestion = await SuggestBinAsync(issue.WarehouseId, new SuggestBinRequest(issue.CargoLineId, issue.QtyIssued));
        if (!suggestion.Found)
            return new WarehouseActionResult(false, "Kho không còn đủ chỗ phù hợp để xếp lại hàng: " + suggestion.Message);

        var ownsTransaction = _db.Database.CurrentTransaction is null;
        await using var transaction = ownsTransaction ? await _db.Database.BeginTransactionAsync() : null;

        var stock = await _db.WarehouseStocks.FirstOrDefaultAsync(s => s.WarehouseId == issue.WarehouseId && s.CargoLineId == issue.CargoLineId);
        if (stock is null)
        {
            stock = new WarehouseStock { WarehouseId = issue.WarehouseId, CargoLineId = issue.CargoLineId, QtyOnHand = 0 };
            _db.WarehouseStocks.Add(stock);
        }
        stock.QtyOnHand += issue.QtyIssued;
        stock.LastUpdatedAt = DateTime.UtcNow;

        var unitVolume = UnitVolumeM3(issue.CargoLine);
        var binCodes = new List<string>();
        foreach (var alloc in suggestion.Allocations)
        {
            var bin = (await _db.StorageBins.FindAsync(alloc.StorageBinId))!;
            bin.CurrentUsedM3 += alloc.QtyToPlace * unitVolume;
            _db.StorageAssignments.Add(new StorageAssignment
            {
                GoodsReceiptId = originReceipt.Id,
                StorageBinId = bin.Id,
                QtyStored = alloc.QtyToPlace,
            });
            binCodes.Add(bin.Code);
        }

        issue.ReversedAt = DateTime.UtcNow;
        issue.ReversedByUserName = userName;

        await _db.SaveChangesAsync();
        if (ownsTransaction) await transaction!.CommitAsync();
        return new WarehouseActionResult(true, $"Đã hoàn tác — trả {issue.QtyIssued:0.##} vào kho, vị trí {string.Join(", ", binCodes)}.");
    }

    /// <summary>CHỈ các đơn liên quan tới kho này (trước đây màn Nhập/Xuất kho
    /// liệt kê MỌI đơn của cả công ty, kể cả đơn đi qua kho khác).</summary>
    public async Task<List<WarehouseOrderDto>> GetOrdersForWarehouseAsync(int warehouseId)
    {
        var orders = await _db.TransportOrders
            .Where(o => o.WarehouseId == warehouseId
                || _db.Trips.Any(t => t.TransportOrderId == o.Id && t.WarehouseId == warehouseId)
                || _db.GoodsReceipts.Any(r => r.TransportOrderId == o.Id && r.WarehouseId == warehouseId)
                || _db.GoodsIssues.Any(i => i.TransportOrderId == o.Id && i.WarehouseId == warehouseId))
            .OrderByDescending(o => o.Id)
            .Select(o => new
            {
                o.Id, o.Code, CustomerName = o.Customer.Name, o.Status, o.PickupMethod,
                OnHand = _db.WarehouseStocks
                    .Where(s => s.WarehouseId == warehouseId && s.CargoLine.TransportOrderId == o.Id)
                    .Sum(s => (decimal?)s.QtyOnHand) ?? 0m,
            })
            .ToListAsync();

        return orders.Select(o => new WarehouseOrderDto(o.Id, o.Code, o.CustomerName, o.Status.ToString(),
            o.PickupMethod.ToString(), o.OnHand)).ToList();
    }

    public async Task<OrderStockFlowDto?> GetOrderStockFlowAsync(int transportOrderId, int warehouseId)
    {
        var order = await _db.TransportOrders.FirstOrDefaultAsync(o => o.Id == transportOrderId);
        if (order is null) return null;

        var cargoLines = await _db.CargoLines.Where(c => c.TransportOrderId == transportOrderId).ToListAsync();
        var cargoIds = cargoLines.Select(c => c.Id).ToList();

        var receipts = await _db.GoodsReceipts
            .Where(r => r.WarehouseId == warehouseId && cargoIds.Contains(r.CargoLineId))
            .Select(r => new { r.CargoLineId, r.TripId, r.QtyReceived })
            .ToListAsync();
        var issues = await _db.GoodsIssues
            .Where(i => i.WarehouseId == warehouseId && cargoIds.Contains(i.CargoLineId) && i.ReversedAt == null)
            .Select(i => new { i.CargoLineId, i.TripId, i.QtyIssued })
            .ToListAsync();
        var stocks = await _db.WarehouseStocks
            .Where(s => s.WarehouseId == warehouseId && cargoIds.Contains(s.CargoLineId))
            .ToDictionaryAsync(s => s.CargoLineId, s => s.QtyOnHand);

        var cargoDtos = new List<CargoStockFlowDto>();
        foreach (var c in cargoLines)
        {
            var received = receipts.Where(r => r.CargoLineId == c.Id).Sum(r => r.QtyReceived);
            var issued = issues.Where(i => i.CargoLineId == c.Id).Sum(i => i.QtyIssued);
            var onHand = stocks.TryGetValue(c.Id, out var q) ? q : 0;
            var reserved = await GetReservedQtyAsync(c.Id);
            var (canReceive, receiveReason) = await GetManualReceiveLimitAsync(c, order, warehouseId);
            var (free, issueReason) = await GetManualIssueLimitAsync(c, order, warehouseId);
            // Phần chuyến Lấy hàng chưa xong sẽ mang về — chưa về kho thì chưa phải "thiếu".
            var pendingPickups = await GetPendingPickupQtyAsync(c.Id);

            string status;
            var closed = order.Status is TransportOrderStatus.Cancelled or TransportOrderStatus.Done;
            if (closed)
            {
                // Đơn đã ĐÓNG: không còn "đang nhập" gì nữa — chỉ báo hàng còn/hết trong kho.
                var closedText = order.Status == TransportOrderStatus.Cancelled ? "Đơn đã hủy" : "Đơn đã hoàn tất";
                status = onHand > 0
                    ? $"🔒 {closedText} — còn {onHand:0.##} trong kho (đã nhập {received:0.##}, đã xuất {issued:0.##})"
                    : $"🔒 {closedText} — không còn hàng trong kho (đã nhập {received:0.##}, đã xuất {issued:0.##})";
            }
            else if (received == 0) status = "⏳ Chưa nhập kho";
            else if (received < c.QtyTotal) status = $"📥 Đang nhập — {received:0.##}/{c.QtyTotal:0.##}";
            else if (issued >= c.QtyTotal) status = $"📤 Đã xuất hết {issued:0.##}/{c.QtyTotal:0.##}";
            else if (onHand > 0 && reserved > 0) status = $"✅ Đã nhập đủ — {onHand:0.##} trong kho, chờ chuyến giao lấy {reserved:0.##}";
            else if (onHand > 0) status = $"✅ Đã nhập đủ — {onHand:0.##} trong kho";
            else status = "⚠️ Đã nhập nhưng kho đang trống";
            if (closed) { /* đơn đã đóng: không cảnh báo thiếu/chờ */ }
            else if (onHand + pendingPickups < reserved)
                status += $" — ⚠️ THIẾU {reserved - onHand - pendingPickups:0.##} so với chuyến giao cần";
            else if (onHand < reserved)
                status += $" — chờ chuyến lấy hàng mang về / nhập kho thêm {reserved - onHand:0.##}";

            cargoDtos.Add(new CargoStockFlowDto(c.Id, c.CargoType, c.QtyTotal, received, issued, onHand, reserved, free, status,
                canReceive, receiveReason, issueReason));
        }

        // Chỉ các chuyến đi QUA KHO NÀY (đơn có thể có chuyến ở kho khác)
        var trips = await _db.Trips
            .Where(t => t.TransportOrderId == transportOrderId && t.WarehouseId == warehouseId)
            .OrderBy(t => t.Id)
            .Select(t => new
            {
                t.Id,
                t.LegType,
                t.Status,
                Driver = t.DriverEmployee.FullName,
                Planned = t.TripLines.Sum(tl => tl.QtyPlanned),
            })
            .ToListAsync();

        var tripDtos = trips.Select(t =>
        {
            var done = t.LegType == TripLegType.Pickup
                ? receipts.Where(r => r.TripId == t.Id).Sum(r => r.QtyReceived)
                : issues.Where(i => i.TripId == t.Id).Sum(i => i.QtyIssued);

            string progress;
            if (t.Status == TripStatus.Cancelled) progress = "— (chuyến đã hủy)";
            else if (t.LegType == TripLegType.Pickup)
                progress = done >= t.Planned && t.Planned > 0 ? $"✅ Đã nhập kho {done:0.##}/{t.Planned:0.##}"
                         : done > 0 ? $"📥 Đã nhập {done:0.##}/{t.Planned:0.##}" + (t.Status == TripStatus.Completed ? $" — còn chờ nhập {t.Planned - done:0.##}" : "")
                         : t.Status == TripStatus.PendingConfirmation ? "⏳ Chờ duyệt — duyệt xong chuyển sang Chờ nhập kho"
                         : t.Status == TripStatus.Completed ? $"📦 Đã về kho — CHỜ NHẬP {t.Planned:0.##} (màn Kho → 📦 Nhập hàng từ chuyến)"
                         : "⏳ Chưa về kho";
            else
                progress = done >= t.Planned && t.Planned > 0 ? $"✅ Đã xuất kho {done:0.##}/{t.Planned:0.##}"
                         : done > 0 ? $"📤 Đã xuất {done:0.##}/{t.Planned:0.##}"
                         : t.LegType == TripLegType.Return ? "⏳ Xuất kho khi chuyến trả hàng hoàn tất"
                         : "⏳ Xuất kho khi chuyến bắt đầu";

            return new TripStockFlowDto(t.Id, LegText(t.LegType), TripStatusText(t.Status), t.Driver, t.Planned, done, progress);
        }).ToList();

        return new OrderStockFlowDto(order.Id, order.Code,
            order.PickupMethod == PickupMethod.SelfDelivery ? "Khách tự mang tới kho" : "Công ty cử xe lấy hàng",
            cargoDtos, tripDtos);
    }

    private static string TripStatusText(TripStatus s) => s switch
    {
        TripStatus.Planned => "Đã lên kế hoạch",
        TripStatus.InProgress => "Đang chạy",
        TripStatus.PendingConfirmation => "Chờ xác nhận",
        TripStatus.Completed => "Hoàn tất",
        TripStatus.Cancelled => "Đã hủy",
        _ => s.ToString(),
    };
}