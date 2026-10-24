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
    public DbSet<EmployeeSalary> EmployeeSalaries => Set<EmployeeSalary>();
    public DbSet<PayrollPeriod> PayrollPeriods => Set<PayrollPeriod>();
    public DbSet<MonthlyAttendance> MonthlyAttendances => Set<MonthlyAttendance>();
    public DbSet<PayrollSlip> PayrollSlips => Set<PayrollSlip>();
    public DbSet<PayrollItem> PayrollItems => Set<PayrollItem>();
    public DbSet<SalaryPayment> SalaryPayments => Set<SalaryPayment>();
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
    public DbSet<ContractAppendix> ContractAppendices => Set<ContractAppendix>();
    public DbSet<PriceList> PriceLists => Set<PriceList>();
    public DbSet<TariffRule> TariffRules => Set<TariffRule>();
    public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();

    // Nhóm C — Vận đơn & Hàng hóa
    public DbSet<TransportOrder> TransportOrders => Set<TransportOrder>();
    public DbSet<DeliveryDeadlineChange> DeliveryDeadlineChanges => Set<DeliveryDeadlineChange>();
    public DbSet<CargoLine> CargoLines => Set<CargoLine>();
    public DbSet<TripLine> TripLines => Set<TripLine>();

    // Nhóm D — Đội xe & Chuyến đi
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<VehicleDocument> VehicleDocuments => Set<VehicleDocument>();
    public DbSet<Trip> Trips => Set<Trip>();
    public DbSet<Waypoint> Waypoints => Set<Waypoint>();
    public DbSet<TripVideoSegment> TripVideoSegments => Set<TripVideoSegment>();
    public DbSet<TripCargoPhoto> TripCargoPhotos => Set<TripCargoPhoto>();
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

    public DbSet<DriverSchedule> DriverSchedules => Set<DriverSchedule>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<CustomerFeedback> CustomerFeedbacks => Set<CustomerFeedback>();


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
        builder.Entity<Employee>().Property(e => e.DateOfBirth).HasColumnType("date");
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

        builder.Entity<DeliveryDeadlineChange>().HasOne(c => c.TransportOrder).WithMany(o => o.DeadlineChanges)
            .HasForeignKey(c => c.TransportOrderId).OnDelete(DeleteBehavior.Restrict);

        builder.Entity<DeliveryDeadlineChange>().HasIndex(c => c.TransportOrderId);

        builder.Entity<DeliveryDeadlineChange>().Property(c => c.VolumeM3AtChange).HasColumnType("numeric(18,4)");

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

        builder.Entity<TripCargoPhoto>(e =>
        {
            e.HasOne(p => p.Trip).WithMany().HasForeignKey(p => p.TripId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(p => p.TripId);
            e.Property(p => p.TakenAt).HasColumnType("timestamp without time zone");
            e.Property(p => p.StoragePath).HasMaxLength(300);
            e.Property(p => p.ContentType).HasMaxLength(50);
            e.Property(p => p.Sha256).HasMaxLength(64);
        });

        builder.Entity<TripVideoSegment>(e =>
        {
            e.HasOne(v => v.Trip).WithMany().HasForeignKey(v => v.TripId).OnDelete(DeleteBehavior.Cascade);
            // UNIQUE: app gửi lại cùng 1 đoạn (mất mạng giữa chừng) không bị lưu trùng.
            e.HasIndex(v => new { v.TripId, v.StartedAt }).IsUnique();
            e.Property(v => v.StartedAt).HasColumnType("timestamp without time zone");
            e.Property(v => v.EndedAt).HasColumnType("timestamp without time zone");
            e.Property(v => v.StoragePath).HasMaxLength(300);
            e.Property(v => v.ContentType).HasMaxLength(50);
            e.Property(v => v.Sha256).HasMaxLength(64);
        });

        builder.Entity<AuditLog>()
            .HasIndex(a => a.Timestamp);

        builder.Entity<Notification>()
            .HasIndex(n => new { n.UserId, n.IsRead });

        ConfigureDataConstraints(builder);
        builder.Entity<Contract>().HasOne(c => c.TariffRule).WithMany().HasForeignKey(c => c.TariffRuleId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Entity<ContractAppendix>(e =>
        {
            e.Property(a => a.Code).HasMaxLength(100);
            e.HasIndex(a => a.Code).IsUnique();
            e.Property(a => a.IssueDate).HasColumnType("date");
            e.Property(a => a.EffectiveDate).HasColumnType("date");
            e.HasOne(a => a.Contract).WithMany().HasForeignKey(a => a.ContractId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(a => a.TariffRule).WithMany().HasForeignKey(a => a.TariffRuleId).OnDelete(DeleteBehavior.Restrict);
        });
        PayrollConfiguration.Configure(builder);

        // ---- Kiểu decimal cho tiền tệ/số lượng — chỉ định precision rõ ràng cho PostgreSQL ----
        foreach (var property in builder.Model.GetEntityTypes()
                     .SelectMany(t => t.GetProperties())
                     .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
        {
            property.SetColumnType("numeric(18,2)");
        }

        // ---- THỂ TÍCH (m³) cần 4 chữ số thập phân — đặt SAU vòng lặp trên (vòng lặp
        // ép mọi decimal về 2 chữ số). Với 2 chữ số, món hàng 0,004 m³ bị lưu thành 0,00
        // -> không chiếm chỗ trong kho; mỗi lần nhập/xuất lại bị làm tròn, sai số dồn dần.
        builder.Entity<CargoLine>().Property(c => c.VolumeM3).HasColumnType("numeric(18,4)");
        builder.Entity<StorageBin>().Property(b => b.CapacityM3).HasColumnType("numeric(18,4)");
        builder.Entity<StorageBin>().Property(b => b.CurrentUsedM3).HasColumnType("numeric(18,4)");
        builder.Entity<Vehicle>().Property(v => v.CapacityM3).HasColumnType("numeric(18,4)");
    }

    /// <summary>Ràng buộc dữ liệu ở MỨC DATABASE — trước đây gần như chỉ kiểm tra trong
    /// code, 2 thao tác cùng lúc vẫn lọt; xóa dữ liệu cha thì xóa dây chuyền mất lịch sử.</summary>
    private static void ConfigureDataConstraints(ModelBuilder builder)
    {
        // ===== 1. UNIQUE — dữ liệu code coi là duy nhất =====
        builder.Entity<TransportOrder>().HasIndex(o => o.Code).IsUnique();
        builder.Entity<Vehicle>().HasIndex(v => v.PlateNumber).IsUnique();          // code lưu biển số IN HOA
        builder.Entity<StorageBin>().HasIndex(b => new { b.WarehouseZoneId, b.Code }).IsUnique(); // code còn kiểm tra trùng trong cả kho
        builder.Entity<DriverSchedule>().HasIndex(s => new { s.DriverEmployeeId, s.ShiftDate }).IsUnique(); // đăng ký theo NGÀY: 1 tài xế 1 ngày 1 dòng
        // Giá nhiên liệu: 1 hãng + 1 loại + 1 ngày chỉ có 1 giá. Giá thị trường chung
        // (FuelProviderId = NULL) cần index riêng vì PostgreSQL coi các NULL là khác nhau.
        builder.Entity<FuelPriceHistory>().HasIndex(p => new { p.FuelProviderId, p.FuelType, p.EffectiveDate })
            .IsUnique().HasDatabaseName("UX_FuelPrice_Provider_Type_Date");
        builder.Entity<FuelPriceHistory>().HasIndex(p => new { p.FuelType, p.EffectiveDate })
            .IsUnique().HasFilter("\"FuelProviderId\" IS NULL").HasDatabaseName("UX_FuelPrice_Market_Type_Date");

        // ===== 2. KHÔNG xóa dây chuyền dữ liệu tài chính / sổ kho / chuyến =====
        // (Restrict: muốn xóa cha thì phải xử lý con trước — không âm thầm mất hóa đơn,
        //  phiếu nhập/xuất, chuyến, khi lỡ xóa khách hàng / đơn / kho.)
        builder.Entity<TransportOrder>().HasOne(o => o.Customer).WithMany(c => c.TransportOrders)
            .HasForeignKey(o => o.CustomerId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<TransportOrder>().HasOne(o => o.Warehouse).WithMany()
            .HasForeignKey(o => o.WarehouseId).OnDelete(DeleteBehavior.Restrict);
        // Hợp đồng đã có đơn chạy theo thì không xóa được (đơn / hóa đơn mất dấu hợp đồng).
        builder.Entity<TransportOrder>().HasOne(o => o.Contract).WithMany()
            .HasForeignKey(o => o.ContractId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<Trip>().HasOne(t => t.TransportOrder).WithMany(o => o.Trips)
            .HasForeignKey(t => t.TransportOrderId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<Invoice>().HasOne(i => i.Customer).WithMany(c => c.Invoices)
            .HasForeignKey(i => i.CustomerId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<Invoice>().HasOne(i => i.TransportOrder).WithMany(o => o.Invoices)
            .HasForeignKey(i => i.TransportOrderId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<Payment>().HasOne(p => p.Invoice).WithMany(i => i.Payments)
            .HasForeignKey(p => p.InvoiceId).OnDelete(DeleteBehavior.Restrict);

        builder.Entity<GoodsReceipt>().HasOne(r => r.TransportOrder).WithMany(o => o.GoodsReceipts)
            .HasForeignKey(r => r.TransportOrderId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<GoodsReceipt>().HasOne(r => r.Warehouse).WithMany(w => w.GoodsReceipts)
            .HasForeignKey(r => r.WarehouseId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<GoodsReceipt>().HasOne(r => r.CargoLine).WithMany()
            .HasForeignKey(r => r.CargoLineId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<GoodsIssue>().HasOne(i => i.TransportOrder).WithMany(o => o.GoodsIssues)
            .HasForeignKey(i => i.TransportOrderId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<GoodsIssue>().HasOne(i => i.Warehouse).WithMany(w => w.GoodsIssues)
            .HasForeignKey(i => i.WarehouseId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<GoodsIssue>().HasOne(i => i.CargoLine).WithMany()
            .HasForeignKey(i => i.CargoLineId).OnDelete(DeleteBehavior.Restrict);

        builder.Entity<StorageAssignment>().HasOne(a => a.StorageBin).WithMany(b => b.Assignments)
            .HasForeignKey(a => a.StorageBinId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<WarehouseStock>().HasOne(s => s.Warehouse).WithMany(w => w.Stocks)
            .HasForeignKey(s => s.WarehouseId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<WarehouseStock>().HasOne(s => s.CargoLine).WithMany()
            .HasForeignKey(s => s.CargoLineId).OnDelete(DeleteBehavior.Restrict);

        // ===== 3. CHECK — không bao giờ âm =====
        builder.Entity<WarehouseStock>().ToTable(t => t.HasCheckConstraint("CK_WarehouseStock_QtyOnHand_NonNegative", "\"QtyOnHand\" >= 0"));
        builder.Entity<StorageAssignment>().ToTable(t => t.HasCheckConstraint("CK_StorageAssignment_QtyStored_NonNegative", "\"QtyStored\" >= 0"));
        builder.Entity<StorageBin>().ToTable(t => t.HasCheckConstraint("CK_StorageBin_CurrentUsedM3_NonNegative", "\"CurrentUsedM3\" >= 0"));
        builder.Entity<FuelLog>().ToTable(t => t.HasCheckConstraint("CK_FuelLog_Positive", "\"Liters\" > 0 AND \"Cost\" >= 0"));
        builder.Entity<FuelPriceHistory>().ToTable(t => t.HasCheckConstraint("CK_FuelPrice_Positive", "\"Price\" > 0"));

        // ===== 4. Chống 2 người sửa CÙNG LÚC (optimistic concurrency) =====
        // Dùng cột hệ thống "xmin" của PostgreSQL (không thêm cột mới vào bảng).
        // 2 người cùng xuất 1 mặt hàng / cùng chia xe trúng 1 xe / bấm hoàn tác 2 lần:
        // người lưu SAU sẽ bị từ chối ("dữ liệu vừa bị thay đổi") thay vì ghi đè số sai.
        builder.Entity<WarehouseStock>().Property<uint>("Version").IsRowVersion();
        builder.Entity<StorageBin>().Property<uint>("Version").IsRowVersion();
        builder.Entity<StorageAssignment>().Property<uint>("Version").IsRowVersion();
        builder.Entity<GoodsIssue>().Property<uint>("Version").IsRowVersion();
        builder.Entity<Vehicle>().Property<uint>("Version").IsRowVersion();
        builder.Entity<TransportOrder>().Property<uint>("Version").IsRowVersion();

    }
}