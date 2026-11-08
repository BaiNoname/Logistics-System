using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_LeaveManagement;

namespace TruckingSystem.Desktop.SERVICE_LeaveManagement;

public class LeaveApiService(ApiConnection connection)
{
    private const string Holidays = "/api/holidays";
    private const string Leaves = "/api/leaves";
    private static readonly LeaveResult NoResult = new(false, "Không nhận được kết quả.");

    // Gửi ngày thuần (Kind=Unspecified) để JSON không kèm múi giờ và API không lệch ngày.
    private static DateTime Unspecified(DateTime d) => DateTime.SpecifyKind(d.Date, DateTimeKind.Unspecified);

    public async Task<List<HolidayDto>> HolidaysAsync(int year)
        => await connection.GetAsync<List<HolidayDto>>($"{Holidays}?year={year}") ?? new();
    public async Task<LeaveResult> AddHolidayAsync(AddHolidayRequest r)
        => await connection.PostAsync<LeaveResult>(Holidays, r with { Date = Unspecified(r.Date) }) ?? NoResult;
    public async Task<LeaveResult> DeleteHolidayAsync(int id)
        => await connection.DeleteAsync<LeaveResult>($"{Holidays}/{id}") ?? NoResult;

    public async Task<List<LeaveTypeDto>> TypesAsync()
        => await connection.GetAsync<List<LeaveTypeDto>>($"{Leaves}/types") ?? new();
    public async Task<List<LeaveDto>> LeavesAsync(int employeeId, int year, int month)
        => await connection.GetAsync<List<LeaveDto>>($"{Leaves}?employeeId={employeeId}&year={year}&month={month}") ?? new();
    public async Task<LeaveQuotaDto> QuotaAsync(int employeeId, int year)
        => await connection.GetAsync<LeaveQuotaDto>($"{Leaves}/quota?employeeId={employeeId}&year={year}") ?? new(year, 0, 0, 0);
    public async Task<LeaveResult> AddLeaveAsync(AddLeaveRequest r)
        => await connection.PostAsync<LeaveResult>(Leaves, r with { From = Unspecified(r.From), To = Unspecified(r.To) }) ?? NoResult;
    public async Task<LeaveResult> DeleteLeaveAsync(int id)
        => await connection.DeleteAsync<LeaveResult>($"{Leaves}/{id}") ?? NoResult;
    public async Task<LeaveResult> DeleteLeaveBatchAsync(Guid batchId)
        => await connection.DeleteAsync<LeaveResult>($"{Leaves}/batch/{batchId}") ?? NoResult;
}
