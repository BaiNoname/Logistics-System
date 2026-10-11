using Microsoft.EntityFrameworkCore;
using TruckingSystem.Application.Common;
using TruckingSystem.Application.Common.Interfaces;
using TruckingSystem.Domain.Entities;
using TruckingSystem.Domain.Enums;

namespace TruckingSystem.Application.Vehicles;

public class VehicleService : IVehicleService
{
    private readonly IApplicationDbContext _db;

    public VehicleService(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<VehicleListItemDto>> GetAllAsync()
    {
        var vehicles = await _db.Vehicles
            .Include(v => v.Warehouse)
            .OrderBy(v => v.PlateNumber)
            .Select(v => new VehicleListItemDto(v.Id, v.PlateNumber, v.VehicleType, v.CapacityQty, v.CapacityKg, v.CapacityM3, v.Status.ToString(), v.FuelConsumptionPer100Km, v.FuelType, v.WarehouseId, v.Warehouse == null ? null : v.Warehouse.Name, null))
            .ToListAsync();

        // Kèm lý do giấy tờ (nếu có) để Desktop biết xe "Rảnh" nào thực ra KHÔNG chia xe được
        // (VD Dashboard "Xe sẵn sàng" trước đây đếm cả xe hết Đăng kiểm).
        var today = VnClock.Today;
        var docsByVehicle = (await VehicleDocumentRules.GetLatestAsync(_db)).ToLookup(d => d.VehicleId);
        return vehicles
            .Select(v => v with { DocumentBlockReason = VehicleDocumentRules.BlockReason(docsByVehicle[v.Id], v.PlateNumber, today) })
            .ToList();
    }

    public async Task<VehicleListItemDto> CreateAsync(CreateVehicleRequest request)
    {
        var plate = (request.PlateNumber ?? "").Trim().ToUpperInvariant(); // lưu biển số IN HOA (khớp ràng buộc UNIQUE)
        if (plate.Length == 0) throw new InvalidOperationException("Biển số xe không được để trống.");
        if (await PlateExistsAsync(plate, excludeId: null))
            throw new InvalidOperationException($"Biển số {plate} đã tồn tại trong hệ thống.");
        var (fuelTypeOk, fuelType) = await NormalizeFuelTypeAsync(request.FuelType);
        if (!fuelTypeOk) throw new InvalidOperationException($"Loại nhiên liệu \"{request.FuelType}\" không có trong danh mục.");
        var capacityError = ValidateCapacity(request.CapacityQty, request.CapacityKg, request.CapacityM3, request.FuelConsumptionPer100Km);
        if (capacityError is not null) throw new InvalidOperationException(capacityError);

        var vehicle = new Vehicle
        {
            PlateNumber = plate,
            VehicleType = request.VehicleType,
            CapacityQty = request.CapacityQty,
            CapacityKg = request.CapacityKg,
            CapacityM3 = request.CapacityM3,
            FuelConsumptionPer100Km = request.FuelConsumptionPer100Km,
            FuelType = fuelType,
            WarehouseId = request.WarehouseId,
            Status = VehicleStatus.Available,
        };

        _db.Vehicles.Add(vehicle);
        await _db.SaveChangesAsync();

        string? warehouseName = null;
        if (request.WarehouseId.HasValue)
            warehouseName = (await _db.Warehouses.FindAsync(request.WarehouseId.Value))?.Name;

        return new VehicleListItemDto(vehicle.Id, vehicle.PlateNumber, vehicle.VehicleType, vehicle.CapacityQty, vehicle.CapacityKg, vehicle.CapacityM3, vehicle.Status.ToString(), vehicle.FuelConsumptionPer100Km, vehicle.FuelType, vehicle.WarehouseId, warehouseName);
    }

    public async Task<VehicleActionResult> UpdateAsync(int id, UpdateVehicleRequest request)
    {
        var vehicle = await _db.Vehicles.FindAsync(id);
        if (vehicle is null) return new VehicleActionResult(false, "Không tìm thấy xe.");

        if (!Enum.TryParse<VehicleStatus>(request.Status, out var status) || !Enum.IsDefined(status))
            return new VehicleActionResult(false, "Trạng thái không hợp lệ.");

        var plate = (request.PlateNumber ?? "").Trim().ToUpperInvariant(); // lưu biển số IN HOA (khớp ràng buộc UNIQUE)
        if (plate.Length == 0) return new VehicleActionResult(false, "Biển số xe không được để trống.");
        if (await PlateExistsAsync(plate, excludeId: id))
            return new VehicleActionResult(false, $"Biển số {plate} đã tồn tại trong hệ thống.");
        var (fuelTypeOk, fuelType) = await NormalizeFuelTypeAsync(request.FuelType);
        if (!fuelTypeOk) return new VehicleActionResult(false, $"Loại nhiên liệu \"{request.FuelType}\" không có trong danh mục.");

        // Trạng thái "Đang chạy" do HỆ THỐNG quản lý (Chia xe đặt, hoàn tất/hủy
        // chuyến trả về) — không cho sửa tay, tránh xe bị "Rảnh" khi vẫn còn chuyến
        // (bị Chia xe lần 2 cho đơn khác) hoặc kẹt "Đang chạy" mãi khi không có chuyến.
        var activeTripIds = await _db.Trips
            .Where(t => t.VehicleId == id &&
                        (t.Status == TripStatus.Planned || t.Status == TripStatus.InProgress || t.Status == TripStatus.PendingConfirmation))
            .Select(t => t.Id)
            .ToListAsync();
        var tripListText = string.Join(", #", activeTripIds);

        if (status != vehicle.Status)
        {
            if (status == VehicleStatus.OnTrip)
                return new VehicleActionResult(false, "Không thể tự đặt trạng thái \"Đang chạy\" — trạng thái này do hệ thống gán khi Chia xe.");

            if (status == VehicleStatus.Available && activeTripIds.Count > 0)
            {
                if (vehicle.Status is VehicleStatus.UnderMaintenance or VehicleStatus.OutOfService)
                    status = VehicleStatus.OnTrip; // sửa xong mà xe vẫn còn chuyến -> quay lại "Đang chạy" để chạy tiếp
                else
                    return new VehicleActionResult(false,
                        $"Xe còn chuyến chưa xong (#{tripListText}) — không thể đặt \"Rảnh\". Xe tự rảnh lại khi các chuyến này hoàn tất hoặc bị hủy.");
            }
            // Bảo trì khi còn chuyến: VẪN cho (xe hỏng giữa chừng, sửa xong chạy tiếp) —
            // chuyến chưa chạy sẽ bị chặn lúc Bắt đầu cho tới khi xe sẵn sàng lại.
            // Ngưng hoạt động (bỏ xe hẳn) khi còn chuyến: CHẶN — chuyến không bao giờ chạy
            // được, tài xế bị tính "đang bận" mãi, đơn không chia xe lại được.
            if (status == VehicleStatus.OutOfService && activeTripIds.Count > 0)
                return new VehicleActionResult(false,
                    $"Xe còn chuyến chưa xong (#{tripListText}) — không thể đặt \"Ngưng hoạt động\". " +
                    "Nếu xe hỏng tạm thời, chọn \"Bảo trì\". Nếu bỏ xe hẳn: chuyển các chuyến chưa chạy sang xe khác " +
                    "(màn Chuyến → Đổi xe / tài xế; chuyến Trả hàng có thể chọn Trả khách tại kho), chuyến đang chạy thì chờ hoàn tất.");
        }

        var capacityError = ValidateCapacity(request.CapacityQty, request.CapacityKg, request.CapacityM3, request.FuelConsumptionPer100Km);
        if (capacityError is not null) return new VehicleActionResult(false, capacityError);

        // Không giảm sức chứa (số lượng / kg / m³) xuống DƯỚI lượng hàng đã xếp cho 1 chuyến
        // còn chưa xong của xe — chuyến đó sẽ thành chở quá tải.
        if (activeTripIds.Count > 0 &&
            (request.CapacityQty < vehicle.CapacityQty || request.CapacityKg < vehicle.CapacityKg || request.CapacityM3 < vehicle.CapacityM3))
        {
            var lines = await _db.TripLines
                .Where(l => activeTripIds.Contains(l.TripId))
                .Select(l => new { l.TripId, l.QtyPlanned, l.CargoLine.QtyTotal, l.CargoLine.WeightKg, l.CargoLine.VolumeM3 })
                .ToListAsync();
            var loads = lines
                .GroupBy(l => l.TripId)
                .Select(g => new
                {
                    Qty = g.Sum(l => l.QtyPlanned),
                    Kg = g.Sum(l => l.QtyTotal > 0 ? l.QtyPlanned * l.WeightKg / l.QtyTotal : 0),
                    M3 = g.Sum(l => l.QtyTotal > 0 ? l.QtyPlanned * l.VolumeM3 / l.QtyTotal : 0),
                })
                .ToList();
            // Làm tròn (kg, m³ tính qua phép chia -> lẻ ở chữ số rất xa) và chỉ xét chiều
            // nào THẬT SỰ bị giảm — tránh chặn oan khi chỉ sửa chiều khác.
            var maxQty = loads.Count == 0 ? 0 : Math.Round(loads.Max(x => x.Qty), 3);
            var maxKg = loads.Count == 0 ? 0 : Math.Round(loads.Max(x => x.Kg), 3);
            var maxM3 = loads.Count == 0 ? 0 : Math.Round(loads.Max(x => x.M3), 4);
            if (request.CapacityQty < vehicle.CapacityQty && request.CapacityQty < maxQty)
                return new VehicleActionResult(false,
                    $"Xe đang được xếp {maxQty:0.##} đơn vị hàng cho chuyến chưa xong (#{tripListText}) — không thể giảm sức chứa xuống {request.CapacityQty:0.##}.");
            if (request.CapacityKg < vehicle.CapacityKg && request.CapacityKg < maxKg)
                return new VehicleActionResult(false,
                    $"Xe đang được xếp {maxKg:0.##} kg hàng cho chuyến chưa xong (#{tripListText}) — không thể giảm tải trọng xuống {request.CapacityKg:0.##} kg.");
            if (request.CapacityM3 < vehicle.CapacityM3 && request.CapacityM3 < maxM3)
                return new VehicleActionResult(false,
                    $"Xe đang được xếp {maxM3:0.##} m³ hàng cho chuyến chưa xong (#{tripListText}) — không thể giảm thể tích xuống {request.CapacityM3:0.##} m³.");
        }

        if (request.WarehouseId != vehicle.WarehouseId && activeTripIds.Count > 0)
            return new VehicleActionResult(false,
                $"Xe còn chuyến chưa xong (#{tripListText}) — chưa thể đổi kho đóng quân.");

        vehicle.PlateNumber = plate;
        vehicle.VehicleType = request.VehicleType;
        vehicle.CapacityQty = request.CapacityQty;
        vehicle.CapacityKg = request.CapacityKg;
        vehicle.CapacityM3 = request.CapacityM3;
        vehicle.Status = status;
        vehicle.FuelConsumptionPer100Km = request.FuelConsumptionPer100Km;
        vehicle.FuelType = fuelType;
        vehicle.WarehouseId = request.WarehouseId;

        await _db.SaveChangesAsync();
        return new VehicleActionResult(true, null);
    }

    public async Task<List<VehicleDocumentDto>> GetDocumentsAsync(int vehicleId)
    {
        return await _db.VehicleDocuments
            .Where(d => d.VehicleId == vehicleId)
            .OrderByDescending(d => d.ExpiryDate)
            .Select(d => new VehicleDocumentDto(d.Id, d.DocType.ToString(), d.IssueDate, d.ExpiryDate, d.FileUrl))
            .ToListAsync();
    }

    /// <summary>Loại nhiên liệu của xe phải khớp danh mục (danh mục chuẩn + các loại đã có
    /// giá) — đưa về ĐÚNG cách viết trong danh mục, để ghi xăng/ước tính tìm được giá.
    /// Bỏ trống = chưa khai báo (hợp lệ).</summary>
    private async Task<(bool Ok, string? Value)> NormalizeFuelTypeAsync(string? input)
    {
        var typed = (input ?? "").Trim();
        if (typed.Length == 0) return (true, null);
        var priced = await _db.FuelPriceHistories.Select(p => p.FuelType).Distinct().ToListAsync();
        var match = TruckingSystem.Application.Fuel.FuelService.StandardFuelTypes
            .Concat(priced)
            .Select(t => t.Trim())
            .FirstOrDefault(t => string.Equals(t, typed, StringComparison.OrdinalIgnoreCase));
        return match is null ? (false, null) : (true, match);
    }

    /// <summary>Sức chứa phải &gt; 0 ở CẢ 3 chiều — Chia xe giới hạn theo số lượng, kg VÀ m³:
    /// xe có kg hoặc m³ = 0 sẽ luôn bị bỏ qua nhưng vẫn hiện "Rảnh" như xe dùng được.</summary>
    private static string? ValidateCapacity(decimal qty, decimal kg, decimal m3, decimal? fuelPer100Km)
    {
        if (qty <= 0) return "Sức chứa (số lượng) phải lớn hơn 0.";
        if (kg <= 0) return "Tải trọng (kg) phải lớn hơn 0.";
        if (m3 <= 0) return "Thể tích (m³) phải lớn hơn 0.";
        if (qty > 1_000_000 || kg > 1_000_000 || m3 > 1_000_000) return "Sức chứa quá lớn (tối đa 1.000.000 cho mỗi chiều).";
        // Định mức xăng âm/0 làm dự toán nhiên liệu ra số âm.
        if (fuelPer100Km is { } f && (f <= 0 || f > 200)) return "Định mức nhiên liệu phải trong khoảng 0 – 200 lít/100 km (để trống nếu chưa biết).";
        return null;
    }

    private Task<bool> PlateExistsAsync(string plate, int? excludeId)
    {
        var normalized = plate.ToUpper();
        return _db.Vehicles.AnyAsync(v => v.PlateNumber.ToUpper() == normalized && (excludeId == null || v.Id != excludeId));
    }

    public async Task<VehicleDocumentDto> CreateDocumentAsync(int vehicleId, CreateVehicleDocumentRequest request)
    {
        if (!await _db.Vehicles.AnyAsync(v => v.Id == vehicleId))
            throw new InvalidOperationException("Không tìm thấy xe.");

        // Trước đây CHỈ kiểm tra ở Desktop (client-side) — gọi thẳng qua API
        // (Swagger, hoặc app khác) sẽ bỏ qua được, lưu dữ liệu vô lý (hết hạn
        // trước ngày cấp). Kiểm tra lại ở Backend cho chắc chắn.
        // Chỉ lưu NGÀY (theo lịch VN) — giờ trong ngày làm lệch phép so "còn hạn".
        // Cắt về ngày TRƯỚC rồi mới so, tránh cùng 1 ngày lọt qua kiểm tra.
        var issueDate = VnClock.ToVnWallClock(request.IssueDate)!.Value.Date;
        var expiryDate = VnClock.ToVnWallClock(request.ExpiryDate)!.Value.Date;
        if (expiryDate <= issueDate)
            throw new InvalidOperationException("Ngày hết hạn phải sau ngày cấp.");

        var docType = Enum.TryParse<VehicleDocumentType>(request.DocType, out var parsed) ? parsed : VehicleDocumentType.Other;

        var doc = new VehicleDocument
        {
            VehicleId = vehicleId,
            DocType = docType,
            IssueDate = issueDate,
            ExpiryDate = expiryDate,
            FileUrl = request.FileUrl,
        };
        _db.VehicleDocuments.Add(doc);
        await _db.SaveChangesAsync();

        return new VehicleDocumentDto(doc.Id, doc.DocType.ToString(), doc.IssueDate, doc.ExpiryDate, doc.FileUrl);
    }

    public async Task<VehicleActionResult> DeleteDocumentAsync(int documentId)
    {
        var doc = await _db.VehicleDocuments.FindAsync(documentId);
        if (doc is null) return new VehicleActionResult(false, "Không tìm thấy giấy tờ.");

        // Xóa Đăng kiểm CÒN HẠN cuối cùng của xe đang có chuyến (chưa chạy / đang chạy) -> các
        // chuyến đó bị chặn lúc xuất phát mà không ai được báo. Nhập bản đúng TRƯỚC rồi xóa bản
        // này (cũng là cách sửa 1 Đăng kiểm nhập nhầm ngày).
        // So CÙNG quy tắc với lúc Bắt đầu chuyến: Đăng kiểm mới nhất CÒN LẠI phải còn hạn tới
        // hết ngày cuối (dự kiến) của từng chuyến CHƯA CHẠY của xe (chuyến đang chạy đã qua bước
        // kiểm tra xuất phát).
        if (doc.DocType == VehicleDocumentType.Registration && VehicleDocumentRules.IsValidOn(doc.ExpiryDate, VnClock.Today))
        {
            var today = VnClock.Today;
            var remainingExpiry = await _db.VehicleDocuments
                .Where(d => d.VehicleId == doc.VehicleId && d.Id != doc.Id && d.DocType == VehicleDocumentType.Registration)
                .MaxAsync(d => (DateTime?)d.ExpiryDate);
            var planned = await _db.Trips
                .Where(t => t.VehicleId == doc.VehicleId && t.Status == TripStatus.Planned)
                .Select(t => new { t.Id, t.EstimatedDurationMinutes })
                .ToListAsync();
            var blocked = planned
                .Where(t =>
                {
                    var lastDay = today.AddDays(Math.Max(1, (int)Math.Ceiling((t.EstimatedDurationMinutes ?? 0) / 480.0)) - 1);
                    return remainingExpiry is null || !VehicleDocumentRules.IsValidOn(remainingExpiry.Value, lastDay);
                })
                .Select(t => t.Id).OrderBy(id => id).Take(5).ToList();
            if (blocked.Count > 0)
                return new VehicleActionResult(false,
                    $"Xóa Đăng kiểm này thì xe không còn Đăng kiểm hợp lệ cho chuyến chưa chạy #{string.Join(", #", blocked)} — các chuyến đó sẽ không xuất phát được. " +
                    "Nếu nhập nhầm, hãy THÊM Đăng kiểm đúng trước rồi mới xóa bản này.");
        }

        _db.VehicleDocuments.Remove(doc);
        await _db.SaveChangesAsync();
        return new VehicleActionResult(true, null);
    }
}
