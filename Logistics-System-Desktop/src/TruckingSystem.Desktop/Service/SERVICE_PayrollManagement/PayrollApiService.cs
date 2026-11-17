using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_PayrollManagement;

namespace TruckingSystem.Desktop.SERVICE_PayrollManagement;

public class PayrollApiService(ApiConnection connection)
{
    public async Task<List<DriverKpiRateDto>> KpiRatesAsync()
        => await connection.GetAsync<List<DriverKpiRateDto>>($"{Root}/kpi-rates") ?? new();
    public async Task<List<KpiPolicyDto>> PoliciesAsync() => await connection.GetAsync<List<KpiPolicyDto>>($"{Root}/kpi-policies") ?? new();
    public async Task<PayrollResult> SavePolicyAsync(SaveKpiPolicyRequest request)
        => await connection.PutAsync<PayrollResult>($"{Root}/kpi-policies", request) ?? new(false, "Không nhận được kết quả.");
    public async Task<List<MonthlyAdjustmentDto>> MonthlyAdjustmentsAsync(int year, int month)
        => await connection.GetAsync<List<MonthlyAdjustmentDto>>($"{Root}/monthly-adjustments?year={year}&month={month}") ?? new();
    public Task<PayrollResult> AddMonthlyAdjustmentAsync(MonthlyAdjustmentRequest request) => PostAsync("monthly-adjustments", request);
    public Task<PayrollResult> AddMonthlyAdjustmentsBulkAsync(BulkMonthlyAdjustmentRequest request) => PostAsync("monthly-adjustments/bulk", request);
    public async Task<PayrollResult> DeleteMonthlyAdjustmentAsync(int id)
        => await connection.DeleteAsync<PayrollResult>($"{Root}/monthly-adjustments/{id}") ?? new(false, "Không nhận được kết quả.");
    private const string Root = "/api/payroll";
    public async Task<List<PayrollEmployeeDto>> EmployeesAsync(int year, int month)
        => await connection.GetAsync<List<PayrollEmployeeDto>>($"{Root}/employees?year={year}&month={month}") ?? new();
    public async Task<List<PayrollPeriodDto>> PeriodsAsync()
        => await connection.GetAsync<List<PayrollPeriodDto>>($"{Root}/periods") ?? new();
    public async Task<List<SalaryDto>> SalariesAsync(int employeeId)
        => await connection.GetAsync<List<SalaryDto>>($"{Root}/employees/{employeeId}/salaries") ?? new();
    public async Task<List<PayrollSlipDto>> SlipsAsync(int periodId)
        => await connection.GetAsync<List<PayrollSlipDto>>($"{Root}/periods/{periodId}/slips") ?? new();
    public async Task<string> ExportAsync(int slipId)
        => await connection.GetAsync<string>($"{Root}/slips/{slipId}/export") ?? throw new InvalidOperationException("Không nhận được phiếu lương.");
    public Task<PayrollResult> AddSalaryAsync(int employeeId, SalaryRequest request) => PostAsync($"employees/{employeeId}/salaries", request);
    public Task<PayrollResult> DeleteDraftAsync(int slipId, int revision) => PostAsync($"slips/{slipId}/delete-draft", new RevisionRequest(revision));
    public Task<PayrollResult> ApproveAsync(int periodId, int revision) => PostAsync($"periods/{periodId}/approve", new RevisionRequest(revision));
    public Task<PayrollResult> PayAsync(int slipId, PaymentRequest request) => PostAsync($"slips/{slipId}/payments", request);
    private async Task<PayrollResult> PostAsync(string path, object request)
        => await connection.PostAsync<PayrollResult>($"{Root}/{path}", request) ?? new(false, "Không nhận được kết quả.");
}
