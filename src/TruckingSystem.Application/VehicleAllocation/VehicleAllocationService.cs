using Microsoft.EntityFrameworkCore;
using TruckingSystem.Application.Common;
using TruckingSystem.Application.Common.Interfaces;
using TruckingSystem.Application.Drivers;
using TruckingSystem.Application.Routing;
using TruckingSystem.Application.Vehicles;
using TruckingSystem.Application.Warehouses;
using TruckingSystem.Domain.Entities;
using TruckingSystem.Domain.Enums;

namespace TruckingSystem.Application.VehicleAllocation;

/// <summary>
/// Cài đặt thuật toán Tham lam (Greedy) cho bài toán phân bổ xe theo tải trọng.
/// Ưu tiên xe có sức chứa còn trống LỚN NHẤT trước, gán tối đa có thể vào 1
/// chuyến, lặp lại cho tới khi đủ hàng hoặc hết xe. Độ phức tạp O(n log n).
///
/// ĐÃ SỬA 6 lỗi so với bản trước:
///  1. Dùng ĐÚNG TẤT CẢ CargoLine (trước đây chỉ lấy dòng đầu tiên) — coi cả
///     danh sách CargoLine như 1 "dòng chảy" liên tục, đóng gói dần vào từng
///     xe theo đúng tinh thần Greedy, xe nào đầy thì chuyển dòng hàng dở dang
///     tiếp sang xe kế tiếp.
///  2. PickupMethod giờ THẬT SỰ ảnh hưởng: CompanyPickup tạo ĐỦ 2 chặng
///     (Pickup + Delivery, cùng 1 xe/tài xế phụ trách cả 2 chặng) khớp đúng
///     kịch bản nghiệp vụ đã mô tả; SelfDelivery chỉ tạo 1 chặng Delivery
///     (khách đã tự mang hàng tới kho, xe chỉ cần giao đi).
///  3. Chọn tài xế đúng — CHỈ chọn nhân viên có DriverProfile thật (là tài
///     xế), không chọn bừa bất kỳ nhân viên Active nào như trước.
///  4. Kiểm tra ĐỦ 3 ràng buộc sức chứa (Qty, Kg, m³) — trước đây CHỈ kiểm
///     tra CapacityQty, có thể gán quá tải khối lượng/thể tích thật dù "số
///     lượng" vẫn còn chỗ trên xe.
///  5. Chỉ chọn tài xế có bằng lái CÒN HẠN (LicenseExpiryDate) — trước đây
///     không kiểm tra, có thể gán tài xế đã hết hạn bằng lái.
///  6. BẮT BUỘC phải có ĐĂNG KIỂM CÒN HẠN mới cho dùng (đúng luật giao
///     thông — xe không đăng kiểm không được lưu thông); xe CHƯA nhập
///     Đăng kiểm nào cũng bị loại như đã hết hạn. Các giấy tờ KHÁC (Bảo
///     hiểm/Giấy phép vận tải) vẫn áp dụng kiểu MỀM — chưa nhập thì vẫn
///     cho qua, chỉ chặn khi phát hiện THẬT SỰ hết hạn.
///  7. BẮT BUỘC tài xế phải có LỊCH LÀM VIỆC đăng ký sẵn, đủ hết tất cả
///     các ngày dự kiến chuyến chạy (ước tính qua VietMap) — tài xế CHƯA
///     đăng ký lịch nào cũng bị loại, không còn kiểu "mềm" như DriverProfile
///     (nhất quán với quy tắc Đăng kiểm xe ở trên).
/// </summary>
public class VehicleAllocationService : IVehicleAllocationService
{
    private readonly IApplicationDbContext _db;
    private readonly IWarehouseSlottingService _warehouseService;
    private readonly IRouteProvider _routeProvider;
    private readonly TruckingSystem.Application.Trips.ITripService _tripService;

    public VehicleAllocationService(IApplicationDbContext db, IWarehouseSlottingService warehouseService, IRouteProvider routeProvider, TruckingSystem.Application.Trips.ITripService tripService)
    {
        _db = db;
        _warehouseService = warehouseService;
        _routeProvider = routeProvider;
        _tripService = tripService;
    }

    public async Task<AllocateVehiclesResponse> AllocateAsync(int transportOrderId, bool acceptLate = false, string? userId = null, string? userName = null,
        DateTime? plannedStartDate = null)
    {
        var order = await _db.TransportOrders
            .Include(o => o.CargoLines)
            .Include(o => o.PickupLocation)
            .Include(o => o.DropoffLocation)
            .Include(o => o.Warehouse)
            .FirstOrDefaultAsync(o => o.Id == transportOrderId);

        if (order is null)
            return new AllocateVehiclesResponse(false, 0, 0, new(), "Không tìm thấy đơn vận chuyển.");

        // Chỉ chia xe cho đơn ĐÃ XÁC NHẬN — khớp Desktop (nút Chia xe chỉ bật khi đơn Đã
        // xác nhận). Trước đây server nhận cả đơn Nháp và tự xác nhận luôn, bỏ qua bước
        // Xác nhận (và các kiểm tra của bước đó). Đơn Hủy/Hoàn tất thì càng không.
        if (order.Status != TransportOrderStatus.Confirmed)
            return new AllocateVehiclesResponse(false, 0, 0, new(),
                order.Status == TransportOrderStatus.Draft
                    ? "Đơn còn Nháp — hãy Xác nhận đơn trước khi Chia xe."
                    : $"Không thể chia xe — đơn đang ở trạng thái {order.Status}. Chỉ chia xe cho đơn Đã xác nhận.");

        // CHẶN chia xe 2 lần cho cùng 1 đơn — trước đây không kiểm tra gì cả,
        // bấm 2 lần (double-click, hoặc mở lại form bấm lại) sẽ tạo THÊM 1 bộ
        // Trip mới đè lên bộ cũ, giao hàng trùng lặp thật sự (không chỉ là lỗi
        // hiển thị). Chỉ cho chia xe khi đơn CHƯA có Trip nào.
        var hasExistingTrips = await _db.Trips.AnyAsync(t =>
            t.TransportOrderId == transportOrderId && t.Status != TripStatus.Cancelled);
        if (hasExistingTrips)
            return new AllocateVehiclesResponse(false, 0, 0, new(), "Đơn này đã được chia xe rồi — không thể chia lại lần nữa.");

        var cargoLines = order.CargoLines.ToList();
        var totalQtyToShip = cargoLines.Sum(c => c.QtyTotal);
        if (totalQtyToShip <= 0)
            return new AllocateVehiclesResponse(false, 0, 0, new(), "Đơn hàng chưa khai báo số lượng hàng hóa.");

        var isCompanyPickup = order.PickupMethod == PickupMethod.CompanyPickup;

        // Kho vẫn phải còn khu đúng loại hàng (khu có thể bị xóa / đổi loại sau khi xác nhận đơn) —
        // không chia xe đi lấy hàng về rồi mới phát hiện không nhập kho được.
        // (Đơn khách tự mang tới: hàng đã nhập đủ trước khi chia — kiểm tra ngay dưới.)
        var zoneError = !isCompanyPickup ? null : await WarehouseZoneRules.MissingReasonAsync(_db, order.WarehouseId,
            cargoLines.Select(c => WarehouseZoneRules.Required(c.IsHazmat, c.IsFragile)));
        if (zoneError is not null)
            return new AllocateVehiclesResponse(false, totalQtyToShip, 0, new(), zoneError);

        // Đơn KHÁCH TỰ MANG TỚI KHO: Chia xe KHÔNG tự nhập kho nữa — khách mang hàng tới thì
        // thủ kho nhập (màn Kho → Nhập/Xuất thủ công: tự chọn vị trí hoặc Best-Fit). Phải nhập
        // ĐỦ số khai báo rồi mới Chia xe (khách giao thiếu thì không chia — hạn chế đã thống
        // nhất: đối tác phải giao đủ). Trước đây chưa nhập gì thì Chia xe tự nhập toàn bộ ->
        // tồn kho thành đủ dù hàng thật có thể chưa tới.
        if (!isCompanyPickup)
        {
            var cargoLineIds = cargoLines.Select(c => c.Id).ToList();
            var receivedByLine = await _db.GoodsReceipts
                .Where(r => cargoLineIds.Contains(r.CargoLineId))
                .GroupBy(r => r.CargoLineId)
                .Select(g => new { CargoLineId = g.Key, Qty = g.Sum(r => r.QtyReceived) })
                .ToDictionaryAsync(x => x.CargoLineId, x => x.Qty);
            var missing = cargoLines
                .Select(c => (Line: c, Missing: c.QtyTotal - (receivedByLine.TryGetValue(c.Id, out var got) ? got : 0)))
                .Where(x => x.Missing > 0)
                .Select(x => $"\"{x.Line.CargoType}\" mới nhập {x.Line.QtyTotal - x.Missing:0.##}/{x.Line.QtyTotal:0.##}")
                .ToList();
            if (missing.Count > 0)
                return new AllocateVehiclesResponse(false, totalQtyToShip, 0, new(),
                    "Đơn khách tự mang hàng tới kho: hàng CHƯA nhập kho đủ — " + string.Join("; ", missing) +
                    ". Khi khách mang hàng tới, vào màn Kho → 🙋 Nhận hàng khách mang tới để nhập (tự chọn vị trí hoặc Tự động Best-Fit) đủ số lượng rồi mới Chia xe.");
        }

        // BỌC TOÀN BỘ phần còn lại trong 1 TRANSACTION DUY NHẤT — Chia xe cho
        // đơn lớn có thể cần NHIỀU xe, mỗi xe gọi SaveChangesAsync RIÊNG để
        // tạo Trip. Trước đây KHÔNG có transaction: nếu xe thứ 2 trở đi gặp
        // lỗi giữa chừng (mất kết nối DB, VietMap timeout ném exception...),
        // Trip của (các) xe TRƯỚC ĐÓ đã lưu VĨNH VIỄN vào DB dù hàm báo THẤT
        // BẠI — khiến đơn bị "kẹt" ở trạng thái nửa vời: có 1 phần Trip nhưng
        // không đủ chở hết hàng, và KHÔNG THỂ chia xe lại (bị chặn bởi luật
        // "đơn đã có Trip rồi"). Với transaction: bất kỳ lỗi/return thất bại
        // nào trong phạm vi này đều tự động ROLLBACK SẠCH, không để lại dữ
        // liệu dở dang — đơn vẫn ở trạng thái "chưa chia xe", chia lại được.
        await using var transaction = await _db.Database.BeginTransactionAsync();
        // Khóa chung "gán xe/tài xế" (cùng khóa với Đổi xe/tài xế): 2 người Chia xe 2 đơn
        // cùng lúc không thể gán TRÙNG 1 tài xế (xe đã có xmin chặn, tài xế thì chưa).
        await _db.Database.ExecuteSqlRawAsync(
            "SELECT pg_advisory_xact_lock(" + TruckingSystem.Application.Trips.TripService.AssignmentLockKey + ")");
        // Kiểm tra LẠI sau khi giành được khóa: 2 lần bấm Chia xe cùng 1 đơn chạy nối tiếp
        // nhau — lần sau phải thấy chuyến lần trước vừa tạo.
        if (await _db.Trips.AnyAsync(t => t.TransportOrderId == transportOrderId && t.Status != TripStatus.Cancelled))
            return new AllocateVehiclesResponse(false, 0, 0, new(), "Đơn này vừa được chia xe xong (thao tác khác) — không chia lại.");

        // Bước 1: sắp xếp giảm dần theo sức chứa — cốt lõi của tính "tham lam"
        // BẮT BUỘC phải có ĐĂNG KIỂM CÒN HẠN mới cho dùng — đúng luật giao
        // thông thực tế (xe không đăng kiểm KHÔNG ĐƯỢC PHÉP lưu thông), khác
        // với các giấy tờ khác (Bảo hiểm/Giấy phép vận tải) vẫn áp dụng kiểu
        // MỀM như cũ (chưa nhập thì vẫn cho qua, chỉ chặn khi PHÁT HIỆN thật
        // sự hết hạn) — Đăng kiểm là giấy tờ DUY NHẤT bị siết chặt vì đây là
        // điều kiện PHÁP LÝ bắt buộc để xe được lưu thông trên đường.
        // Chỉ xét giấy tờ MỚI NHẤT của từng loại (gia hạn = thêm dòng mới, dòng cũ
        // hết hạn vẫn còn) và so theo NGÀY Việt Nam — xem VehicleDocumentRules.
        var today = VnClock.Today;
        // Giấy tờ xe + bằng lái phải còn hạn tới HẾT NGÀY CUỐI chuyến dự kiến (cùng khoảng
        // ngày bắt tài xế đăng ký ngày làm bên dưới) — trước đây chỉ xét hôm nay: đăng
        // kiểm hết hạn ngày mai vẫn chia được, tới ngày 2 chuyến bị chặn không chạy được.
        var estimatedTripDays = await EstimateTripDurationDaysAsync(order);
        // KHOẢNG THỜI GIAN CHUYẾN = [ngày xuất phát dự kiến .. ngày cuối dự kiến]. Mọi giấy tờ / ngày
        // làm đều xét đúng khoảng này (trước đây luôn tính từ HÔM NAY dù xe chạy ngày khác).
        var startDay = plannedStartDate?.Date ?? today;
        if (startDay < today)
            return new AllocateVehiclesResponse(false, totalQtyToShip, 0, new(), $"Ngày xuất phát dự kiến ({startDay:dd/MM/yyyy}) đã qua — chọn từ hôm nay trở đi.");
        if (startDay > today.AddDays(60))
            return new AllocateVehiclesResponse(false, totalQtyToShip, 0, new(), "Ngày xuất phát dự kiến quá xa (tối đa 60 ngày tới).");
        var lastTripDay = startDay.AddDays(estimatedTripDays - 1);

        // HẠN GIAO (DeliveryDeadlineRules): chia xe hôm nay thì giao sớm nhất ngày nào — trễ hạn
        // thì KHÔNG chặn hẳn (ước tính có thể lệch, Vận hành có thể đã thỏa thuận lại với khách)
        // mà trả về để Desktop HỎI XÁC NHẬN; đồng ý thì chia và ghi nhật ký ai đã chấp nhận trễ.
        string? lateNote = null;
        if (order.CommittedDeliveryDate is { } dueVn)
        {
            var earliest = TruckingSystem.Application.Trips.DeliveryDeadlineRules.EarliestDelivery(startDay, estimatedTripDays);
            var lateDays = (earliest - dueVn.Date).Days;
            if (lateDays > 0)
            {
                lateNote = $"Ước tính chuyến cần ~{estimatedTripDays} ngày: xuất phát {startDay:dd/MM/yyyy} thì giao sớm nhất {earliest:dd/MM/yyyy}, " +
                           $"TRỄ {lateDays} ngày so với hạn giao {dueVn:dd/MM/yyyy}.";
                if (!acceptLate)
                    return new AllocateVehiclesResponse(false, totalQtyToShip, 0, new(),
                        "⏰ KHÔNG KỊP HẠN GIAO — " + lateNote, estimatedTripDays, NeedsLateConfirmation: true);
            }
        }
        // Giấy tờ xe: CÙNG quy tắc VehicleDocumentRules với Đổi xe / Bắt đầu chuyến — đủ 3 loại bắt
        // buộc và còn hạn tới HẾT NGÀY CUỐI chuyến dự kiến.
        var docsByVehicle = (await VehicleDocumentRules.GetLatestAsync(_db)).ToLookup(d => d.VehicleId);
        var availableVehicles = (await _db.Vehicles
            .Where(v => v.Status == VehicleStatus.Available
                     && (v.WarehouseId == null || v.WarehouseId == order.WarehouseId))
            .OrderByDescending(v => v.CapacityQty)
            .ToListAsync())
            .Where(v => VehicleDocumentRules.BlockReason(docsByVehicle[v.Id], v.PlateNumber, lastTripDay) is null)
            .ToList();

        if (!availableVehicles.Any())
            return new AllocateVehiclesResponse(false, totalQtyToShip, 0, new(), $"Không còn xe nào đang rảnh VÀ đang đóng quân đúng kho của đơn này VÀ có ĐỦ giấy tờ (Đăng kiểm, Bảo hiểm, Phù hiệu xe) còn hạn từ {startDay:dd/MM} tới hết ngày {lastTripDay:dd/MM} (thời gian chuyến dự kiến).");

        // Danh sách tài xế hợp lệ (có DriverProfile thật + đang Active + bằng
        // lái CÒN HẠN) — xoay vòng nếu cần nhiều xe hơn số tài xế rảnh (đơn
        // giản hoá: không kiểm tra trùng lịch, cải tiến sau nếu cần).
        // Loại trừ tài xế ĐANG BẬN chuyến khác (Planned/InProgress/PendingConfirmation — chuyến đã báo xong nhưng CHƯA được Vận hành duyệt thì tài xế/xe vẫn chưa thật sự rảnh) — tránh
        // gán trùng 1 tài xế cho 2 chuyến cùng lúc.
        var busyDriverIds = await _db.Trips
            .Where(t => t.Status == TripStatus.Planned || t.Status == TripStatus.InProgress || t.Status == TripStatus.PendingConfirmation)
            .Select(t => t.DriverEmployeeId)
            .Distinct()
            .ToListAsync();

        // Tài xế = CHỨC DANH Tài xế + có hồ sơ (cùng quy tắc với màn Tài xế — DriverPositionRules):
        // người đã đổi sang chức danh khác mà còn hồ sơ cũ KHÔNG được tự gán chuyến nữa.
        var driverPositionIds = await DriverPositionRules.DriverPositionIdsAsync(_db);
        var driverCandidates = await _db.DriverProfiles
            .Include(dp => dp.Employee)
            // Bằng lái + giấy khám sức khỏe (nếu đã nhập) phải còn hạn tới hết ngày cuối chuyến.
            .Where(dp => dp.Employee.Status == EmployeeStatus.Active && dp.LicenseExpiryDate >= lastTripDay
                      && (dp.HealthCertExpiryDate == null || dp.HealthCertExpiryDate >= lastTripDay)
                      && dp.Employee.PositionId != null && driverPositionIds.Contains(dp.Employee.PositionId.Value)
                      && (dp.WarehouseId == null || dp.WarehouseId == order.WarehouseId))
            .Select(dp => new { dp.EmployeeId, dp.LicenseClass })
            .ToListAsync();
        // Hạng bằng của từng tài xế — mỗi xe chỉ giao cho tài xế có hạng bằng lái ĐÚNG với
        // tải trọng / loại xe (DriverLicenseRules), trước đây không xét -> bằng B2 chạy xe 5 tấn.
        var licenseByDriver = driverCandidates.ToDictionary(d => d.EmployeeId, d => d.LicenseClass);
        var driverIds = driverCandidates.Select(d => d.EmployeeId).Except(busyDriverIds).ToList();

        // BẮT BUỘC tài xế phải có NGÀY LÀM ĐĂNG KÝ SẴN, đủ hết TẤT CẢ các
        // ngày trong toàn bộ thời gian chuyến dự kiến chạy — nhất quán với quy
        // tắc Đăng kiểm xe (cũng bắt buộc phải có, không cho qua nếu thiếu dữ
        // liệu). Tài xế CHƯA đăng ký lịch nào cũng bị loại giống như thiếu ca
        // — quan trọng với tuyến ĐƯỜNG DÀI (VD Hà Nội - Sài Gòn ~1.700km,
        // thực tế thường mất HƠN 1 NGÀY do quy định giờ lái xe liên tục + nghỉ
        // ngơi bắt buộc), tránh trường hợp tài xế "hết ca" giữa đường mà hệ
        // thống không hề biết.
        // Ngày làm tính theo GIỜ VIỆT NAM (UTC+7). Dùng UtcNow.Date sẽ
        // lệch 1 ngày khi chia xe trong khoảng 0h–7h sáng (giờ VN).
        var neededDates = Enumerable.Range(0, estimatedTripDays).Select(offset => startDay.AddDays(offset)).ToList();

        var scheduleDatesByDriver = await _db.DriverSchedules
            .Where(s => driverIds.Contains(s.DriverEmployeeId))
            .Select(s => new { s.DriverEmployeeId, s.ShiftDate })
            .ToListAsync();
        var scheduledDateSetByDriver = scheduleDatesByDriver
            .GroupBy(s => s.DriverEmployeeId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.ShiftDate.Date).ToHashSet());

        driverIds = driverIds.Where(id =>
            scheduledDateSetByDriver.TryGetValue(id, out var dates) && neededDates.All(d => dates.Contains(d))
        ).ToList();

        if (driverIds.Count == 0)
        {
            var dayRangeText = estimatedTripDays > 1
                ? $" từ {neededDates.First():dd/MM} tới {neededDates.Last():dd/MM} (ước tính chuyến kéo dài {estimatedTripDays} ngày do quãng đường xa)"
                : $" ngày {neededDates.First():dd/MM}";
            return new AllocateVehiclesResponse(false, totalQtyToShip, 0, new(),
                $"Không có tài xế nào khả dụng (không có chức danh Tài xế / chưa có hồ sơ, bằng lái hoặc giấy khám sức khỏe hết hạn trước khi chuyến xong, đang bận chuyến khác, không đóng quân đúng kho của đơn này, hoặc CHƯA đăng ký ĐỦ ngày làm{dayRangeText} — bắt buộc phải đăng ký ngày làm trước, màn Tài xế → Ngày làm).");
        }

        // Dùng ĐÚNG kho trung chuyển Dispatcher đã CHỌN khi tạo đơn (order.WarehouseId)
        // — trước đây luôn lấy kho ĐẦU TIÊN trong hệ thống bất kể đơn đi đâu,
        // gây ra lộ trình vô lý (VD đơn Bình Dương -> Hà Nội nhưng lại đi qua
        // 1 kho không liên quan gì tới tuyến đường thực tế).
        var defaultWarehouseId = order.WarehouseId;

        var results = new List<AllocationResultItem>();
        int vehicleIndex = 0;
        // Tài xế đã nhận xe trong lần chia này (mỗi xe 1 tài xế RIÊNG).
        var usedDriverIds = new HashSet<int>();
        // Xe bị bỏ qua vì không còn tài xế nào có hạng bằng phù hợp (để báo lý do rõ).
        var licenseSkippedPlates = new List<string>();
        decimal vehicleRemainingQty = availableVehicles[0].CapacityQty;
        decimal vehicleRemainingKg = availableVehicles[0].CapacityKg;
        decimal vehicleRemainingM3 = availableVehicles[0].CapacityM3;
        decimal totalAssigned = 0;

        // Dòng hàng hiện đang xử lý dở (đóng gói liên tục qua nhiều dòng hàng
        // và nhiều xe, đúng tinh thần Greedy — không dừng lại ở từng dòng).
        int cargoIndex = 0;
        decimal cargoRemaining = cargoLines.Count > 0 ? cargoLines[0].QtyTotal : 0;

        Trip? currentPickupTrip = null, currentDeliveryTrip = null;
        int currentDriverEmployeeId = 0;

        while (cargoIndex < cargoLines.Count && vehicleIndex < availableVehicles.Count)
        {
            if (cargoRemaining <= 0)
            {
                cargoIndex++;
                if (cargoIndex >= cargoLines.Count) break;
                cargoRemaining = cargoLines[cargoIndex].QtyTotal;
                continue;
            }

            // Bắt đầu xe mới -> tạo Trip (1 hoặc 2 chặng tùy PickupMethod)
            if (currentDeliveryTrip is null)
            {
                // Mỗi xe 1 tài xế RIÊNG — trước đây xoay vòng (driverIndex % số tài
                // xế) nên 1 tài xế bị gán cho 2 xe cùng lúc khi thiếu tài xế.
                // Hết tài xế rảnh thì dừng; phần hàng còn lại sẽ bị báo thiếu ở dưới.
                if (usedDriverIds.Count >= driverIds.Count) break;
                var vehicle = availableVehicles[vehicleIndex];
                // Tài xế RẢNH đầu tiên có hạng bằng lái được lái xe này.
                var fitDriverId = driverIds.FirstOrDefault(id => !usedDriverIds.Contains(id)
                    && DriverLicenseRules.BlockReason(licenseByDriver[id], vehicle.CapacityKg, vehicle.VehicleType) is null);
                if (fitDriverId == 0)
                {
                    // Không ai đủ hạng bằng cho xe này -> bỏ qua xe, thử xe kế tiếp (xe nhỏ hơn).
                    licenseSkippedPlates.Add($"{vehicle.PlateNumber} (cần bằng {DriverLicenseRules.RequiredText(vehicle.CapacityKg, vehicle.VehicleType)})");
                    vehicleIndex++;
                    if (vehicleIndex < availableVehicles.Count)
                    {
                        vehicleRemainingQty = availableVehicles[vehicleIndex].CapacityQty;
                        vehicleRemainingKg = availableVehicles[vehicleIndex].CapacityKg;
                        vehicleRemainingM3 = availableVehicles[vehicleIndex].CapacityM3;
                    }
                    continue;
                }
                var driverEmployeeId = fitDriverId;
                usedDriverIds.Add(driverEmployeeId);
                currentDriverEmployeeId = driverEmployeeId;

                if (isCompanyPickup)
                {
                    currentPickupTrip = new Trip
                    {
                        TransportOrderId = order.Id,
                        VehicleId = vehicle.Id,
                        DriverEmployeeId = driverEmployeeId,
                        Status = TripStatus.Planned,
                        PlannedStartDate = startDay,
                        LegType = TripLegType.Pickup,
                        WarehouseId = defaultWarehouseId,
                    };
                    _db.Trips.Add(currentPickupTrip);
                }

                currentDeliveryTrip = new Trip
                {
                    TransportOrderId = order.Id,
                    VehicleId = vehicle.Id,
                    DriverEmployeeId = driverEmployeeId,
                    Status = TripStatus.Planned,
                    PlannedStartDate = startDay,
                    LegType = TripLegType.Delivery,
                    WarehouseId = defaultWarehouseId,
                };
                _db.Trips.Add(currentDeliveryTrip);
                await _db.SaveChangesAsync(); // cần Id thật trước khi tạo TripLine

                // TỰ ĐỘNG tính + LƯU LẠI ước tính khoảng cách/thời gian từ
                // VietMap ngay lúc Chia xe — để có sẵn dữ liệu SO SÁNH với số
                // liệu THỰC TẾ sau khi chuyến chạy xong (phục vụ thống kê),
                // không phải đợi Dispatcher tự bấm "📏 Ước tính" thủ công.
                if (currentPickupTrip is not null)
                    await _tripService.CalculateAndSaveRouteEstimateAsync(currentPickupTrip.Id);
                await _tripService.CalculateAndSaveRouteEstimateAsync(currentDeliveryTrip.Id);

                vehicle.Status = VehicleStatus.OnTrip;
                results.Add(new AllocationResultItem(currentDeliveryTrip.Id, vehicle.PlateNumber, 0));
            }

            var qtyThisStep = cargoRemaining;
            var cargoLine = cargoLines[cargoIndex];

            // Quy đổi theo TỈ LỆ trên mỗi đơn vị QtyTotal — VD: CargoLine khai
            // báo tổng 1000kg cho 10 đơn vị => mỗi đơn vị nặng 100kg. Nhờ vậy
            // biết chính xác gán thêm bao nhiêu đơn vị nữa thì sẽ VỪA HẾT
            // khối lượng/thể tích còn trống của xe, không chỉ dựa vào Qty.
            var unitWeight = cargoLine.QtyTotal > 0 ? cargoLine.WeightKg / cargoLine.QtyTotal : 0;
            var unitVolume = cargoLine.QtyTotal > 0 ? cargoLine.VolumeM3 / cargoLine.QtyTotal : 0;

            qtyThisStep = Math.Min(qtyThisStep, vehicleRemainingQty);
            if (unitWeight > 0) qtyThisStep = Math.Min(qtyThisStep, vehicleRemainingKg / unitWeight);
            if (unitVolume > 0) qtyThisStep = Math.Min(qtyThisStep, vehicleRemainingM3 / unitVolume);
            qtyThisStep = Math.Max(qtyThisStep, 0); // phòng hờ sai số âm cực nhỏ

            // Hàng đếm theo đơn vị nguyên (số lượng khai là số nguyên) thì không chia
            // lẻ khi giới hạn kg/m³ cắt giữa chừng (VD 2.5 cái máy) — làm tròn xuống,
            // phần lẻ chuyển sang xe kế tiếp.
            // (+0.0001: phép chia kg/m³ có thể ra 3,9999… -> vẫn tính là 4 đơn vị.)
            if (cargoLine.QtyTotal == Math.Floor(cargoLine.QtyTotal))
                qtyThisStep = Math.Min(cargoRemaining, Math.Floor(qtyThisStep + 0.0001m));
            // Hàng số lượng LẺ (VD 2,5 tấn): cắt về 2 chữ số thập phân (đúng độ chính xác
            // cột số lượng trong DB) — trước đây 0,8333 bị DB làm tròn thành 0,83 ×3 = 2,49,
            // đơn kẹt 99,6% không bao giờ Hoàn tất (hoặc làm tròn lên -> nhập kho "vượt số lượng").
            else if (qtyThisStep < cargoRemaining)
                qtyThisStep = Math.Floor(qtyThisStep * 100m) / 100m;

            // Bảo vệ vòng lặp vô hạn: nếu xe HIỆN TẠI (dù còn nguyên, chưa
            // chở gì) vẫn không đủ sức chở NỔI 1 đơn vị hàng này (hàng quá
            // nặng/quá khổ so với sức chứa xe) — ép chuyển sang xe kế tiếp
            // ngay, tránh lặp lại mãi cùng 1 trạng thái không tiến triển.
            if (qtyThisStep <= 0)
            {
                // Xe vừa mở mà chưa chở được gì (Trip rỗng sẽ bị dọn ở dưới) -> trả
                // lại tài xế đó cho xe kế tiếp, không tính là đã dùng.
                if (results.Count > 0 && results[^1].QtyAssigned <= 0) usedDriverIds.Remove(currentDriverEmployeeId);
                vehicleIndex++;
                currentPickupTrip = null;
                currentDeliveryTrip = null;
                if (vehicleIndex < availableVehicles.Count)
                {
                    vehicleRemainingQty = availableVehicles[vehicleIndex].CapacityQty;
                    vehicleRemainingKg = availableVehicles[vehicleIndex].CapacityKg;
                    vehicleRemainingM3 = availableVehicles[vehicleIndex].CapacityM3;
                }
                continue;
            }

            // CompanyPickup: cùng số lượng đó cần ghi vào CẢ 2 chặng (chặng
            // Pickup mang về kho, chặng Delivery mang đi giao) — cùng lô hàng
            // đi qua 2 chặng, không phải 2 lô khác nhau.
            if (isCompanyPickup && currentPickupTrip is not null)
            {
                _db.TripLines.Add(new TripLine { TripId = currentPickupTrip.Id, CargoLineId = cargoLine.Id, QtyPlanned = qtyThisStep });
            }
            // SelfDelivery: hàng ĐÃ được thủ kho nhập đủ trước khi Chia xe (kiểm tra ở đầu hàm)
            // — Chia xe không nhập kho gì nữa, chỉ tạo chặng Giao.
            _db.TripLines.Add(new TripLine { TripId = currentDeliveryTrip.Id, CargoLineId = cargoLine.Id, QtyPlanned = qtyThisStep });

            // Cộng dồn đúng vào kết quả của Trip (Delivery) tương ứng đang hiển thị cho Dispatcher
            var lastResult = results[^1];
            results[^1] = lastResult with { QtyAssigned = lastResult.QtyAssigned + qtyThisStep };

            cargoRemaining -= qtyThisStep;
            vehicleRemainingQty -= qtyThisStep;
            vehicleRemainingKg -= qtyThisStep * unitWeight;
            vehicleRemainingM3 -= qtyThisStep * unitVolume;
            totalAssigned += qtyThisStep;

            // Chuyển sang xe kế tiếp nếu BẤT KỲ chiều nào (Qty/Kg/m³) đã hết
            // chỗ — không chỉ riêng Qty như bản trước. Ngưỡng nhỏ (0.001) để
            // tránh kẹt vòng lặp do sai số làm tròn của kiểu decimal.
            const decimal epsilon = 0.001m;
            if (vehicleRemainingQty <= epsilon || vehicleRemainingKg <= epsilon || vehicleRemainingM3 <= epsilon)
            {
                vehicleIndex++;
                currentPickupTrip = null;
                currentDeliveryTrip = null;
                if (vehicleIndex < availableVehicles.Count)
                {
                    vehicleRemainingQty = availableVehicles[vehicleIndex].CapacityQty;
                    vehicleRemainingKg = availableVehicles[vehicleIndex].CapacityKg;
                    vehicleRemainingM3 = availableVehicles[vehicleIndex].CapacityM3;
                }
            }
        }

        // Dọn dẹp: trường hợp hiếm gặp — 1 xe vừa được gán Trip nhưng phát
        // hiện ngay không chở nổi dù chỉ 1 đơn vị hàng (hàng quá nặng/quá khổ
        // so với CHÍNH xe đó dù Qty vẫn còn dư) — xóa Trip rỗng, trả xe lại Available.
        var emptyTrips = results.Where(r => r.QtyAssigned <= 0).ToList();
        foreach (var empty in emptyTrips)
        {
            var trip = await _db.Trips.Include(t => t.Vehicle).FirstAsync(t => t.Id == empty.TripId);
            var relatedTrips = _db.Trips.Local.Where(t => t.VehicleId == trip.VehicleId && t.TransportOrderId == order.Id);
            foreach (var rt in relatedTrips.ToList()) _db.Trips.Remove(rt);
            trip.Vehicle.Status = VehicleStatus.Available;
            results.Remove(empty);
        }

        // Luôn "chạm" vào đơn để kích hoạt kiểm tra cập nhật đồng thời (cột xmin): 2 người
        // bấm Chia xe cùng 1 đơn cùng lúc -> người lưu sau bị từ chối, không tạo 2 bộ chuyến.
        order.UpdatedAt = DateTime.UtcNow;

        if (lateNote is not null)
        {
            // Ghi lại AI đã chấp nhận chia xe trễ hạn (truy được trách nhiệm khi khách khiếu nại).
            _db.AuditLogs.Add(new AuditLog
            {
                UserId = userId,
                Action = "AllocateVehiclesLate",
                EntityName = nameof(TransportOrder),
                EntityId = order.Id,
                Detail = $"Chia xe TRỄ HẠN cho đơn {order.Code} — đã xác nhận bởi {userName ?? "(không rõ)"}. {lateNote}",
            });
        }

        _db.AuditLogs.Add(new AuditLog
        {
            UserId = userId,
            Action = "AllocateVehicles",
            EntityName = nameof(TransportOrder),
            EntityId = order.Id,
            Detail = $"Thuật toán Greedy đã gán {results.Count} xe, tổng {totalAssigned}/{totalQtyToShip}." +
                     (isCompanyPickup ? " (2 chặng Pickup+Delivery)" : " (1 chặng Delivery)"),
        });

        // KHÔNG chấp nhận chia xe THIẾU: trước đây vẫn báo thành công khi chỉ chở
        // được 1 phần, nhưng sau đó KHÔNG chia thêm được (đơn đã có chuyến) -> đơn
        // kẹt "Đang chạy" mãi ở tiến độ < 100%, cũng không hủy được. Giờ thiếu xe/tài
        // xế thì hủy toàn bộ lần chia này (Rollback), đơn giữ nguyên để chia lại sau.
        var remaining = totalQtyToShip - totalAssigned;
        if (remaining > 0.001m)
        {
            var reason = usedDriverIds.Count >= driverIds.Count && vehicleIndex < availableVehicles.Count
                ? $"chỉ có {driverIds.Count} tài xế rảnh đủ điều kiện"
                : $"chỉ có {availableVehicles.Count} xe rảnh đủ điều kiện";
            if (licenseSkippedPlates.Count > 0)
                reason += $"; không còn tài xế rảnh có hạng bằng phù hợp cho xe {string.Join(", ", licenseSkippedPlates)}";
            return new AllocateVehiclesResponse(false, totalQtyToShip, totalAssigned, new(),
                $"Không đủ xe/tài xế để chở hết đơn — {reason}, chở được {totalAssigned}/{totalQtyToShip}, còn thiếu {remaining}. " +
                "Chưa chia xe (không lưu gì). Hãy chờ thêm xe/tài xế rảnh (hoặc đăng ký thêm ngày làm cho tài xế) rồi chia lại.");
        }

        await _db.SaveChangesAsync();
        await transaction.CommitAsync();
        return new AllocateVehiclesResponse(true, totalQtyToShip, totalAssigned, results,
            lateNote is null ? null : "Đã chia xe dù không kịp hạn (đã ghi nhật ký). " + lateNote + " Nên báo trước cho khách.",
            estimatedTripDays);
    }

    public async Task<int> EstimateRouteDaysAsync(int? pickupLocationId, int? dropoffLocationId, int warehouseId, string pickupMethod)
    {
        // Đơn TẠM (không lưu) chỉ để dùng lại đúng hàm ước tính của Chia xe.
        var draft = new TransportOrder
        {
            PickupMethod = Enum.TryParse<PickupMethod>(pickupMethod, out var m) ? m : PickupMethod.CompanyPickup,
            PickupLocation = pickupLocationId is int p ? await _db.Locations.FindAsync(p) : null,
            DropoffLocation = dropoffLocationId is int d ? await _db.Locations.FindAsync(d) : null,
            Warehouse = (await _db.Warehouses.FindAsync(warehouseId))!,
        };
        if (draft.Warehouse is null) return 1;
        return await EstimateTripDurationDaysAsync(draft);
    }

    /// <summary>Ước tính chuyến này có thể mất BAO NHIÊU NGÀY để hoàn tất —
    /// gọi VietMap API THẬT (đã tích hợp sẵn cho tính năng "Xem lộ trình") để
    /// lấy đúng THỜI GIAN DI CHUYỂN THỰC TẾ (có tính đường sá, giới hạn tốc độ
    /// thật...) cho từng chặng Điểm lấy hàng → Kho và Kho → Điểm giao hàng,
    /// thay vì tự ước lượng bằng công thức toán (đường chim bay) kém chính
    /// xác hơn nhiều. Cộng dồn thời gian cả 2 chặng, chia cho số giờ lái xe
    /// tối đa/ngày theo quy định (có tính giờ nghỉ bắt buộc).
    /// Nếu THIẾU tọa độ hoặc VietMap API lỗi (mất mạng, hết quota...) -> KHÔNG
    /// chặn nghiệp vụ, mặc định 1 ngày (giữ hành vi cũ, an toàn).</summary>
    private async Task<int> EstimateTripDurationDaysAsync(TransportOrder order)
    {
        const int maxDrivingMinutesPerDay = 480; // 8 giờ lái/ngày — đã trừ giờ nghỉ bắt buộc theo quy định

        int totalMinutes = 0;
        bool gotAnyRealRoute = false;

        double? pickupLat = order.PickupLocation?.Lat, pickupLng = order.PickupLocation?.Lng;
        double? dropoffLat = order.DropoffLocation?.Lat, dropoffLng = order.DropoffLocation?.Lng;
        double? warehouseLat = order.Warehouse?.Lat, warehouseLng = order.Warehouse?.Lng;

        // Chặng Lấy hàng CHỈ có khi công ty cử xe đi lấy — đơn khách tự mang tới thì không
        // cộng đoạn này (trước đây vẫn cộng nếu đơn có điểm lấy hàng -> đòi tài xế đăng ký
        // thừa ngày làm).
        if (order.PickupMethod == PickupMethod.CompanyPickup
            && pickupLat.HasValue && pickupLng.HasValue && warehouseLat.HasValue && warehouseLng.HasValue)
        {
            // Chặng Lấy hàng là round-trip THẬT SỰ: xe xuất phát RỖNG từ Kho ra
            // chỗ đối tác lấy hàng, RỒI QUAY VỀ ĐÚNG KHO ĐÓ để nộp hàng — trước
            // đây chỉ tính 1 chiều (Đối tác -> Kho), THIẾU hẳn đoạn xe chạy
            // rỗng ra điểm lấy hàng, khiến ước tính số ngày bị THẤP hơn thực
            // tế. Đường 2 chiều dài bằng nhau -> nhân đôi thời gian 1 chiều,
            // đỡ tốn thêm 1 lần gọi API cho chiều ngược lại (cùng 1 con đường).
            var leg1 = await _routeProvider.GetRouteAsync(new RouteRequest(pickupLat.Value, pickupLng.Value, warehouseLat.Value, warehouseLng.Value));
            if (leg1.Success) { totalMinutes += leg1.DurationMinutes * 2; gotAnyRealRoute = true; }
        }
        if (warehouseLat.HasValue && warehouseLng.HasValue && dropoffLat.HasValue && dropoffLng.HasValue)
        {
            // Chặng Giao hàng CHỈ 1 chiều (Kho -> Điểm giao) — xe giao xong coi
            // như xong việc CHO ĐƠN NÀY, không cần tính đường về (đường về sẽ
            // tính vào đơn/chuyến TIẾP THEO của xe đó, không thuộc phạm vi đơn
            // này).
            var leg2 = await _routeProvider.GetRouteAsync(new RouteRequest(warehouseLat.Value, warehouseLng.Value, dropoffLat.Value, dropoffLng.Value));
            if (leg2.Success) { totalMinutes += leg2.DurationMinutes; gotAnyRealRoute = true; }
        }

        // Thiếu tọa độ HOẶC VietMap không trả được lộ trình nào cả (lỗi mạng,
        // hết quota, thiếu API key...) -> không ước tính được, mặc định 1
        // ngày như hành vi cũ, không để lỗi API bên ngoài chặn cả nghiệp vụ
        // Chia xe quan trọng hơn nhiều.
        if (!gotAnyRealRoute) return 1;

        var estimatedDays = (int)Math.Ceiling((double)totalMinutes / maxDrivingMinutesPerDay);
        return Math.Max(1, estimatedDays);
    }
}