using Microsoft.EntityFrameworkCore;
using TruckingSystem.Application.Common.Interfaces;
using TruckingSystem.Domain.Entities;
using TruckingSystem.Domain.Enums;

namespace TruckingSystem.Application.VehicleAllocation;

/// <summary>
/// Cài đặt thuật toán Tham lam (Greedy) cho bài toán phân bổ xe theo tải trọng.
/// Ưu tiên xe có sức chứa còn trống LỚN NHẤT trước, gán tối đa có thể vào 1
/// chuyến, lặp lại cho tới khi đủ hàng hoặc hết xe. Độ phức tạp O(n log n).
/// Mỗi xe được gán sẽ tự động sinh ra 1 Trip + TripLine tương ứng.
/// </summary>
public class VehicleAllocationService : IVehicleAllocationService
{
    private readonly IApplicationDbContext _db;

    public VehicleAllocationService(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<AllocateVehiclesResponse> AllocateAsync(int transportOrderId)
    {
        var order = await _db.TransportOrders
            .Include(o => o.CargoLines)
            .FirstOrDefaultAsync(o => o.Id == transportOrderId);

        if (order is null)
            return new AllocateVehiclesResponse(false, 0, 0, new(), "Không tìm thấy đơn vận chuyển.");

        // Tổng số lượng cần chở = tổng QtyTotal của tất cả CargoLine trong đơn
        var totalQtyToShip = order.CargoLines.Sum(c => c.QtyTotal);
        if (totalQtyToShip <= 0)
            return new AllocateVehiclesResponse(false, 0, 0, new(), "Đơn hàng chưa khai báo số lượng hàng hóa.");

        // Bước 1: sắp xếp giảm dần theo sức chứa — cốt lõi của tính "tham lam"
        var availableVehicles = await _db.Vehicles
            .Where(v => v.Status == VehicleStatus.Available)
            .OrderByDescending(v => v.CapacityQty)
            .ToListAsync();

        if (!availableVehicles.Any())
            return new AllocateVehiclesResponse(false, totalQtyToShip, 0, new(), "Không còn xe nào đang rảnh.");

        var remaining = totalQtyToShip;
        var results = new List<AllocationResultItem>();

        // Chỉ lấy 1 CargoLine đại diện để gán vào TripLine (đơn giản hoá: coi cả đơn là 1 loại hàng gộp)
        var primaryCargoLine = order.CargoLines.First();

        // Bước 2: duyệt qua từng xe, gán tối đa có thể, dừng khi đủ hàng hoặc hết xe
        foreach (var vehicle in availableVehicles)
        {
            if (remaining <= 0) break;

            var qtyAssigned = Math.Min(remaining, vehicle.CapacityQty);

            var trip = new Trip
            {
                TransportOrderId = order.Id,
                VehicleId = vehicle.Id,
                // Đơn giản hoá: gán tạm tài xế đầu tiên tìm thấy có hồ sơ DriverProfile.
                // Cải tiến sau: cho phép Dispatcher chọn tài xế cụ thể trước khi xác nhận.
                DriverEmployeeId = await GetAnyDriverEmployeeIdAsync(),
                Status = TripStatus.Planned,
                LegType = TripLegType.Delivery,
            };
            _db.Trips.Add(trip);
            await _db.SaveChangesAsync(); // cần Id thật của Trip trước khi tạo TripLine

            _db.TripLines.Add(new TripLine
            {
                TripId = trip.Id,
                CargoLineId = primaryCargoLine.Id,
                QtyPlanned = qtyAssigned,
            });

            vehicle.Status = VehicleStatus.OnTrip; // xe đã được gán, không còn rảnh nữa

            results.Add(new AllocationResultItem(trip.Id, vehicle.PlateNumber, qtyAssigned));
            remaining -= qtyAssigned;
        }

        // Đơn chuyển sang Confirmed vì đã có chuyến được lên kế hoạch
        if (order.Status == TransportOrderStatus.Draft)
            order.Status = TransportOrderStatus.Confirmed;

        _db.AuditLogs.Add(new Domain.Entities.AuditLog
        {
            Action = "AllocateVehicles",
            EntityName = nameof(TransportOrder),
            EntityId = order.Id,
            Detail = $"Thuật toán Greedy đã gán {results.Count} xe, tổng {totalQtyToShip - remaining}/{totalQtyToShip}.",
        });

        await _db.SaveChangesAsync();

        var message = remaining > 0
            ? $"Chỉ đủ xe chở {totalQtyToShip - remaining}/{totalQtyToShip} — còn thiếu {remaining}, cần thêm xe hoặc chờ xe rảnh."
            : null;

        return new AllocateVehiclesResponse(true, totalQtyToShip, totalQtyToShip - remaining, results, message);
    }

    private async Task<int> GetAnyDriverEmployeeIdAsync()
    {
        var driver = await _db.Employees
            .Where(e => e.Status == EmployeeStatus.Active)
            .OrderBy(e => e.Id)
            .FirstOrDefaultAsync();

        return driver?.Id ?? 0;
    }
}
