using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_LeaveManagement;

namespace TruckingSystem.Desktop.SERVICE_LeaveManagement;

public class LeaveApiService(ApiConnection connection)
{
    private const string Holidays = "/api/holidays";
    private const string Leaves = "/api/leaves";
    private static readonly LeaveResult NoResult = new(false, "Không nhận được kết quả.");

    public async Task<List<HolidayDto>> HolidaysAsync(int year)
        => await connection.GetAsync<List<HolidayDto>>($"{Holidays}?year={year}") ?? new();
    public async Task<LeaveResult> AddHolidayAsync(AddHolidayRequest r)
        => await connection.PostAsync<LeaveResult>(Holidays, r with { Date = r.Date.Date }) ?? NoResult;
    public async Task<LeaveResult> DeleteHolidayAsync(int id)
        => await connection.DeleteAsync<LeaveResult>($"{Holidays}/{id}") ?? NoResult;

    public async Task<List<LeaveTypeDto>> TypesAsync()
        => await connection.GetAsync<List<LeaveTypeDto>>($"{Leaves}/types") ?? new();
    public async Task<List<LeaveDto>> LeavesAsync(int employeeId, int year, int month)
        => await connection.GetAsync<List<LeaveDto>>($"{Leaves}?employeeId={employeeId}&year={year}&month={month}") ?? new();
    public async Task<LeaveQuotaDto> QuotaAsync(int employeeId, int year)
        => await connection.GetAsync<LeaveQuotaDto>($"{Leaves}/quota?employeeId={employeeId}&year={year}") ?? new(year, 0, 0, 0);
    public async Task<LeaveResult> AddLeaveAsync(AddLeaveRequest r)
        => await connection.PostAsync<LeaveResult>(Leaves, r with { From = r.From.Date, To = r.To.Date }) ?? NoResult;
    public async Task<LeaveResult> DeleteLeaveAsync(int id)
        => await connection.DeleteAsync<LeaveResult>($"{Leaves}/{id}") ?? NoResult;
    public async Task<LeaveResult> DeleteLeaveBatchAsync(Guid batchId)
        => await connection.DeleteAsync<LeaveResult>($"{Leaves}/batch/{batchId}") ?? NoResult;
}
