using Microsoft.EntityFrameworkCore;
using TruckingSystem.Application.Common;
using TruckingSystem.Application.Common.Interfaces;
using TruckingSystem.Domain.Entities;
using TruckingSystem.Domain.Enums;

namespace TruckingSystem.Application.TransportOrders;

public class TransportOrderService : ITransportOrderService
{
    private readonly IApplicationDbContext _db;

    public TransportOrderService(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<TransportOrderListItemDto>> GetAllAsync()
    {
        return await _db.TransportOrders
            .Include(o => o.Customer)
            .OrderByDescending(o => o.Id)
            .Select(o => new TransportOrderListItemDto(
                o.Id, o.Code, o.Customer.Name, o.Status.ToString(),
                o.ProgressPercent, o.PickupPoint, o.DropoffPoint, o.PublicTrackingToken,
                o.Trips.Count(t => t.Status != TripStatus.Cancelled)))
            .ToListAsync();
    }

    public async Task<TransportOrderDetailDto?> GetByIdAsync(int id)
    {
        var order = await _db.TransportOrders
            .Include(o => o.Customer)
            .Include(o => o.Warehouse)
            .Include(o => o.CargoLines)
            .Include(o => o.PickupLocation)
            .Include(o => o.DropoffLocation)
            .Include(o => o.Trips).ThenInclude(t => t.Vehicle)
            .Include(o => o.Trips).ThenInclude(t => t.DriverEmployee)
            .Include(o => o.Trips).ThenInclude(t => t.TripLines)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order is null) return null;

        return MapToDetail(order);
    }

    public async Task<TransportOrderDetailDto> CreateAsync(CreateTransportOrderRequest request)
    {
        var customer = await _db.Customers.FindAsync(request.CustomerId)
            ?? throw new InvalidOperationException("Khách hàng không tồn tại.");

        var warehouse = await _db.Warehouses.FindAsync(request.WarehouseId)
            ?? throw new InvalidOperationException("Kho trung chuyển đã chọn không tồn tại.");

        if (request.CargoLines is null || request.CargoLines.Count == 0)
            throw new InvalidOperationException("Đơn hàng phải có ít nhất 1 dòng hàng hóa.");

        var cargoLineError = ValidateCargoLines(request.CargoLines);
        if (cargoLineError is not null)
            throw new InvalidOperationException(cargoLineError);

        var locationError = await ValidateLocationsAsync(request.PickupMethod, request.PickupLocationId, request.DropoffLocationId, request.CustomerId);
        if (locationError is not null)
            throw new InvalidOperationException(locationError);

        // Mã đơn theo giây — 2 đơn tạo cùng 1 giây trước đây bị TRÙNG mã. Giờ
        // nếu mã đã có thì thêm hậu tố -2, -3...
        var baseCode = $"TO-{DateTime.UtcNow:yyyyMMddHHmmss}";
        var code = baseCode;
        for (var n = 2; await _db.TransportOrders.AnyAsync(o => o.Code == code); n++)
            code = $"{baseCode}-{n}";

        var order = new TransportOrder
        {
            Code = code,
            CustomerId = request.CustomerId,
            PickupPoint = request.PickupPoint,
            DropoffPoint = request.DropoffPoint,
            WarehouseId = request.WarehouseId,
            CommittedDeliveryDate = VnClock.ToVnWallClock(request.CommittedDeliveryDate), // lưu theo GIỜ VN
            Status = TransportOrderStatus.Draft,
            PickupLocationId = request.PickupLocationId,
            DropoffLocationId = request.DropoffLocationId,
            PickupMethod = ParsePickupMethod(request.PickupMethod),
            CargoLines = request.CargoLines.Select(MapCargoLine).ToList(),
        };

        _db.TransportOrders.Add(order);
        await _db.SaveChangesAsync();

        // Tải lại kèm Location để MapToDetail lấy được tọa độ Lat/Lng (nếu có chọn)
        if (request.PickupLocationId.HasValue)
            order.PickupLocation = await _db.Locations.FindAsync(request.PickupLocationId.Value);
        if (request.DropoffLocationId.HasValue)
            order.DropoffLocation = await _db.Locations.FindAsync(request.DropoffLocationId.Value);

        order.Customer = customer; // gán tay để MapToDetail không phải query lại
        order.Warehouse = warehouse;
        order.Trips = new List<Trip>(); // đơn mới tạo chưa có chuyến nào
        return MapToDetail(order);
    }

    public async Task<OrderActionResult> UpdateAsync(int id, UpdateTransportOrderRequest request)
    {
        var order = await _db.TransportOrders
            .Include(o => o.CargoLines)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order is null) return new OrderActionResult(false, "Không tìm thấy đơn hàng.");
        if (order.Status != TransportOrderStatus.Draft)
            return new OrderActionResult(false, "Chỉ có thể sửa đơn khi còn ở trạng thái Nháp (Draft) — đơn này đã xác nhận/đang chạy.");

        // Sửa đơn sẽ XÓA rồi tạo lại các dòng hàng — nếu đã có phiếu nhập kho (dữ liệu
        // cũ) thì xóa kéo theo mất phiếu nhập nhưng vị trí kho vẫn bị tính là đầy.
        var cargoLineIds = order.CargoLines.Select(c => c.Id).ToList();
        if (await _db.GoodsReceipts.AnyAsync(r => cargoLineIds.Contains(r.CargoLineId)))
            return new OrderActionResult(false, "Đơn này đã có hàng nhập kho — không thể sửa dòng hàng nữa.");

        if (request.CargoLines is null || request.CargoLines.Count == 0)
            return new OrderActionResult(false, "Đơn hàng phải có ít nhất 1 dòng hàng hóa.");

        var cargoLineError = ValidateCargoLines(request.CargoLines);
        if (cargoLineError is not null)
            return new OrderActionResult(false, cargoLineError);

        var warehouseExists = await _db.Warehouses.AnyAsync(w => w.Id == request.WarehouseId);
        if (!warehouseExists)
            return new OrderActionResult(false, "Kho trung chuyển đã chọn không tồn tại.");

        var locationError = await ValidateLocationsAsync(request.PickupMethod, request.PickupLocationId, request.DropoffLocationId, order.CustomerId);
        if (locationError is not null)
            return new OrderActionResult(false, locationError);

        order.PickupPoint = request.PickupPoint;
        order.DropoffPoint = request.DropoffPoint;
        order.WarehouseId = request.WarehouseId;
        order.CommittedDeliveryDate = VnClock.ToVnWallClock(request.CommittedDeliveryDate); // lưu theo GIỜ VN
        order.PickupLocationId = request.PickupLocationId;
        order.DropoffLocationId = request.DropoffLocationId;
        order.PickupMethod = ParsePickupMethod(request.PickupMethod);

        // Thay hẳn toàn bộ danh sách dòng hàng cũ bằng danh sách mới — đơn giản
        // và an toàn hơn nhiều so với dò từng dòng để so sánh thêm/sửa/xóa,
        // chấp nhận được vì đơn còn Draft nên chưa gắn Trip/StorageAssignment nào.
        _db.CargoLines.RemoveRange(order.CargoLines);
        order.CargoLines = request.CargoLines.Select(MapCargoLine).ToList();

        await _db.SaveChangesAsync();
        return new OrderActionResult(true, null);
    }

    public async Task<OrderActionResult> ConfirmAsync(int id)
    {
        var order = await _db.TransportOrders
            .Include(o => o.CargoLines)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order is null) return new OrderActionResult(false, "Không tìm thấy đơn hàng.");
        if (order.Status != TransportOrderStatus.Draft)
            return new OrderActionResult(false, "Chỉ có thể xác nhận đơn đang ở trạng thái Nháp (Draft).");
        if (order.CargoLines.Count == 0)
            return new OrderActionResult(false, "Đơn hàng chưa có dòng hàng hóa nào — không thể xác nhận.");

        order.Status = TransportOrderStatus.Confirmed;
        await _db.SaveChangesAsync();
        return new OrderActionResult(true, null);
    }

    /// <summary>Hủy đơn — xử lý theo 3 mức tùy tình trạng hàng hóa thật tại
    /// thời điểm hủy (KHÔNG hủy lẻ từng Trip, chỉ hủy qua Đơn):
    ///   Mức 1 — Chưa chạm kho (Planned/InProgress, chưa Pickup xong): hủy
    ///     thẳng mọi Trip còn hoạt động, trả xe/tài xế rảnh ngay.
    ///   Mức 2 — Hàng đã nằm trong kho (Pickup đã Completed, hoặc
    ///     SelfDelivery vừa nhập ngay lúc chia xe) nhưng Giao hàng còn
    ///     Planned (chưa xuất): hủy Delivery cũ, TẠO THÊM 1 Trip "Trả hàng"
    ///     (LegType.Return) dùng lại xe/tài xế đã có sẵn gần kho. Xuất kho
    ///     thật + trả xe rảnh chỉ xảy ra SAU, khi tài xế chạy xong và bấm
    ///     Hoàn tất (tái dùng cơ chế Complete có sẵn, xem HandleArrivalAsync).
    ///   Mức 3 — Giao hàng đã InProgress (hàng đã xuất kho thật): CHẶN,
    ///     không cho hủy nữa.</summary>
    public async Task<OrderActionResult> CancelAsync(int id)
    {
        // KHÓA đơn + mọi chuyến của đơn NGAY ĐẦU (SELECT ... FOR UPDATE) rồi mới đọc
        // và kiểm tra. Trước đây đọc/kiểm tra xong mới mở transaction: đúng lúc đó app
        // tài xế gửi GPS bắt đầu chuyến Giao (đã xuất kho) hoặc Dispatcher Hoàn tất
        // chặng Lấy hàng (đã nhập kho) -> lệnh hủy vẫn ghi đè chuyến thành "Đã hủy" dựa
        // trên dữ liệu cũ. Bắt đầu / Hoàn tất chuyến cũng UPDATE đúng các dòng này
        // trong transaction của chúng -> 2 bên phải chờ nhau, bên sau đọc dữ liệu mới.
        // (Mọi lệnh return bên dưới khi chưa Commit -> transaction tự Rollback, nhả khóa.)
        await using var transaction = await _db.Database.BeginTransactionAsync();
        // Thứ tự khóa: CHUYẾN trước, ĐƠN sau — đúng thứ tự Bắt đầu/Hoàn tất chuyến
        // (giành dòng chuyến rồi mới cập nhật đơn), tránh 2 bên khóa chéo nhau (deadlock).
        await _db.Database.ExecuteSqlInterpolatedAsync($"SELECT 1 FROM \"Trips\" WHERE \"TransportOrderId\" = {id} ORDER BY \"Id\" FOR UPDATE");
        await _db.Database.ExecuteSqlInterpolatedAsync($"SELECT 1 FROM \"TransportOrders\" WHERE \"Id\" = {id} FOR UPDATE");

        var order = await _db.TransportOrders
            .Include(o => o.Trips).ThenInclude(t => t.Vehicle)
            .Include(o => o.Trips).ThenInclude(t => t.TripLines)
            .Include(o => o.CargoLines)
            .FirstOrDefaultAsync(o => o.Id == id);
        if (order is null) return new OrderActionResult(false, "Không tìm thấy đơn hàng.");

        if (order.Status == TransportOrderStatus.Done)
            return new OrderActionResult(false, "Đơn đã hoàn tất — không thể hủy.");
        if (order.Status == TransportOrderStatus.Cancelled)
            return new OrderActionResult(false, "Đơn này đã bị hủy từ trước.");

        // Mức 3 — CHẶN nếu BẤT KỲ chuyến Giao hàng nào (của bất kỳ xe nào) đã
        // xuất kho: đang chạy, đang chờ xác nhận, hoặc đã giao xong.
        if (order.Trips.Any(t => t.LegType == TripLegType.Delivery
                && t.Status is TripStatus.InProgress or TripStatus.PendingConfirmation or TripStatus.Completed))
            return new OrderActionResult(false, "Đã có xe xuất kho và đang/đã giao hàng — không thể hủy đơn nữa.");

        // CHẶN nếu có chuyến Lấy hàng đang "Chờ xác nhận": tài xế đã chở hàng về
        // kho nhưng hàng CHƯA được nhập (nhập kho chỉ xảy ra lúc Duyệt). Hủy lúc
        // này sẽ làm hàng nằm ở kho mà hệ thống không ghi nhận, cũng không tạo
        // chuyến Trả hàng. Phải Duyệt (nhập kho) hoặc Từ chối chuyến đó trước.
        var pendingPickupIds = order.Trips
            .Where(t => t.LegType == TripLegType.Pickup && t.Status == TripStatus.PendingConfirmation)
            .Select(t => t.Id).ToList();
        if (pendingPickupIds.Count > 0)
            return new OrderActionResult(false,
                "Chuyến lấy hàng #" + string.Join(", #", pendingPickupIds) +
                " đang CHỜ XÁC NHẬN (hàng đã về kho nhưng chưa nhập). " +
                "Hãy Duyệt hoặc Từ chối chuyến đó trước, rồi mới hủy đơn.");

        // Chuyến Lấy hàng đang chạy: vẫn cho hủy, nhưng báo lại để điều phối gọi
        // tài xế dừng (app tài xế sẽ tự ngừng tracking khi thấy chuyến bị hủy).
        var pickupsOnRoadIds = order.Trips
            .Where(t => t.LegType == TripLegType.Pickup && t.Status == TripStatus.InProgress)
            .Select(t => t.Id).ToList();
        string? cancelNote = pickupsOnRoadIds.Count > 0
            ? "Đã hủy đơn. Lưu ý: chuyến lấy hàng #" + string.Join(", #", pickupsOnRoadIds) +
              " đang trên đường — hãy liên hệ tài xế để dừng lại."
            : null;

        // SelfDelivery: hàng nhập kho NGAY lúc Chia xe -> có tồn kho thật là đủ.
        var isSelfDelivery = order.PickupMethod == PickupMethod.SelfDelivery;
        var cargoLineIds = order.CargoLines.Select(c => c.Id).ToList();
        var orderHasStock = isSelfDelivery && await _db.WarehouseStocks
            .AnyAsync(s => cargoLineIds.Contains(s.CargoLineId) && s.QtyOnHand > 0);

        var reason = "Hủy theo đơn hàng #" + order.Id;
        var now = DateTime.UtcNow;

        // Chia xe có thể tách 1 đơn ra NHIỀU XE — mỗi xe 1 cặp chuyến (Lấy hàng +
        // Giao hàng, cùng xe + tài xế). Xử lý RIÊNG TỪNG XE, vì mỗi xe có thể đang
        // ở giai đoạn khác nhau (xe A đã lấy hàng về kho, xe B còn đang đi lấy).
        var tripsByVehicle = order.Trips
            .Where(t => t.LegType != TripLegType.Return && t.Status != TripStatus.Cancelled)
            .GroupBy(t => t.VehicleId)
            .ToList();

        // KIỂM TRA TRƯỚC KHI ĐỔI GÌ: hàng của đơn ĐANG NẰM TRONG KHO phải có chuyến Trả
        // hàng mang đi — VD đơn khách tự mang tới đã nhập kho tay nhưng chưa Chia xe:
        // trước đây hủy đơn xong hàng nằm kẹt trong kho mãi. Chặn và hướng dẫn.
        decimal qtyCoveredByReturn = 0; // phần hàng chuyến Trả hàng (sắp tạo) sẽ mang đi
        foreach (var group in tripsByVehicle)
        {
            var pickup = group.FirstOrDefault(t => t.LegType == TripLegType.Pickup);
            var delivery = group.FirstOrDefault(t => t.LegType == TripLegType.Delivery);
            var goodsInWarehouse = isSelfDelivery ? orderHasStock : pickup?.Status == TripStatus.Completed;
            if (goodsInWarehouse && delivery != null) qtyCoveredByReturn += delivery.TripLines.Sum(l => l.QtyPlanned);
        }
        var activeReturnQty = order.Trips
            .Where(t => t.LegType == TripLegType.Return && t.Status != TripStatus.Cancelled && t.Status != TripStatus.Completed)
            .SelectMany(t => t.TripLines)
            .Sum(l => l.QtyPlanned);
        var qtyInWarehouse = await _db.WarehouseStocks
            .Where(s => cargoLineIds.Contains(s.CargoLineId) && s.QtyOnHand > 0)
            .SumAsync(s => (decimal?)s.QtyOnHand) ?? 0m;
        var uncovered = qtyInWarehouse - qtyCoveredByReturn - activeReturnQty;
        if (uncovered > 0)
            return new OrderActionResult(false,
                $"Đơn còn {uncovered:0.##} đơn vị hàng đang nằm trong kho mà không có chuyến Trả hàng nào mang đi. " +
                "Hãy xuất kho thủ công phần hàng này (màn Kho → Xuất kho) cho khách nhận lại, rồi mới hủy đơn.");

        foreach (var group in tripsByVehicle)
        {
            var pickup = group.FirstOrDefault(t => t.LegType == TripLegType.Pickup);
            var delivery = group.FirstOrDefault(t => t.LegType == TripLegType.Delivery);

            // Hàng của RIÊNG xe này đã thật sự nằm trong kho chưa?
            var goodsInWarehouse = isSelfDelivery ? orderHasStock : pickup?.Status == TripStatus.Completed;

            if (goodsInWarehouse && delivery != null)
            {
                // Mức 2 — tạo chuyến Trả hàng cho ĐÚNG phần hàng của xe này, dùng
                // lại xe/tài xế đang ở sẵn gần kho. Xe CHƯA rảnh, còn phải chạy
                // chuyến trả hàng (xe rảnh lại khi chuyến đó được Duyệt/Hoàn tất).
                _db.Trips.Add(new Trip
                {
                    TransportOrderId = order.Id,
                    VehicleId = delivery.VehicleId,
                    DriverEmployeeId = delivery.DriverEmployeeId,
                    WarehouseId = delivery.WarehouseId,
                    LegType = TripLegType.Return,
                    Status = TripStatus.Planned,
                    TripLines = delivery.TripLines.Select(l => new TripLine
                    {
                        CargoLineId = l.CargoLineId,
                        QtyPlanned = l.QtyPlanned,
                    }).ToList(),
                });

                delivery.Status = TripStatus.Cancelled;
                delivery.CancelReason = reason;
            }
            else
            {
                // Mức 1 — hàng của xe này chưa vào kho -> hủy thẳng mọi chuyến còn
                // hoạt động của xe này (gồm cả Lấy hàng đã báo xong nhưng CHƯA
                // duyệt: hàng chưa nhập kho, nhập kho chỉ xảy ra lúc duyệt), trả xe.
                foreach (var trip in group.Where(t =>
                    t.Status is TripStatus.Planned or TripStatus.InProgress or TripStatus.PendingConfirmation))
                {
                    trip.Status = TripStatus.Cancelled;
                    trip.CancelReason = reason;
                    // Chuyến đã chạy dở -> ghi giờ kết thúc = lúc hủy (giới hạn khung giờ
                    // nhận video, thời lượng chuyến trong báo cáo).
                    if (trip.StartTime.HasValue) trip.EndTime ??= now;
                }
                // Chỉ trả xe về Rảnh nếu xe đang "Đang chạy" — không ghi đè Bảo trì /
                // Ngưng hoạt động mà người dùng đã đặt.
                var vehicle = group.First().Vehicle;
                if (vehicle != null && vehicle.Status == VehicleStatus.OnTrip) vehicle.Status = VehicleStatus.Available;
            }
        }

        order.Status = TransportOrderStatus.Cancelled;
        order.UpdatedAt = now;
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();
        return new OrderActionResult(true, cancelNote);
    }

    /// <summary>Điểm giao (và điểm lấy, nếu công ty cử xe đi lấy) BẮT BUỘC chọn
    /// từ danh mục Địa điểm — địa chỉ gõ tay không có tọa độ, khiến bản đồ, ước
    /// tính quãng đường/số ngày chạy và nhiên liệu đều không tính được.</summary>
    private async Task<string?> ValidateLocationsAsync(string? pickupMethod, int? pickupLocationId, int? dropoffLocationId, int customerId)
    {
        var method = ParsePickupMethod(pickupMethod ?? "");
        if (method == PickupMethod.CompanyPickup && !pickupLocationId.HasValue)
            return "Vui lòng chọn Điểm lấy hàng từ danh sách Địa điểm (công ty cử xe đến lấy cần tọa độ).";
        if (!dropoffLocationId.HasValue)
            return "Vui lòng chọn Điểm giao hàng từ danh sách Địa điểm.";

        var ids = new[] { pickupLocationId, dropoffLocationId }.Where(i => i.HasValue).Select(i => i!.Value).Distinct().ToList();
        var locations = await _db.Locations
            .Where(l => ids.Contains(l.Id))
            .Select(l => new { l.Id, l.Name, l.Lat, l.Lng, l.CustomerId })
            .ToListAsync();
        if (locations.Count != ids.Count) return "Địa điểm đã chọn không tồn tại.";

        // Bắt buộc có TỌA ĐỘ (đúng lý do phải chọn từ danh mục) — địa điểm thiếu tọa độ
        // làm Chia xe / ước tính quãng đường / nhiên liệu không tính được.
        var noCoords = locations.FirstOrDefault(l => l.Lat is null || l.Lng is null);
        if (noCoords is not null)
            return $"Địa điểm \"{noCoords.Name}\" chưa có tọa độ — chọn địa điểm khác hoặc tạo lại địa điểm qua ô tìm kiếm bản đồ.";

        // Địa điểm RIÊNG của 1 khách hàng chỉ dùng cho đơn của chính khách đó.
        var otherCustomers = locations.FirstOrDefault(l => l.CustomerId.HasValue && l.CustomerId.Value != customerId);
        if (otherCustomers is not null)
            return $"Địa điểm \"{otherCustomers.Name}\" là địa chỉ riêng của khách hàng khác — không dùng cho đơn này.";
        return null;
    }

    /// <summary>Danh mục loại hàng gợi ý sẵn — người dùng vẫn gõ được loại mới;
    /// loại mới dùng rồi sẽ tự xuất hiện trong gợi ý lần sau.</summary>
    private static readonly string[] StandardCargoTypes =
    {
        "Hàng khô đóng kiện", "Hàng điện tử", "Đồ gia dụng", "Thực phẩm khô", "Đồ uống",
        "Hàng may mặc", "Vật liệu xây dựng", "Máy móc thiết bị", "Hàng dễ vỡ (gốm sứ, thủy tinh)",
        "Hóa chất", "Hàng nguy hiểm (xăng dầu, gas)",
    };

    public async Task<List<string>> GetCargoTypeSuggestionsAsync()
    {
        var used = await _db.CargoLines.Select(c => c.CargoType).Distinct().ToListAsync();
        return StandardCargoTypes.Concat(used)
            .Select(t => (t ?? "").Trim())
            .Where(t => t.Length > 0)
            .GroupBy(t => t, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .OrderBy(t => t)
            .ToList();
    }

    private static PickupMethod ParsePickupMethod(string value)
        => Enum.TryParse<PickupMethod>(value, out var parsed) ? parsed : PickupMethod.CompanyPickup;

    /// <summary>Kiểm tra TẤT CẢ dòng hàng đều có số liệu HỢP LỆ (dương) trước
    /// khi cho tạo/sửa đơn — trước đây KHÔNG kiểm tra gì cả, có thể tạo 1
    /// dòng hàng với số lượng/khối lượng/thể tích = 0 hoặc ÂM, gây sai lệch
    /// khi tính tổng ở bước Chia xe (Greedy) sau này. Trả về null nếu hợp lệ,
    /// hoặc thông báo lỗi nếu có dòng nào sai.</summary>
    private static string? ValidateCargoLines(List<CargoLineDto> cargoLines)
    {
        for (int i = 0; i < cargoLines.Count; i++)
        {
            var c = cargoLines[i];
            var lineLabel = string.IsNullOrWhiteSpace(c.CargoType) ? $"Dòng hàng #{i + 1}" : $"Dòng hàng \"{c.CargoType}\"";

            if (string.IsNullOrWhiteSpace(c.CargoType))
                return $"Dòng hàng #{i + 1} chưa nhập loại hàng hóa.";
            if (c.QtyTotal <= 0)
                return $"{lineLabel}: số lượng phải lớn hơn 0.";
            if (c.PackageCount <= 0)
                return $"{lineLabel}: số kiện phải lớn hơn 0.";
            if (c.WeightKg <= 0)
                return $"{lineLabel}: khối lượng phải lớn hơn 0.";
            if (c.VolumeM3 <= 0)
                return $"{lineLabel}: thể tích phải lớn hơn 0.";
        }
        return null;
    }

    private static CargoLine MapCargoLine(CargoLineDto c) => new()
    {
        CargoType = c.CargoType,
        PackageCount = c.PackageCount,
        WeightKg = c.WeightKg,
        VolumeM3 = c.VolumeM3,
        IsFragile = c.IsFragile,
        IsHazmat = c.IsHazmat,
        QtyTotal = c.QtyTotal,
    };

    private static TransportOrderDetailDto MapToDetail(TransportOrder order) => new(
        order.Id, order.Code, order.Customer.Name, order.Status.ToString(), order.ProgressPercent,
        order.PickupPoint, order.DropoffPoint, order.CommittedDeliveryDate, order.PublicTrackingToken,
        order.CargoLines.Select(c => new CargoLineDto(
            c.CargoType, c.PackageCount, c.WeightKg, c.VolumeM3, c.IsFragile, c.IsHazmat, c.QtyTotal)).ToList(),
        order.Trips.Select(t => new TripSummaryDto(
            t.Id, t.Vehicle.PlateNumber, t.DriverEmployee.FullName, t.Status.ToString(),
            t.TripLines.Sum(tl => tl.QtyPlanned))).ToList(),
        order.PickupLocation?.Lat, order.PickupLocation?.Lng,
        order.DropoffLocation?.Lat, order.DropoffLocation?.Lng,
        order.PickupMethod.ToString(), order.CustomerId, order.WarehouseId, order.Warehouse.Name,
        order.PickupLocationId, order.DropoffLocationId
    );
}