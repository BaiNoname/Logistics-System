using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using TruckingSystem.Application;
using TruckingSystem.Infrastructure;

// ---- Sửa lỗi 500 kinh điển với Npgsql: WinForms DateTimePicker (và nhiều
// nguồn khác) trả về DateTime với Kind=Unspecified/Local — nhưng Npgsql bản
// mới BẮT BUỘC DateTime phải Kind=Utc khi ghi vào cột timestamptz, nếu không
// sẽ ném lỗi "Cannot write DateTime with Kind=Unspecified..." (500 Internal
// Server Error). Switch này tắt kiểm tra nghiêm ngặt đó — áp dụng 1 LẦN DUY
// NHẤT ở đây, tránh phải tự SpecifyKind(Utc) thủ công ở TỪNG chỗ tạo/sửa các
// trường ngày tháng rải rác khắp hệ thống (TransportOrder, Trip, GoodsReceipt,
// DriverProfile, VehicleDocument...).
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// ---- 2 dòng này thay thế cho hàng chục dòng cấu hình DbContext/Identity/JWT/Service
//      từng nằm rải rác trực tiếp trong Program.cs trước đây — giờ mỗi layer tự
//      "đóng gói" cách đăng ký DI của chính nó, Program.cs chỉ đứng vai trò
//      "composition root" lắp ráp lại, không cần biết chi tiết bên trong. ----
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// ---- CORS cho React Web (nếu sau này cần) ----
builder.Services.AddCors(options => options.AddPolicy("ReactApp", policy =>
    policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
          .AllowAnyHeader()
          .AllowAnyMethod()
          .AllowCredentials()));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "Nhập: Bearer {token}",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
    });
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer",
                },
            },
            Array.Empty<string>()
        },
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;
    options.AddPolicy("freight-chat", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
});
var app = builder.Build();

app.UseMiddleware<TruckingSystem.API.Middleware.DbConflictMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // phục vụ wwwroot/track.html cho trang tracking công khai
app.UseCors("ReactApp");
app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();
app.MapControllers();

// Kiểm tra database trước khi chạy: máy nào pull code mới mà quên Update-Database thì báo RÕ migration nào
// còn thiếu + lệnh cần chạy, thay vì để API chạy rồi văng lỗi khó hiểu kiểu
// "42P01: relation \"MaxDiscountPercent\" does not exist" ở giữa chừng.
// Seed dữ liệu mẫu khi khởi động lần đầu (roles, tài khoản admin/driver1, Trip Id=1)
using (var scope = app.Services.CreateScope())
{
    var startupLog = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
    var db = scope.ServiceProvider.GetRequiredService<TruckingSystem.Infrastructure.Persistence.AppDbContext>();
    List<string> pending;
    try
    {
        pending = (await Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions.GetPendingMigrationsAsync(db.Database)).ToList();
    }
    catch (Exception ex)
    {
        startupLog.LogCritical("KHÔNG KẾT NỐI ĐƯỢC DATABASE: {Message}\n" +
            "-> Kiểm tra PostgreSQL đã chạy chưa và chuỗi ConnectionStrings:DefaultConnection trong appsettings.json.", ex.Message);
        return;
    }
    if (pending.Count > 0)
    {
        startupLog.LogCritical("DATABASE CHƯA CẬP NHẬT — còn {Count} migration chưa chạy:\n  {List}\n" +
            "-> Mở Package Manager Console và chạy:\n" +
            "   Update-Database -Project TruckingSystem.Infrastructure -StartupProject TruckingSystem.API\n" +
            "   rồi chạy lại API.", pending.Count, string.Join("\n  ", pending));
        return;
    }

    try
    {
        await TruckingSystem.API.Data.DbSeeder.SeedAsync(scope.ServiceProvider);
    }
    catch (Exception ex)
    {
        // Seed chỉ tạo dữ liệu mẫu — lỗi ở đây không được làm sập cả API.
        startupLog.LogError(ex, "Seed dữ liệu mẫu lỗi (API vẫn chạy tiếp): {Message}", ex.Message);
    }
}

app.Run();
