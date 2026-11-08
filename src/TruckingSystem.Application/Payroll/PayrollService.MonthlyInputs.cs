using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TruckingSystem.Domain.Entities;
using TruckingSystem.Domain.Payroll;

namespace TruckingSystem.Application.Payroll;

public partial class PayrollService
{
    public async Task<List<KpiPolicyDto>> GetKpiPoliciesAsync()
        => (await db.PayrollKpiPolicies.AsNoTracking().OrderByDescending(p => p.EffectiveFrom).ToListAsync())
            .Select(p => new KpiPolicyDto(p.Id, p.EffectiveFrom, p.KmPerPoint, p.TargetPoints,
                JsonSerializer.Deserialize<List<KpiFactorDto>>(p.FactorsJson)!)).ToList();

    public Task<PayrollResult> SaveKpiPolicyAsync(SaveKpiPolicyRequest r, string actor) => WriteAsync(async () =>
    {
        if (!ValidPeriod(r.EffectiveFrom.Year, r.EffectiveFrom.Month) || r.EffectiveFrom.Day != 1
            || r.KmPerPoint <= 0 || r.KmPerPoint > 1_000_000 || !PayrollRules.ValidQuantity(r.KmPerPoint)
            || r.TargetPoints <= 0 || r.TargetPoints > 1_000_000 || !PayrollRules.ValidQuantity(r.TargetPoints)
            || r.Factors is null || r.Factors.Count == 0 || r.Factors.Any(f => f is null || string.IsNullOrWhiteSpace(f.VehicleType)
                || f.VehicleType.Length > 100 || f.Factor <= 0 || f.Factor > 1000 || !PayrollRules.ValidQuantity(f.Factor)))
            return Fail("Cấu hình hiệu lực từ ngày 1; K và điểm chuẩn phải dương, tối đa 1.000.000; H dương, tối đa 1.000; tối đa 2 số lẻ.");
        if (r.Factors.Select(f => f.VehicleType).Distinct().Count() != r.Factors.Count) return Fail("Loại xe bị trùng.");
        var policy = await db.PayrollKpiPolicies.SingleOrDefaultAsync(p => p.EffectiveFrom == r.EffectiveFrom.Date);
        if (policy is null) { policy = new PayrollKpiPolicy { EffectiveFrom = r.EffectiveFrom.Date }; db.PayrollKpiPolicies.Add(policy); }
        policy.KmPerPoint = r.KmPerPoint;
        policy.TargetPoints = r.TargetPoints;
        policy.FactorsJson = JsonSerializer.Serialize(r.Factors.OrderBy(f => f.VehicleType));
        policy.UpdatedAt = DateTime.UtcNow;
        Audit(actor, "SaveKpiPolicy", "PayrollKpiPolicy", null, r);
        return new(true);
    });

    public Task<List<MonthlyAdjustmentDto>> GetMonthlyAdjustmentsAsync(int year, int month)
        => db.PayrollMonthlyAdjustments.AsNoTracking().Where(a => a.Year == year && a.Month == month)
            .OrderBy(a => a.Id).Select(a => new MonthlyAdjustmentDto(a.Id, a.EmployeeId, a.Employee.FullName,
                a.Year, a.Month, a.Kind.ToString(), a.Amount, a.Reason)).ToListAsync();

    public Task<PayrollResult> AddMonthlyAdjustmentAsync(MonthlyAdjustmentRequest r, string actor) => WriteAsync(async () =>
    {
        if (!ValidPeriod(r.Year, r.Month) || !PayrollRules.ValidMoney(r.Amount) || !ValidReason(r.Reason)
            || !Enum.TryParse<PayrollItemKind>(r.Kind, out var kind) || !Enum.IsDefined(kind)) return Fail("Khoản cộng/trừ không hợp lệ.");
        var period = await db.PayrollPeriods.SingleOrDefaultAsync(p => p.Year == r.Year && p.Month == r.Month);
        if (period?.ApprovedAt is not null) return Fail("Bảng đã chốt; ghi khoản điều chỉnh ở tháng chưa chốt, kèm căn cứ tháng gốc.");
        if (!(await GetEmployeesAsync(r.Year, r.Month)).Any(e => e.Id == r.EmployeeId)) return Fail("Nhân viên không thuộc tháng này.");
        db.PayrollMonthlyAdjustments.Add(new PayrollMonthlyAdjustment { EmployeeId = r.EmployeeId, Year = r.Year,
            Month = r.Month, Kind = kind, Amount = r.Amount, Reason = r.Reason.Trim(), CreatedBy = actor });
        if (period is not null) Touch(period);
        Audit(actor, "AddMonthlyAdjustment", "PayrollMonthlyAdjustment", null, r);
        return new(true);
    });

    public Task<PayrollResult> AddMonthlyAdjustmentsBulkAsync(BulkMonthlyAdjustmentRequest r, string actor) => WriteAsync(async () =>
    {
        if (r.EmployeeIds is null || r.EmployeeIds.Count == 0) return Fail("Chưa chọn tài xế nào.");
        if (r.EmployeeIds.Distinct().Count() != r.EmployeeIds.Count) return Fail("Danh sách tài xế bị trùng.");
        if (!ValidPeriod(r.Year, r.Month) || !PayrollRules.ValidMoney(r.Amount) || !ValidReason(r.Reason)
            || !Enum.TryParse<PayrollItemKind>(r.Kind, out var kind) || (kind != PayrollItemKind.Allowance && kind != PayrollItemKind.Bonus))
            return Fail("Khoản cộng/trừ không hợp lệ.");
        var period = await db.PayrollPeriods.SingleOrDefaultAsync(p => p.Year == r.Year && p.Month == r.Month);
        if (period?.ApprovedAt is not null) return Fail("Bảng đã chốt; ghi khoản điều chỉnh ở tháng chưa chốt, kèm căn cứ tháng gốc.");
        var employees = (await GetEmployeesAsync(r.Year, r.Month)).ToDictionary(e => e.Id);
        var errors = new List<string>();
        foreach (var id in r.EmployeeIds)
        {
            if (!employees.TryGetValue(id, out var emp)) errors.Add($"{id}: không thuộc tháng này");
            else if (!emp.IsDriver) errors.Add($"{emp.Code}: không phải tài xế");
        }
        if (errors.Count > 0) return Fail(string.Join(Environment.NewLine, errors));
        foreach (var id in r.EmployeeIds)
            db.PayrollMonthlyAdjustments.Add(new PayrollMonthlyAdjustment { EmployeeId = id, Year = r.Year,
                Month = r.Month, Kind = kind, Amount = r.Amount, Reason = r.Reason.Trim(), CreatedBy = actor });
        if (period is not null) Touch(period);
        Audit(actor, "AddMonthlyAdjustmentsBulk", "PayrollMonthlyAdjustment", null,
            new { r.EmployeeIds, r.Year, r.Month, r.Kind, r.Amount, r.Reason });
        return new(true, $"Đã thêm khoản cho {r.EmployeeIds.Count} tài xế.");
    });

    public Task<PayrollResult> DeleteMonthlyAdjustmentAsync(int id, string actor) => WriteAsync(async () =>
    {
        var row = await db.PayrollMonthlyAdjustments.FindAsync(id);
        if (row is null) return Fail("Không tìm thấy khoản cộng/trừ.");
        var period = await db.PayrollPeriods.SingleOrDefaultAsync(p => p.Year == row.Year && p.Month == row.Month);
        if (period?.ApprovedAt is not null) return Fail("Không sửa khoản của tháng đã chốt.");
        db.PayrollMonthlyAdjustments.Remove(row);
        if (period is not null) Touch(period);
        Audit(actor, "DeleteMonthlyAdjustment", "PayrollMonthlyAdjustment", id, new { row.EmployeeId, row.Amount, row.Reason });
        return new(true);
    });
}
