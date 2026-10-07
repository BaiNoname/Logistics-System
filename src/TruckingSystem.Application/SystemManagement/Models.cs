namespace TruckingSystem.Application.SystemManagement;

public record SystemItemDto(int Id, string Name, int EmployeeCount);
public record NameRequest(string? Name);
public enum SystemActionError { None, InvalidName, Duplicate, NotFound, InUse }
public record SystemActionResult(SystemItemDto? Item = null, SystemActionError Error = SystemActionError.None, string? Message = null);
