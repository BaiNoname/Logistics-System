using Microsoft.EntityFrameworkCore;
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
                o.ProgressPercent, o.PickupPoint, o.DropoffPoint, o.PublicTrackingToken))
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

        var code = $"TO-{DateTime.UtcNow:yyyyMMddHHmmss}";

        var order = new TransportOrder
        {
            Code = code,
            CustomerId = request.CustomerId,
            PickupPoint = request.PickupPoint,
            DropoffPoint = request.DropoffPoint,
            WarehouseId = request.WarehouseId,
            CommittedDeliveryDate = request.CommittedDeliveryDate,
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
        if (request.CargoLines is null || request.CargoLines.Count == 0)
            return new OrderActionResult(false, "Đơn hàng phải có ít nhất 1 dòng hàng hóa.");

        var cargoLineError = ValidateCargoLines(request.CargoLines);
        if (cargoLineError is not null)
            return new OrderActionResult(false, cargoLineError);

        var warehouseExists = await _db.Warehouses.AnyAsync(w => w.Id == request.WarehouseId);
        if (!warehouseExists)
            return new OrderActionResult(false, "Kho trung chuyển đã chọn không tồn tại.");

        order.PickupPoint = request.PickupPoint;
        order.DropoffPoint = request.DropoffPoint;
        order.WarehouseId = request.WarehouseId;
        order.CommittedDeliveryDate = request.CommittedDeliveryDate;
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

        var deliveryTrip = order.Trips.FirstOrDefault(t => t.LegType == TripLegType.Delivery);
        var pickupTrip = order.Trips.FirstOrDefault(t => t.LegType == TripLegType.Pickup);

        // Mức 3 — CHẶN nếu Giao hàng đã xuất kho (InProgress trở lên)
        if (deliveryTrip != null && deliveryTrip.Status is TripStatus.InProgress or TripStatus.Completed)
            return new OrderActionResult(false, "Hàng đã xuất kho và đang/đã giao đi — không thể hủy đơn nữa.");

        // Đơn này ĐÃ CÓ tồn kho thật chưa? (CompanyPickup: Pickup Completed;
        // SelfDelivery: hàng nhập ngay lúc chia xe, Delivery Planned là đủ)
        var cargoLineIds = order.CargoLines.Select(c => c.Id).ToList();
        var hasRealStock = await _db.WarehouseStocks
            .AnyAsync(s => cargoLineIds.Contains(s.CargoLineId) && s.QtyOnHand > 0);

        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            if (hasRealStock && deliveryTrip != null)
            {
                // Mức 2 — Xác định xe/tài xế dùng cho chuyến Trả hàng: ưu
                // tiên Pickup Trip (nếu có, CompanyPickup); nếu không thì
                // dùng chính Delivery Trip hiện tại (SelfDelivery — xe/tài
                // xế duy nhất từng gán, đã sẵn ở kho).
                var sourceTrip = pickupTrip ?? deliveryTrip;

                var returnTrip = new Trip
                {
                    TransportOrderId = order.Id,
                    VehicleId = sourceTrip.VehicleId,
                    DriverEmployeeId = sourceTrip.DriverEmployeeId,
                    WarehouseId = sourceTrip.WarehouseId,
                    LegType = TripLegType.Return,
                    Status = TripStatus.Planned,
                    TripLines = deliveryTrip.TripLines.Select(l => new TripLine
                    {
                        CargoLineId = l.CargoLineId,
                        QtyPlanned = l.QtyPlanned,
                    }).ToList(),
                };
                _db.Trips.Add(returnTrip);

                deliveryTrip.Status = TripStatus.Cancelled;
                deliveryTrip.CancelReason = "Hủy theo đơn hàng #" + order.Id;
                // Xe CHƯA rảnh ngay — còn phải chạy xong chuyến Trả hàng đã
                // tạo ở trên (giữ nguyên Vehicle.Status = OnTrip).
            }
            else
            {
                // Mức 1 — Chưa có tồn kho thật nào -> hủy thẳng mọi Trip còn
                // hoạt động, trả xe rảnh ngay.
                foreach (var trip in order.Trips.Where(t =>
                    t.Status == TripStatus.Planned || t.Status == TripStatus.InProgress))
                {
                    trip.Status = TripStatus.Cancelled;
                    trip.CancelReason = "Hủy theo đơn hàng #" + order.Id;
                    if (trip.Vehicle != null)
                        trip.Vehicle.Status = VehicleStatus.Available;
                }
            }

            order.Status = TransportOrderStatus.Cancelled;
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
            return new OrderActionResult(true, null);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
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
        order.PickupMethod.ToString(), order.CustomerId, order.WarehouseId, order.Warehouse.Name
    );
}