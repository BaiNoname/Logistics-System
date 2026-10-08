using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using TruckingSystem.Application.Auth;
using TruckingSystem.Application.Calendar;
using TruckingSystem.Application.Common.Interfaces;
using TruckingSystem.Application.Customers;
using TruckingSystem.Application.Locations;
using TruckingSystem.Application.Routing;
using TruckingSystem.Application.TripVideos;
using TruckingSystem.Domain.Identity;
using TruckingSystem.Infrastructure.Auth;
using TruckingSystem.Infrastructure.ExternalServices;
using TruckingSystem.Infrastructure.Persistence;
using TruckingSystem.Infrastructure.Storage;

namespace TruckingSystem.Infrastructure;

/// <summary>
/// Toàn bộ "chi tiết kỹ thuật" (PostgreSQL, Identity, JWT) được đăng ký gọn trong
/// 1 chỗ. Program.cs (API) chỉ cần gọi builder.Services.AddInfrastructure(config)
/// — không cần biết bên trong dùng Npgsql hay SQL Server, đúng tinh thần Clean
/// Architecture: Presentation không phụ thuộc trực tiếp vào chi tiết Infrastructure,
/// chỉ "lắp ráp" chúng lại ở Program.cs (composition root).
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Đăng ký AppDbContext (lớp cụ thể) là cài đặt cho IApplicationDbContext (interface)
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<AppDbContext>());

        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;
        })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!)),
                };
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var users = context.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
                        var id = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                        var user = id is null ? null : await users.FindByIdAsync(id);
                        if (user is null || !user.IsActive)
                        {
                            context.Fail("Tài khoản không còn hoạt động.");
                            return;
                        }

                        if (context.Principal!.FindFirstValue("security_stamp") != await users.GetSecurityStampAsync(user))
                        {
                            context.Fail("Phiên đăng nhập đã hết hiệu lực. Vui lòng đăng nhập lại.");
                            return;
                        }

                        var actualRoles = await users.GetRolesAsync(user);
                        var tokenRoles = context.Principal!.FindAll(ClaimTypes.Role).Select(c => c.Value);
                        if (!actualRoles.ToHashSet(StringComparer.OrdinalIgnoreCase).SetEquals(tokenRoles))
                            context.Fail("Quyền tài khoản đã thay đổi. Vui lòng đăng nhập lại.");
                    },
                };
            });

        services.AddScoped<ITokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAccountManagementService, AccountManagementService>();

        // AddHttpClient<TInterface, TImplementation> tự quản lý vòng đời
        // HttpClient đúng chuẩn (tránh lỗi rò rỉ socket khi tự new HttpClient()).
        services.AddHttpClient<IRouteProvider, VietMapRouteProvider>();
        services.AddHttpClient<IPlaceSearchProvider, VietMapPlaceProvider>();
        services.AddHttpClient<ICustomerBusinessCardReader, CustomerBusinessCardReader>();

        // Singleton vì GoogleCalendarService tự đọc + giữ credential 1 lần khi
        // khởi động, không cần tạo lại mỗi request (khác VietMap dùng HttpClient
        // theo từng request qua AddHttpClient).
        services.AddSingleton<IGoogleCalendarService, GoogleCalendarService>();
        services.AddSingleton<IVideoStorage>(
        new LocalVideoStorage(configuration["VideoStorage:RootPath"]));

        return services;
    }
}
