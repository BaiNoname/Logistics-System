using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Microsoft.Extensions.Configuration;
using TruckingSystem.Application.Calendar;

namespace TruckingSystem.Infrastructure.ExternalServices;

/// <summary>
/// Gọi Google Calendar API v3 thật qua Service Account (KHÔNG qua OAuth đăng
/// nhập trình duyệt) — phù hợp cho backend tự động tạo sự kiện mà không cần
/// tương tác người dùng mỗi lần. ĐIỀU KIỆN BẮT BUỘC: Calendar đích (của từng
/// tài xế/khách hàng) phải đã được người sở hữu CHIA SẺ (Share) quyền
/// "Make changes to events" cho đúng email Service Account này từ trước —
/// làm thủ công 1 lần trên giao diện Google Calendar, hệ thống không tự làm
/// được bước này. Cần cấu hình "GoogleCalendar:ServiceAccountKeyPath" trỏ
/// đúng file JSON key đã tải về từ Google Cloud Console.
/// </summary>
public class GoogleCalendarService : IGoogleCalendarService
{
    private readonly CalendarService? _service;
    private readonly string? _configError;

    public GoogleCalendarService(IConfiguration config)
    {
        var keyPath = config["GoogleCalendar:ServiceAccountKeyPath"];
        var appName = config["GoogleCalendar:ApplicationName"] ?? "TruckingSystem";

        if (string.IsNullOrWhiteSpace(keyPath))
        {
            // KHÔNG throw exception ở constructor — để hệ thống vẫn chạy bình
            // thường nếu chưa cấu hình Google Calendar (tính năng tùy chọn,
            // không phải lõi hệ thống). Lỗi sẽ báo rõ khi THỰC SỰ gọi tới.
            _configError = "Chưa cấu hình \"GoogleCalendar:ServiceAccountKeyPath\" trong appsettings.json.";
            return;
        }

        if (!File.Exists(keyPath))
        {
            _configError = $"Không tìm thấy file JSON key tại đường dẫn đã cấu hình: {keyPath}";
            return;
        }

        try
        {
            GoogleCredential credential;
            using (var stream = new FileStream(keyPath, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream).CreateScoped(CalendarService.Scope.Calendar);
            }

            _service = new CalendarService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = appName,
            });
        }
        catch (Exception ex)
        {
            _configError = "Lỗi đọc file Service Account key: " + ex.Message;
        }
    }

    public async Task<CalendarEventResult> CreateOrUpdateEventAsync(CalendarEventRequest request)
    {
        if (_service is null)
            return new CalendarEventResult(false, null, _configError ?? "Google Calendar chưa sẵn sàng.");

        var calendarEvent = new Event
        {
            Summary = request.Title,
            Description = request.Description,
            Location = request.Location,
            // Dùng DateTimeRaw (chuỗi RFC3339) thay vì DateTimeDateTimeOffset —
            // thuộc tính này CÓ Ở MỌI PHIÊN BẢN của Google.Apis.Calendar.v3
            // (DateTimeDateTimeOffset chỉ có ở bản mới, gây lỗi build nếu
            // NuGet resolve về bản khác) — an toàn tuyệt đối, không phụ thuộc
            // đúng version nào đang cài. Ép Kind=Utc để định dạng "o" LUÔN có
            // hậu tố "Z" (VD: "2026-09-25T10:30:00.0000000Z") — đúng chuẩn
            // RFC3339 Google yêu cầu; nếu để Kind=Unspecified, "o" sẽ THIẾU
            // hậu tố timezone, Google có thể từ chối hoặc hiểu sai giờ.
            Start = new EventDateTime { DateTimeRaw = DateTime.SpecifyKind(request.StartUtc, DateTimeKind.Utc).ToString("o"), TimeZone = "UTC" },
            End = new EventDateTime { DateTimeRaw = DateTime.SpecifyKind(request.EndUtc, DateTimeKind.Utc).ToString("o"), TimeZone = "UTC" },
        };

        try
        {
            if (!string.IsNullOrEmpty(request.ExistingEventId))
            {
                // CẬP NHẬT sự kiện cũ — tránh tạo trùng mỗi lần bấm đồng bộ lại
                var updateRequest = _service.Events.Update(calendarEvent, request.CalendarId, request.ExistingEventId);
                var updated = await updateRequest.ExecuteAsync();
                return new CalendarEventResult(true, updated.Id, null);
            }
            else
            {
                var insertRequest = _service.Events.Insert(calendarEvent, request.CalendarId);
                var inserted = await insertRequest.ExecuteAsync();
                return new CalendarEventResult(true, inserted.Id, null);
            }
        }
        catch (Google.GoogleApiException ex)
        {
            // Lỗi PHỔ BIẾN nhất thực tế: Calendar CHƯA được chia sẻ quyền cho
            // Service Account -> Google trả về 403/404 -> báo rõ nguyên nhân
            // thay vì để lộ nguyên văn lỗi kỹ thuật khó hiểu.
            var friendlyMessage = ex.HttpStatusCode is System.Net.HttpStatusCode.Forbidden or System.Net.HttpStatusCode.NotFound
                ? $"Không ghi được vào Calendar \"{request.CalendarId}\" — kiểm tra lại đã CHIA SẺ (Share) quyền \"Make changes to events\" cho đúng email Service Account chưa."
                : "Lỗi Google Calendar API: " + ex.Message;
            return new CalendarEventResult(false, null, friendlyMessage);
        }
        catch (Exception ex)
        {
            return new CalendarEventResult(false, null, "Lỗi không xác định: " + ex.Message);
        }
    }

    public async Task<bool> DeleteEventAsync(string calendarId, string eventId)
    {
        if (_service is null) return false;
        try
        {
            await _service.Events.Delete(calendarId, eventId).ExecuteAsync();
            return true;
        }
        catch (Google.GoogleApiException ex) when (ex.HttpStatusCode is System.Net.HttpStatusCode.NotFound or System.Net.HttpStatusCode.Gone)
        {
            return true; // sự kiện đã không còn trên Google (bị xóa tay) = coi như gỡ xong
        }
        catch
        {
            return false; // Không chặn nghiệp vụ nếu xóa lỗi — Calendar không phải nguồn dữ liệu chính
        }
    }
}