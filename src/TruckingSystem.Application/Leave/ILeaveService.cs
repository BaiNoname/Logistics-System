namespace TruckingSystem.Application.Leave;

public interface ILeaveService
{
    Task<List<HolidayDto>> GetHolidaysAsync(int year);
    Task<LeaveResult> AddHolidayAsync(AddHolidayRequest request, string actor);
    Task<LeaveResult> DeleteHolidayAsync(int id, string actor);
}
