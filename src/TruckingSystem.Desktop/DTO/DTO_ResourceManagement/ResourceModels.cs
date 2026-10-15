namespace TruckingSystem.Desktop.DTO_ResourceManagement
{
    /// <summary>Khớp ResourceOverviewDto bên API — màn "🧭 Nguồn lực theo kho".</summary>
    public class ResourceOverview
    {
        public int? WarehouseId { get; set; }
        public List<ResourceWarehouse> Warehouses { get; set; } = new();
        public ResourceSummary Summary { get; set; } = new();
        public List<VehicleResource> Vehicles { get; set; } = new();
        public List<DriverResource> Drivers { get; set; } = new();
    }

    public class ResourceWarehouse
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

    public class ResourceSummary
    {
        public int VehiclesReady { get; set; }
        public int VehiclesTotal { get; set; }
        public int DriversReady { get; set; }
        public int DriversTotal { get; set; }
        public int DocsExpiringSoon { get; set; }
        public int DocsExpiredOrMissing { get; set; }
        public int TripsStartingToday { get; set; }
        public int TripsRunning { get; set; }
    }

    public class VehicleResource
    {
        public int Id { get; set; }
        public string PlateNumber { get; set; } = "";
        public string VehicleType { get; set; } = "";
        public decimal CapacityKg { get; set; }
        public string RequiredLicense { get; set; } = "";
        public int? WarehouseId { get; set; }
        public string? WarehouseName { get; set; }
        public string Status { get; set; } = "";
        public DateTime? RegistrationExpiry { get; set; }
        public DateTime? InsuranceExpiry { get; set; }
        public DateTime? PermitExpiry { get; set; }
        public decimal? FuelCurrentLiters { get; set; }
        public decimal? FuelTankCapacityLiters { get; set; }
        public string? CurrentTrip { get; set; }
        public bool CanRunToday { get; set; }
        public string? BlockReason { get; set; }
        public int ExpiringSoonCount { get; set; }
    }

    public class DriverResource
    {
        public int EmployeeId { get; set; }
        public string Code { get; set; } = "";
        public string FullName { get; set; } = "";
        public string? Phone { get; set; }
        public int? WarehouseId { get; set; }
        public string? WarehouseName { get; set; }
        public bool HasProfile { get; set; }
        public string? LicenseClass { get; set; }
        public DateTime? LicenseExpiry { get; set; }
        public DateTime? HealthCertExpiry { get; set; }
        public bool WorksToday { get; set; }
        public int WorkDaysNext7 { get; set; }
        public string? CurrentTrip { get; set; }
        public string? CurrentVehiclePlate { get; set; }
        public bool CanRunToday { get; set; }
        public string? BlockReason { get; set; }
        public int ExpiringSoonCount { get; set; }
        public List<int> DrivableVehicleIds { get; set; } = new();
    }
}
