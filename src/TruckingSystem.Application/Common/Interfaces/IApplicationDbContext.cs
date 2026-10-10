using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using TruckingSystem.Domain.Entities;

namespace TruckingSystem.Application.Common.Interfaces;

/// <summary>
/// Đây là "hợp đồng" trung tâm của Clean Architecture trong dự án này:
/// Application CHỈ biết interface này (khai báo có những DbSet gì, có SaveChangesAsync),
/// KHÔNG biết đây là PostgreSQL hay EF Core cụ thể ra sao.
///
/// TruckingSystem.Infrastructure.AppDbContext sẽ "implements IApplicationDbContext" —
/// tức là lớp ngoài (Infrastructure) phụ thuộc vào lớp trong (Application), đúng
/// chiều Dependency Rule của Clean Architecture — không phải chiều ngược lại như
/// N-layer cũ (Controller gọi thẳng AppDbContext).
///
/// Lưu ý: vẫn dùng using Microsoft.EntityFrameworkCore ở đây chỉ để có kiểu DbSet&lt;T&gt;
/// cho tiện viết LINQ trong Application Services — đây là đánh đổi thực dụng rất phổ
/// biến (interface kiểu "IApplicationDbContext" trong nhiều dự án Clean Architecture
/// thực tế, ví dụ template nổi tiếng của Jason Taylor), thay vì phải viết thêm 1 tầng
/// Repository/UnitOfWork đầy đủ cho từng entity — việc đó tốn thời gian hơn nhiều so
/// với giá trị mang lại ở quy mô đồ án này.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<Employee> Employees { get; }
    DbSet<Department> Departments { get; }
    DbSet<Position> Positions { get; }
    DbSet<DriverProfile> DriverProfiles { get; }
    DbSet<AuditLog> AuditLogs { get; }

    DbSet<Customer> Customers { get; }
    DbSet<Location> Locations { get; }
    DbSet<Contract> Contracts { get; }
    DbSet<PriceList> PriceLists { get; }
    DbSet<TariffRule> TariffRules { get; }
    DbSet<SalesOrder> SalesOrders { get; }

    DbSet<TransportOrder> TransportOrders { get; }
    DbSet<CargoLine> CargoLines { get; }
    DbSet<TripLine> TripLines { get; }

    DbSet<Vehicle> Vehicles { get; }
    DbSet<VehicleDocument> VehicleDocuments { get; }
    DbSet<FuelLog> FuelLogs { get; }
    DbSet<Trip> Trips { get; }
    DbSet<Waypoint> Waypoints { get; }
    DbSet<TripVideoSegment> TripVideoSegments { get; }
    DbSet<TripCargoPhoto> TripCargoPhotos { get; }
    DbSet<IncidentReport> IncidentReports { get; }
    DbSet<TripExpense> TripExpenses { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<DriverSchedule> DriverSchedules { get; }

    DbSet<Invoice> Invoices { get; }
    DbSet<Payment> Payments { get; }
    DbSet<FuelProvider> FuelProviders { get; }
    DbSet<FuelPriceHistory> FuelPriceHistories { get; }
    DbSet<TripCalendarEvent> TripCalendarEvents { get; }

    DbSet<Warehouse> Warehouses { get; }
    DbSet<GoodsReceipt> GoodsReceipts { get; }
    DbSet<GoodsIssue> GoodsIssues { get; }
    DbSet<WarehouseStock> WarehouseStocks { get; }
    DbSet<WarehouseZone> WarehouseZones { get; }
    DbSet<StorageBin> StorageBins { get; }
    DbSet<StorageAssignment> StorageAssignments { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>Truy cập trực tiếp API Transaction của EF Core — dùng khi 1
    /// nghiệp vụ cần NHIỀU lần SaveChangesAsync nhưng phải "tất cả hoặc không
    /// gì cả" (VD: Chia xe cho nhiều xe cùng lúc — nếu xe thứ 2 lỗi giữa
    /// chừng, KHÔNG được để lại Trip của xe thứ 1 đã lưu dở dang trong DB).</summary>
    DatabaseFacade Database { get; }
}
