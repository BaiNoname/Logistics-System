using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_PayrollManagement;

namespace TruckingSystem.Desktop.SERVICE_PayrollManagement;

public class PayrollApiService(ApiConnection connection)
{
    private const string Root = "/api/payroll";
    public async Task<List<PayrollEmployeeDto>> EmployeesAsync(int year, int month)
        => await connection.GetAsync<List<PayrollEmployeeDto>>($"{Root}/employees?year={year}&month={month}") ?? new();
    public async Task<List<PayrollPeriodDto>> PeriodsAsync()
        => await connection.GetAsync<List<PayrollPeriodDto>>($"{Root}/periods") ?? new();
    public async Task<List<SalaryDto>> SalariesAsync(int employeeId)
        => await connection.GetAsync<List<SalaryDto>>($"{Root}/employees/{employeeId}/salaries") ?? new();
    public async Task<List<AttendanceDto>> AttendanceAsync(int periodId)
        => await connection.GetAsync<List<AttendanceDto>>($"{Root}/periods/{periodId}/attendance") ?? new();
    public async Task<DriverPayrollDaysDto> DriverWorkDaysAsync(int periodId, int employeeId)
        => await connection.GetAsync<DriverPayrollDaysDto>($"{Root}/periods/{periodId}/driver-attendance/{employeeId}")
            ?? throw new InvalidOperationException("Không lấy được lịch tài xế trong kỳ.");
    public async Task<PayrollResult> ImportDriverAttendanceAsync(int periodId, int employeeId, DriverAttendanceRequest request)
        => await connection.PutAsync<PayrollResult>($"{Root}/periods/{periodId}/driver-attendance/{employeeId}", request)
            ?? new(false, "Không nhận được kết quả.");
    public async Task<List<PayrollSlipDto>> SlipsAsync(int periodId)
        => await connection.GetAsync<List<PayrollSlipDto>>($"{Root}/periods/{periodId}/slips") ?? new();
    public async Task<string> ExportAsync(int slipId)
        => await connection.GetAsync<string>($"{Root}/slips/{slipId}/export") ?? throw new InvalidOperationException("Không nhận được phiếu lương.");
    public Task<PayrollResult> CreatePeriodAsync(PeriodRequest request) => PostAsync("periods", request);
    public Task<PayrollResult> AddSalaryAsync(int employeeId, SalaryRequest request) => PostAsync($"employees/{employeeId}/salaries", request);
    public async Task<PayrollResult> SaveAttendanceAsync(int periodId, int employeeId, AttendanceRequest request)
        => await connection.PutAsync<PayrollResult>($"{Root}/periods/{periodId}/attendance/{employeeId}", request) ?? new(false, "Không nhận được kết quả.");
    public Task<PayrollResult> ConfirmAttendanceAsync(int periodId, int employeeId, int revision)
        => PostAsync($"periods/{periodId}/attendance/{employeeId}/confirm", new RevisionRequest(revision));
    public Task<PayrollResult> GenerateAsync(int periodId, int employeeId, int revision)
        => PostAsync($"periods/{periodId}/slips/{employeeId}", new RevisionRequest(revision));
    public Task<PayrollResult> DeleteDraftAsync(int slipId, int revision) => PostAsync($"slips/{slipId}/delete-draft", new RevisionRequest(revision));
    public Task<PayrollResult> AddItemAsync(int slipId, PayrollItemRequest request, bool adjustment)
        => PostAsync($"slips/{slipId}/{(adjustment ? "adjustments" : "items")}", request);
    public Task<PayrollResult> DeleteItemAsync(int slipId, int itemId, int revision)
        => PostAsync($"slips/{slipId}/items/{itemId}/delete", new RevisionRequest(revision));
    public Task<PayrollResult> ApproveAsync(int periodId, int revision) => PostAsync($"periods/{periodId}/approve", new RevisionRequest(revision));
    public Task<PayrollResult> PayAsync(int slipId, PaymentRequest request) => PostAsync($"slips/{slipId}/payments", request);
    private async Task<PayrollResult> PostAsync(string path, object request)
        => await connection.PostAsync<PayrollResult>($"{Root}/{path}", request) ?? new(false, "Không nhận được kết quả.");
}
