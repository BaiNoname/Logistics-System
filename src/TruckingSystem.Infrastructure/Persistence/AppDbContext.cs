using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TruckingSystem.Application.Common.Interfaces;
using TruckingSystem.Domain.Entities;
using TruckingSystem.Domain.Identity;

namespace TruckingSystem.Infrastructure.Persistence;

/// <summary>
/// DbContext chính. Kế thừa IdentityDbContext để có sẵn AspNetUsers/AspNetRoles/
/// AspNetUserRoles... — không cần tự viết bảng đăng nhập.
///
/// Implement thêm IApplicationDbContext (định nghĩa bên Application) — đây chính
/// là "mối nối" của Clean Architecture: lớp ngoài (Infrastructure) implement
/// interface do lớp trong (Application) khai báo, chứ Application không hề
/// biết tới class AppDbContext này tồn tại.
/// </summary>
public class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>, IApplicationDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Nhóm A — Nhân viên & Phân quyền
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Position> Positions => Set<Position>();
    public DbSet<DriverProfile> DriverProfiles => Set<DriverProfile>();
    public DbSet<EmployeeDocument> EmployeeDocuments => Set<EmployeeDocument>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    // Nhóm B — Khách hàng & Kinh doanh
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<CustomerContact> CustomerContacts => Set<CustomerContact>();
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<PriceList> PriceLists => Set<PriceList>();
    public DbSet<TariffRule> TariffRules => Set<TariffRule>();
    public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();

    // Nhóm C — Vận đơn & Hàng hóa
    public DbSet<TransportOrder> TransportOrders => Set<TransportOrder>();
    public DbSet<CargoLine> CargoLines => Set<CargoLine>();
    public DbSet<TripLine> TripLines => Set<TripLine>();

    // Nhóm D — Đội xe & Chuyến đi
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<VehicleDocument> VehicleDocuments => Set<VehicleDocument>();
    public DbSet<Trip> Trips => Set<Trip>();
    public DbSet<Waypoint> Waypoints => Set<Waypoint>();
    public DbSet<IncidentReport> IncidentReports => Set<IncidentReport>();

    // Nhóm E — Tài chính
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceLine> InvoiceLines => Set<InvoiceLine>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<TripExpense> TripExpenses => Set<TripExpense>();
    public DbSet<FuelLog> FuelLogs => Set<FuelLog>();
    public DbSet<FuelPriceHistory> FuelPriceHistories => Set<FuelPriceHistory>();
    public DbSet<FuelProvider> FuelProviders => Set<FuelProvider>();
    public DbSet<TripCalendarEvent> TripCalendarEvents => Set<TripCalendarEvent>();

    // Nhóm F — Vận hành khác
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<DriverSchedule> DriverSchedules => Set<DriverSchedule>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<CustomerFeedback> CustomerFeedbacks => Set<CustomerFeedback>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();

    // Nhóm G — Đồng bộ Video Osmo Pocket

    // Nhóm H — Kho trung chuyển (Cross-dock)
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<GoodsReceipt> GoodsReceipts => Set<GoodsReceipt>();
    public DbSet<GoodsIssue> GoodsIssues => Set<GoodsIssue>();
    public DbSet<WarehouseStock> WarehouseStocks => Set<WarehouseStock>();
    public DbSet<WarehouseZone> WarehouseZones => Set<WarehouseZone>();
    public DbSet<StorageBin> StorageBins => Set<StorageBin>();
    public DbSet<StorageAssignment> StorageAssignments => Set<StorageAssignment>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder); // BẮT BUỘC gọi trước để Identity tự cấu hình bảng của nó

        // ---- RolePermission: khóa chính ghép (RoleId, PermissionId) ----
        builder.Entity<RolePermission>(e =>
        {
            e.HasKey(rp => new { rp.RoleId, rp.PermissionId });
            e.HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId);
            e.HasOne<ApplicationRole>()
                .WithMany()
                .HasForeignKey(rp => rp.RoleId);
        });

        // ---- Employee 1-1 DriverProfile ----
        builder.Entity<Employee>()
            .HasOne(e => e.DriverProfile)
            .WithOne(d => d.Employee)
            .HasForeignKey<DriverProfile>(d => d.EmployeeId);

        // ---- TransportOrder 1-1 SalesOrder ----
        builder.Entity<SalesOrder>()
            .HasOne(s => s.TransportOrder)
            .WithOne(t => t.SalesOrder)
            .HasForeignKey<TransportOrder>(t => t.SalesOrderId);

        // ---- TransportOrder 1-1 CustomerFeedback ----
        builder.Entity<TransportOrder>()
            .HasOne(t => t.Feedback)
            .WithOne(f => f.TransportOrder)
            .HasForeignKey<CustomerFeedback>(f => f.TransportOrderId);

        // ---- Token public tracking phải là duy nhất (mỗi đơn 1 link riêng) ----
        builder.Entity<TransportOrder>()
            .HasIndex(t => t.PublicTrackingToken)
            .IsUnique();

        // ---- Tránh xóa dây chuyền (cascade delete) không mong muốn giữa Trip <-> Vehicle/Employee ----
        // Vì Vehicle/Employee là dữ liệu tham chiếu lâu dài, không nên bị xóa cascade
        // theo Trip. Nếu xóa Trip, không kéo theo xóa Vehicle/Employee.
        builder.Entity<Trip>()
            .HasOne(t => t.Vehicle)
            .WithMany(v => v.Trips)
            .HasForeignKey(t => t.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Trip>()
            .HasOne(t => t.DriverEmployee)
            .WithMany()
            .HasForeignKey(t => t.DriverEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        // ---- Module Kho: GoodsReceipt.TripId nullable (khách tự mang hàng tới không cần Trip) ----
        builder.Entity<GoodsReceipt>()
            .HasOne(gr => gr.Trip)
            .WithMany()
            .HasForeignKey(gr => gr.TripId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        builder.Entity<GoodsIssue>()
            .HasOne(gi => gi.Trip)
            .WithMany()
            .HasForeignKey(gi => gi.TripId)
            .OnDelete(DeleteBehavior.Restrict);


        //     — phải khai báo tường minh + Restrict để tránh lỗi "multiple cascade paths" ----
        builder.Entity<TransportOrder>()
            .HasOne(o => o.PickupLocation)
            .WithMany()
            .HasForeignKey(o => o.PickupLocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<TransportOrder>()
            .HasOne(o => o.DropoffLocation)
            .WithMany()
            .HasForeignKey(o => o.DropoffLocationId)
            .OnDelete(DeleteBehavior.Restrict);

        // ---- Mỗi (Warehouse, CargoLine) chỉ có đúng 1 dòng tồn kho ----
        builder.Entity<WarehouseStock>()
            .HasIndex(ws => new { ws.WarehouseId, ws.CargoLineId })
            .IsUnique();

        // ---- Index cho các bảng dữ liệu lớn / hay truy vấn realtime ----
        builder.Entity<Waypoint>()
            .HasIndex(w => new { w.TripId, w.Timestamp });

        builder.Entity<AuditLog>()
            .HasIndex(a => a.Timestamp);

        builder.Entity<Notification>()
            .HasIndex(n => new { n.UserId, n.IsRead });

        // ---- Kiểu decimal cho tiền tệ/số lượng — chỉ định precision rõ ràng cho PostgreSQL ----
        foreach (var property in builder.Model.GetEntityTypes()
                     .SelectMany(t => t.GetProperties())
                     .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
        {
            property.SetColumnType("numeric(18,2)");
        }
    }
}
