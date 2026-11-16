using TruckingSystem.ChatBot.Application;
using TruckingSystem.ChatBot.Infrastructure;
using TruckingSystem.Infrastructure;

// Host riêng của chatbot tra cước (đóng gói tách khỏi TruckingSystem.API).
// Dùng chung DB với API chính: đọc bảng giá/phụ phí, ghi bảng FreightVectors. Không chạy DbSeeder
// và không quản lý migration — DB phải được API chính migrate trước.
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Chỉ đăng ký những gì chatbot cần: không có Identity/JWT và các service quản trị của API chính,
// nên host này không cần (và không được giữ) khóa ký token đăng nhập.
builder.Services.AddFreightChat();
builder.Services.AddInfrastructureCore(builder.Configuration);
builder.Services.AddFreightChatInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddFreightChatRateLimiter();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRateLimiter();
app.MapControllers();

app.Run();
