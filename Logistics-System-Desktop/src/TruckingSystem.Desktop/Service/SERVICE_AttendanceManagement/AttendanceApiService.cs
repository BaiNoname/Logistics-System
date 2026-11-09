using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_AttendanceManagement;

namespace TruckingSystem.Desktop.SERVICE_AttendanceManagement;

public class AttendanceApiService(ApiConnection connection)
{
    private const string Root = "/api/attendance";

    public async Task<List<AttendanceSiteDto>> SitesAsync()
        => await connection.GetAsync<List<AttendanceSiteDto>>($"{Root}/sites") ?? new();
    public async Task<AttendanceResult> UpdateSiteAsync(int warehouseId, UpdateAttendanceSiteRequest request)
        => await connection.PutAsync<AttendanceResult>($"{Root}/sites/{warehouseId}", request) ?? new(false, "Không nhận được kết quả.");
    /// <summary>API từ chối (400 {success:false,message}) thì ném lỗi kèm đúng thông báo, không trả về mã rỗng
    /// (ApiConnection đọc body 400 vào kiểu đích nên QrContent sẽ null).</summary>
    public async Task<AttendanceQrDto> QrAsync(int warehouseId, DateTime date)
    {
        var qr = await connection.GetAsync<AttendanceQrDto>($"{Root}/sites/{warehouseId}/qr?date={date:yyyy-MM-dd}");
        if (qr is null || string.IsNullOrWhiteSpace(qr.QrContent))
            throw new InvalidOperationException(qr?.Message ?? "Không nhận được mã QR.");
        return qr;
    }
    public async Task<List<AttendanceRecordDto>> RecordsAsync(DateTime from, DateTime to)
        => await connection.GetAsync<List<AttendanceRecordDto>>($"{Root}/records?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}") ?? new();
    public async Task<List<AttendanceScanLogDto>> ScansAsync(DateTime date)
        => await connection.GetAsync<List<AttendanceScanLogDto>>($"{Root}/scans?date={date:yyyy-MM-dd}") ?? new();
    public async Task<List<AdjustmentRequestDto>> AdjustmentsAsync(string? status)
        => await connection.GetAsync<List<AdjustmentRequestDto>>($"{Root}/adjustments" + (string.IsNullOrEmpty(status) ? "" : $"?status={status}")) ?? new();
    public async Task<AttendanceResult> ReviewAsync(int id, ReviewAdjustmentRequest request)
        => await connection.PostAsync<AttendanceResult>($"{Root}/adjustments/{id}/review", request) ?? new(false, "Không nhận được kết quả.");
}
