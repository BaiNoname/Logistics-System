using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using TruckingSystem.Application.Common;
using TruckingSystem.Application.Common.Interfaces;
using TruckingSystem.Domain.Attendance;
using TruckingSystem.Domain.Entities;
using TruckingSystem.Domain.Enums;

namespace TruckingSystem.Application.Attendance;

public class AttendanceService : IAttendanceService
{
    private const int MaxHistoryDays = 93;
    private const int MaxQrDaysAhead = 30;
    private const int MaxNoteLength = 500;
    private readonly IApplicationDbContext _db;

    public AttendanceService(IApplicationDbContext db) => _db = db;

    // ================= Mobile =================

    public async Task<MobileTodayDto?> GetMyTodayAsync(string userId)
    {
        var employee = await FindEmployeeAsync(userId);
        if (employee is null) return null;
        var today = VnClock.Today;
        var record = await RecordQuery().FirstOrDefaultAsync(r => r.EmployeeId == employee.Id && r.WorkDate == today);
        var pending = await PendingDatesAsync(employee.Id, today, today);
        return new MobileTodayDto(ToProfile(employee), DateTime.UtcNow.AddHours(7), ToDay(today, record, pending.Contains(today), today));
    }

    public async Task<AttendanceScanResponse> ScanAsync(string userId, AttendanceScanRequest request)
    {
        var employee = await FindEmployeeAsync(userId);
        if (employee is null)
            return Fail("NotEmployee", "Tài khoản chưa được liên kết với hồ sơ nhân viên đang làm việc.");

        var now = DateTime.UtcNow;
        var today = VnClock.Today;
        var scan = new AttendanceScan
        {
            EmployeeId = employee.Id, WorkDate = today, ScannedAt = now,
            Lat = Finite(request.Latitude), Lng = Finite(request.Longitude), AccuracyMeters = Finite(request.AccuracyMeters),
            IsMockLocation = request.IsMockLocation, Result = AttendanceScanResult.Rejected,
        };

        var token = AttendanceRules.ParseToken(request.QrContent);
        var qr = token is null ? null
            : await _db.AttendanceQrCodes.Include(q => q.Warehouse).FirstOrDefaultAsync(q => q.Token == token);
        if (qr is null)
            return await RejectAsync(scan, "InvalidQr", "Mã QR không phải mã chấm công hợp lệ.");
        scan.QrCodeId = qr.Id;
        scan.WarehouseId = qr.WarehouseId;
        if (qr.WorkDate.Date != today)
            return await RejectAsync(scan, "QrExpired", $"Mã QR này dùng cho ngày {qr.WorkDate:dd/MM/yyyy}, không phải hôm nay.", qr.Warehouse.Name);

        var site = qr.Warehouse;
        if (site.Lat is null || site.Lng is null)
            return await RejectAsync(scan, "SiteNotReady", $"Kho {site.Name} chưa có tọa độ — báo quản lý cập nhật.", site.Name);

        var locationError = AttendanceRules.CheckLocation(request.Latitude, request.Longitude, request.AccuracyMeters,
            request.IsMockLocation, site.Lat.Value, site.Lng.Value, site.AttendanceRadiusMeters, site.AttendanceMaxAccuracyMeters,
            out var distance);
        scan.DistanceMeters = Finite(distance);
        if (locationError is not null)
            return await RejectAsync(scan, "LocationRejected", locationError, site.Name);

        var record = await _db.AttendanceRecords.FirstOrDefaultAsync(r => r.EmployeeId == employee.Id && r.WorkDate == today);
        var checkIn = record?.AdjustedCheckInAt ?? record?.CheckInAt;
        var checkOut = record?.AdjustedCheckOutAt ?? record?.CheckOutAt;

        string code, message;
        if (checkIn is null)
        {
            record ??= _db.AttendanceRecords.Add(new AttendanceRecord { EmployeeId = employee.Id, WorkDate = today }).Entity;
            record.CheckInAt = now;
            record.CheckInWarehouseId = site.Id;
            scan.Action = AttendanceScanAction.CheckIn;
            (code, message) = ("CheckedIn", $"Check-in thành công lúc {now.AddHours(7):HH:mm} tại {site.Name}.");
        }
        else if (checkOut is not null)
            return await RejectAsync(scan, "AlreadyCheckedOut", $"Bạn đã Check-out hôm nay lúc {checkOut.Value.AddHours(7):HH:mm}. Nếu cần sửa, hãy gửi yêu cầu điều chỉnh.", site.Name);
        else if (now - checkIn.Value < AttendanceRules.MinCheckOutGap)
            return await RejectAsync(scan, "TooSoon", $"Bạn vừa Check-in lúc {checkIn.Value.AddHours(7):HH:mm}. Chờ ít nhất 1 phút trước khi Check-out.", site.Name);
        else if (!request.ConfirmCheckOut)
            // Chưa ghi gì: chờ người dùng xác nhận, lần gửi sau backend kiểm tra lại toàn bộ.
            return new AttendanceScanResponse(true, "ConfirmCheckOut",
                $"Bạn đã Check-in lúc {checkIn.Value.AddHours(7):HH:mm}. Xác nhận Check-out tại {site.Name}?", site.Name, null);
        else
        {
            record!.CheckOutAt = now;
            record.CheckOutWarehouseId = site.Id;
            scan.Action = AttendanceScanAction.CheckOut;
            (code, message) = ("CheckedOut", $"Check-out thành công lúc {now.AddHours(7):HH:mm} tại {site.Name}.");
        }

        scan.Result = AttendanceScanResult.Accepted;
        _db.AttendanceScans.Add(scan);
        try { await _db.SaveChangesAsync(); }
        catch (DbUpdateException)
        {
            // 2 lượt quét gần như đồng thời: unique (EmployeeId, WorkDate) chặn ghi trùng.
            return Fail("Conflict", "Lượt quét vừa được ghi nhận từ yêu cầu khác. Tải lại trang chủ để xem trạng thái.");
        }

        var pending = await PendingDatesAsync(employee.Id, today, today);
        var saved = await RecordQuery().FirstAsync(r => r.EmployeeId == employee.Id && r.WorkDate == today);
        return new AttendanceScanResponse(true, code, message, site.Name, ToDay(today, saved, pending.Contains(today), today));
    }

    public async Task<List<AttendanceDayDto>?> GetMyHistoryAsync(string userId, DateTime from, DateTime to)
    {
        var employee = await FindEmployeeAsync(userId);
        if (employee is null) return null;
        from = from.Date; to = to.Date;
        if (to < from || (to - from).TotalDays > MaxHistoryDays) return new List<AttendanceDayDto>();
        var today = VnClock.Today;
        var records = await RecordQuery()
            .Where(r => r.EmployeeId == employee.Id && r.WorkDate >= from && r.WorkDate <= to)
            .ToListAsync();
        var pending = await PendingDatesAsync(employee.Id, from, to);
        var dates = records.Select(r => r.WorkDate.Date).Union(pending).OrderByDescending(d => d);
        return dates.Select(d => ToDay(d, records.FirstOrDefault(r => r.WorkDate.Date == d), pending.Contains(d), today)).ToList();
    }

    public async Task<List<AdjustmentRequestDto>?> GetMyAdjustmentsAsync(string userId)
    {
        var employee = await FindEmployeeAsync(userId);
        if (employee is null) return null;
        return await LoadAdjustmentsAsync(_db.AttendanceAdjustmentRequests.Where(a => a.EmployeeId == employee.Id), 100);
    }

    public async Task<AttendanceResult> CreateAdjustmentAsync(string userId, CreateAdjustmentRequest request)
    {
        var employee = await FindEmployeeAsync(userId);
        if (employee is null) return new(false, "Tài khoản chưa được liên kết với hồ sơ nhân viên đang làm việc.");

        var today = VnClock.Today;
        var workDate = request.WorkDate.Date;
        if (!AttendanceRules.CanRequestAdjustment(workDate, today))
            return new(false, $"Chỉ gửi điều chỉnh cho ngày từ {today.AddDays(-AttendanceRules.AdjustmentWindowDays):dd/MM/yyyy} đến hôm nay.");
        if (!Enum.TryParse<AttendanceAdjustmentReason>(request.Reason, true, out var reason) || !Enum.IsDefined(reason))
            return new(false, "Chọn lý do điều chỉnh.");
        var note = (request.Note ?? "").Trim();
        if (note.Length == 0) return new(false, "Nhập nội dung giải thích cho yêu cầu.");
        if (note.Length > MaxNoteLength) return new(false, $"Nội dung tối đa {MaxNoteLength} ký tự.");
        if (request.RequestedCheckInAt is null && request.RequestedCheckOutAt is null)
            return new(false, "Nhập giờ Check-in và/hoặc giờ Check-out đề xuất.");

        var nowVn = DateTime.UtcNow.AddHours(7);
        foreach (var t in new[] { request.RequestedCheckInAt, request.RequestedCheckOutAt })
        {
            if (t is null) continue;
            var vn = VnClock.ToVnWallClock(t)!.Value;
            if (vn.Date != workDate) return new(false, "Giờ đề xuất phải nằm trong ngày cần điều chỉnh.");
            if (vn > nowVn) return new(false, "Giờ đề xuất không được ở tương lai.");
        }

        var record = await _db.AttendanceRecords.FirstOrDefaultAsync(r => r.EmployeeId == employee.Id && r.WorkDate == workDate);
        var error = ValidateOrder(record, ToUtc(request.RequestedCheckInAt), ToUtc(request.RequestedCheckOutAt));
        if (error is not null) return new(false, error);

        if (await _db.AttendanceAdjustmentRequests.AnyAsync(a => a.EmployeeId == employee.Id && a.WorkDate == workDate
                && a.Status == AttendanceAdjustmentStatus.Pending))
            return new(false, "Ngày này đã có yêu cầu đang chờ duyệt.");

        _db.AttendanceAdjustmentRequests.Add(new AttendanceAdjustmentRequest
        {
            EmployeeId = employee.Id, WorkDate = workDate, Reason = reason, Note = note,
            RequestedCheckInAt = ToUtc(request.RequestedCheckInAt), RequestedCheckOutAt = ToUtc(request.RequestedCheckOutAt),
        });
        try { await _db.SaveChangesAsync(); }
        catch (DbUpdateException) { return new(false, "Ngày này đã có yêu cầu đang chờ duyệt."); }
        return new(true, "Đã gửi yêu cầu điều chỉnh. Kết quả sẽ hiển thị khi quản lý xử lý.");
    }

    public List<AdjustmentReasonDto> GetAdjustmentReasons()
        => Enum.GetValues<AttendanceAdjustmentReason>().Select(r => new AdjustmentReasonDto(r.ToString(), ReasonText(r))).ToList();

    // ================= Desktop =================

    public async Task<List<AttendanceSiteDto>> GetSitesAsync()
        => (await _db.Warehouses.AsNoTracking().OrderBy(w => w.Name).ToListAsync())
            .Select(w => new AttendanceSiteDto(w.Id, w.Name, w.Address, w.Lat, w.Lng,
                w.AttendanceRadiusMeters, w.AttendanceMaxAccuracyMeters, w.Lat is not null && w.Lng is not null))
            .ToList();

    public async Task<AttendanceResult> UpdateSiteAsync(int warehouseId, UpdateAttendanceSiteRequest request)
    {
        if (request.RadiusMeters is < AttendanceRules.MinRadiusMeters or > AttendanceRules.MaxRadiusMeters)
            return new(false, $"Bán kính phải từ {AttendanceRules.MinRadiusMeters} đến {AttendanceRules.MaxRadiusMeters} m.");
        if (request.MaxAccuracyMeters is < AttendanceRules.MinAccuracyMeters or > AttendanceRules.MaxAccuracyMeters)
            return new(false, $"Sai số GPS tối đa phải từ {AttendanceRules.MinAccuracyMeters} đến {AttendanceRules.MaxAccuracyMeters} m.");
        var warehouse = await _db.Warehouses.FindAsync(warehouseId);
        if (warehouse is null) return new(false, "Không tìm thấy kho.");
        warehouse.AttendanceRadiusMeters = request.RadiusMeters;
        warehouse.AttendanceMaxAccuracyMeters = request.MaxAccuracyMeters;
        warehouse.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return new(true, "Đã lưu cấu hình chấm công của kho.");
    }

    public async Task<(AttendanceResult Result, AttendanceQrDto? Qr)> GetOrCreateQrAsync(int warehouseId, DateTime workDate, string createdBy)
    {
        workDate = workDate.Date;
        var warehouse = await _db.Warehouses.FindAsync(warehouseId);
        if (warehouse is null) return (new(false, "Không tìm thấy kho."), null);

        var qr = await _db.AttendanceQrCodes.FirstOrDefaultAsync(q => q.WarehouseId == warehouseId && q.WorkDate == workDate);
        if (qr is null)
        {
            var today = VnClock.Today;
            if (workDate < today || workDate > today.AddDays(MaxQrDaysAhead))
                return (new(false, $"Chỉ tạo mã QR cho ngày từ hôm nay đến {today.AddDays(MaxQrDaysAhead):dd/MM/yyyy}."), null);
            if (warehouse.Lat is null || warehouse.Lng is null)
                return (new(false, $"Kho {warehouse.Name} chưa có tọa độ — cập nhật địa chỉ kho trước khi phát QR."), null);
            qr = _db.AttendanceQrCodes.Add(new AttendanceQrCode
            {
                WarehouseId = warehouseId, WorkDate = workDate, CreatedBy = createdBy,
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(24)).Replace('+', '-').Replace('/', '_').TrimEnd('='),
            }).Entity;
            try { await _db.SaveChangesAsync(); }
            catch (DbUpdateException)
            {
                return (new(false, "Mã QR vừa được tạo từ máy khác — mở lại để xem."), null);
            }
        }
        return (new(true, null), new AttendanceQrDto(warehouseId, warehouse.Name, workDate, AttendanceRules.QrPrefix + qr.Token));
    }

    public async Task<List<AttendanceRecordDto>> GetRecordsAsync(DateTime from, DateTime to)
    {
        from = from.Date; to = to.Date;
        if (to < from || (to - from).TotalDays > MaxHistoryDays) return new List<AttendanceRecordDto>();
        var today = VnClock.Today;
        var records = await RecordQuery().Include(r => r.Employee)
            .Where(r => r.WorkDate >= from && r.WorkDate <= to)
            .OrderByDescending(r => r.WorkDate).ThenBy(r => r.Employee.Code)
            .ToListAsync();
        return records.Select(r => new AttendanceRecordDto(r.Id, r.EmployeeId, r.Employee.Code, r.Employee.FullName, r.WorkDate,
                ToVn(r.CheckInAt), r.CheckInWarehouse?.Name, ToVn(r.CheckOutAt), r.CheckOutWarehouse?.Name,
                ToVn(r.AdjustedCheckInAt), ToVn(r.AdjustedCheckOutAt), ToDay(r.WorkDate, r, false, today).StatusText))
            .ToList();
    }

    public async Task<List<AttendanceScanLogDto>> GetScansAsync(DateTime workDate)
    {
        workDate = workDate.Date;
        var scans = await _db.AttendanceScans.AsNoTracking().Include(s => s.Employee).Include(s => s.Warehouse)
            .Where(s => s.WorkDate == workDate).OrderByDescending(s => s.ScannedAt).Take(1000).ToListAsync();
        return scans.Select(s => new AttendanceScanLogDto(s.Id, s.Employee.Code, s.Employee.FullName, s.ScannedAt.AddHours(7),
                s.Warehouse?.Name, ActionText(s.Action), s.Result == AttendanceScanResult.Accepted ? "Hợp lệ" : "Từ chối",
                s.DistanceMeters is null ? null : Math.Round(s.DistanceMeters.Value), s.AccuracyMeters, s.RejectReason))
            .ToList();
    }

    public Task<List<AdjustmentRequestDto>> GetAdjustmentsAsync(string? status)
    {
        var query = _db.AttendanceAdjustmentRequests.AsQueryable();
        if (Enum.TryParse<AttendanceAdjustmentStatus>(status, true, out var s) && Enum.IsDefined(s))
            query = query.Where(a => a.Status == s);
        return LoadAdjustmentsAsync(query, 500);
    }

    public async Task<AttendanceResult> ReviewAdjustmentAsync(int id, ReviewAdjustmentRequest request, string reviewer)
    {
        var adjustment = await _db.AttendanceAdjustmentRequests.FirstOrDefaultAsync(a => a.Id == id);
        if (adjustment is null) return new(false, "Không tìm thấy yêu cầu điều chỉnh.");
        if (adjustment.Status != AttendanceAdjustmentStatus.Pending) return new(false, "Yêu cầu này đã được xử lý.");
        var note = (request.Note ?? "").Trim();
        if (note.Length > MaxNoteLength) return new(false, $"Ghi chú tối đa {MaxNoteLength} ký tự.");
        if (!request.Approve && note.Length == 0) return new(false, "Nhập lý do từ chối để nhân viên biết.");

        if (request.Approve)
        {
            var record = await _db.AttendanceRecords.FirstOrDefaultAsync(r => r.EmployeeId == adjustment.EmployeeId && r.WorkDate == adjustment.WorkDate);
            var error = ValidateOrder(record, adjustment.RequestedCheckInAt, adjustment.RequestedCheckOutAt);
            if (error is not null) return new(false, "Không thể duyệt: " + error + " Hãy từ chối và yêu cầu gửi lại.");
            record ??= _db.AttendanceRecords.Add(new AttendanceRecord { EmployeeId = adjustment.EmployeeId, WorkDate = adjustment.WorkDate }).Entity;
            if (adjustment.RequestedCheckInAt is not null) record.AdjustedCheckInAt = adjustment.RequestedCheckInAt;
            if (adjustment.RequestedCheckOutAt is not null) record.AdjustedCheckOutAt = adjustment.RequestedCheckOutAt;
            record.AdjustmentRequestId = adjustment.Id;
            record.UpdatedAt = DateTime.UtcNow;
        }

        adjustment.Status = request.Approve ? AttendanceAdjustmentStatus.Approved : AttendanceAdjustmentStatus.Rejected;
        adjustment.ReviewedBy = reviewer;
        adjustment.ReviewedAt = DateTime.UtcNow;
        adjustment.ReviewNote = note.Length == 0 ? null : note;
        adjustment.UpdatedAt = DateTime.UtcNow;
        try { await _db.SaveChangesAsync(); }
        catch (DbUpdateException) { return new(false, "Dữ liệu công vừa thay đổi — tải lại rồi thử lại."); }
        return new(true, request.Approve ? "Đã duyệt yêu cầu điều chỉnh." : "Đã từ chối yêu cầu điều chỉnh.");
    }

    // ================= Helpers =================

    /// <summary>Nhân viên đang làm việc gắn với tài khoản đăng nhập.</summary>
    private async Task<Employee?> FindEmployeeAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId)) return null;
        var today = VnClock.Today;
        return await _db.Employees.AsNoTracking().Include(e => e.Department).Include(e => e.Position).Include(e => e.DriverProfile)
            .FirstOrDefaultAsync(e => e.UserId == userId && e.Status != EmployeeStatus.Terminated
                                      && (e.TerminationDate == null || e.TerminationDate >= today));
    }

    private IQueryable<AttendanceRecord> RecordQuery()
        => _db.AttendanceRecords.AsNoTracking().Include(r => r.CheckInWarehouse).Include(r => r.CheckOutWarehouse);

    private async Task<HashSet<DateTime>> PendingDatesAsync(int employeeId, DateTime from, DateTime to)
        => (await _db.AttendanceAdjustmentRequests
                .Where(a => a.EmployeeId == employeeId && a.Status == AttendanceAdjustmentStatus.Pending && a.WorkDate >= from && a.WorkDate <= to)
                .Select(a => a.WorkDate).ToListAsync())
            .Select(d => d.Date).ToHashSet();

    private async Task<List<AdjustmentRequestDto>> LoadAdjustmentsAsync(IQueryable<AttendanceAdjustmentRequest> query, int take)
    {
        var items = await query.AsNoTracking().Include(a => a.Employee)
            .OrderByDescending(a => a.CreatedAt).Take(take).ToListAsync();
        var employeeIds = items.Select(a => a.EmployeeId).Distinct().ToList();
        var dates = items.Select(a => a.WorkDate).Distinct().ToList();
        var records = await _db.AttendanceRecords.AsNoTracking()
            .Where(r => employeeIds.Contains(r.EmployeeId) && dates.Contains(r.WorkDate)).ToListAsync();
        return items.Select(a =>
        {
            var r = records.FirstOrDefault(x => x.EmployeeId == a.EmployeeId && x.WorkDate == a.WorkDate);
            return new AdjustmentRequestDto(a.Id, a.EmployeeId, a.Employee.Code, a.Employee.FullName, a.WorkDate,
                ToVn(r?.EffectiveCheckInAt), ToVn(r?.EffectiveCheckOutAt), ToVn(a.RequestedCheckInAt), ToVn(a.RequestedCheckOutAt),
                a.Reason.ToString(), ReasonText(a.Reason), a.Note, a.Status.ToString(), StatusText(a.Status),
                a.CreatedAt.AddHours(7), a.ReviewedBy, ToVn(a.ReviewedAt), a.ReviewNote);
        }).ToList();
    }

    /// <summary>Giờ vào phải trước giờ ra (kết hợp giờ hiện có với giờ đề xuất).</summary>
    private static string? ValidateOrder(AttendanceRecord? record, DateTime? newIn, DateTime? newOut)
    {
        var checkIn = newIn ?? record?.EffectiveCheckInAt;
        var checkOut = newOut ?? record?.EffectiveCheckOutAt;
        if (checkOut is not null && checkIn is null)
            return "Ngày này chưa có giờ Check-in — đề xuất kèm giờ Check-in.";
        if (checkIn is not null && checkOut is not null && checkOut <= checkIn)
            return "Giờ Check-out phải sau giờ Check-in.";
        return null;
    }

    private static AttendanceDayDto ToDay(DateTime date, AttendanceRecord? r, bool pending, DateTime today)
    {
        var checkIn = r?.EffectiveCheckInAt;
        var checkOut = r?.EffectiveCheckOutAt;
        var (status, text) = checkIn is null
            ? ("NotCheckedIn", date.Date == today ? "Chưa Check-in" : "Không có lượt quét")
            : checkOut is null
                ? date.Date == today ? ("CheckedIn", "Đang làm việc") : ("MissingCheckOut", "Thiếu Check-out")
                : ("CheckedOut", "Đủ lượt");
        return new AttendanceDayDto(date.Date, ToVn(checkIn), r?.CheckInWarehouse?.Name, ToVn(checkOut), r?.CheckOutWarehouse?.Name,
            r?.AdjustedCheckInAt is not null || r?.AdjustedCheckOutAt is not null, status, text,
            pending ? "Đang chờ duyệt điều chỉnh" : null);
    }

    private static MobileProfileDto ToProfile(Employee e)
        => new(e.Id, e.Code, e.FullName, e.Department?.Name, e.Position?.Name, e.DriverProfile is not null);

    private async Task<AttendanceScanResponse> RejectAsync(AttendanceScan scan, string code, string message, string? siteName = null)
    {
        scan.RejectReason = message;
        _db.AttendanceScans.Add(scan);
        await _db.SaveChangesAsync();
        return new AttendanceScanResponse(false, code, message, siteName, null);
    }

    private static AttendanceScanResponse Fail(string code, string message) => new(false, code, message, null, null);

    private static DateTime? ToVn(DateTime? utc) => utc?.AddHours(7);
    private static DateTime? ToUtc(DateTime? value) => value is null ? null : VnClock.VnToUtc(VnClock.ToVnWallClock(value)!.Value);
    private static double? Finite(double value) => double.IsFinite(value) ? value : null;

    private static string ActionText(AttendanceScanAction a) => a switch
    {
        AttendanceScanAction.CheckIn => "Check-in",
        AttendanceScanAction.CheckOut => "Check-out",
        _ => "—",
    };

    private static string ReasonText(AttendanceAdjustmentReason r) => r switch
    {
        AttendanceAdjustmentReason.ForgotScan => "Quên quét QR",
        AttendanceAdjustmentReason.GpsError => "Lỗi GPS / vị trí",
        AttendanceAdjustmentReason.NetworkError => "Mất mạng",
        AttendanceAdjustmentReason.WrongRecord => "Ghi nhận sai",
        AttendanceAdjustmentReason.OffSiteWork => "Làm việc ngoài cơ sở",
        _ => "Lý do khác",
    };

    private static string StatusText(AttendanceAdjustmentStatus s) => s switch
    {
        AttendanceAdjustmentStatus.Approved => "Đã duyệt",
        AttendanceAdjustmentStatus.Rejected => "Từ chối",
        _ => "Chờ duyệt",
    };
}
