using TruckingSystem.Application;
using TruckingSystem.Infrastructure;
using TruckingSystem.API.RateLimiting;

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
// Chạy được như DỊCH VỤ WINDOWS (bật máy là API tự chạy, không cần mở Visual Studio — xem installer\api_service_install.bat).
// Chạy bằng Visual Studio / gõ lệnh như bình thường thì dòng này không làm gì.
builder.Host.UseWindowsService();
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// ---- 2 dòng này thay thế cho hàng chục dòng cấu hình DbContext/Identity/JWT/Service
//      từng nằm rải rác trực tiếp trong Program.cs trước đây — giờ mỗi layer tự
//      "đóng gói" cách đăng ký DI của chính nó, Program.cs chỉ đứng vai trò
//      "composition root" lắp ráp lại, không cần biết chi tiết bên trong. ----
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddFreightChatRateLimiter();

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
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Kiểm tra database lúc khởi động: máy nào pull code mới mà quên Update-Database thì API VẪN CHẠY (không tắt —
// tắt thì Visual Studio chỉ báo "web server is no longer running", cửa sổ console đóng mất, không đọc được lý do),
// nhưng mọi lượt gọi trả về 503 kèm câu báo rõ ràng -> app Desktop hiện đúng câu đó (thay vì lỗi khó hiểu kiểu
// "42P01: relation \"MaxDiscountPercent\" does not exist"). Cửa sổ console cũng in danh sách migration còn thiếu.
// Seed dữ liệu mẫu khi khởi động lần đầu (roles, tài khoản admin/driver1, Trip Id=1) — chỉ khi database đã đủ.
string? startupProblem = null;
using (var scope = app.Services.CreateScope())
{
    var startupLog = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
    var db = scope.ServiceProvider.GetRequiredService<TruckingSystem.Infrastructure.Persistence.AppDbContext>();
    try
    {
        var pending = (await Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions.GetPendingMigrationsAsync(db.Database)).ToList();
        if (pending.Count > 0)
        {
            startupProblem = $"Database chưa cập nhật — còn {pending.Count} migration chưa chạy ({string.Join(", ", pending)}). " +
                             "Mở Package Manager Console chạy: Update-Database -Project TruckingSystem.Infrastructure " +
                             "-StartupProject TruckingSystem.API, rồi chạy lại API.";
            startupLog.LogCritical("{Problem}", startupProblem);
        }
    }
    catch (Exception ex)
    {
        startupProblem = "API không kết nối được database: " + ex.Message +
                         " — kiểm tra PostgreSQL đã chạy chưa và chuỗi ConnectionStrings:DefaultConnection trong appsettings.json.";
        startupLog.LogCritical("{Problem}", startupProblem);
    }

    if (startupProblem is null)
    {
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
}

if (startupProblem is not null)
{
    // Chạy trước controller (endpoint luôn ở cuối pipeline): trả câu báo cho mọi lượt gọi.
    app.Use(async (HttpContext context, Func<Task> _) =>
    {
        context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        context.Response.ContentType = "application/json; charset=utf-8";
        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new { success = false, message = startupProblem }));
    });
}

app.Run();
