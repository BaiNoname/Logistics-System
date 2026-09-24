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
            LengthMeters = 30, // kho hình chữ nhật 30m x 20m — đủ chỗ đặt 2 khu cạnh nhau
            WidthMeters = 20,
        };
        db.Warehouses.Add(warehouse);
        await db.SaveChangesAsync();

        // 2 khu đặt CẠNH NHAU theo chiều ngang trong sơ đồ 30m x 20m —
        // Khu A chiếm nửa trái (0,0 tới 18,20), Khu B nửa phải (20,0 tới 30,20).
        var zoneNormal = new WarehouseZone
        {
            WarehouseId = warehouse.Id,
            Name = "Khu A - Hàng thường",
            ZoneType = ZoneType.Normal,
            PosX = 0,
            PosY = 0,
            WidthMeters = 18,
            HeightMeters = 20,
        };
        var zoneFragile = new WarehouseZone
        {
            WarehouseId = warehouse.Id,
            Name = "Khu B - Hàng dễ vỡ",
            ZoneType = ZoneType.Fragile,
            PosX = 20,
            PosY = 0,
            WidthMeters = 10,
            HeightMeters = 20,
        };
        db.WarehouseZones.AddRange(zoneNormal, zoneFragile);
        await db.SaveChangesAsync();

        // 4 vị trí ở Khu A với sức chứa khác nhau để thấy rõ thuật toán Best-Fit
        // chọn đúng ô vừa khít nhất khi test qua Swagger. Tọa độ đặt KHÔNG đè
        // lên nhau, nằm gọn trong ranh giới từng khu vực (A: 18x20, B: 10x20).
        db.StorageBins.AddRange(
            new StorageBin { WarehouseZoneId = zoneNormal.Id, Code = "A-01", CapacityM3 = 5.0m, MaxWeightKg = 2000, CurrentUsedM3 = 0, PosX = 0, PosY = 0, WidthMeters = 5, HeightMeters = 4 },
            new StorageBin { WarehouseZoneId = zoneNormal.Id, Code = "A-02", CapacityM3 = 2.3m, MaxWeightKg = 1000, CurrentUsedM3 = 0, PosX = 6, PosY = 0, WidthMeters = 5, HeightMeters = 4 },
            new StorageBin { WarehouseZoneId = zoneNormal.Id, Code = "A-03", CapacityM3 = 8.0m, MaxWeightKg = 3000, CurrentUsedM3 = 0, PosX = 0, PosY = 5, WidthMeters = 6, HeightMeters = 5 },
            new StorageBin { WarehouseZoneId = zoneFragile.Id, Code = "B-01", CapacityM3 = 3.0m, MaxWeightKg = 800, CurrentUsedM3 = 0, PosX = 0, PosY = 0, WidthMeters = 4, HeightMeters = 4 }
        );
        await db.SaveChangesAsync();

        // ---- 6. Kho THỨ 2 — chuyên để test "chia nhiều KHU VỰC dễ vỡ" khi 1
        // khu không đủ chứa hết đơn hàng (đúng kịch bản bạn mô tả: 2 khu Dễ vỡ,
        // đơn số lượng vượt quá 1 khu, phải cộng dồn cả 2 khu mới đủ chỗ). ----
        var warehouse2 = new Warehouse
        {
            Name = "Kho Hà Nội - Test đa khu Dễ vỡ",
            Address = "Long Biên, Hà Nội",
            CapacityM3 = 200,
            LengthMeters = 20,
            WidthMeters = 20,
        };
        db.Warehouses.Add(warehouse2);
        await db.SaveChangesAsync();

        // 2 khu DỄ VỠ đặt cạnh nhau (mỗi khu 10x20) — CỐ TÌNH để sức chứa mỗi
        // khu nhỏ (chỉ 4 m³/khu, tổng 8 m³) để dễ tạo đơn hàng VƯỢT quá 1 khu
        // (VD: đặt 6 m³) — bắt buộc thuật toán phải chia sang khu thứ 2.
        var zoneFragileC1 = new WarehouseZone
        {
            WarehouseId = warehouse2.Id,
            Name = "Khu C1 - Dễ vỡ",
            ZoneType = ZoneType.Fragile,
            PosX = 0,
            PosY = 0,
            WidthMeters = 10,
            HeightMeters = 20,
        };
        var zoneFragileC2 = new WarehouseZone
        {
            WarehouseId = warehouse2.Id,
            Name = "Khu C2 - Dễ vỡ",
            ZoneType = ZoneType.Fragile,
            PosX = 10,
            PosY = 0,
            WidthMeters = 10,
            HeightMeters = 20,
        };
        db.WarehouseZones.AddRange(zoneFragileC1, zoneFragileC2);
        await db.SaveChangesAsync();

        db.StorageBins.AddRange(
            new StorageBin { WarehouseZoneId = zoneFragileC1.Id, Code = "C1-01", CapacityM3 = 4.0m, MaxWeightKg = 500, CurrentUsedM3 = 0, PosX = 0, PosY = 0, WidthMeters = 5, HeightMeters = 4 },
            new StorageBin { WarehouseZoneId = zoneFragileC2.Id, Code = "C2-01", CapacityM3 = 4.0m, MaxWeightKg = 500, CurrentUsedM3 = 0, PosX = 0, PosY = 0, WidthMeters = 5, HeightMeters = 4 }
        );
        await db.SaveChangesAsync();

        // Đơn hàng dễ vỡ, số lượng 6 m³ — VƯỢT quá 1 mình C1-01 (chỉ 4 m³) —
        // bấm "Gợi ý vị trí" ở kho Hà Nội, chọn đúng dòng hàng này để thấy
        // thuật toán TỰ CHIA ra cả C1-01 (4 m³) và C2-01 (2 m³).
        var customer2 = new Customer
        {
            Name = "Công ty Gốm Sứ Minh Long",
            CustomerType = CustomerType.Company,
            Phone = "0922222222",
            Address = "45 Trần Phú, Hà Nội",
        };
        db.Customers.Add(customer2);
        await db.SaveChangesAsync();

        var orderSplitTest = new TransportOrder
        {
            Code = "TO-DEMO-SPLIT",
            CustomerId = customer2.Id,
            PickupPoint = "Xưởng gốm Bát Tràng, Hà Nội",
            DropoffPoint = "Kho Hà Nội - Test đa khu Dễ vỡ",
            Status = TransportOrderStatus.Confirmed,
            CommittedDeliveryDate = DateTime.UtcNow.AddDays(10), // KHÔNG gấp — để thấy đúng logic Best-Fit thường, không phải ưu tiên gần cửa
        };
        db.TransportOrders.Add(orderSplitTest);
        await db.SaveChangesAsync();

        db.CargoLines.Add(new CargoLine
        {
            TransportOrderId = orderSplitTest.Id,
            CargoType = "Gốm sứ dễ vỡ",
            PackageCount = 60,
            WeightKg = 1200,
            VolumeM3 = 6,
            QtyTotal = 6, // đúng 6 m³ — vượt quá C1-01 (4 m³), phải chia sang C2-01
            IsFragile = true,
        });
        await db.SaveChangesAsync();

        // ---- 7. Đơn hàng NGUY HIỂM nhưng CHƯA có khu Hazmat nào — dùng để
        // test đúng tình huống bạn phát hiện trước đó: hệ thống phải CHẶN
        // nhập kho, báo rõ "chưa có khu vực Hàng nguy hiểm phù hợp". ----
        var orderHazmatTest = new TransportOrder
        {
            Code = "TO-DEMO-HAZMAT",
            CustomerId = customer.Id,
            PickupPoint = "Nhà máy hóa chất, Long An",
            DropoffPoint = "Kho TP.HCM",
            Status = TransportOrderStatus.Confirmed,
        };
        db.TransportOrders.Add(orderHazmatTest);
        await db.SaveChangesAsync();

        db.CargoLines.Add(new CargoLine
        {
            TransportOrderId = orderHazmatTest.Id,
            CargoType = "Hóa chất công nghiệp",
            PackageCount = 20,
            WeightKg = 800,
            VolumeM3 = 2,
            QtyTotal = 2,
            IsHazmat = true,
        });
        await db.SaveChangesAsync();

        // ---- 8. Giấy tờ xe — 1 CÒN HẠN, 1 ĐÃ HẾT HẠN (test Greedy tự loại xe
        // hết hạn giấy tờ khi chia xe, và test cảnh báo "⚠️ Đã hết hạn" ở màn
        // Giấy tờ xe). ----
        db.VehicleDocuments.AddRange(
            new VehicleDocument
            {
                VehicleId = vehicle.Id,
                DocType = VehicleDocumentType.Registration,
                IssueDate = DateTime.UtcNow.AddYears(-1),
                ExpiryDate = DateTime.UtcNow.AddYears(1), // còn hạn
            },
            new VehicleDocument
            {
                VehicleId = vehicle.Id,
                DocType = VehicleDocumentType.Insurance,
                IssueDate = DateTime.UtcNow.AddYears(-2),
                ExpiryDate = DateTime.UtcNow.AddDays(-10), // ĐÃ HẾT HẠN 10 ngày trước
            }
        );
        await db.SaveChangesAsync();

        // ---- 9. Hãng xăng + lịch sử giá — test màn Nhiên liệu mới ----
        var fuelProvider1 = new FuelProvider { Name = "Petrolimex", ContactPhone = "1900545422", Address = "Toàn quốc" };
        var fuelProvider2 = new FuelProvider { Name = "PV Oil", ContactPhone = "1800545458", Address = "Toàn quốc" };
        db.FuelProviders.AddRange(fuelProvider1, fuelProvider2);
        await db.SaveChangesAsync();

        db.FuelPriceHistories.AddRange(
            new FuelPriceHistory { FuelType = "Dầu DO", Price = 19500, EffectiveDate = DateTime.UtcNow.AddDays(-30), FuelProviderId = fuelProvider1.Id },
            new FuelPriceHistory { FuelType = "Dầu DO", Price = 19800, EffectiveDate = DateTime.UtcNow.AddDays(-5), FuelProviderId = fuelProvider1.Id },
            new FuelPriceHistory { FuelType = "Xăng RON95", Price = 21200, EffectiveDate = DateTime.UtcNow.AddDays(-5), FuelProviderId = fuelProvider2.Id },
            new FuelPriceHistory { FuelType = "Dầu DO", Price = 19600, EffectiveDate = DateTime.UtcNow.AddDays(-3), FuelProviderId = null } // giá thị trường chung, không theo hãng
        );
        await db.SaveChangesAsync();

        // ---- 10. Xe thứ 2 + tài xế thứ 2 với BẰNG LÁI ĐÃ HẾT HẠN — test Greedy
        // tự động LOẠI tài xế này khi chia xe (không bao giờ được chọn). ----
        var vehicle2 = new Vehicle
        {
            PlateNumber = "51D-678.90",
            VehicleType = "Xe tải 3 tấn",
            CapacityQty = 3000,
            CapacityKg = 3000,
            CapacityM3 = 15,
            Status = VehicleStatus.Available,
        };
        db.Vehicles.Add(vehicle2);
        await db.SaveChangesAsync();

        var driverUser2 = await userManager.FindByNameAsync("driver2");
        if (driverUser2 is null)
        {
            driverUser2 = new ApplicationUser { UserName = "driver2", Email = "driver2@trucking.local", EmailConfirmed = true };
            await userManager.CreateAsync(driverUser2, "Driver@123");
            await userManager.AddToRoleAsync(driverUser2, "Driver");
        }

        var driverEmployee2 = new Employee
        {
            Code = "NV002",
            FullName = "Trần Văn Hết Hạn",
            Phone = "0900000002",
            HireDate = DateTime.UtcNow.AddMonths(-6),
            Status = EmployeeStatus.Active,
            DepartmentId = department.Id,
            PositionId = position.Id,
            UserId = driverUser2.Id,
        };
        db.Employees.Add(driverEmployee2);
        await db.SaveChangesAsync();

        db.DriverProfiles.Add(new DriverProfile
        {
            EmployeeId = driverEmployee2.Id,
            LicenseNumber = "B2-999999",
            LicenseClass = "B2",
            LicenseExpiryDate = DateTime.UtcNow.AddDays(-15), // ĐÃ HẾT HẠN — Greedy phải tự loại
            YearsOfExperience = 2,
        });
        await db.SaveChangesAsync();

        // ---- 11. Đơn hàng CHƯA chia xe — dùng để test trực tiếp nút "Chia xe
        // (Greedy)" từ đầu, xem đúng quy trình đầy đủ (Xác nhận -> Chia xe ->
        // Bắt đầu chuyến -> Hoàn tất...). Có đủ 2 dòng hàng để test Greedy đóng
        // gói nhiều dòng vào 1 xe. ----
        var orderFreshTest = new TransportOrder
        {
            Code = "TO-DEMO-FRESH",
            CustomerId = customer.Id,
            PickupPoint = "382 Nguyễn Thị Minh Khai, Q3, TP.HCM",
            DropoffPoint = "Kho TP.HCM",
            PickupMethod = PickupMethod.CompanyPickup,
            Status = TransportOrderStatus.Confirmed,
            CommittedDeliveryDate = DateTime.UtcNow.AddDays(2), // GẤP — test ưu tiên gần cửa khi nhập kho
        };
        db.TransportOrders.Add(orderFreshTest);
        await db.SaveChangesAsync();

        db.CargoLines.AddRange(
            new CargoLine { TransportOrderId = orderFreshTest.Id, CargoType = "Thiết bị điện tử", PackageCount = 30, WeightKg = 1500, VolumeM3 = 6, QtyTotal = 1500 },
            new CargoLine { TransportOrderId = orderFreshTest.Id, CargoType = "Phụ kiện nhựa", PackageCount = 200, WeightKg = 800, VolumeM3 = 3, QtyTotal = 800 }
        );
        await db.SaveChangesAsync();
    }
}