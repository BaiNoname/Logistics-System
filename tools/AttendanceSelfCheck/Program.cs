using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TruckingSystem.Application.Attendance;
using TruckingSystem.Application.Common;
using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.GUI_AttendanceManagement;
using TruckingSystem.Domain.Attendance;
using TruckingSystem.Domain.Entities;
using TruckingSystem.Domain.Enums;
using TruckingSystem.Infrastructure.Persistence;

// Tự kiểm tra nghiệp vụ chấm công QR + GPS (EF InMemory: không chứng minh ràng buộc SQL, giao dịch hay phân quyền HTTP).
var checks = 0;
void Check(bool condition, string name)
{
    if (!condition) throw new Exception("FAIL: " + name);
    checks++;
}

// ---------- Domain rules ----------
var d = AttendanceRules.DistanceMeters(10.7769, 106.7009, 10.7779, 106.7009);
Check(d is > 105 and < 118, "Haversine ~111 m per 0.001° latitude");
Check(AttendanceRules.ParseToken("TSATT:abcdefghijklmnop_-12") == "abcdefghijklmnop_-12", "Parse valid token");
Check(AttendanceRules.ParseToken("abcdefghijklmnopqrst") is null, "Reject missing prefix");
Check(AttendanceRules.ParseToken("TSATT:short") is null, "Reject short token");
Check(AttendanceRules.ParseToken("TSATT:abcdefghijklmnop'; drop") is null, "Reject unsafe characters");
Check(AttendanceRules.CheckLocation(10.7770, 106.7009, 20, false, 10.7769, 106.7009, 150, 100, out _) is null, "Inside radius accepted");
Check(AttendanceRules.CheckLocation(10.7800, 106.7009, 20, false, 10.7769, 106.7009, 150, 100, out var far)!.Contains("cách") && far > 300, "Outside radius rejected");
Check(AttendanceRules.CheckLocation(10.7770, 106.7009, 250, false, 10.7769, 106.7009, 150, 100, out _)!.Contains("Sai số"), "Poor accuracy rejected");
Check(AttendanceRules.CheckLocation(10.7770, 106.7009, 10, true, 10.7769, 106.7009, 150, 100, out _)!.Contains("giả lập"), "Mock location rejected");
Check(AttendanceRules.CheckLocation(0, 0, 10, false, 10.7769, 106.7009, 150, 100, out _) is not null, "Zero coordinates rejected");
// Như JSON từ Mobile: ngày/giờ VN không kèm múi giờ (Kind=Unspecified).
var today = DateTime.SpecifyKind(VnClock.Today, DateTimeKind.Unspecified);
Check(AttendanceRules.CanRequestAdjustment(today, today) && AttendanceRules.CanRequestAdjustment(today.AddDays(-7), today), "Adjustment window includes today and 7 days back");
Check(!AttendanceRules.CanRequestAdjustment(today.AddDays(-8), today) && !AttendanceRules.CanRequestAdjustment(today.AddDays(1), today), "Adjustment window excludes older and future days");

// ---------- Service ----------
var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
    .UseInMemoryDatabase(Guid.NewGuid().ToString())
    .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning)).Options);
const double SiteLat = 10.7769, SiteLng = 106.7009;
db.Warehouses.Add(new Warehouse { Id = 1, Name = "Kho A", Lat = SiteLat, Lng = SiteLng });
db.Warehouses.Add(new Warehouse { Id = 2, Name = "Kho chưa tọa độ" });
db.Employees.Add(new Employee { Id = 1, Code = "NV01", FullName = "Nhân viên 1", HireDate = new(2020, 1, 1), UserId = "u1" });
db.Employees.Add(new Employee { Id = 2, Code = "NV02", FullName = "Đã nghỉ", HireDate = new(2020, 1, 1), UserId = "u2",
    Status = EmployeeStatus.Terminated, TerminationDate = new(2021, 1, 1) });
db.AttendanceQrCodes.Add(new AttendanceQrCode { Id = 99, WarehouseId = 1, WorkDate = today.AddDays(-1), Token = "yesterdaytoken_0123456789", CreatedBy = "seed" });
db.SaveChanges();
var service = new AttendanceService(db);

var (qrResult, qr) = await service.GetOrCreateQrAsync(1, today, "admin");
Check(qrResult.Success && qr!.QrContent.StartsWith(AttendanceRules.QrPrefix) && AttendanceRules.ParseToken(qr.QrContent) is not null, "Create today's QR");
var (_, qrAgain) = await service.GetOrCreateQrAsync(1, today, "dispatcher");
Check(qrAgain!.QrContent == qr.QrContent && db.AttendanceQrCodes.Count(q => q.WarehouseId == 1 && q.WorkDate == today) == 1, "One QR per site per day");
Check(!(await service.GetOrCreateQrAsync(1, today.AddDays(-3), "admin")).Result.Success, "Cannot create past QR");
Check(!(await service.GetOrCreateQrAsync(1, today.AddDays(31), "admin")).Result.Success, "Cannot create QR too far ahead");
var noCoords = (await service.GetOrCreateQrAsync(2, today, "admin")).Result;
Check(!noCoords.Success && noCoords.Message!.Contains("tọa độ"), "Site without coordinates cannot issue QR");
Check(!(await service.GetOrCreateQrAsync(404, today, "admin")).Result.Success, "Unknown site rejected");

Check(await service.GetMyTodayAsync("u3") is null && await service.GetMyTodayAsync("u2") is null, "Unlinked and terminated accounts blocked");
var me = await service.GetMyTodayAsync("u1");
Check(me is not null && me.Employee.Code == "NV01" && me.Today.Status == "NotCheckedIn", "Linked employee sees today status");

AttendanceScanRequest Scan(string content, double lat = SiteLat + 0.0002, double lng = SiteLng, double acc = 15, bool mock = false, bool confirm = false)
    => new(content, lat, lng, acc, mock, confirm);

Check((await service.ScanAsync("u3", Scan(qr.QrContent))).Code == "NotEmployee", "Scan requires linked employee");
Check((await service.ScanAsync("u1", Scan("hello world"))).Code == "InvalidQr", "Random QR rejected");
Check((await service.ScanAsync("u1", Scan("TSATT:unknowntoken_0123456789"))).Code == "InvalidQr", "Unknown token rejected");
Check((await service.ScanAsync("u1", Scan("TSATT:yesterdaytoken_0123456789"))).Code == "QrExpired", "Yesterday's QR rejected");
Check((await service.ScanAsync("u1", Scan(qr.QrContent, lat: SiteLat + 0.01))).Code == "LocationRejected", "Far location rejected");
Check((await service.ScanAsync("u1", Scan(qr.QrContent, acc: 500))).Code == "LocationRejected", "Inaccurate GPS rejected");
Check((await service.ScanAsync("u1", Scan(qr.QrContent, mock: true))).Code == "LocationRejected", "Mock GPS rejected");
Check((await service.ScanAsync("u1", Scan(qr.QrContent, lat: double.NaN))).Code == "LocationRejected", "NaN GPS rejected");
Check(!db.AttendanceRecords.Any(), "Rejected scans create no attendance record");
Check(db.AttendanceScans.Count(s => s.Result == AttendanceScanResult.Rejected) == 7, "Rejected scans are logged for audit");

var before = DateTime.UtcNow;
var checkIn = await service.ScanAsync("u1", Scan(qr.QrContent, confirm: true));
Check(checkIn.Success && checkIn.Code == "CheckedIn" && checkIn.Today!.Status == "CheckedIn", "First valid scan checks in");
var rec = db.AttendanceRecords.Single();
Check(rec.CheckInAt >= before && rec.CheckInAt <= DateTime.UtcNow && rec.CheckInWarehouseId == 1, "Check-in time is server time");
Check((await service.ScanAsync("u1", Scan(qr.QrContent, confirm: true))).Code == "TooSoon", "Immediate repeat scan blocked");

rec.CheckInAt = DateTime.UtcNow.AddHours(-8);
db.SaveChanges();
var scansBeforeConfirm = db.AttendanceScans.Count();
var ask = await service.ScanAsync("u1", Scan(qr.QrContent));
Check(ask.Success && ask.Code == "ConfirmCheckOut" && db.AttendanceRecords.Single().CheckOutAt is null, "Second scan asks for confirmation without writing");
Check(db.AttendanceScans.Count() == scansBeforeConfirm, "Confirmation prompt is not logged as a scan");
Check((await service.ScanAsync("u1", Scan(qr.QrContent, lat: SiteLat + 0.01, confirm: true))).Code == "LocationRejected", "Check-out re-validates location");
var checkOut = await service.ScanAsync("u1", Scan(qr.QrContent, confirm: true));
Check(checkOut.Success && checkOut.Code == "CheckedOut" && checkOut.Today!.Status == "CheckedOut", "Confirmed scan checks out");
Check((await service.ScanAsync("u1", Scan(qr.QrContent, confirm: true))).Code == "AlreadyCheckedOut", "No third scan after check-out");
Check(db.AttendanceScans.Count(s => s.Result == AttendanceScanResult.Accepted) == 2, "Exactly two accepted scans");

var history = (await service.GetMyHistoryAsync("u1", today.AddDays(-30), today))!;
Check(history.Count == 1 && history[0].Status == "CheckedOut", "History shows the completed day");
Check(await service.GetMyHistoryAsync("u2", today, today) is null, "History blocked for terminated account");
Check((await service.GetMyHistoryAsync("u1", today.AddDays(-200), today))!.Count == 0, "Oversized history range returns nothing");

// ---------- Adjustments ----------
var yesterday = today.AddDays(-1);
CreateAdjustmentRequest Adj(DateTime date, TimeSpan? inT, TimeSpan? outT, string reason = "ForgotScan", string? note = "Quên quét")
    => new(date, inT is null ? null : date + inT.Value, outT is null ? null : date + outT.Value, reason, note);

Check(!(await service.CreateAdjustmentAsync("u1", Adj(yesterday, null, null))).Success, "Adjustment needs a time");
Check(!(await service.CreateAdjustmentAsync("u1", Adj(today.AddDays(-8), TimeSpan.FromHours(8), null))).Success, "Adjustment window enforced");
Check(!(await service.CreateAdjustmentAsync("u1", Adj(yesterday, TimeSpan.FromHours(8), null, reason: "Nonsense"))).Success, "Invalid reason rejected");
Check(!(await service.CreateAdjustmentAsync("u1", Adj(yesterday, TimeSpan.FromHours(8), null, note: "  "))).Success, "Note required");
Check(!(await service.CreateAdjustmentAsync("u1", Adj(yesterday, null, TimeSpan.FromHours(17)))).Success, "Check-out only needs an existing check-in");
Check(!(await service.CreateAdjustmentAsync("u1", Adj(yesterday, TimeSpan.FromHours(17), TimeSpan.FromHours(8)))).Success, "Check-out must follow check-in");
Check(!(await service.CreateAdjustmentAsync("u1", new CreateAdjustmentRequest(yesterday, today.AddHours(8), null, "ForgotScan", "x"))).Success, "Requested time must be on the work date");
Check(!(await service.CreateAdjustmentAsync("u1", Adj(today, null, TimeSpan.FromMinutes(1)))).Success, "Today's check-out cannot precede check-in");
Check(!(await service.CreateAdjustmentAsync("u2", Adj(yesterday, TimeSpan.FromHours(8), null))).Success, "Terminated account cannot request");
var created = await service.CreateAdjustmentAsync("u1", Adj(yesterday, TimeSpan.FromHours(8), TimeSpan.FromHours(17), reason: "gpserror"));
Check(created.Success, "Valid adjustment created (reason case-insensitive): " + created.Message);
Check(!(await service.CreateAdjustmentAsync("u1", Adj(yesterday, TimeSpan.FromHours(9), null))).Success, "One pending request per day");
var stored = db.AttendanceAdjustmentRequests.Single();
Check(stored.RequestedCheckInAt == yesterday.AddHours(1) && stored.Reason == AttendanceAdjustmentReason.GpsError, "Requested VN time stored as UTC");
var pendingDay = (await service.GetMyHistoryAsync("u1", yesterday, yesterday))!;
Check(pendingDay.Count == 1 && pendingDay[0].PendingAdjustmentText is not null, "History marks pending adjustment");
var mine = (await service.GetMyAdjustmentsAsync("u1"))!;
Check(mine.Count == 1 && mine[0].RequestedCheckInAt == yesterday.AddHours(8) && mine[0].StatusText == "Chờ duyệt", "Employee sees own request in VN time");

Check(!(await service.ReviewAdjustmentAsync(stored.Id, new ReviewAdjustmentRequest(false, " "), "admin")).Success, "Reject requires reason");
Check((await service.GetAdjustmentsAsync("Pending")).Count == 1 && (await service.GetAdjustmentsAsync("Approved")).Count == 0, "Manager filters by status");
Check((await service.ReviewAdjustmentAsync(stored.Id, new ReviewAdjustmentRequest(true, null), "admin")).Success, "Approve adjustment");
Check(!(await service.ReviewAdjustmentAsync(stored.Id, new ReviewAdjustmentRequest(true, null), "admin")).Success, "Cannot review twice");
var adjusted = db.AttendanceRecords.Single(r => r.WorkDate == yesterday);
Check(adjusted.CheckInAt is null && adjusted.AdjustedCheckInAt == yesterday.AddHours(1) && adjusted.AdjustedCheckOutAt == yesterday.AddHours(10)
      && adjusted.AdjustmentRequestId == stored.Id, "Approval writes adjusted times, not scan times");
var reviewed = db.AttendanceAdjustmentRequests.Single();
Check(reviewed.Status == AttendanceAdjustmentStatus.Approved && reviewed.ReviewedBy == "admin" && reviewed.ReviewedAt is not null, "Approval records reviewer and time");
var yDay = (await service.GetMyHistoryAsync("u1", yesterday, yesterday))!.Single();
Check(yDay.IsAdjusted && yDay.Status == "CheckedOut" && yDay.CheckInAt == yesterday.AddHours(8), "Approved adjustment appears in history");

var todayFix = await service.CreateAdjustmentAsync("u1", new CreateAdjustmentRequest(today, null, null, "WrongRecord", "sai"));
Check(!todayFix.Success, "Empty today request rejected");
var rejectFlow = await service.CreateAdjustmentAsync("u1", Adj(today.AddDays(-2), TimeSpan.FromHours(8), TimeSpan.FromHours(12), reason: "OffSiteWork", note: "Giao hàng ngoài kho"));
var second = db.AttendanceAdjustmentRequests.Single(a => a.Status == AttendanceAdjustmentStatus.Pending);
Check(rejectFlow.Success && (await service.ReviewAdjustmentAsync(second.Id, new ReviewAdjustmentRequest(false, "Không có xác nhận"), "dispatcher")).Success,
    "Reject with reason");
Check(!db.AttendanceRecords.Any(r => r.WorkDate == today.AddDays(-2)), "Rejected adjustment changes no record");
Check(service.GetAdjustmentReasons().Count == Enum.GetValues<AttendanceAdjustmentReason>().Length, "All reasons exposed");

// ---------- Sites / manager views ----------
Check(!(await service.UpdateSiteAsync(1, new UpdateAttendanceSiteRequest(5, 100))).Success, "Radius lower bound");
Check(!(await service.UpdateSiteAsync(1, new UpdateAttendanceSiteRequest(150, 5000))).Success, "Accuracy upper bound");
Check((await service.UpdateSiteAsync(1, new UpdateAttendanceSiteRequest(300, 50))).Success && db.Warehouses.Find(1)!.AttendanceRadiusMeters == 300, "Update site settings");
var sites = await service.GetSitesAsync();
Check(sites.Count == 2 && sites.Single(s => s.WarehouseId == 2).IsReady == false, "Sites report readiness");
Check((await service.GetRecordsAsync(today.AddDays(-7), today)).Count == 2, "Manager sees records");
Check((await service.GetRecordsAsync(today, today.AddDays(-1))).Count == 0, "Reversed range returns nothing");
var scans = await service.GetScansAsync(today);
Check(scans.Count == db.AttendanceScans.Count() && scans.Any(s => s.Result == "Từ chối" && s.RejectReason != null), "Scan log lists rejects with reason");

// ---------- Desktop: QR bị API từ chối (400) không còn thành mã rỗng ----------
// ApiConnection đọc body 400 {success,message} vào kiểu đích — trước đây QrContent null làm QRCoder NullReferenceException.
var rejected = Newtonsoft.Json.JsonConvert.DeserializeObject<TruckingSystem.Desktop.DTO_AttendanceManagement.AttendanceQrDto>(
    "{\"success\":false,\"message\":\"Kho X chưa có tọa độ\"}")!;
Check(rejected.QrContent is null && rejected.Message == "Kho X chưa có tọa độ", "Rejected QR body keeps the API message");
var okQr = Newtonsoft.Json.JsonConvert.DeserializeObject<TruckingSystem.Desktop.DTO_AttendanceManagement.AttendanceQrDto>(
    Newtonsoft.Json.JsonConvert.SerializeObject(new { warehouseId = 1, warehouseName = "Kho A", workDate = today, qrContent = qr.QrContent }))!;
Check(okQr.QrContent == qr.QrContent && okQr.WarehouseName == "Kho A", "Successful QR body still parses");

// ---------- Migration matches model (offline, no DB connection) ----------
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
using (var pg = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseNpgsql("Host=localhost;Database=offline_check").Options))
{
    Check(!pg.Database.HasPendingModelChanges(), "Migration matches Npgsql model");
    var sql = pg.GetService<IMigrator>().GenerateScript("20261102065005_AddDriverKpiAndAutomaticStandardDays", "20261102111437_AddMobileQrAttendance");
    Check(sql.Contains("AttendanceRecords") && sql.Contains("DEFAULT 150") && !sql.Contains("DROP TABLE") && !sql.Contains("DROP COLUMN"),
        "Migration is additive with defaults for existing warehouses");
    Check(sql.Contains("IX_AttendanceAdjustmentRequests_OnePending") && sql.Contains("WHERE \"Status\" = 'Pending'"), "Partial unique index for pending requests");
    File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "attendance-migration.sql"), sql);
}

// ---------- Desktop control renders (STA, no window shown) ----------
Exception? uiError = null;
var thread = new Thread(() =>
{
    try
    {
        var api = new ApiServices("http://localhost:1");
        api.Connection.SetToken("x", new[] { "Admin" });
        using var control = new AttendanceControl(api) { Size = new Size(1200, 760) };
        using var bmp = new Bitmap(1200, 760);
        control.CreateControl();
        control.DrawToBitmap(bmp, new Rectangle(0, 0, 1200, 760));
        bmp.Save(Path.Combine(AppContext.BaseDirectory, "attendance-preview.png"));
    }
    catch (Exception ex) { uiError = ex; }
});
thread.SetApartmentState(ApartmentState.STA);
thread.Start();
thread.Join();
Check(uiError is null, "Desktop attendance control builds: " + uiError?.Message);

Console.WriteLine($"PASS: {checks} attendance checks. InMemory does not verify SQL constraints, concurrency, or HTTP authorization.");
