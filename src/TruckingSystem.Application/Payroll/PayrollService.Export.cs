using System.Globalization;
using System.Net;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace TruckingSystem.Application.Payroll;

public partial class PayrollService
{
    public async Task<string?> ExportSlipAsync(int slipId)
    {
        var slip = await db.PayrollSlips.AsNoTracking().Include(s => s.PayrollPeriod)
            .Include(s => s.Items).Include(s => s.Payments).FirstOrDefaultAsync(s => s.Id == slipId);
        if (slip is null) return null;
        var s = ToDto(slip);
        var period = slip.PayrollPeriod;
        static string H(string? text) => WebUtility.HtmlEncode(text ?? "");
        static string Money(decimal value) => value.ToString("N0", CultureInfo.GetCultureInfo("vi-VN")) + " đ";
        static string Kind(string value) => value switch { "Allowance" => "Phụ cấp", "Bonus" => "Thưởng", "Overtime" => "Tăng ca", _ => "Khấu trừ" };
        var html = new StringBuilder($$"""
            <!doctype html><html lang="vi"><meta charset="utf-8"><title>Phiếu lương {{H(s.EmployeeCode)}}</title>
            <style>body{font:15px 'Segoe UI',sans-serif;max-width:900px;margin:32px auto;color:#0f172a;padding:16px}h1{font-size:24px}table{border-collapse:collapse;width:100%;margin:16px 0}th,td{border:1px solid #cbd5e1;padding:8px;text-align:left;overflow-wrap:anywhere}th{background:#f1f5f9}td.money{text-align:right}tr{break-inside:avoid}@media print{body{margin:0;max-width:none} }</style>
            <h1>PHIẾU LƯƠNG THÁNG {{period.Month:00}}/{{period.Year}}</h1>
            <p>{{(s.IsDriver ? "TÀI XẾ — LỊCH ĐĂNG KÝ" : "NHÂN VIÊN — CÔNG TỔNG HỢP")}}<br>Kỳ công: {{(s.WorkFrom ?? new DateTime(period.Year, period.Month, 1)):dd/MM/yyyy}}–{{(s.WorkTo ?? new DateTime(period.Year, period.Month, 1).AddMonths(1).AddDays(-1)):dd/MM/yyyy}}</p>
            <p>{{(period.ApprovedAt.HasValue ? "ĐÃ DUYỆT" : "BẢN NHÁP – CHƯA DUYỆT")}} · Phiếu #{{s.Id}}</p>
            <p><strong>{{H(s.EmployeeCode)}} — {{H(s.EmployeeName)}}</strong><br>Phòng ban: {{H(s.DepartmentName)}} · Chức danh: {{H(s.PositionName)}}</p>
            <p>Lương tháng: {{Money(s.MonthlySalary)}} · Hiệu lực: {{s.SalaryEffectiveFrom:dd/MM/yyyy}}<br>
            Công chuẩn: {{s.StandardDays}} · Làm việc: {{s.WorkedDays}} · Nghỉ hưởng lương: {{s.PaidLeaveDays}} · Lễ hưởng lương: {{s.HolidayDays}} · Nghỉ không lương: {{s.UnpaidLeaveDays}}<br>
            Tăng ca (giờ) — ngày thường: {{s.WeekdayOvertimeHours}} · ngày nghỉ: {{s.RestDayOvertimeHours}} · ngày lễ: {{s.HolidayOvertimeHours}}<br>
            Xác nhận công: {{H(s.AttendanceConfirmedBy)}} · {{s.AttendanceConfirmedAt.AddHours(7):dd/MM/yyyy HH:mm}} (VN)<br>{{H(s.AttendanceNote)}}</p>
            <table><thead><tr><th>Khoản</th><th>Căn cứ / lý do</th><th>Cách tính</th><th>Số tiền</th></tr></thead><tbody>
            <tr><td>Lương theo công</td><td>Lương tháng / công chuẩn × (làm + nghỉ hưởng lương + lễ)</td><td>Theo công</td><td class="money">{{Money(s.EarnedSalary)}}</td></tr>
            """);
        if (s.IsDriver)
            html.Append($"<tr><td>Ngày công tài xế</td><td colspan=\"3\">Chốt ngày 25; mỗi ngày đăng ký tính 1 công. {H(s.DriverWorkDates)}</td></tr>");
        foreach (var item in s.Items)
            html.Append($"<tr><td>{Kind(item.Kind)}{(item.IsAdjustment ? " (điều chỉnh sau duyệt)" : "")}</td><td>{H(item.Reason)}</td><td>{(item.ProrateByAttendance ? Money(item.Amount) + " × công hưởng / công chuẩn" : "Cố định")}</td><td class=\"money\">{(item.Kind == "Deduction" ? "−" : "+")}{Money(item.CalculatedAmount)}</td></tr>");
        html.Append($"</tbody></table><p>Tổng thu nhập: <strong>{Money(s.GrossIncome)}</strong> · Khấu trừ: {Money(s.Deductions)}<br>Thực lĩnh: <strong>{Money(s.NetPay)}</strong><br>");
        if (s.ApprovedNetPay.HasValue) html.Append($"Thực lĩnh tại lần duyệt gốc: {Money(s.ApprovedNetPay.Value)}<br>");
        html.Append($"Đã ứng/trả được phân bổ: {Money(s.PaidAmount)} · <strong>Còn phải trả: {Money(s.RemainingAmount)}</strong></p>");
        html.Append("<h2>Lịch sử chi trả</h2><table><thead><tr><th>Ngày</th><th>Hình thức</th><th>Chứng từ</th><th>Số tiền</th></tr></thead><tbody>");
        foreach (var payment in s.Payments)
            html.Append($"<tr><td>{payment.PaidOn:dd/MM/yyyy}</td><td>{(payment.Method == "Cash" ? "Tiền mặt" : "Chuyển khoản")}</td><td>{H(payment.Reference)}</td><td class=\"money\">{Money(payment.Amount)}</td></tr>");
        html.Append($"</tbody></table><p>Người duyệt: {H(period.ApprovedBy)} · {period.ApprovedAt?.AddHours(7):dd/MM/yyyy HH:mm} (VN)</p><p>Tiền được làm tròn đến đồng, điểm giữa làm tròn ra xa 0. Các khoản thuế/bảo hiểm do kế toán xác định.</p></html>");
        return html.ToString();
    }
}
