namespace TruckingSystem.Application.Leave;

public record LeaveResult(bool Success, string? Message = null, List<string>? Warnings = null);
public record HolidayDto(int Id, DateTime Date, string Name);
public record AddHolidayRequest(DateTime Date, string Name);

public record LeaveDto(int Id, int EmployeeId, DateTime LeaveDate, string Type, string TypeName, bool IsPaid,
    Guid BatchId, string Note, string CreatedBy, bool Locked);
public record LeaveQuotaDto(int Year, int Used, int Quota, int Remaining);
public record LeaveTypeDto(string Type, string Name, string Group, bool IsPaid, bool DeductsQuota, string LimitText);
public record AddLeaveRequest(int EmployeeId, string Type, DateTime From, DateTime To, string? Note);
