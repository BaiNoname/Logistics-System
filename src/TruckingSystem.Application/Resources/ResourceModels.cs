namespace TruckingSystem.Application.Resources;

/// <summary>Màn "Nguồn lực theo kho": xe + tài xế của 1 kho (hoặc mọi kho), mỗi dòng kèm
/// tình trạng giấy tờ và "Chạy được hôm nay?" + lý do — tính theo ĐÚNG các quy tắc Chia xe /
/// Bắt đầu chuyến đang dùng (VehicleDocumentRules, DriverPositionRules, DriverLicenseRules,
/// ngày làm, chuyến đang bận). Chỉ ĐỌC, không thay đổi gì.</summary>
public record ResourceOverviewDto(
    int? WarehouseId,
    List<ResourceWarehouseDto> Warehouses,
    ResourceSummaryDto Summary,
    List<VehicleResourceDto> Vehicles,
    List<DriverResourceDto> Drivers);

public record ResourceWarehouseDto(int Id, string Name);

public record ResourceSummaryDto(
    int VehiclesReady, int VehiclesTotal,
    int DriversReady, int DriversTotal,
    int DocsExpiringSoon, int DocsExpiredOrMissing,
    int TripsStartingToday, int TripsRunning);

/// <summary>Hạn giấy tờ: null = chưa nhập. CanRunToday + BlockReason: xe chạy được hôm nay không, vì sao.</summary>
public record VehicleResourceDto(
    int Id, string PlateNumber, string VehicleType, decimal CapacityKg, string RequiredLicense,
    int? WarehouseId, string? WarehouseName, string Status,
    DateTime? RegistrationExpiry, DateTime? InsuranceExpiry, DateTime? PermitExpiry,
    decimal? FuelCurrentLiters, decimal? FuelTankCapacityLiters,
    string? CurrentTrip, bool CanRunToday, string? BlockReason, int ExpiringSoonCount);

/// <summary>DrivableVehicleIds: các xe (trong danh sách) mà hạng bằng của tài xế được phép lái.</summary>
public record DriverResourceDto(
    int EmployeeId, string Code, string FullName, string? Phone,
    int? WarehouseId, string? WarehouseName, bool HasProfile,
    string? LicenseClass, DateTime? LicenseExpiry, DateTime? HealthCertExpiry,
    bool WorksToday, int WorkDaysNext7,
    string? CurrentTrip, string? CurrentVehiclePlate,
    bool CanRunToday, string? BlockReason, int ExpiringSoonCount,
    List<int> DrivableVehicleIds);
