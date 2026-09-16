namespace TruckingSystem.Application.Employees;

public record EmployeeListItemDto(int Id, string Code, string FullName, string? Phone, string? DepartmentName, string? PositionName, string Status);
public record CreateEmployeeRequest(string Code, string FullName, string? Phone, int? DepartmentId, int? PositionId);
public record LookupItemDto(int Id, string Name);
