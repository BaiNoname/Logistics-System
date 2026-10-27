using Microsoft.EntityFrameworkCore;
using TruckingSystem.Application.Common;
using TruckingSystem.Application.Calendar;
using TruckingSystem.Application.Common.Interfaces;
using TruckingSystem.Application.Warehouses;
using TruckingSystem.Domain.Entities;
using TruckingSystem.Domain.Enums;

namespace TruckingSystem.Application.TransportOrders;

public class TransportOrderService : ITransportOrderService
{
    private readonly IApplicationDbContext _db;
    private readonly IWarehouseSlottingService _warehouseService;
    private readonly IGoogleCalendarService _calendarService;

    public TransportOrderService(IApplicationDbContext db, IWarehouseSlottingService warehouseService, IGoogleCalendarService calendarService)
    {
        _db = db;
        _warehouseService = warehouseService;
        _calendarService = calendarService;
    }

    public async Task<List<TransportOrderListItemDto>> GetAllAsync()
    {
        return await _db.TransportOrders
            .Include(o => o.Customer)
            .OrderByDescending(o => o.Id)
            // Truyền ĐỦ tham số (trong truy vấn EF không được bỏ tham số tùy chọn).
            .Select(o => new TransportOrderListItemDto(
                o.Id, o.Code, o.Customer.Name, o.Status.ToString(),
                o.ProgressPercent, o.PickupPoint, o.DropoffPoint, o.PublicTrackingToken,
                o.Trips.Count(t => t.Status != TripStatus.Cancelled),
                o.CommittedDeliveryDate, o.Warehouse.Name,
                o.Contract != null ? o.Contract.Code : null))
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
            .Include(o => o.Contract)
            .Include(o => o.Trips).ThenInclude(t => t.Vehicle)
            .Include(o => o.Trips).ThenInclude(t => t.DriverEmployee)
            .Include(o => o.Trips).ThenInclude(t => t.TripLines)
            .Include(o => o.Trips).ThenInclude(t => t.Warehouse)
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

        var zoneError = await ValidateWarehouseZonesAsync(request.WarehouseId, request.CargoLines.Select(c => (c.IsHazmat, c.IsFragile)));
        if (zoneError is not null)
            throw new InvalidOperationException(zoneError);

        var locationError = await ValidateLocationsAsync(request.PickupMethod, request.PickupLocationId, request.DropoffLocationId, request.CustomerId);
        if (locationError is not null)
            throw new InvalidOperationException(locationError);

        var contractError = await ValidateContractAsync(request.ContractId, request.CustomerId);
        if (contractError is not null)
            throw new InvalidOperationException(contractError);

        // HẠN GIAO BẮT BUỘC với đơn mới — cả hệ thống nhắc hạn (Chia xe cảnh báo trễ, mốc xuất
        // phát muộn nhất, thông báo, tỉ lệ giao đúng hạn) dựa vào ngày này.
        if (request.CommittedDeliveryDate is null)
            throw new InvalidOperationException(MissingCommittedDateMessage);
        var committedError = ValidateCommittedDate(request.CommittedDeliveryDate);
        if (committedError is not null)
            throw new InvalidOperationException(committedError);

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
            ContractId = request.ContractId,
            PickupPoint = request.PickupPoint,
            DropoffPoint = request.DropoffPoint,
            WarehouseId = request.WarehouseId,
            CommittedDeliveryDate = VnClock.ToVnWallClock(request.CommittedDeliveryDate), // lưu theo GIỜ VN
            Status = TransportOrderStatus.Draft,
            // Khách tự mang tới kho: không có chặng lấy hàng -> không lưu điểm lấy hàng
            // (tránh ước tính số ngày / lộ trình cộng nhầm đoạn đi lấy hàng).
            PickupLocationId = ParsePickupMethod(request.PickupMethod) == PickupMethod.SelfDelivery ? null : request.PickupLocationId,
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

        if (request.ContractId.HasValue)
            order.Contract = await _db.Contracts.FindAsync(request.ContractId.Value);
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

        var zoneError = await ValidateWarehouseZonesAsync(request.WarehouseId, request.CargoLines.Select(c => (c.IsHazmat, c.IsFragile)));
        if (zoneError is not null)
            return new OrderActionResult(false, zoneError);

        var locationError = await ValidateLocationsAsync(request.PickupMethod, request.PickupLocationId, request.DropoffLocationId, order.CustomerId);
        if (locationError is not null)
            return new OrderActionResult(false, locationError);

        // Chỉ kiểm tra hiệu lực khi ĐỔI hợp đồng — nháp cũ giữ nguyên hợp đồng vẫn sửa được
        // thông tin khác (lúc Xác nhận sẽ kiểm tra lại hiệu lực).
        if (request.ContractId != order.ContractId)
        {
            var contractError = await ValidateContractAsync(request.ContractId, order.CustomerId);
            if (contractError is not null)
                return new OrderActionResult(false, contractError);
        }

        // Chỉ kiểm tra "không phải ngày đã qua" khi NGÀY hạn giao bị đổi — đơn nháp cũ
        // giữ nguyên hạn giao (đã qua) vẫn sửa được các thông tin khác.
        if (request.CommittedDeliveryDate is null)
            return new OrderActionResult(false, MissingCommittedDateMessage);
        var committedChanged = VnClock.ToVnWallClock(request.CommittedDeliveryDate)?.Date != order.CommittedDeliveryDate?.Date;
        var committedError = committedChanged ? ValidateCommittedDate(request.CommittedDeliveryDate) : null;
        if (committedError is not null)
            return new OrderActionResult(false, committedError);

        order.PickupPoint = request.PickupPoint;
        order.DropoffPoint = request.DropoffPoint;
        order.WarehouseId = request.WarehouseId;
        order.CommittedDeliveryDate = VnClock.ToVnWallClock(request.CommittedDeliveryDate); // lưu theo GIỜ VN
        order.PickupLocationId = ParsePickupMethod(request.PickupMethod) == PickupMethod.SelfDelivery ? null : request.PickupLocationId;
        order.DropoffLocationId = request.DropoffLocationId;
        order.PickupMethod = ParsePickupMethod(request.PickupMethod);
        order.ContractId = request.ContractId;

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

        // Kho có thể đã bị sửa (xóa khu) sau khi tạo đơn nháp -> kiểm tra lại lúc xác nhận.
        var zoneError = await ValidateWarehouseZonesAsync(order.WarehouseId, order.CargoLines.Select(c => (c.IsHazmat, c.IsFragile)));
        if (zoneError is not null)
            return new OrderActionResult(false, zoneError);

        // Đơn NHÁP tạo trước khi có các quy tắc mới (bắt buộc hạn giao, điểm lấy ≠ điểm giao, địa
        // điểm có tọa độ) hoặc để lâu tới khi hạn giao đã qua -> kiểm tra lại lúc xác nhận; trước
        // đây xác nhận thẳng, tới Chia xe mới vướng (hoặc chia xe với hạn giao rỗng / đã qua).
        if (order.CommittedDeliveryDate is null)
            return new OrderActionResult(false, MissingCommittedDateMessage + " Bấm Sửa đơn để nhập hạn giao rồi xác nhận lại.");
        if (order.CommittedDeliveryDate.Value.Date < VnClock.Today)
            return new OrderActionResult(false,
                $"Hạn giao {order.CommittedDeliveryDate.Value:dd/MM/yyyy} đã qua — bấm Sửa đơn để thỏa thuận hạn mới với khách rồi xác nhận lại.");
        var locationError = await ValidateLocationsAsync(order.PickupMethod.ToString(), order.PickupLocationId, order.DropoffLocationId, order.CustomerId);
        if (locationError is not null)
            return new OrderActionResult(false, locationError + " (Bấm Sửa đơn để chọn lại địa điểm.)");
        // Hợp đồng có thể đã hết hạn / bị sửa trong lúc đơn còn nháp -> kiểm tra lại.
        var contractError = await ValidateContractAsync(order.ContractId, order.CustomerId);
        if (contractError is not null)
            return new OrderActionResult(false, contractError + " (Bấm Sửa đơn để chọn hợp đồng khác hoặc bỏ trống.)");

        order.Status = TransportOrderStatus.Confirmed;
        await _db.SaveChangesAsync();
        return new OrderActionResult(true, null);
    }

    /// <summary>ĐỔI HẠN GIAO cho đơn đã xác nhận / đang chạy — nghiệp vụ thật: khách xin dời ngày,
    /// hoặc hai bên thỏa thuận lùi hạn (kẹt đường, thời tiết...). Cũng dùng để BỔ SUNG hạn cho
    /// đơn cũ chưa có hạn. Không mở sửa cả đơn (địa điểm/hàng đã gắn với chuyến, tồn kho, chi phí).
    /// Quy tắc: bắt buộc lý do; không chọn ngày đã qua; đơn Hoàn tất / Hủy thì KHÔNG đổi (tránh
    /// sửa lùi cho đẹp tỉ lệ giao đúng hạn); ghi nhật ký người đổi + hạn cũ -> hạn mới + lý do.
    /// Tình trạng hạn / mốc xuất phát muộn nhất / thông báo tự tính lại theo hạn mới.</summary>
    public async Task<OrderActionResult> ChangeDeadlineAsync(int id, ChangeDeadlineRequest request, string? userId, string? userName)
    {
        var order = await _db.TransportOrders.FirstOrDefaultAsync(o => o.Id == id);
        if (order is null) return new OrderActionResult(false, "Không tìm thấy đơn.");
        if (order.Status == TransportOrderStatus.Draft)
            return new OrderActionResult(false, "Đơn còn Nháp — đổi hạn giao bằng nút Sửa đơn.");
        if (order.Status is TransportOrderStatus.Done or TransportOrderStatus.Cancelled)
            return new OrderActionResult(false, "Đơn đã Hoàn tất / Đã hủy — không đổi hạn giao được nữa.");

        if (ParseRequester(request.RequestedBy) is not DeadlineChangeRequester requester)
            return new OrderActionResult(false, "Vui lòng chọn Bên yêu cầu đổi hạn (Khách hàng / Công ty) — khách xin dời trễ hơn thì tính phí lưu kho.");

        var reason = (request.Reason ?? "").Trim();
        if (reason.Length < 5)
            return new OrderActionResult(false, "Phải ghi lý do đổi hạn giao (ít nhất 5 ký tự) — VD \"Khách đề nghị dời ngày nhận\".");
        if (reason.Length > 500) reason = reason[..500];

        var newDate = VnClock.ToVnWallClock(request.NewDeliveryDate)!.Value.Date;
        var dateError = ValidateCommittedDate(newDate);
        if (dateError is not null) return new OrderActionResult(false, dateError);

        var oldDate = order.CommittedDeliveryDate?.Date;
        if (oldDate == newDate) return new OrderActionResult(false, "Hạn giao mới trùng hạn hiện tại — không có gì thay đổi.");

        // PHÍ LƯU KHO: khách xin dời TRỄ hơn -> ước tính ngay (m³ đang trong kho × số ngày dời × đơn giá kho),
        // chốt đơn giá + lưu lịch sử; số chính thức tính theo ngày hàng thực sự còn nằm kho (StorageFeeRules).
        var preview = await BuildStorageFeePreviewAsync(order, newDate, requester);
        _db.DeliveryDeadlineChanges.Add(new DeliveryDeadlineChange
        {
            TransportOrderId = order.Id,
            OldDate = oldDate,
            NewDate = newDate,
            RequestedBy = requester,
            Reason = reason,
            ChangedByUserName = userName,
            DelayDays = preview.DelayDays,
            StorageFeePerM3PerDay = preview.RatePerM3PerDay,
            VolumeM3AtChange = preview.VolumeM3,
            EstimatedFee = preview.EstimatedFee,
        });

        order.CommittedDeliveryDate = newDate;
        order.UpdatedAt = DateTime.UtcNow; // kích hoạt kiểm tra cập nhật đồng thời (xmin)
        _db.AuditLogs.Add(new AuditLog
        {
            UserId = userId,
            Action = "ChangeDeliveryDeadline",
            EntityName = nameof(TransportOrder),
            EntityId = order.Id,
            Detail = $"Đổi hạn giao đơn {order.Code}: {(oldDate is { } o ? o.ToString("dd/MM/yyyy") : "(chưa có hạn)")} -> {newDate:dd/MM/yyyy}. " +
                     $"Bên yêu cầu: {StorageFeeRules.RequesterText(requester)}. Lý do: {reason}. Người đổi: {userName ?? "(không rõ)"}." +
                     (preview.EstimatedFee > 0 ? $" Phí lưu kho ước tính: {preview.EstimatedFee:N0} đ." : ""),
        });
        await _db.SaveChangesAsync();

        var feeText = preview.EstimatedFee > 0 ? $"\n💰 {preview.Note}" : "";

        // Hạn MỚI sớm hơn mà các chuyến đã chia không còn kịp -> báo ngay (không chặn: Vận hành có
        // thể đã thỏa thuận, sẽ đổi xe / cho chạy sớm hơn). Cùng quy tắc với cột "Tình trạng hạn".
        var orderTrips = await _db.Trips.Where(t => t.TransportOrderId == order.Id && t.Status != TripStatus.Cancelled).ToListAsync();
        var lateTrips = orderTrips
            .Where(t => t.Status is TripStatus.Planned or TripStatus.InProgress)
            .Select(t => (Trip: t, Info: TruckingSystem.Application.Trips.DeliveryDeadlineRules.Evaluate(t, orderTrips, newDate, VnClock.Today)))
            .Where(x => x.Info.Level == TruckingSystem.Application.Trips.DeliveryDeadlineRules.Level.Late)
            .Select(x => $"#{x.Trip.Id}: {x.Info.Text}")
            .ToList();
        if (lateTrips.Count > 0)
            feeText += $"\n⚠️ Với hạn mới, {lateTrips.Count} chuyến sẽ KHÔNG kịp: {string.Join("; ", lateTrips.Take(5))}. " +
                       "Cho xe xuất phát sớm hơn / đổi xe-tài xế, hoặc báo khách.";
        return new OrderActionResult(true, (oldDate is null
            ? $"Đã bổ sung hạn giao {newDate:dd/MM/yyyy}."
            : $"Đã đổi hạn giao từ {oldDate:dd/MM/yyyy} sang {newDate:dd/MM/yyyy} (đã ghi nhật ký).") + feeText);
    }

    private static DeadlineChangeRequester? ParseRequester(string? value)
        => Enum.TryParse<DeadlineChangeRequester>(value, true, out var r) && Enum.IsDefined(r) ? r : null;

    /// <summary>Phí lưu kho ƯỚC TÍNH nếu đổi hạn giao sang <paramref name="newDate"/> — cho hộp đổi hạn
    /// hiện ngay trước khi lưu. requestedBy chưa chọn thì coi như Khách hàng (để báo trước số tiền).</summary>
    public async Task<StorageFeePreviewDto?> PreviewStorageFeeAsync(int id, DateTime newDate, string? requestedBy)
    {
        var order = await _db.TransportOrders.FirstOrDefaultAsync(o => o.Id == id);
        if (order is null) return null;
        var requester = ParseRequester(requestedBy) ?? DeadlineChangeRequester.Customer;
        return await BuildStorageFeePreviewAsync(order, VnClock.ToVnWallClock(newDate)!.Value.Date, requester);
    }

    public Task<StorageFeeDto?> GetStorageFeeAsync(int id) => StorageFeeRules.ComputeAsync(_db, id);

    private async Task<StorageFeePreviewDto> BuildStorageFeePreviewAsync(TransportOrder order, DateTime newDate, DeadlineChangeRequester requester)
    {
        var oldDate = order.CommittedDeliveryDate?.Date;
        var delayDays = oldDate is DateTime od && newDate.Date > od ? (newDate.Date - od).Days : 0;
        var rate = await _db.Warehouses.Where(w => w.Id == order.WarehouseId).Select(w => w.StorageFeePerM3PerDay).FirstOrDefaultAsync();
        var volume = await StorageFeeRules.CurrentVolumeM3Async(_db, order.Id, order.WarehouseId);

        if (requester == DeadlineChangeRequester.Company)
            return new StorageFeePreviewDto(delayDays, volume, rate, 0, "Công ty yêu cầu đổi hạn — KHÔNG tính phí lưu kho cho khách.");
        if (oldDate is null)
            return new StorageFeePreviewDto(0, volume, rate, 0, "Đơn chưa có hạn giao trước đó (bổ sung hạn) — không tính phí lưu kho.");
        if (delayDays == 0)
            return new StorageFeePreviewDto(0, volume, rate, 0, "Hạn mới không trễ hơn hạn cũ — không tính phí lưu kho.");
        if (rate <= 0)
            return new StorageFeePreviewDto(delayDays, volume, 0, 0, "Kho chưa khai báo phí lưu kho (Sửa kho → Phí lưu kho) — chưa tính phí.");
        if (volume <= 0)
            return new StorageFeePreviewDto(delayDays, 0, rate, 0,
                $"Khách dời thêm {delayDays} ngày — hiện hàng của đơn CHƯA nằm trong kho nên chưa phát sinh phí; phí tính theo ngày hàng thực sự nằm kho.");
        var fee = Math.Round(delayDays * volume * rate, 0);
        return new StorageFeePreviewDto(delayDays, volume, rate, fee,
            $"Phí lưu kho ước tính (khách chịu): {delayDays} ngày × {volume:0.##} m³ × {rate:N0} đ = {fee:N0} đ. Số chốt theo ngày hàng thực sự còn trong kho.");
    }

    /// <summary>Hủy đơn — xử lý theo 3 mức tùy tình trạng hàng hóa thật tại
    /// thời điểm hủy (KHÔNG hủy lẻ từng Trip, chỉ hủy qua Đơn):
    ///   Mức 1 — Chưa chạm kho (Planned/InProgress, chưa Pickup xong): hủy
    ///     thẳng mọi Trip còn hoạt động, trả xe/tài xế rảnh ngay.
    ///   Mức 2 — Hàng đã nằm trong kho (Pickup đã Completed VÀ đã nhập kho đủ
    ///     — còn hàng chờ nhập thì CHẶN hủy) nhưng Giao hàng còn
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

        // Kho đã XÁC NHẬN XUẤT (hàng đã bốc lên xe) cho chuyến giao CHƯA chạy -> hàng đang nằm trên xe,
        // không còn trong kho: hủy lúc này sẽ tính sai chuyến Trả hàng / trả khách. Hoàn tác xuất trước.
        var loadedTripIds = await _db.GoodsIssues
            .Where(g => g.TransportOrderId == order.Id && g.TripId != null && g.ReversedAt == null
                        && g.Trip!.LegType == TripLegType.Delivery && g.Trip.Status == TripStatus.Planned)
            .Select(g => g.TripId!.Value).Distinct().ToListAsync();
        if (loadedTripIds.Count > 0)
            return new OrderActionResult(false,
                $"Kho đã xuất hàng lên xe cho chuyến giao #{string.Join(", #", loadedTripIds)} (xe chưa chạy) — vào màn Kho → 📤 Xuất kho theo chuyến, " +
                "bấm Hoàn tác xuất (dỡ hàng về kệ) rồi mới hủy đơn.");

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
                "Hãy DUYỆT chuyến đó rồi NHẬP KHO (màn Kho → 📦 Nhập hàng từ chuyến) trước, rồi mới hủy đơn — hàng trong kho sẽ được trả lại khi hủy. " +
                "CHỈ Từ chối khi tài xế báo nhầm và hàng THẬT SỰ chưa về kho.");

        // CHẶN nếu có chuyến Lấy hàng đã về kho (Hoàn tất) mà hàng CHƯA NHẬP KHO hết: hàng
        // nằm ở kho nhưng chưa có tồn kho -> hủy lúc này không trả được hàng (không có gì để
        // xuất), cũng không tạo đúng chuyến Trả hàng. Nhập kho trước rồi hủy (giữ logic đơn giản).
        var waitingReceipt = await PendingReceiptRules.LoadAsync(_db, l => l.Trip.TransportOrderId == id);
        if (waitingReceipt.Count > 0)
            return new OrderActionResult(false,
                "Chuyến lấy hàng " + PendingReceiptRules.Describe(waitingReceipt) +
                " đã về kho nhưng hàng CHƯA NHẬP KHO. Hãy nhập kho trước (màn Kho → 📦 Nhập hàng từ chuyến — thủ công hoặc Best-Fit), " +
                "rồi mới hủy đơn — hàng trong kho sẽ được trả lại khách khi hủy.");

        // Chuyến Lấy hàng đang chạy: vẫn cho hủy, nhưng báo lại để điều phối gọi
        // tài xế dừng (app tài xế sẽ tự ngừng tracking khi thấy chuyến bị hủy).
        var pickupsOnRoadIds = order.Trips
            .Where(t => t.LegType == TripLegType.Pickup && t.Status == TripStatus.InProgress)
            .Select(t => t.Id).ToList();
        string? cancelNote = pickupsOnRoadIds.Count > 0
            ? "Đã hủy đơn. Lưu ý: chuyến lấy hàng #" + string.Join(", #", pickupsOnRoadIds) +
              " đang trên đường — hãy liên hệ tài xế để dừng lại."
            : null;

        var isSelfDelivery = order.PickupMethod == PickupMethod.SelfDelivery;
        var cargoLineIds = order.CargoLines.Select(c => c.Id).ToList();

        var reason = "Hủy theo đơn hàng #" + order.Id;
        var now = DateTime.UtcNow;

        // Chia xe có thể tách 1 đơn ra NHIỀU XE — mỗi xe 1 cặp chuyến (Lấy hàng +
        // Giao hàng, cùng xe + tài xế). Xử lý RIÊNG TỪNG XE, vì mỗi xe có thể đang
        // ở giai đoạn khác nhau (xe A đã lấy hàng về kho, xe B còn đang đi lấy).
        var tripsByVehicle = order.Trips
            .Where(t => t.LegType != TripLegType.Return && t.Status != TripStatus.Cancelled)
            .GroupBy(t => t.VehicleId)
            .ToList();

        var cancelledTripIds = new List<int>();
        // Phần hàng trong kho mà chuyến Trả hàng (tạo mới / đang có) sẽ mang đi — theo (kho, dòng hàng).
        var coveredByReturn = new Dictionary<(int WarehouseId, int CargoLineId), decimal>();
        void Cover(int? warehouseId, IEnumerable<TripLine> lines)
        {
            if (!warehouseId.HasValue) return;
            foreach (var l in lines)
            {
                var key = (warehouseId.Value, l.CargoLineId);
                coveredByReturn[key] = (coveredByReturn.TryGetValue(key, out var q) ? q : 0) + l.QtyPlanned;
            }
        }
        foreach (var r in order.Trips.Where(t => t.LegType == TripLegType.Return && t.Status != TripStatus.Cancelled && t.Status != TripStatus.Completed))
            Cover(r.WarehouseId, r.TripLines);

        // Chuyến Giao đã được ĐỔI sang xe khác (nút Đổi xe/tài xế) thì không còn nằm cùng
        // nhóm với chuyến Lấy hàng của nó -> xét theo TỒN KHO THẬT: kho còn đủ hàng (chưa
        // dành cho chuyến Trả hàng nào) cho các dòng của chuyến giao này = hàng đã ở kho.
        var stockByKey = (await _db.WarehouseStocks
                .Where(s => cargoLineIds.Contains(s.CargoLineId))
                .Select(s => new { s.WarehouseId, s.CargoLineId, s.QtyOnHand })
                .ToListAsync())
            .GroupBy(s => (s.WarehouseId, s.CargoLineId))
            .ToDictionary(g => g.Key, g => g.Sum(x => x.QtyOnHand));
        bool StockCovers(Trip dTrip) =>
            dTrip.WarehouseId.HasValue && dTrip.TripLines.All(dl =>
            {
                var stockKey = (dTrip.WarehouseId.Value, dl.CargoLineId);
                var stockOnHand = stockByKey.TryGetValue(stockKey, out var sq) ? sq : 0;
                var stockCovered = coveredByReturn.TryGetValue(stockKey, out var sc) ? sc : 0;
                return stockOnHand - stockCovered >= dl.QtyPlanned;
            });
        var vehiclesToRelease = new List<Vehicle>();

        foreach (var group in tripsByVehicle)
        {
            var pickup = group.FirstOrDefault(t => t.LegType == TripLegType.Pickup);
            var delivery = group.FirstOrDefault(t => t.LegType == TripLegType.Delivery);

            // Hàng của RIÊNG xe này đã thật sự nằm trong kho chưa? (Chỉ đơn công ty đi lấy
            // hàng mới cần chuyến Trả hàng chở về đối tác. Đơn KHÁCH TỰ MANG TỚI: khách tự
            // nhận lại tại kho — không tạo chuyến Trả hàng (đơn này không có địa chỉ để trả),
            // phần hàng trong kho được xuất trả tại kho ở bước cuối bên dưới.)
            var goodsInWarehouse = !isSelfDelivery &&
                (pickup is not null ? pickup.Status == TripStatus.Completed : delivery is not null && StockCovers(delivery));

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
                    // Ước tính kế hoạch cho chuyến Trả hàng (Kho -> điểm lấy hàng ban đầu) = 1 nửa
                    // chặng Lấy hàng (vốn là khứ hồi Kho -> đối tác -> Kho) của chính xe này —
                    // để báo cáo xăng có "km kế hoạch" và tính đúng số ngày làm của tài xế.
                    EstimatedDistanceKm = pickup?.EstimatedDistanceKm is decimal pickupKm ? Math.Round(pickupKm / 2, 1) : null,
                    EstimatedDurationMinutes = pickup?.EstimatedDurationMinutes is int pickupMin ? pickupMin / 2 : null,
                    TripLines = delivery.TripLines.Select(l => new TripLine
                    {
                        CargoLineId = l.CargoLineId,
                        QtyPlanned = l.QtyPlanned,
                    }).ToList(),
                });
                Cover(delivery.WarehouseId, delivery.TripLines);

                delivery.Status = TripStatus.Cancelled;
                delivery.CancelReason = reason;
                cancelledTripIds.Add(delivery.Id);
            }
            else
            {
                // Mức 1 — hàng của xe này chưa vào kho (hoặc đơn khách tự mang tới) -> hủy
                // thẳng mọi chuyến còn hoạt động của xe này (gồm cả Lấy hàng đã báo xong
                // nhưng CHƯA duyệt: hàng chưa nhập kho, nhập kho chỉ xảy ra lúc duyệt), trả xe.
                foreach (var trip in group.Where(t =>
                    t.Status is TripStatus.Planned or TripStatus.InProgress or TripStatus.PendingConfirmation))
                {
                    trip.Status = TripStatus.Cancelled;
                    trip.CancelReason = reason;
                    // Chuyến đã chạy dở -> ghi giờ kết thúc = lúc hủy (giới hạn khung giờ
                    // nhận video, thời lượng chuyến trong báo cáo).
                    if (trip.StartTime.HasValue) trip.EndTime ??= now;
                    cancelledTripIds.Add(trip.Id);
                }
                // Trả xe về Rảnh (xét SAU vòng lặp): chỉ khi xe "Đang chạy" — không ghi đè Bảo
                // trì / Ngưng hoạt động — VÀ xe không còn chuyến nào của ĐƠN KHÁC (xe đã được
                // đổi ra khỏi đơn này có thể đang chạy đơn khác).
                var vehicle = group.First().Vehicle;
                if (vehicle != null) vehiclesToRelease.Add(vehicle);
            }
        }

        foreach (var vehicle in vehiclesToRelease)
        {
            if (vehicle.Status != VehicleStatus.OnTrip) continue;
            var busyElsewhere = await _db.Trips.AnyAsync(t => t.VehicleId == vehicle.Id && t.TransportOrderId != order.Id &&
                (t.Status == TripStatus.Planned || t.Status == TripStatus.InProgress || t.Status == TripStatus.PendingConfirmation));
            var hasNewReturnTrip = _db.Trips.Local.Any(t => t.Id == 0 && t.VehicleId == vehicle.Id && t.LegType == TripLegType.Return);
            if (!busyElsewhere && !hasNewReturnTrip) vehicle.Status = VehicleStatus.Available;
        }

        order.Status = TransportOrderStatus.Cancelled;
        order.UpdatedAt = now;
        await _db.SaveChangesAsync();

        // Hàng của đơn còn NẰM TRONG KHO mà không có chuyến Trả hàng nào mang đi (đơn khách
        // tự mang tới; hoặc đã nhập kho tay nhưng chưa Chia xe) -> XUẤT TRẢ KHÁCH TẠI KHO
        // ngay khi hủy. Trước đây hàng nằm kẹt trong kho mãi, không chuyến nào xuất ra.
        var stocks = await _db.WarehouseStocks
            .Where(s => cargoLineIds.Contains(s.CargoLineId) && s.QtyOnHand > 0)
            .Select(s => new { s.WarehouseId, s.CargoLineId, s.QtyOnHand })
            .ToListAsync();
        decimal returnedAtWarehouse = 0;
        foreach (var stock in stocks)
        {
            var covered = coveredByReturn.TryGetValue((stock.WarehouseId, stock.CargoLineId), out var coveredQty) ? coveredQty : 0;
            var uncovered = stock.QtyOnHand - covered;
            if (uncovered <= 0) continue;

            var issue = await _warehouseService.IssueGoodsAsync(stock.WarehouseId, order.Id, stock.CargoLineId,
                uncovered, issuedByEmployeeId: null, tripId: null, performedByUserName: $"{WarehouseSlottingService.CancelReturnPerformerPrefix}{order.Code} — trả khách tại kho");
            if (!issue.Success)
                return new OrderActionResult(false, "Không thể xuất trả hàng còn trong kho: " + issue.Message); // chưa Commit -> Rollback
            returnedAtWarehouse += uncovered;
        }

        await transaction.CommitAsync();

        // Lịch Google: KHÔNG gỡ sự kiện nữa — controller cập nhật các chuyến vừa hủy sang màu ĐỎ "Đã hủy"
        // (tài xế / khách vẫn thấy chuyến đã bị hủy thay vì sự kiện biến mất không rõ lý do).

        if (returnedAtWarehouse > 0)
        {
            var note = $"Đã xuất trả {returnedAtWarehouse:0.##} đơn vị hàng còn trong kho (không có chuyến trả hàng) — khách nhận lại tại kho.";
            cancelNote = cancelNote is null ? "Đã hủy đơn. " + note : cancelNote + "\n" + note;
        }
        return new OrderActionResult(true, cancelNote);
    }

    /// <summary>Hợp đồng của đơn (tuỳ chọn): phải tồn tại, của ĐÚNG khách hàng và đang hiệu lực
    /// hôm nay (giờ VN) — không nhận đơn theo hợp đồng chưa bắt đầu / đã hết hạn.</summary>
    private async Task<string?> ValidateContractAsync(int? contractId, int customerId)
    {
        if (contractId is null) return null;
        var contract = await _db.Contracts
            .Where(c => c.Id == contractId.Value)
            .Select(c => new { c.Code, c.CustomerId, c.StartDate, c.EndDate })
            .FirstOrDefaultAsync();
        if (contract is null) return "Hợp đồng đã chọn không tồn tại (có thể vừa bị xóa) — chọn lại hợp đồng.";
        if (contract.CustomerId != customerId)
            return $"Hợp đồng {contract.Code} không phải hợp đồng của khách hàng này.";
        return Contract.GetStatus(contract.StartDate, contract.EndDate, VnClock.Today) switch
        {
            ContractStatus.Upcoming => $"Hợp đồng {contract.Code} chưa có hiệu lực (bắt đầu {contract.StartDate:dd/MM/yyyy}).",
            ContractStatus.Expired => $"Hợp đồng {contract.Code} đã hết hạn ngày {contract.EndDate:dd/MM/yyyy}.",
            _ => null,
        };
    }

    public async Task<List<OrderContractOptionDto>> GetContractOptionsAsync(int customerId)
    {
        var today = VnClock.Today;
        var contracts = await _db.Contracts
            .Where(c => c.CustomerId == customerId)
            .OrderByDescending(c => c.StartDate)
            .Select(c => new { c.Id, c.Code, c.StartDate, c.EndDate, PriceListName = c.PriceList != null ? c.PriceList.Name : null })
            .ToListAsync();
        return contracts
            .Where(c => Contract.GetStatus(c.StartDate, c.EndDate, today) == ContractStatus.Active)
            .Select(c => new OrderContractOptionDto(c.Id, c.Code, c.StartDate, c.EndDate, c.PriceListName))
            .ToList();
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

        // Điểm lấy và điểm giao KHÔNG được trùng (cùng 1 địa điểm) — chở hàng từ A về chính A là vô lý.
        if (method == PickupMethod.CompanyPickup && pickupLocationId == dropoffLocationId)
            return "Điểm lấy hàng và điểm giao hàng đang là CÙNG 1 địa điểm — hãy chọn điểm giao khác.";

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

        // 2 địa điểm khác tên nhưng CÙNG VỊ TRÍ (lệch < ~11 m) cũng coi là trùng.
        if (method == PickupMethod.CompanyPickup && pickupLocationId.HasValue)
        {
            var p = locations.First(l => l.Id == pickupLocationId.Value);
            var d = locations.First(l => l.Id == dropoffLocationId.Value);
            if (Math.Abs(p.Lat!.Value - d.Lat!.Value) < 0.0001 && Math.Abs(p.Lng!.Value - d.Lng!.Value) < 0.0001)
                return $"Điểm lấy hàng \"{p.Name}\" và điểm giao hàng \"{d.Name}\" ở CÙNG 1 vị trí trên bản đồ — hãy chọn điểm giao khác.";
        }

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

    /// <summary>Hạn giao cam kết (nếu có) không được là ngày đã qua (theo ngày Việt Nam).</summary>
    private const string MissingCommittedDateMessage =
        "Phải nhập Hạn giao cam kết với khách — hệ thống dùng ngày này để cảnh báo chia xe trễ hạn, nhắc mốc xuất phát và tính tỉ lệ giao đúng hạn.";

    private static string? ValidateCommittedDate(DateTime? committed)
    {
        var vn = VnClock.ToVnWallClock(committed);
        if (vn is { } d && d.Date < VnClock.Today)
            return $"Hạn giao cam kết ({d:dd/MM/yyyy}) đã qua — chọn ngày từ hôm nay ({VnClock.Today:dd/MM/yyyy}) trở đi.";
        return null;
    }

    private static PickupMethod ParsePickupMethod(string value)
        => Enum.TryParse<PickupMethod>(value, out var parsed) ? parsed : PickupMethod.CompanyPickup;

    /// <summary>Kiểm tra TẤT CẢ dòng hàng đều có số liệu HỢP LỆ (dương) trước
    /// khi cho tạo/sửa đơn — trước đây KHÔNG kiểm tra gì cả, có thể tạo 1
    /// dòng hàng với số lượng/khối lượng/thể tích = 0 hoặc ÂM, gây sai lệch
    /// khi tính tổng ở bước Chia xe (Greedy) sau này. Trả về null nếu hợp lệ,
    /// hoặc thông báo lỗi nếu có dòng nào sai.</summary>
    private const decimal MaxQty = 1_000_000m;
    private const decimal MaxWeightKg = 100_000_000m;   // 100.000 tấn
    private const decimal MaxVolumeM3 = 1_000_000m;

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

            // Giới hạn trên + độ chính xác đúng theo cột DB: số quá lớn trước đây làm tràn cột
            // (lỗi 500); số quá nhỏ (VD 0,00004 m³) bị DB làm tròn về 0 -> hàng "không chiếm chỗ".
            if (c.QtyTotal > MaxQty) return $"{lineLabel}: số lượng tối đa {MaxQty:N0}.";
            if (c.QtyTotal != Math.Round(c.QtyTotal, 2)) return $"{lineLabel}: số lượng tối đa 2 chữ số thập phân.";
            if (c.PackageCount > 1_000_000) return $"{lineLabel}: số kiện tối đa 1.000.000.";
            if (c.WeightKg > MaxWeightKg) return $"{lineLabel}: khối lượng tối đa {MaxWeightKg:N0} kg.";
            if (Math.Round(c.WeightKg, 2) <= 0) return $"{lineLabel}: khối lượng quá nhỏ (tối thiểu 0,01 kg).";
            if (c.VolumeM3 > MaxVolumeM3) return $"{lineLabel}: thể tích tối đa {MaxVolumeM3:N0} m³.";
            if (Math.Round(c.VolumeM3, 4) <= 0) return $"{lineLabel}: thể tích quá nhỏ (tối thiểu 0,0001 m³).";
        }
        return null;
    }

    /// <summary>Kho trung chuyển phải có KHU đúng tính chất từng dòng hàng (Hàng nguy hiểm /
    /// Dễ vỡ / Thường) và khu đó có ít nhất 1 vị trí — trước đây Desktop cho chọn mọi kho, tới
    /// lúc nhập kho (sau khi đã chạy xe đi lấy hàng về) mới báo "kho chưa có khu phù hợp".</summary>
    private Task<string?> ValidateWarehouseZonesAsync(int warehouseId, IEnumerable<(bool IsHazmat, bool IsFragile)> lines)
        => WarehouseZoneRules.MissingReasonAsync(_db, warehouseId, lines.Select(l => WarehouseZoneRules.Required(l.IsHazmat, l.IsFragile)));

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
        order.Trips.Select(t =>
        {
            var route = TripRouteText.For(t.LegType, t.Warehouse?.Name,
                order.PickupLocation?.Name, order.PickupPoint, order.DropoffLocation?.Name, order.DropoffPoint);
            return new TripSummaryDto(
                t.Id, t.Vehicle.PlateNumber, t.DriverEmployee.FullName, t.Status.ToString(),
                t.TripLines.Sum(tl => tl.QtyPlanned), t.LegType.ToString(), route.From, route.To);
        }).ToList(),
        order.PickupLocation?.Lat, order.PickupLocation?.Lng,
        order.DropoffLocation?.Lat, order.DropoffLocation?.Lng,
        order.PickupMethod.ToString(), order.CustomerId, order.WarehouseId, order.Warehouse.Name,
        order.PickupLocationId, order.DropoffLocationId,
        order.Warehouse.Lat, order.Warehouse.Lng,
        order.ContractId, order.Contract?.Code
    );
}