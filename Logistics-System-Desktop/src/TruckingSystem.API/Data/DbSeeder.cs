using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TruckingSystem.Domain.Entities;
using TruckingSystem.Domain.Enums;
using TruckingSystem.Domain.Identity;
using TruckingSystem.Infrastructure.Persistence;

namespace TruckingSystem.API.Data;

/// <summary>
/// Tự tạo dữ liệu NỀN khi app khởi động (chỉ tạo phần còn thiếu, chạy lại không sinh trùng):
/// - Mức giảm giá tối đa mặc định (20%)
/// - Roles mặc định + tài khoản admin (Admin@123) + tài khoản driver1 (Driver@123) để đăng nhập
/// KHÔNG còn tạo dữ liệu nghiệp vụ mẫu (khách, kho, xe, tài xế, đơn, chuyến, giá nhiên liệu…):
/// dữ liệu nghiệp vụ nhập tay qua phần mềm cho đúng thực tế. Tài khoản driver1 chưa gắn nhân viên —
/// tạo nhân viên tài xế rồi chọn tài khoản driver1 để đăng nhập app tài xế.
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

        if (!await db.MaxDiscountPercents.AnyAsync(p => p.Id == 1))
        {
            db.MaxDiscountPercents.Add(new MaxDiscountPercent { Value = 20, UpdatedAt = DateTime.UtcNow });
            await db.SaveChangesAsync();
        }

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
    }
}
