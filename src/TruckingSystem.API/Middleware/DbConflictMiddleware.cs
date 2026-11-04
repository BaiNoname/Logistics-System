using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace TruckingSystem.API.Middleware;

/// <summary>Đổi lỗi RÀNG BUỘC DỮ LIỆU của database thành phản hồi 409 có câu tiếng
/// Việt dễ hiểu (thay vì lỗi 500 khó hiểu):
///   - 2 người sửa cùng 1 dữ liệu cùng lúc (cột xmin) -> "vừa bị thay đổi, tải lại".
///   - Trùng dữ liệu duy nhất (mã đơn, biển số, mã vị trí, giá cùng ngày...).
///   - Xóa dữ liệu đang được nơi khác dùng (khóa ngoại Restrict).
///   - Vi phạm điều kiện (tồn kho âm...).
/// Desktop (ApiConnection) tự đọc trường "message" và hiện cho người dùng.</summary>
public class DbConflictMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<DbConflictMiddleware> _logger;

    public DbConflictMiddleware(RequestDelegate next, ILogger<DbConflictMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict on {Path}", context.Request.Path);
            await WriteAsync(context, "Dữ liệu vừa bị người khác thay đổi cùng lúc — hãy tải lại màn hình rồi thử lại.");
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg)
        {
            _logger.LogWarning(ex, "Database constraint {Code} ({Constraint}) on {Path}", pg.SqlState, pg.ConstraintName, context.Request.Path);
            var message = pg.SqlState switch
            {
                PostgresErrorCodes.UniqueViolation => UniqueMessage(pg.ConstraintName),
                PostgresErrorCodes.ForeignKeyViolation => "Không thể thực hiện — dữ liệu này đang được dữ liệu khác sử dụng (đơn hàng, chuyến, phiếu kho...).",
                PostgresErrorCodes.CheckViolation => "Dữ liệu không hợp lệ (số lượng/tồn kho không được âm) — hãy tải lại và kiểm tra.",
                PostgresErrorCodes.SerializationFailure or PostgresErrorCodes.DeadlockDetected => "Dữ liệu vừa thay đổi trong lúc xử lý — tải lại và thử lại thao tác.",
                _ => null,
            };
            if (message is null) throw;
            await WriteAsync(context, message);
        }
        catch (Exception ex) when (IsTransactionConflict(ex))
        {
            _logger.LogWarning(ex, "Concurrent transaction on {Path}", context.Request.Path);
            await WriteAsync(context, "Dữ liệu vừa thay đổi trong lúc xử lý — tải lại và thử lại thao tác.");
        }
    }

    private static bool IsTransactionConflict(Exception ex)
        => ex is PostgresException { SqlState: PostgresErrorCodes.SerializationFailure or PostgresErrorCodes.DeadlockDetected }
           || (ex.InnerException is not null && IsTransactionConflict(ex.InnerException));

    private static string UniqueMessage(string? constraint)
    {
        var c = constraint ?? "";
        if (c.Contains("PlateNumber")) return "Biển số xe này đã tồn tại.";
        if (c.Contains("TransportOrders") && c.Contains("Code")) return "Mã đơn bị trùng — hãy thử lưu lại.";
        if (c.Contains("StorageBins")) return "Mã vị trí đã có trong khu này.";
        if (c == "IX_SurchargeLists_EffectiveDate") return "Ngày hiệu lực này đã có bảng phụ phí được công bố.";
        if (c.Contains("FuelPrice")) return "Đã có giá cho loại nhiên liệu này trong cùng ngày.";
        if (c.Contains("DriverSchedules")) return "Tài xế đã đăng ký ca này trong ngày đó rồi.";
        if (c.Contains("TripVideoSegments")) return "Đoạn video này đã được nhận trước đó.";
        return "Dữ liệu bị trùng với dữ liệu đã có.";
    }

    private static async Task WriteAsync(HttpContext context, string message)
    {
        if (context.Response.HasStarted) return;
        context.Response.Clear();
        context.Response.StatusCode = StatusCodes.Status409Conflict;
        context.Response.ContentType = "application/json; charset=utf-8";
        await context.Response.WriteAsync(JsonSerializer.Serialize(new { success = false, message }));
    }
}
