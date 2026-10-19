namespace TruckingSystem.Application.Payroll;

public interface IPayrollService
{
    Task<List<PayrollEmployeeDto>> GetEmployeesAsync(int year, int month);
    Task<List<SalaryDto>> GetSalariesAsync(int employeeId);
    Task<PayrollResult> AddSalaryAsync(int employeeId, SalaryRequest request, string actor);
    Task<List<PayrollPeriodDto>> GetPeriodsAsync();
    Task<PayrollResult> CreatePeriodAsync(PeriodRequest request, string actor);
    Task<List<AttendanceDto>> GetAttendanceAsync(int periodId);
    Task<DriverPayrollDaysDto?> GetDriverWorkDaysAsync(int periodId, int employeeId);
    Task<PayrollResult> ImportDriverAttendanceAsync(int periodId, int employeeId, DriverAttendanceRequest request, string actor);
    Task<PayrollResult> SaveAttendanceAsync(int periodId, int employeeId, AttendanceRequest request, string actor);
    Task<PayrollResult> ConfirmAttendanceAsync(int periodId, int employeeId, RevisionRequest request, string actor);
    Task<List<PayrollSlipDto>> GetSlipsAsync(int periodId);
    Task<PayrollResult> GenerateSlipAsync(int periodId, int employeeId, RevisionRequest request, string actor);
    Task<PayrollResult> DeleteDraftSlipAsync(int slipId, RevisionRequest request, string actor);
    Task<PayrollResult> AddItemAsync(int slipId, PayrollItemRequest request, bool adjustment, string actor);
    Task<PayrollResult> DeleteItemAsync(int slipId, int itemId, RevisionRequest request, string actor);
    Task<PayrollResult> ApproveAsync(int periodId, RevisionRequest request, string actor);
    Task<PayrollResult> PayAsync(int slipId, PaymentRequest request, string actor);
    Task<string?> ExportSlipAsync(int slipId);
}
