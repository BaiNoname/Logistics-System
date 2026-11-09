using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruckingSystem.Application.Reports;

namespace TruckingSystem.API.Controllers;

[ApiController]
[Route("api/reports")]
// Báo cáo lãi/lỗ, doanh thu: chỉ Quản trị / Điều phối / Kế toán (trước đây mọi tài
// khoản đăng nhập — kể cả Tài xế, Thủ kho — đều xem được số liệu tài chính).
[Authorize(Roles = "Admin,Dispatcher,Accountant")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    /// <summary>Báo cáo lãi/lỗ cho các đơn tạo trong kỳ. VD: /api/reports/profit?from=2026-08-01&amp;to=2026-10-31</summary>
    [HttpGet("profit")]
    public async Task<ActionResult<ProfitReportDto>> GetProfit([FromQuery] DateTime from, [FromQuery] DateTime to)
        => Ok(await _reportService.GetProfitReportAsync(from, to));

    /// <summary>Chi tiết từng khoản chi của 1 đơn.</summary>
    [HttpGet("orders/{orderId:int}/costs")]
    public async Task<ActionResult<OrderCostDetailDto>> GetOrderCosts(int orderId)
    {
        var detail = await _reportService.GetOrderCostDetailAsync(orderId);
        return detail is null ? NotFound() : Ok(detail);
    }
}
