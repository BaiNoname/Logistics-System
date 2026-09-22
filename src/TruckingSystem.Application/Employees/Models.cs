namespace TruckingSystem.Application.Employees;

public record EmployeeListItemDto(int Id, string Code, string FullName, string? Phone, string? DepartmentName, string? PositionName, string Status, bool IsDriver);
public record CreateEmployeeRequest(string Code, string FullName, string? Phone, int? DepartmentId, int? PositionId);
public record LookupItemDto(int Id, string Name);

/// <summary>Tạo hồ sơ tài xế (DriverProfile) cho 1 nhân viên đã có sẵn — quan
/// hệ 1-1, mỗi Employee chỉ có tối đa 1 DriverProfile.</summary>
public record CreateDriverProfileRequest(string LicenseNumber, string LicenseClass, DateTime LicenseExpiryDate, DateTime? HealthCertExpiryDate, int YearsOfExperience);

public record DriverProfileDto(string LicenseNumber, string LicenseClass, DateTime LicenseExpiryDate, DateTime? HealthCertExpiryDate, int YearsOfExperience);

/// <summary>Sửa hồ sơ tài xế — cùng cấu trúc CreateDriverProfileRequest.</summary>
public record UpdateDriverProfileRequest(string LicenseNumber, string LicenseClass, DateTime LicenseExpiryDate, DateTime? HealthCertExpiryDate, int YearsOfExperience);

public record EmployeeActionResult(bool Success, string? Message);