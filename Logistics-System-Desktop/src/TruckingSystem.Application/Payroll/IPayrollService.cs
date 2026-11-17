namespace TruckingSystem.Application.Payroll;

public interface IPayrollService
{
    Task<PayrollResult> GenerateMonthAsync(GenerateMonthRequest request, string actor);
    Task<List<(int Year, int Month)>> GetOpenMonthsBeforeAsync(int year, int month, CancellationToken cancellationToken);
    Task RunAutomaticCalculationAsync(int year, int month, DateTime scheduledAtUtc, CancellationToken cancellationToken);
    Task<List<KpiPolicyDto>> GetKpiPoliciesAsync();
    Task<PayrollResult> SaveKpiPolicyAsync(SaveKpiPolicyRequest request, string actor);
    Task<List<MonthlyAdjustmentDto>> GetMonthlyAdjustmentsAsync(int year, int month);
    Task<PayrollResult> AddMonthlyAdjustmentAsync(MonthlyAdjustmentRequest request, string actor);
    Task<PayrollResult> AddMonthlyAdjustmentsBulkAsync(BulkMonthlyAdjustmentRequest request, string actor);
    Task<PayrollResult> DeleteMonthlyAdjustmentAsync(int id, string actor);
    Task<List<DriverKpiRateDto>> GetKpiRatesAsync();
    Task<PayrollResult> SaveKpiRatesAsync(List<DriverKpiRateDto> request, string actor);
    Task<List<PayrollEmployeeDto>> GetEmployeesAsync(int year, int month);
    Task<List<SalaryDto>> GetSalariesAsync(int employeeId);
    Task<PayrollResult> AddSalaryAsync(int employeeId, SalaryRequest request, string actor);
    Task<List<PayrollPeriodDto>> GetPeriodsAsync();
    Task<List<PayrollSlipDto>> GetSlipsAsync(int periodId);
    Task<PayrollResult> GenerateSlipAsync(int periodId, int employeeId, RevisionRequest request, string actor);
    Task<PayrollResult> DeleteDraftSlipAsync(int slipId, RevisionRequest request, string actor);
    Task<PayrollResult> AddItemAsync(int slipId, PayrollItemRequest request, bool adjustment, string actor);
    Task<PayrollResult> DeleteItemAsync(int slipId, int itemId, RevisionRequest request, string actor);
    Task<PayrollResult> ApproveAsync(int periodId, RevisionRequest request, string actor);
    Task<PayrollResult> PayAsync(int slipId, PaymentRequest request, string actor);
    Task<string?> ExportSlipAsync(int slipId);
}
