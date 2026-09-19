using Microsoft.AspNetCore.Identity;
using TruckingSystem.Domain.Entities;
using TruckingSystem.Domain.Enums;
using TruckingSystem.Domain.Identity;
using TruckingSystem.Infrastructure.Persistence;

namespace TruckingSystem.API.Data;

/// <summary>
/// Tự tạo dữ liệu mẫu khi app khởi động lần đầu (nếu DB đang rỗng):
/// - Roles mặc định + 1 tài khoản Admin + 1 tài khoản Driver để login test
/// - 1 chuỗi dữ liệu mẫu: Customer -> TransportOrder -> CargoLine -> Vehicle -> Trip (Id=1)
///   để app driver có sẵn Trip ID = 1 dùng test ping GPS ngay, không cần insert tay.
/// </summary>
public static class DbSeeder
{
    private static readonly string[] RoleNames =
        { "Admin", "Dispatcher", "Driver", "Accountant", "WarehouseStaff", "CustomerPortal" };

    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var db = services.GetRequiredService<AppDbContext>();

        // ---- 1. Roles ----
        foreach (var roleName in RoleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
                await roleManager.CreateAsync(new ApplicationRole { Name = roleName });
        }

        // ---- 2. Tài khoản Admin (dùng để vào Swagger test mọi API) ----
        var adminUser = await userManager.FindByNameAsync("admin");
        if (adminUser is null)
        {
            adminUser = new ApplicationUser { UserName = "admin", Email = "admin@trucking.local", EmailConfirmed = true };
            await userManager.CreateAsync(adminUser, "Admin@123");
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }

        // ---- 3. Tài khoản tài xế (dùng để login trên app driver thật) ----
        var driverUser = await userManager.FindByNameAsync("driver1");
        if (driverUser is null)
        {
            driverUser = new ApplicationUser { UserName = "driver1", Email = "driver1@trucking.local", EmailConfirmed = true };
            await userManager.CreateAsync(driverUser, "Driver@123");
            await userManager.AddToRoleAsync(driverUser, "Driver");
        }

        // Nếu đã có sẵn Trip nào rồi thì coi như đã seed dữ liệu nghiệp vụ, không tạo lại
        if (db.Trips.Any()) return;

        // ---- 4. Dữ liệu nghiệp vụ mẫu ----
        var department = new Department { Name = "Vận hành" };
        var position = new Position { Name = "Tài xế" };
        db.Departments.Add(department);
        db.Positions.Add(position);
        await db.SaveChangesAsync();

        var driverEmployee = new Employee
        {
            Code = "NV001",
            FullName = "Nguyễn Văn Tài",
            Phone = "0900000000",
            HireDate = DateTime.UtcNow.AddYears(-1),
            Status = EmployeeStatus.Active,
            DepartmentId = department.Id,
            PositionId = position.Id,
            UserId = driverUser.Id, // liên kết với tài khoản đăng nhập driver1
        };
        db.Employees.Add(driverEmployee);
        await db.SaveChangesAsync();

        db.DriverProfiles.Add(new DriverProfile
        {
            EmployeeId = driverEmployee.Id,
            LicenseNumber = "B2-000000",
            LicenseClass = "B2",
            LicenseExpiryDate = DateTime.UtcNow.AddYears(3),
            YearsOfExperience = 5,
        });

        var customer = new Customer
        {
            Name = "Công ty TNHH Khách Hàng Mẫu",
            CustomerType = CustomerType.Company,
            Phone = "0911111111",
            Address = "123 Đường ABC, Quận 1, TP.HCM",
        };
        db.Customers.Add(customer);
        await db.SaveChangesAsync();

        var transportOrder = new TransportOrder
        {
            Code = "TO-DEMO-001",
            CustomerId = customer.Id,
            PickupPoint = "Kho A, Quận 7, TP.HCM",
            DropoffPoint = "Kho B, Biên Hòa, Đồng Nai",
            Status = TransportOrderStatus.Confirmed,
        };
        db.TransportOrders.Add(transportOrder);
        await db.SaveChangesAsync();

        var cargoLine = new CargoLine
        {
            TransportOrderId = transportOrder.Id,
            CargoType = "Hàng khô đóng kiện",
            PackageCount = 100,
            WeightKg = 5000,
            VolumeM3 = 20,
            QtyTotal = 5000, // tính theo Kg cho demo
        };
        db.CargoLines.Add(cargoLine);
        await db.SaveChangesAsync();

        var vehicle = new Vehicle
        {
            PlateNumber = "51C-123.45",
            VehicleType = "Xe tải 5 tấn",
            CapacityQty = 5000,
            CapacityKg = 5000,
            CapacityM3 = 25,
            Status = VehicleStatus.OnTrip,
        };
        db.Vehicles.Add(vehicle);
        await db.SaveChangesAsync();

        // Trip này sẽ có Id = 1 vì là Trip đầu tiên trong DB — dùng để test app driver ngay
        var trip = new Trip
        {
            TransportOrderId = transportOrder.Id,
            VehicleId = vehicle.Id,
            DriverEmployeeId = driverEmployee.Id,
            Status = TripStatus.Planned,
        };
        db.Trips.Add(trip);
        await db.SaveChangesAsync();

        db.TripLines.Add(new TripLine
        {
            TripId = trip.Id,
            CargoLineId = cargoLine.Id,
            QtyPlanned = cargoLine.QtyTotal,
        });
        await db.SaveChangesAsync();

        // ---- 5. Dữ liệu mẫu cho Kho trung chuyển + tối ưu vị trí (Best-Fit) ----
        var warehouse = new Warehouse
        {
            Name = "Kho TP.HCM",
            Address = "Quận 7, TP.HCM",
            CapacityM3 = 500,
        };
        db.Warehouses.Add(warehouse);
        await db.SaveChangesAsync();

        var zoneNormal = new WarehouseZone { WarehouseId = warehouse.Id, Name = "Khu A - Hàng thường", ZoneType = ZoneType.Normal };
        var zoneFragile = new WarehouseZone { WarehouseId = warehouse.Id, Name = "Khu B - Hàng dễ vỡ", ZoneType = ZoneType.Fragile };
        db.WarehouseZones.AddRange(zoneNormal, zoneFragile);
        await db.SaveChangesAsync();

        // 4 vị trí ở Khu A với sức chứa khác nhau để thấy rõ thuật toán Best-Fit
        // chọn đúng ô vừa khít nhất khi test qua Swagger.
        db.StorageBins.AddRange(
            new StorageBin { WarehouseZoneId = zoneNormal.Id, Code = "A-01", CapacityM3 = 5.0m, MaxWeightKg = 2000, CurrentUsedM3 = 0, Row = 0, Column = 0 },
            new StorageBin { WarehouseZoneId = zoneNormal.Id, Code = "A-02", CapacityM3 = 2.3m, MaxWeightKg = 1000, CurrentUsedM3 = 0, Row = 0, Column = 1 },
            new StorageBin { WarehouseZoneId = zoneNormal.Id, Code = "A-03", CapacityM3 = 8.0m, MaxWeightKg = 3000, CurrentUsedM3 = 0, Row = 1, Column = 0 },
            new StorageBin { WarehouseZoneId = zoneFragile.Id, Code = "B-01", CapacityM3 = 3.0m, MaxWeightKg = 800, CurrentUsedM3 = 0, Row = 0, Column = 0 }
        );
        await db.SaveChangesAsync();
    }
}