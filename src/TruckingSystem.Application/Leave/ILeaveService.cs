namespace TruckingSystem.Application.Leave;

public interface ILeaveService
{
    Task<List<HolidayDto>> GetHolidaysAsync(int year);
    Task<LeaveResult> AddHolidayAsync(AddHolidayRequest request, string actor);
    Task<LeaveResult> DeleteHolidayAsync(int id, string actor);

    Task<List<LeaveTypeDto>> GetLeaveTypesAsync();
    Task<List<LeaveDto>> GetLeavesAsync(int employeeId, int year, int month);
    Task<LeaveQuotaDto> GetQuotaAsync(int employeeId, int year);
    Task<LeaveResult> AddLeaveAsync(AddLeaveRequest request, string actor);
    Task<LeaveResult> DeleteLeaveAsync(int id, string actor);
    Task<LeaveResult> DeleteLeaveBatchAsync(Guid batchId, string actor);
}
