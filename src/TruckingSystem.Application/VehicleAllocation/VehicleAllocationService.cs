using Microsoft.EntityFrameworkCore;
using TruckingSystem.Application.Common.Interfaces;
using TruckingSystem.Application.Routing;
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

    public async Task<AllocateVehiclesResponse> AllocateAsync(int transportOrderId)
    {
        var order = await _db.TransportOrders
            .Include(o => o.CargoLines)
            .Include(o => o.PickupLocation)
            .Include(o => o.DropoffLocation)
            .Include(o => o.Warehouse)
            .FirstOrDefaultAsync(o => o.Id == transportOrderId);

        if (order is null)
            return new AllocateVehiclesResponse(false, 0, 0, new(), "Không tìm thấy đơn vận chuyển.");

        // Chỉ chia xe cho đơn còn ở bước đầu (Nháp / Đã xác nhận). Đơn đã Hủy
        // hoặc đã Hoàn tất mà vẫn chia xe được sẽ tạo chuyến "ma" và giữ xe,
        // tài xế vô ích (trước đây không kiểm tra trạng thái đơn).
        if (order.Status is not (TransportOrderStatus.Draft or TransportOrderStatus.Confirmed))
            return new AllocateVehiclesResponse(false, 0, 0, new(),
                $"Không thể chia xe — đơn đang ở trạng thái {order.Status}. Chỉ chia xe cho đơn Nháp hoặc Đã xác nhận.");

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

        // Bước 1: sắp xếp giảm dần theo sức chứa — cốt lõi của tính "tham lam"
        // BẮT BUỘC phải có ĐĂNG KIỂM CÒN HẠN mới cho dùng — đúng luật giao
        // thông thực tế (xe không đăng kiểm KHÔNG ĐƯỢC PHÉP lưu thông), khác
        // với các giấy tờ khác (Bảo hiểm/Giấy phép vận tải) vẫn áp dụng kiểu
        // MỀM như cũ (chưa nhập thì vẫn cho qua, chỉ chặn khi PHÁT HIỆN thật
        // sự hết hạn) — Đăng kiểm là giấy tờ DUY NHẤT bị siết chặt vì đây là
        // điều kiện PHÁP LÝ bắt buộc để xe được lưu thông trên đường.
        var today = DateTime.UtcNow;
        var vehicleIdsWithValidRegistration = await _db.VehicleDocuments
            .Where(d => d.DocType == VehicleDocumentType.Registration && d.ExpiryDate > today)
            .Select(d => d.VehicleId)
            .Distinct()
            .ToListAsync();

        var vehicleIdsWithExpiredOtherDocs = await _db.VehicleDocuments
            .Where(d => d.DocType != VehicleDocumentType.Other && d.DocType != VehicleDocumentType.Registration && d.ExpiryDate <= today)
            .Select(d => d.VehicleId)
            .Distinct()
            .ToListAsync();

        var availableVehicles = await _db.Vehicles
            .Where(v => v.Status == VehicleStatus.Available
                     && vehicleIdsWithValidRegistration.Contains(v.Id)
                     && !vehicleIdsWithExpiredOtherDocs.Contains(v.Id)
                     && (v.WarehouseId == null || v.WarehouseId == order.WarehouseId))
            .OrderByDescending(v => v.CapacityQty)
            .ToListAsync();

        if (!availableVehicles.Any())
            return new AllocateVehiclesResponse(false, totalQtyToShip, 0, new(), "Không còn xe nào đang rảnh VÀ đang đóng quân đúng kho của đơn này VÀ có Đăng kiểm còn hạn (xe chưa nhập Đăng kiểm hoặc Đăng kiểm đã hết hạn đều bị loại — bắt buộc theo luật giao thông).");

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

        var driverIds = await _db.DriverProfiles
            .Include(dp => dp.Employee)
            .Where(dp => dp.Employee.Status == EmployeeStatus.Active && dp.LicenseExpiryDate > today
                      && (dp.WarehouseId == null || dp.WarehouseId == order.WarehouseId))
            .Select(dp => dp.EmployeeId)
            .ToListAsync();
        driverIds = driverIds.Except(busyDriverIds).ToList();

        // BẮT BUỘC tài xế phải có ca làm việc ĐĂNG KÝ SẴN, đủ hết TẤT CẢ các
        // ngày trong toàn bộ thời gian chuyến dự kiến chạy — nhất quán với quy
        // tắc Đăng kiểm xe (cũng bắt buộc phải có, không cho qua nếu thiếu dữ
        // liệu). Tài xế CHƯA đăng ký lịch nào cũng bị loại giống như thiếu ca
        // — quan trọng với tuyến ĐƯỜNG DÀI (VD Hà Nội - Sài Gòn ~1.700km,
        // thực tế thường mất HƠN 1 NGÀY do quy định giờ lái xe liên tục + nghỉ
        // ngơi bắt buộc), tránh trường hợp tài xế "hết ca" giữa đường mà hệ
        // thống không hề biết.
        var estimatedTripDays = await EstimateTripDurationDaysAsync(order);
        // Ngày ca làm việc tính theo GIỜ VIỆT NAM (UTC+7). Dùng UtcNow.Date sẽ
        // lệch 1 ngày khi chia xe trong khoảng 0h–7h sáng (giờ VN).
        var todayVn = DateTime.UtcNow.AddHours(7).Date;
        var neededDates = Enumerable.Range(0, estimatedTripDays).Select(offset => todayVn.AddDays(offset)).ToList();

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
                $"Không có tài xế nào khả dụng (đã hết hạn bằng lái, đang bận chuyến khác, không đóng quân đúng kho của đơn này, hoặc CHƯA đăng ký ĐỦ ca làm việc{dayRangeText} — bắt buộc phải đăng ký lịch trước).");
        }

        // Dùng ĐÚNG kho trung chuyển Dispatcher đã CHỌN khi tạo đơn (order.WarehouseId)
        // — trước đây luôn lấy kho ĐẦU TIÊN trong hệ thống bất kể đơn đi đâu,
        // gây ra lộ trình vô lý (VD đơn Bình Dương -> Hà Nội nhưng lại đi qua
        // 1 kho không liên quan gì tới tuyến đường thực tế).
        var defaultWarehouseId = order.WarehouseId;

        var isCompanyPickup = order.PickupMethod == PickupMethod.CompanyPickup;

        var results = new List<AllocationResultItem>();
        int vehicleIndex = 0, driverIndex = 0;
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
                var vehicle = availableVehicles[vehicleIndex];
                var driverEmployeeId = driverIds[driverIndex % driverIds.Count];
                driverIndex++;
                currentDriverEmployeeId = driverEmployeeId;

                if (isCompanyPickup)
                {
                    currentPickupTrip = new Trip
                    {
                        TransportOrderId = order.Id,
                        VehicleId = vehicle.Id,
                        DriverEmployeeId = driverEmployeeId,
                        Status = TripStatus.Planned,
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

            // Bảo vệ vòng lặp vô hạn: nếu xe HIỆN TẠI (dù còn nguyên, chưa
            // chở gì) vẫn không đủ sức chở NỔI 1 đơn vị hàng này (hàng quá
            // nặng/quá khổ so với sức chứa xe) — ép chuyển sang xe kế tiếp
            // ngay, tránh lặp lại mãi cùng 1 trạng thái không tiến triển.
            if (qtyThisStep <= 0)
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
                continue;
            }

            // CompanyPickup: cùng số lượng đó cần ghi vào CẢ 2 chặng (chặng
            // Pickup mang về kho, chặng Delivery mang đi giao) — cùng lô hàng
            // đi qua 2 chặng, không phải 2 lô khác nhau.
            if (isCompanyPickup && currentPickupTrip is not null)
            {
                _db.TripLines.Add(new TripLine { TripId = currentPickupTrip.Id, CargoLineId = cargoLine.Id, QtyPlanned = qtyThisStep });
            }
            else if (!isCompanyPickup)
            {
                // SelfDelivery: khách đã tự mang hàng tới kho rồi — không có
                // chặng Pickup nào để "hoàn tất" và kích hoạt nhập kho sau này
                // như CompanyPickup, nên phải nhập kho NGAY tại đây.
                await _db.SaveChangesAsync(); // cần lưu trước để có CargoLine/Trip Id ổn định cho ReceiveGoodsAsync
                var receiveResult = await _warehouseService.ReceiveGoodsAsync(
                    defaultWarehouseId, order.Id, cargoLine.Id, qtyThisStep, currentDriverEmployeeId, tripId: null);
                // Trước đây BỎ QUA kết quả — kho hết chỗ/thiếu khu vực phù hợp vẫn
                // báo chia xe thành công, nhưng hàng KHÔNG hề vào kho, tới lúc giao
                // mới lỗi "không đủ tồn kho". Giờ dừng ngay; return khi chưa Commit
                // -> transaction tự Rollback, không để lại chuyến/nhập kho dở dang.
                if (!receiveResult.Success)
                    return new AllocateVehiclesResponse(false, totalQtyToShip, 0, new(),
                        "Không thể nhập kho cho đơn khách tự mang tới: " + receiveResult.Message);
            }
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

        // Đơn chuyển sang Confirmed nếu vì lý do nào đó vẫn còn Draft khi Chia
        // xe (phòng hờ gọi trực tiếp API bỏ qua bước Xác nhận thủ công ở UI).
        if (order.Status == TransportOrderStatus.Draft)
            order.Status = TransportOrderStatus.Confirmed;

        _db.AuditLogs.Add(new AuditLog
        {
            Action = "AllocateVehicles",
            EntityName = nameof(TransportOrder),
            EntityId = order.Id,
            Detail = $"Thuật toán Greedy đã gán {results.Count} xe, tổng {totalAssigned}/{totalQtyToShip}." +
                     (isCompanyPickup ? " (2 chặng Pickup+Delivery)" : " (1 chặng Delivery)"),
        });

        await _db.SaveChangesAsync();

        var remaining = totalQtyToShip - totalAssigned;
        var message = remaining > 0
            ? $"Chỉ đủ xe chở {totalAssigned}/{totalQtyToShip} — còn thiếu {remaining}, cần thêm xe hoặc chờ xe rảnh."
            : null;

        await transaction.CommitAsync();
        return new AllocateVehiclesResponse(true, totalQtyToShip, totalAssigned, results, message, estimatedTripDays);
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

        if (pickupLat.HasValue && pickupLng.HasValue && warehouseLat.HasValue && warehouseLng.HasValue)
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