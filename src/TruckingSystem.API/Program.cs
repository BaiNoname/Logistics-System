using TruckingSystem.Application;
using TruckingSystem.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // phục vụ wwwroot/track.html cho trang tracking công khai
app.UseCors("ReactApp");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Seed dữ liệu mẫu khi khởi động lần đầu (roles, tài khoản admin/driver1, Trip Id=1)
using (var scope = app.Services.CreateScope())
{
    await TruckingSystem.API.Data.DbSeeder.SeedAsync(scope.ServiceProvider);
}

app.Run();
