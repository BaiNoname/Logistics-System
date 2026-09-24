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

        if (request.CargoLines is null || request.CargoLines.Count == 0)
            throw new InvalidOperationException("Đơn hàng phải có ít nhất 1 dòng hàng hóa.");

        var code = $"TO-{DateTime.UtcNow:yyyyMMddHHmmss}";

        var order = new TransportOrder
        {
            Code = code,
            CustomerId = request.CustomerId,
            PickupPoint = request.PickupPoint,
            DropoffPoint = request.DropoffPoint,
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

        order.PickupPoint = request.PickupPoint;
        order.DropoffPoint = request.DropoffPoint;
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

    public async Task<OrderActionResult> CancelAsync(int id)
    {
        var order = await _db.TransportOrders.FindAsync(id);
        if (order is null) return new OrderActionResult(false, "Không tìm thấy đơn hàng.");

        if (order.Status is TransportOrderStatus.InProgress or TransportOrderStatus.Done)
            return new OrderActionResult(false, "Không thể hủy đơn đã có xe đang chạy hoặc đã hoàn tất.");
        if (order.Status == TransportOrderStatus.Cancelled)
            return new OrderActionResult(false, "Đơn này đã bị hủy từ trước.");

        // CHẶN hủy nếu đơn ĐÃ được chia xe (có Trip) — kể cả khi Status vẫn
        // đang là "Confirmed" (chưa chuyển InProgress, việc này chỉ xảy ra
        // khi 1 Trip THẬT SỰ bắt đầu chạy). Lỗ hổng thật: đơn SelfDelivery
        // nhập kho NGAY khi Chia xe (trước khi Trip nào bắt đầu chạy) — nếu
        // chỉ dựa vào Status, Dispatcher vẫn hủy được đơn NGAY SAU KHI hàng
        // đã vào kho, để lại tồn kho "mồ côi" cho 1 đơn đã hủy.
        var hasTrips = await _db.Trips.AnyAsync(t => t.TransportOrderId == id);
        if (hasTrips)
            return new OrderActionResult(false, "Đơn này đã được chia xe (có thể hàng đã vào kho) — không thể hủy. Cần xử lý thủ công (xuất trả hàng...) trước khi hủy.");

        order.Status = TransportOrderStatus.Cancelled;
        await _db.SaveChangesAsync();
        return new OrderActionResult(true, null);
    }

    private static PickupMethod ParsePickupMethod(string value)
        => Enum.TryParse<PickupMethod>(value, out var parsed) ? parsed : PickupMethod.CompanyPickup;

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
        order.PickupMethod.ToString(), order.CustomerId
    );
}