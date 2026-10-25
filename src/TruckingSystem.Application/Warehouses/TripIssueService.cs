using Microsoft.EntityFrameworkCore;
using TruckingSystem.Application.Common;
using TruckingSystem.Application.Common.Interfaces;
using TruckingSystem.Domain.Enums;

namespace TruckingSystem.Application.Warehouses;

/// <summary>1 dòng hàng của 1 chuyến GIAO chưa chạy tại kho — màn "📤 Xuất kho theo chuyến".
/// State / CanIssue / CanUndo / BlockReason là của CẢ CHUYẾN (lặp lại trên mọi dòng của chuyến).
/// BinsText: hàng đang nằm ở ô nào (lô nhập trước xuất trước) để nhân viên kho soạn hàng.</summary>
public record TripIssueLineDto(int TripId, int TransportOrderId, string OrderCode, string CustomerName,
    string DriverName, string PlateNumber, DateTime? PlannedStartDate,
    string TripState, bool CanIssue, bool CanUndo, string? BlockReason,
    int CargoLineId, string CargoType, decimal QtyPlanned, decimal QtyIssued, decimal QtyPending, decimal QtyOnHand,
    string BinsText, string Destination = "—"); // điểm giao của chuyến (xe chở hàng đi đâu)

public interface ITripIssueService
{
    Task<List<TripIssueLineDto>> GetAsync(int warehouseId);
    Task<WarehouseActionResult> IssueTripAsync(int warehouseId, int tripId, StockPerformer performer);
    Task<WarehouseActionResult> UndoTripAsync(int warehouseId, int tripId, StockPerformer performer);
}

/// <summary>XUẤT KHO THEO CHUYẾN — đúng quy trình kho thật: chặng Giao đã chia xe -> nhân viên kho
/// soạn hàng theo danh sách (hàng nào, bao nhiêu, ở ô nào), bốc lên đúng xe rồi bấm XÁC NHẬN XUẤT
/// (trừ tồn kho, gắn phiếu xuất với chuyến). Chỉ khi kho đã xuất ĐỦ thì chặng Giao mới bắt đầu được
/// (Desktop hay app tài xế — TripService.GetStartBlockReasonAsync). Số lượng xuất = ĐÚNG tổng số lượng
/// của chuyến (không xuất lẻ, không xuất ngoài chuyến) -> không lệch tồn kho.
/// Xuất nhầm / cần đổi xe / hủy đơn trước khi xe chạy: "Hoàn tác xuất" (dỡ hàng về kệ, phiếu xuất
/// giữ lại và đánh dấu đã hoàn tác).</summary>
public class TripIssueService : ITripIssueService
{
    private readonly IApplicationDbContext _db;
    private readonly IWarehouseSlottingService _slotting;

    public TripIssueService(IApplicationDbContext db, IWarehouseSlottingService slotting)
    {
        _db = db;
        _slotting = slotting;
    }

    public async Task<List<TripIssueLineDto>> GetAsync(int warehouseId)
    {
        var trips = await _db.Trips
            .Where(t => t.WarehouseId == warehouseId && t.LegType == TripLegType.Delivery && t.Status == TripStatus.Planned)
            .Select(t => new
            {
                t.Id, t.TransportOrderId, OrderCode = t.TransportOrder.Code, CustomerName = t.TransportOrder.Customer.Name,
                DriverName = t.DriverEmployee.FullName, t.Vehicle.PlateNumber, t.PlannedStartDate,
                DropoffName = t.TransportOrder.DropoffLocation != null ? t.TransportOrder.DropoffLocation.Name : null,
                t.TransportOrder.DropoffPoint,
            })
            .ToListAsync();
        if (trips.Count == 0) return new List<TripIssueLineDto>();

        var tripIds = trips.Select(t => t.Id).ToList();
        var orderIds = trips.Select(t => t.TransportOrderId).Distinct().ToList();
        var lines = await _db.TripLines
            .Where(l => tripIds.Contains(l.TripId))
            .Select(l => new { l.TripId, l.CargoLineId, l.QtyPlanned, l.CargoLine.CargoType })
            .ToListAsync();
        var issued = await _db.GoodsIssues
            .Where(g => g.TripId != null && tripIds.Contains(g.TripId.Value) && g.ReversedAt == null)
            .GroupBy(g => new { TripId = g.TripId!.Value, g.CargoLineId })
            .Select(g => new { g.Key.TripId, g.Key.CargoLineId, Qty = g.Sum(x => x.QtyIssued) })
            .ToListAsync();
        var cargoIds = lines.Select(l => l.CargoLineId).Distinct().ToList();
        var stock = await _db.WarehouseStocks
            .Where(s => s.WarehouseId == warehouseId && cargoIds.Contains(s.CargoLineId))
            .ToDictionaryAsync(s => s.CargoLineId, s => s.QtyOnHand);
        var bins = (await _db.StorageAssignments
                .Where(a => a.GoodsReceipt.WarehouseId == warehouseId && cargoIds.Contains(a.GoodsReceipt.CargoLineId) && a.QtyStored > 0)
                .OrderBy(a => a.AssignedAt)
                .Select(a => new { a.GoodsReceipt.CargoLineId, a.StorageBin.Code, a.QtyStored })
                .ToListAsync())
            .GroupBy(a => a.CargoLineId)
            .ToDictionary(g => g.Key, g => string.Join(", ", g.GroupBy(x => x.Code).Select(b => $"{b.Key} ({b.Sum(x => x.QtyStored):0.##})")));
        var pendingPickupOrders = (await _db.Trips
                .Where(t => orderIds.Contains(t.TransportOrderId) && t.LegType == TripLegType.Pickup
                            && t.Status != TripStatus.Cancelled && t.Status != TripStatus.Completed)
                .Select(t => t.TransportOrderId).Distinct().ToListAsync())
            .ToHashSet();
        var waitingReceiptOrders = (await PendingReceiptRules.LoadAsync(_db, l => orderIds.Contains(l.Trip.TransportOrderId)))
            .Select(p => p.TripId).ToList();
        var waitingOrderIds = waitingReceiptOrders.Count == 0 ? new HashSet<int>()
            : (await _db.Trips.Where(t => waitingReceiptOrders.Contains(t.Id)).Select(t => t.TransportOrderId).ToListAsync()).ToHashSet();

        var result = new List<TripIssueLineDto>();
        foreach (var t in trips)
        {
            var tLines = lines.Where(l => l.TripId == t.Id).Select(l =>
            {
                var done = issued.FirstOrDefault(i => i.TripId == t.Id && i.CargoLineId == l.CargoLineId)?.Qty ?? 0;
                return new { l.CargoLineId, l.CargoType, l.QtyPlanned, Issued = done, Pending = Math.Max(0, l.QtyPlanned - done),
                             OnHand = stock.TryGetValue(l.CargoLineId, out var q) ? q : 0 };
            }).ToList();
            var anyIssued = tLines.Any(l => l.Issued > 0);
            var allIssued = tLines.All(l => l.Pending <= 0);

            string state; string? reason = null;
            if (allIssued) state = "✅ Đã xuất — chờ xe chạy";
            else if (pendingPickupOrders.Contains(t.TransportOrderId))
            { state = "⏳ Chờ chặng Lấy hàng"; reason = "Chặng Lấy hàng của đơn chưa hoàn tất — hàng chưa về kho."; }
            else if (waitingOrderIds.Contains(t.TransportOrderId))
            { state = "📦 Chờ nhập kho"; reason = "Hàng đã về kho nhưng chưa nhập kho xong — nhập ở nút 📦 Nhập hàng từ chuyến trước."; }
            else if (tLines.FirstOrDefault(l => l.Pending > l.OnHand) is { } shortLine)
            { state = "⚠️ Kho không đủ hàng"; reason = $"\"{shortLine.CargoType}\": cần xuất {shortLine.Pending:0.##}, trong kho chỉ còn {shortLine.OnHand:0.##}."; }
            else state = anyIssued ? "🟡 Đã xuất một phần" : "🟢 Sẵn sàng xuất";

            foreach (var l in tLines)
                result.Add(new TripIssueLineDto(t.Id, t.TransportOrderId, t.OrderCode, t.CustomerName, t.DriverName, t.PlateNumber,
                    t.PlannedStartDate, state, !allIssued && reason is null, anyIssued, reason,
                    l.CargoLineId, l.CargoType, l.QtyPlanned, l.Issued, l.Pending, l.OnHand,
                    bins.TryGetValue(l.CargoLineId, out var b) ? b : "—",
                    TripRouteText.For(TripLegType.Delivery, null, null, null, t.DropoffName, t.DropoffPoint).To));
        }
        // Chuyến xuất phát sớm nhất lên đầu.
        return result.OrderBy(r => r.PlannedStartDate ?? DateTime.MaxValue).ThenBy(r => r.TripId).ThenBy(r => r.CargoLineId).ToList();
    }

    public async Task<WarehouseActionResult> IssueTripAsync(int warehouseId, int tripId, StockPerformer performer)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync();
        var (trip, error) = await LockAndCheckAsync(warehouseId, tripId);
        if (trip is null) return new WarehouseActionResult(false, error);

        // Xe đang Bảo trì / Ngưng hoạt động thì không bốc hàng lên — chuyến không chạy được, hàng
        // nằm trên xe hỏng (và chuyến đã xuất thì không Đổi xe được nữa). Đổi xe trước rồi xuất.
        if (trip.VehicleStatus is VehicleStatus.UnderMaintenance or VehicleStatus.OutOfService)
            return new WarehouseActionResult(false,
                $"Xe {trip.Plate} đang {(trip.VehicleStatus == VehicleStatus.UnderMaintenance ? "Bảo trì" : "Ngưng hoạt động")} — chưa xuất hàng lên xe này. " +
                "Đổi xe cho chuyến (màn Chuyến → Đổi xe / tài xế) hoặc chờ xe sửa xong.");
        // Không bốc hàng lên xe quá sớm: hàng rời kho từ lúc xuất -> nằm trên xe nhiều ngày, xe bị giữ,
        // và phí lưu kho (tính theo ngày hàng còn TRONG KHO) ngừng tính sớm hơn thực tế.
        if (trip.PlannedStartDate is DateTime planned && planned.Date > VnClock.Today.AddDays(MaxIssueDaysBeforeStart))
            return new WarehouseActionResult(false,
                $"Chuyến #{tripId} dự kiến xuất phát {planned:dd/MM/yyyy} — chỉ xuất kho từ ngày {planned.Date.AddDays(-MaxIssueDaysBeforeStart):dd/MM/yyyy} " +
                $"(trước ngày chạy tối đa {MaxIssueDaysBeforeStart} ngày).");

        var pendingPickup = await _db.Trips.AnyAsync(t => t.TransportOrderId == trip.TransportOrderId && t.LegType == TripLegType.Pickup
                                                          && t.Status != TripStatus.Cancelled && t.Status != TripStatus.Completed);
        if (pendingPickup)
            return new WarehouseActionResult(false, "Chặng Lấy hàng của đơn chưa hoàn tất — hàng chưa về kho, chưa xuất được.");
        var waiting = await PendingReceiptRules.LoadAsync(_db, l => l.Trip.TransportOrderId == trip.TransportOrderId);
        if (waiting.Count > 0)
            return new WarehouseActionResult(false, "Hàng của đơn đã về kho nhưng CHƯA NHẬP KHO xong — nhập ở nút 📦 Nhập hàng từ chuyến trước rồi mới xuất.");

        var lines = await _db.TripLines.Where(l => l.TripId == tripId)
            .Select(l => new { l.CargoLineId, l.QtyPlanned, l.CargoLine.CargoType }).ToListAsync();
        var done = new List<string>();
        foreach (var line in lines)
        {
            var issued = await _db.GoodsIssues
                .Where(g => g.TripId == tripId && g.CargoLineId == line.CargoLineId && g.ReversedAt == null)
                .SumAsync(g => (decimal?)g.QtyIssued) ?? 0m;
            var qty = line.QtyPlanned - issued;
            if (qty <= 0) continue;
            var result = await _slotting.IssueGoodsAsync(warehouseId, trip.TransportOrderId, line.CargoLineId, qty,
                performer.EmployeeId, tripId, performer.UserName);
            // 1 dòng không đủ hàng -> KHÔNG xuất dở dang (chưa Commit -> Rollback cả chuyến).
            if (!result.Success)
                return new WarehouseActionResult(false, $"Không xuất được \"{line.CargoType}\" — {result.Message} Chưa xuất dòng nào của chuyến này.");
            done.Add($"• \"{line.CargoType}\": {qty:0.##}");
        }
        if (done.Count == 0) return new WarehouseActionResult(false, $"Hàng của chuyến #{tripId} đã xuất đủ từ trước.");

        await transaction.CommitAsync();
        return new WarehouseActionResult(true,
            $"Đã xác nhận xuất kho cho chuyến #{tripId} (xe {trip.Plate}):\n{string.Join("\n", done)}\nTài xế có thể bắt đầu chuyến giao.");
    }

    public async Task<WarehouseActionResult> UndoTripAsync(int warehouseId, int tripId, StockPerformer performer)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync();
        var (trip, error) = await LockAndCheckAsync(warehouseId, tripId);
        if (trip is null) return new WarehouseActionResult(false, error);

        var result = await _slotting.ReverseTripIssuesAsync(tripId, performer.UserName);
        if (!result.Success) return result; // chưa Commit -> Rollback

        await transaction.CommitAsync();
        return new WarehouseActionResult(true, $"Đã hoàn tác xuất kho chuyến #{tripId} — hàng dỡ khỏi xe, trả về kệ:\n{result.Message}");
    }

    private sealed record LockedTrip(int TransportOrderId, string Plate, VehicleStatus VehicleStatus, DateTime? PlannedStartDate);

    /// <summary>Số ngày được xuất (bốc hàng lên xe) TRƯỚC ngày xuất phát dự kiến.</summary>
    private const int MaxIssueDaysBeforeStart = 1;

    /// <summary>KHÓA dòng chuyến (SELECT ... FOR UPDATE) rồi mới kiểm tra — Bắt đầu chuyến cũng cập
    /// nhật đúng dòng này, nên xuất / hoàn tác và bắt đầu chuyến không chen nhau.</summary>
    private async Task<(LockedTrip? Trip, string? Error)> LockAndCheckAsync(int warehouseId, int tripId)
    {
        await _db.Database.ExecuteSqlInterpolatedAsync($"SELECT 1 FROM \"Trips\" WHERE \"Id\" = {tripId} FOR UPDATE");
        var trip = await _db.Trips.Where(t => t.Id == tripId)
            .Select(t => new { t.LegType, t.Status, t.WarehouseId, t.TransportOrderId, t.Vehicle.PlateNumber, VehicleStatus = t.Vehicle.Status, t.PlannedStartDate })
            .FirstOrDefaultAsync();
        if (trip is null) return (null, "Không tìm thấy chuyến.");
        if (trip.LegType != TripLegType.Delivery) return (null, $"Chuyến #{tripId} không phải chuyến Giao hàng.");
        if (trip.WarehouseId != warehouseId) return (null, $"Chuyến #{tripId} xuất từ kho khác.");
        if (trip.Status != TripStatus.Planned)
            return (null, $"Chuyến #{tripId} không còn ở trạng thái chưa chạy (xe đã xuất phát / đã xong / đã hủy).");
        return (new LockedTrip(trip.TransportOrderId, trip.PlateNumber, trip.VehicleStatus, trip.PlannedStartDate), null);
    }
}
