namespace TruckingSystem.Application.Leave;

public record LeaveResult(bool Success, string? Message = null, List<string>? Warnings = null);
public record HolidayDto(int Id, DateTime Date, string Name);
public record AddHolidayRequest(DateTime Date, string Name);
