using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using TruckingSystem.Application.Auth;
using TruckingSystem.Application.Common.Interfaces;
using TruckingSystem.Application.Locations;
using TruckingSystem.Application.Routing;
using TruckingSystem.Domain.Identity;
using TruckingSystem.Infrastructure.Auth;
using TruckingSystem.Infrastructure.ExternalServices;
using TruckingSystem.Infrastructure.Persistence;

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
            });

        services.AddScoped<ITokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IAuthService, AuthService>();

        // AddHttpClient<TInterface, TImplementation> tự quản lý vòng đời
        // HttpClient đúng chuẩn (tránh lỗi rò rỉ socket khi tự new HttpClient()).
        services.AddHttpClient<IRouteProvider, VietMapRouteProvider>();
        services.AddHttpClient<IPlaceSearchProvider, VietMapPlaceProvider>();

        return services;
    }
}