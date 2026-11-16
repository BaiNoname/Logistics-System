using Microsoft.EntityFrameworkCore;
using TruckingSystem.Application.Common.Interfaces;
using TruckingSystem.Domain.Enums;

namespace TruckingSystem.Application.Warehouses;

/// <summary>Màn "Hàng chờ nhập kho": chuyến Lấy hàng đã về kho (Hoàn tất) nhưng hàng chưa
/// lên kệ. Vận hành có 2 cách nhập:
///   1. Nhập TỰ ĐỘNG theo Best-Fit — nhập hết mọi dòng còn chờ của chuyến.
///   2. Nhập THỦ CÔNG — chọn vị trí + số lượng cho từng dòng (nhập nhiều lần, chia nhiều vị trí).
/// Phiếu nhập luôn GẮN chuyến Lấy hàng -> trừ đúng phần còn chờ, không nhập trùng.</summary>
public interface IPendingReceiptService
{
    Task<List<PendingReceiptLineDto>> GetPendingAsync(int warehouseId);
    Task<ReceiveGoodsResult> ReceiveAutoAsync(int warehouseId, int tripId, StockPerformer performer);
    Task<ReceiveGoodsResult> ReceiveManualAsync(int warehouseId, int tripId, PendingManualReceiveRequest request, StockPerformer performer);
}

public class PendingReceiptService : IPendingReceiptService
{
    private readonly IApplicationDbContext _db;
    private readonly IWarehouseSlottingService _slotting;

    public PendingReceiptService(IApplicationDbContext db, IWarehouseSlottingService slotting)
    {
        _db = db;
        _slotting = slotting;
    }

    public async Task<List<PendingReceiptLineDto>> GetPendingAsync(int warehouseId)
    {
        var pending = await PendingReceiptRules.LoadAsync(_db, l => l.Trip.WarehouseId == warehouseId);
        if (pending.Count == 0) return new List<PendingReceiptLineDto>();

        var tripIds = pending.Select(p => p.TripId).Distinct().ToList();
        var trips = await _db.Trips
            .Where(t => tripIds.Contains(t.Id))
            .Select(t => new
            {
                t.Id,
                t.TransportOrderId,
                OrderCode = t.TransportOrder.Code,
                CustomerName = t.TransportOrder.Customer.Name,
                DriverName = t.DriverEmployee.FullName,
                t.Vehicle.PlateNumber,
                t.EndTime,
            })
            .ToDictionaryAsync(t => t.Id);

        var cargoIds = pending.Select(p => p.CargoLineId).Distinct().ToList();
        var cargos = await _db.CargoLines
            .Where(c => cargoIds.Contains(c.Id))
            .Select(c => new { c.Id, c.CargoType, c.IsHazmat, c.IsFragile, c.QtyTotal })
            .ToDictionaryAsync(c => c.Id);

        // Chặng GIAO của đơn phải xuất phát trước ngày nào (cùng quy tắc màn Chuyến) — hàng của chuyến giao sắp tới
        // mốc thì nhập trước, kẻo kho soạn không kịp.
        var orderIds = trips.Values.Select(t => t.TransportOrderId).Distinct().ToList();
        var orderTrips = (await _db.Trips.AsNoTracking().Where(t => orderIds.Contains(t.TransportOrderId)).ToListAsync())
            .ToLookup(t => t.TransportOrderId);
        var deadlines = await _db.TransportOrders.Where(o => orderIds.Contains(o.Id)).ToDictionaryAsync(o => o.Id, o => o.CommittedDeliveryDate);
        var today = TruckingSystem.Application.Common.VnClock.Today;
        DateTime? LatestStart(int orderId) => orderTrips[orderId]
            .Where(t => t.LegType == TripLegType.Delivery && t.Status == TripStatus.Planned)
            .Select(t => TruckingSystem.Application.Trips.DeliveryDeadlineRules.Evaluate(t, orderTrips[orderId], deadlines.GetValueOrDefault(orderId), today).LatestStart)
            .Where(d => d.HasValue).Min();

        return pending.Select(p =>
        {
            var t = trips[p.TripId];
            var c = cargos[p.CargoLineId];
            var zone = c.IsHazmat ? "Hazmat" : c.IsFragile ? "Fragile" : "Normal";
            return new PendingReceiptLineDto(p.TripId, t.TransportOrderId, t.OrderCode, t.CustomerName,
                t.DriverName, t.PlateNumber, t.EndTime,
                p.CargoLineId, c.CargoType, zone, c.QtyTotal == Math.Floor(c.QtyTotal),
                p.QtyPlanned, p.QtyReceived, p.QtyPending, LatestStart(t.TransportOrderId));
        })
        // Chuyến giao gấp (mốc xuất phát sớm) trước; còn lại xe về kho trước -> nhập trước.
        .OrderBy(d => d.DeliveryLatestStart ?? DateTime.MaxValue).ThenBy(d => d.ArrivedAt ?? DateTime.MaxValue).ThenBy(d => d.TripId).ThenBy(d => d.CargoLineId)
        .ToList();
    }

    public async Task<ReceiveGoodsResult> ReceiveAutoAsync(int warehouseId, int tripId, StockPerformer performer)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync();
        var (orderId, error) = await LockAndCheckTripAsync(warehouseId, tripId);
        if (error is not null) return Fail(error);

        var pending = await PendingReceiptRules.LoadAsync(_db, l => l.TripId == tripId);
        if (pending.Count == 0) return Fail($"Hàng của chuyến #{tripId} đã nhập kho đủ — không còn gì chờ nhập.");

        var cargoNames = await CargoNamesAsync(pending.Select(p => p.CargoLineId));
        var lines = new List<string>();
        var allBins = new List<string>();
        foreach (var line in pending)
        {
            var result = await _slotting.ReceiveGoodsAsync(warehouseId, orderId, line.CargoLineId, line.QtyPending,
                performer.EmployeeId, tripId, chosenBinId: null, performedByUserName: performer.UserName);
            // 1 dòng không xếp được -> KHÔNG nhập dở dang: chưa Commit -> Rollback cả chuyến.
            if (!result.Success)
            {
                // Lối tắt: mở sơ đồ ĐÚNG kho này để thêm vị trí rồi nhập lại.
                var whName = await _db.Warehouses.Where(w => w.Id == warehouseId).Select(w => w.Name).FirstOrDefaultAsync();
                return Fail($"Không nhập tự động được \"{cargoNames[line.CargoLineId]}\" ({line.QtyPending:0.##}) — {result.Message} " +
                            "Chưa nhập dòng nào của chuyến này. Có thể thêm vị trí kho rồi thử lại, hoặc nhập thủ công từng phần.") with
                {
                    Fixes = new List<TruckingSystem.Application.Common.FixHint>
                    {
                        new("WarehouseLayout", $"🗺️ Mở sơ đồ kho {whName} để thêm vị trí", WarehouseId: warehouseId, Name: whName,
                            Note: "Thêm vị trí / khu còn thiếu rồi nhập lại chuyến này"),
                    },
                };
            }
            lines.Add($"• {cargoNames[line.CargoLineId]}: {line.QtyPending:0.##} → {string.Join(", ", result.BinCodesUsed)}");
            allBins.AddRange(result.BinCodesUsed);
        }

        await transaction.CommitAsync();
        return new ReceiveGoodsResult(true,
            $"Đã nhập kho tự động toàn bộ hàng của chuyến #{tripId}:\n" + string.Join("\n", lines),
            allBins.Distinct().ToList());
    }

    public async Task<ReceiveGoodsResult> ReceiveManualAsync(int warehouseId, int tripId, PendingManualReceiveRequest request, StockPerformer performer)
    {
        if (request.Qty <= 0) return Fail("Số lượng nhập phải lớn hơn 0.");

        await using var transaction = await _db.Database.BeginTransactionAsync();
        var (orderId, error) = await LockAndCheckTripAsync(warehouseId, tripId);
        if (error is not null) return Fail(error);

        var line = (await PendingReceiptRules.LoadAsync(_db, l => l.TripId == tripId && l.CargoLineId == request.CargoLineId))
            .FirstOrDefault();
        if (line is null)
            return Fail($"Dòng hàng này của chuyến #{tripId} không còn gì chờ nhập (đã nhập đủ, hoặc không thuộc chuyến).");

        var cargo = await _db.CargoLines.Where(c => c.Id == request.CargoLineId)
            .Select(c => new { c.CargoType, c.QtyTotal }).FirstAsync();
        // Cùng quy tắc với nhập/xuất tay: hàng đếm nguyên chiếc không nhận số lẻ; tối đa 2 chữ số thập phân.
        if (cargo.QtyTotal == Math.Floor(cargo.QtyTotal) && request.Qty != Math.Floor(request.Qty))
            return Fail($"Dòng hàng \"{cargo.CargoType}\" đếm theo đơn vị nguyên — số lượng phải là số nguyên.");
        if (request.Qty != Math.Round(request.Qty, 2))
            return Fail("Số lượng tối đa 2 chữ số thập phân.");
        if (request.Qty > line.QtyPending)
            return Fail($"\"{cargo.CargoType}\" của chuyến #{tripId} chỉ còn chờ nhập {line.QtyPending:0.##} — không nhập {request.Qty:0.##}.");

        var result = await _slotting.ReceiveGoodsAsync(warehouseId, orderId, request.CargoLineId, request.Qty,
            performer.EmployeeId, tripId, chosenBinId: request.BinId, performedByUserName: performer.UserName);
        if (!result.Success) return result; // chưa Commit -> Rollback

        await transaction.CommitAsync();
        var left = line.QtyPending - request.Qty;
        return new ReceiveGoodsResult(true,
            $"Đã nhập {request.Qty:0.##} \"{cargo.CargoType}\" vào {string.Join(", ", result.BinCodesUsed)}." +
            (left > 0 ? $" Còn chờ nhập {left:0.##}." : " Dòng hàng này đã nhập đủ.") +
            (result.Message is null ? "" : "\n" + result.Message),
            result.BinCodesUsed);
    }

    /// <summary>KHÓA dòng chuyến (SELECT ... FOR UPDATE) rồi mới đọc phần còn chờ: 2 người
    /// bấm nhập cùng lúc cho cùng chuyến thì người sau phải chờ và thấy số đã nhập mới —
    /// không nhập trùng. Hủy đơn cũng khóa chuyến trước (cùng thứ tự khóa).</summary>
    private async Task<(int OrderId, string? Error)> LockAndCheckTripAsync(int warehouseId, int tripId)
    {
        await _db.Database.ExecuteSqlInterpolatedAsync($"SELECT 1 FROM \"Trips\" WHERE \"Id\" = {tripId} FOR UPDATE");
        var trip = await _db.Trips.Where(t => t.Id == tripId)
            .Select(t => new { t.LegType, t.Status, t.WarehouseId, t.TransportOrderId })
            .FirstOrDefaultAsync();
        if (trip is null) return (0, "Không tìm thấy chuyến.");
        if (trip.LegType != TripLegType.Pickup) return (0, $"Chuyến #{tripId} không phải chuyến Lấy hàng.");
        if (trip.Status != TripStatus.Completed)
            return (0, $"Chuyến lấy hàng #{tripId} chưa hoàn tất (xe chưa về kho / chưa được duyệt) — chưa nhập kho được.");
        if (trip.WarehouseId != warehouseId) return (0, $"Chuyến #{tripId} chở hàng về kho khác — không nhập vào kho này.");
        return (trip.TransportOrderId, null);
    }

    private async Task<Dictionary<int, string>> CargoNamesAsync(IEnumerable<int> cargoLineIds)
    {
        var ids = cargoLineIds.Distinct().ToList();
        return await _db.CargoLines.Where(c => ids.Contains(c.Id)).ToDictionaryAsync(c => c.Id, c => c.CargoType);
    }

    private static ReceiveGoodsResult Fail(string message) => new(false, message, new List<string>());
}
