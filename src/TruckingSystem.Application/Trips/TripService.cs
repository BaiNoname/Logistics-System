using Microsoft.EntityFrameworkCore;
using TruckingSystem.Application.Common.Interfaces;
using TruckingSystem.Application.Warehouses;
using TruckingSystem.Domain.Entities;
using TruckingSystem.Domain.Enums;

namespace TruckingSystem.Application.Trips;

/// <summary>
/// Gộp 2 nhóm chức năng dùng chung 1 Trip:
///   1. Quản lý Trip cho Dispatcher (Desktop) — xem danh sách, đổi trạng thái
///      thủ công (chưa có GPS thật để tự động).
///   2. App tài xế gọi trực tiếp — ghi nhận GPS ping, xác nhận đến nơi.
/// CẢ 2 nhóm dùng chung 1 hàm lõi HandleArrivalAsync() để đảm bảo xử lý ĐÚNG
/// theo LegType dù được gọi từ Desktop hay từ App tài xế — tránh lặp lại lỗi
/// cũ: coi MỌI lần "đến nơi" là giao hàng xong toàn bộ đơn.
///
/// ĐÃ NỐI ĐẦY ĐỦ với Kho trung chuyển (IWarehouseSlottingService):
///   - Chặng Pickup hoàn tất -> tự động ReceiveGoodsAsync (tạo GoodsReceipt +
///     chạy Best-Fit chọn vị trí + cộng WarehouseStock)
///   - Chặng Delivery bắt đầu chạy -> tự động IssueGoodsAsync (tạo GoodsIssue
///     + trừ dần StorageAssignment theo FIFO + trừ WarehouseStock)
///   - Đơn SelfDelivery (không có chặng Pickup) -> nhập kho NGAY lúc Chia xe,
///     xem VehicleAllocationService.
/// </summary>
public class TripService : ITripService
{
    private readonly IApplicationDbContext _db;
    private readonly IWarehouseSlottingService _warehouseService;

    public TripService(IApplicationDbContext db, IWarehouseSlottingService warehouseService)
    {
        _db = db;
        _warehouseService = warehouseService;
    }

    // ================= Quản lý Trip (Desktop) =================

    public async Task<List<TripListItemDto>> GetAllAsync()
    {
        return await _db.Trips
            .Include(t => t.TransportOrder)
            .Include(t => t.Vehicle)
            .Include(t => t.DriverEmployee)
            .Include(t => t.TripLines)
            .OrderByDescending(t => t.Id)
            .Select(t => new TripListItemDto(
                t.Id, t.TransportOrder.Code, t.Vehicle.PlateNumber, t.DriverEmployee.FullName,
                t.Status.ToString(), t.LegType.ToString(), t.TripLines.Sum(tl => tl.QtyPlanned)))
            .ToListAsync();
    }

    public async Task<TripActionResult> StartAsync(int id)
    {
        var trip = await _db.Trips
            .Include(t => t.TransportOrder)
            .Include(t => t.TripLines)
            .FirstOrDefaultAsync(t => t.Id == id);
        if (trip is null) return new TripActionResult(false, "Không tìm thấy chuyến.");
        if (trip.Status != TripStatus.Planned)
            return new TripActionResult(false, "Chỉ có thể bắt đầu chuyến đang ở trạng thái Đã lên kế hoạch (Planned).");

        // Chặng Delivery bắt đầu chạy = hàng THẬT SỰ rời khỏi kho — xuất kho
        // ngay tại đây (không đợi tới lúc giao xong, vì giao xong đã sang xe khác chở đi rồi).
        if (trip.LegType == TripLegType.Delivery && trip.WarehouseId.HasValue)
        {
            foreach (var line in trip.TripLines)
            {
                var issueResult = await _warehouseService.IssueGoodsAsync(
                    trip.WarehouseId.Value, trip.TransportOrderId, line.CargoLineId,
                    line.QtyPlanned, trip.DriverEmployeeId, trip.Id);

                if (!issueResult.Success)
                    return new TripActionResult(false, "Không thể bắt đầu chuyến — " + issueResult.Message);
            }
        }

        trip.Status = TripStatus.InProgress;
        trip.StartTime = DateTime.UtcNow;

        if (trip.TransportOrder.Status == TransportOrderStatus.Confirmed)
            trip.TransportOrder.Status = TransportOrderStatus.InProgress;

        await _db.SaveChangesAsync();
        return new TripActionResult(true, null);
    }

    public async Task<TripActionResult> CompleteAsync(int id)
    {
        var trip = await LoadTripForArrivalAsync(id);
        if (trip is null) return new TripActionResult(false, "Không tìm thấy chuyến.");
        if (trip.Status != TripStatus.InProgress)
            return new TripActionResult(false, "Chỉ có thể hoàn tất chuyến đang ở trạng thái Đang chạy (InProgress).");

        await HandleArrivalAsync(trip);
        await _db.SaveChangesAsync();
        return new TripActionResult(true, null);
    }

    public async Task<TripActionResult> RecordFuelAsync(int id, RecordFuelRequest request)
    {
        var trip = await _db.Trips.FirstOrDefaultAsync(t => t.Id == id);
        if (trip is null) return new TripActionResult(false, "Không tìm thấy chuyến.");
        if (request.Liters <= 0 || request.Cost <= 0)
            return new TripActionResult(false, "Số lít và chi phí phải lớn hơn 0.");

        _db.FuelLogs.Add(new FuelLog
        {
            TripId = id,
            Liters = request.Liters,
            Cost = request.Cost,
            StationName = request.StationName,
        });

        trip.ActualFuelCost += request.Cost;

        await _db.SaveChangesAsync();
        return new TripActionResult(true, null);
    }

    // ================= App tài xế (đã có từ trước) =================

    public async Task<ServiceResult> PingAsync(int tripId, GpsPingRequest request)
    {
        var trip = await _db.Trips.FirstOrDefaultAsync(t => t.Id == tripId);
        if (trip is null) return new ServiceResult(false, "Không tìm thấy chuyến.");

        _db.Waypoints.Add(new Waypoint
        {
            TripId = tripId,
            Lat = request.Lat,
            Lng = request.Lng,
            SpeedKmh = request.SpeedKmh,
            Accuracy = request.Accuracy,
            Timestamp = request.Timestamp ?? DateTime.UtcNow,
        });

        // Trip tự động chuyển InProgress ngay khi nhận điểm GPS đầu tiên (nếu còn Planned)
        if (trip.Status == TripStatus.Planned)
        {
            trip.Status = TripStatus.InProgress;
            trip.StartTime ??= DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        return new ServiceResult(true, null);
    }

    public async Task<ServiceResult> ConfirmArrivalAsync(int tripId, ConfirmArrivalRequest request)
    {
        var trip = await LoadTripForArrivalAsync(tripId);
        if (trip is null) return new ServiceResult(false, "Không tìm thấy chuyến.");
        if (trip.Status != TripStatus.InProgress)
            return new ServiceResult(false, "Chuyến này không ở trạng thái đang chạy.");

        await HandleArrivalAsync(trip);
        await _db.SaveChangesAsync();
        return new ServiceResult(true, null);
    }

    // ================= Logic lõi dùng chung =================

    private Task<Trip?> LoadTripForArrivalAsync(int id) => _db.Trips
        .Include(t => t.TransportOrder).ThenInclude(o => o.CargoLines)
        .Include(t => t.TransportOrder).ThenInclude(o => o.Trips).ThenInclude(t => t.TripLines)
        .Include(t => t.Vehicle)
        .Include(t => t.TripLines)
        .FirstOrDefaultAsync(t => t.Id == id);

    /// <summary>ĐÚNG logic phân biệt theo LegType — khác bản cũ coi MỌI lần
    /// "đến nơi" là giao hàng xong toàn bộ đơn:
    ///   - LegType.Pickup: hàng mới VỀ TỚI KHO, chưa giao cho khách — KHÔNG
    ///     tính vào tiến độ giao hàng, KHÔNG cho xe rảnh lại (thường tiếp tục
    ///     chặng Delivery ngay). Tự động ReceiveGoodsAsync (Best-Fit + nhập kho).
    ///   - LegType.Delivery: hàng đã tới tay khách thật sự — tính vào tiến độ,
    ///     xe được rảnh lại, có thể tự động chuyển đơn sang Done.</summary>
    private async Task HandleArrivalAsync(Trip trip)
    {
        trip.Status = TripStatus.Completed;
        trip.EndTime = DateTime.UtcNow;

        foreach (var line in trip.TripLines)
        {
            line.QtyDelivered = line.QtyPlanned;
            line.Status = TripLineStatus.Delivered;
        }

        if (trip.LegType == TripLegType.Delivery)
        {
            trip.Vehicle.Status = VehicleStatus.Available;
            RecalculateOrderProgress(trip.TransportOrder);
        }
        else if (trip.LegType == TripLegType.Pickup && trip.WarehouseId.HasValue)
        {
            // Xe về tới kho — hàng THẬT SỰ nhập kho tại đây: tạo GoodsReceipt,
            // tự chạy Best-Fit chọn vị trí, cộng WarehouseStock.
            await _db.SaveChangesAsync(); // lưu trạng thái Completed trước, tránh xung đột khi ReceiveGoodsAsync tự SaveChanges
            foreach (var line in trip.TripLines)
            {
                await _warehouseService.ReceiveGoodsAsync(
                    trip.WarehouseId.Value, trip.TransportOrderId, line.CargoLineId,
                    line.QtyPlanned, trip.DriverEmployeeId, trip.Id);
            }
            // Xe vẫn Status = OnTrip (đang tiếp tục chặng Delivery), không đổi
        }
    }

    private static void RecalculateOrderProgress(TransportOrder order)
    {
        var totalToShip = order.CargoLines.Sum(c => c.QtyTotal);
        var totalDelivered = order.Trips
            .Where(t => t.LegType == TripLegType.Delivery)
            .SelectMany(t => t.TripLines)
            .Sum(tl => tl.QtyDelivered);

        order.ProgressPercent = totalToShip == 0 ? 0 : Math.Round(totalDelivered / totalToShip * 100, 2);

        if (order.ProgressPercent >= 100 && order.Status == TransportOrderStatus.InProgress)
            order.Status = TransportOrderStatus.Done;
    }
}