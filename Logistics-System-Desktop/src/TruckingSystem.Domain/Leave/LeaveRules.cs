using TruckingSystem.Domain.Enums;

namespace TruckingSystem.Domain.Leave;

/// <summary>Số ngày công trong một khoảng, phân loại theo nguồn (chấm công, lễ, nghỉ có/không lương).</summary>
public record PayrollDayBreakdown(
    decimal WorkedDays, decimal HolidayDays, decimal PaidLeaveDays, decimal UnpaidLeaveDays,
    IReadOnlyDictionary<LeaveType, int> LeaveCounts,
    IReadOnlyList<DateTime> HolidayDates,
    IReadOnlyList<(DateTime Date, LeaveType Type)> LeaveDates);

/// <summary>Quy tắc nghiệp vụ ngày nghỉ (thuần, không phụ thuộc hạ tầng).</summary>
public static class LeaveRules
{
    /// <summary>Quỹ phép năm cố định mỗi năm dương lịch.</summary>
    public const int AnnualQuotaDays = 12;
    /// <summary>Khoảng ngày tối đa cho một lần thêm.</summary>
    public const int MaxRangeDays = 366;

    public static bool IsPaid(LeaveType t) => t switch
    {
        LeaveType.AnnualLeave or LeaveType.PersonalWithQuota or LeaveType.SickWithQuota
            or LeaveType.OwnMarriage or LeaveType.ChildMarriage or LeaveType.CloseBereavement
            or LeaveType.RelativeEvent or LeaveType.WorkAccident => true,
        _ => false
    };

    public static bool DeductsAnnualQuota(LeaveType t)
        => t is LeaveType.AnnualLeave or LeaveType.PersonalWithQuota or LeaveType.SickWithQuota;

    public static int MinDaysPerRequest(LeaveType t) => t == LeaveType.PaternityLeave ? 5 : 1;

    /// <summary>null = không giới hạn (Thai sản giới hạn theo tháng lịch, kiểm tra riêng).</summary>
    public static int? MaxDaysPerRequest(LeaveType t) => t switch
    {
        LeaveType.OwnMarriage => 3,
        LeaveType.ChildMarriage => 1,
        LeaveType.CloseBereavement => 3,
        LeaveType.RelativeEvent => 1,
        LeaveType.PaternityLeave => 14,
        _ => null
    };

    public static bool RequiresFemale(LeaveType t) => t == LeaveType.Maternity;

    public static LeaveGroup Group(LeaveType t) => t switch
    {
        LeaveType.AnnualLeave or LeaveType.PersonalWithQuota or LeaveType.SickWithQuota => LeaveGroup.AnnualQuota,
        LeaveType.OwnMarriage or LeaveType.ChildMarriage or LeaveType.CloseBereavement
            or LeaveType.RelativeEvent or LeaveType.WorkAccident => LeaveGroup.PaidSpecial,
        LeaveType.Maternity or LeaveType.PaternityLeave or LeaveType.SickInsurance => LeaveGroup.UnpaidInsurance,
        _ => LeaveGroup.Unpaid
    };

    public static string DisplayName(LeaveType t) => t switch
    {
        LeaveType.AnnualLeave => "Nghỉ phép năm",
        LeaveType.PersonalWithQuota => "Việc riêng (trừ phép)",
        LeaveType.SickWithQuota => "Ốm (trừ phép)",
        LeaveType.OwnMarriage => "Bản thân kết hôn",
        LeaveType.ChildMarriage => "Con đẻ, con nuôi kết hôn",
        LeaveType.CloseBereavement => "Cha/mẹ (đẻ, nuôi, bên vợ/chồng), vợ/chồng, con mất",
        LeaveType.RelativeEvent => "Ông bà, anh chị em ruột mất; cha/mẹ, anh chị em ruột kết hôn",
        LeaveType.WorkAccident => "Tai nạn lao động, bệnh nghề nghiệp",
        LeaveType.Maternity => "Thai sản",
        LeaveType.PaternityLeave => "Vợ sinh con",
        LeaveType.SickInsurance => "Ốm (BHXH chi trả)",
        LeaveType.UnpaidPersonal => "Việc riêng không lương",
        _ => t.ToString()
    };

    /// <summary>Mô tả giới hạn của loại nghỉ để hiển thị trên form.</summary>
    public static string LimitText(LeaveType t) => t switch
    {
        LeaveType.AnnualLeave or LeaveType.PersonalWithQuota or LeaveType.SickWithQuota
            => $"Trừ quỹ {AnnualQuotaDays} ngày phép năm",
        LeaveType.Maternity => "Chỉ nhân viên nữ; tối đa 6 tháng/đơn",
        LeaveType.PaternityLeave => "Từ 5 đến 14 ngày/đơn",
        _ => MaxDaysPerRequest(t) is int m ? $"Tối đa {m} ngày/đơn" : "Không giới hạn"
    };

    /// <summary>Các ngày trong [from, to] (bỏ phần giờ); Chủ nhật và ngày lễ được đưa vào danh sách Skipped.</summary>
    public static (List<DateTime> Days, List<DateTime> Skipped) ExpandWorkingDays(
        DateTime from, DateTime to, IReadOnlySet<DateTime> holidays)
    {
        var days = new List<DateTime>();
        var skipped = new List<DateTime>();
        for (var d = from.Date; d <= to.Date; d = d.AddDays(1))
        {
            if (d.DayOfWeek == DayOfWeek.Sunday || holidays.Contains(d)) skipped.Add(d);
            else days.Add(d);
        }
        return (days, skipped);
    }

    /// <summary>Kiểm tra đơn nghỉ; trả về lý do từ chối hoặc null nếu hợp lệ.</summary>
    public static string? ValidateRequest(LeaveType type, DateTime from, DateTime to, int workingDays, EmployeeGender? gender)
    {
        if (from.Date > to.Date) return "Ngày bắt đầu không được sau ngày kết thúc.";
        if ((to.Date - from.Date).TotalDays + 1 > MaxRangeDays) return $"Khoảng nghỉ không được quá {MaxRangeDays} ngày.";
        if (workingDays == 0) return "Khoảng đã chọn không có ngày làm việc nào.";
        if (RequiresFemale(type) && gender != EmployeeGender.Female)
            return $"{DisplayName(type)} chỉ áp dụng cho nhân viên nữ.";
        if (type == LeaveType.Maternity && to.Date > from.Date.AddMonths(6).AddDays(-1))
            return "Thai sản tối đa 6 tháng cho mỗi đơn.";
        var min = MinDaysPerRequest(type);
        if (workingDays < min) return $"{DisplayName(type)} phải nghỉ tối thiểu {min} ngày/đơn.";
        if (MaxDaysPerRequest(type) is int max && workingDays > max)
            return $"{DisplayName(type)} chỉ được nghỉ tối đa {max} ngày/đơn.";
        return null;
    }

    /// <summary>Kiểm tra quỹ phép năm theo từng năm dương lịch của các ngày mới.</summary>
    public static List<string> QuotaErrors(IReadOnlyDictionary<int, int> usedByYear, IEnumerable<DateTime> newDays)
    {
        var errors = new List<string>();
        foreach (var g in newDays.GroupBy(d => d.Year).OrderBy(g => g.Key))
        {
            var used = usedByYear.TryGetValue(g.Key, out var u) ? u : 0;
            var count = g.Count();
            if (used + count > AnnualQuotaDays)
                errors.Add($"Năm {g.Key}: đã dùng {used}/{AnnualQuotaDays} ngày phép, thêm {count} ngày sẽ vượt quỹ.");
        }
        return errors;
    }

    /// <summary>
    /// Phân loại từng ngày trong [from, to] để tính lương: chấm công được ưu tiên (kể cả Chủ nhật);
    /// ngược lại bỏ Chủ nhật, rồi ngày lễ, rồi đơn nghỉ; còn lại là vắng.
    /// </summary>
    public static PayrollDayBreakdown ClassifyPayrollDays(DateTime from, DateTime to,
        IReadOnlyDictionary<DateTime, decimal> attendancePaidDays, IReadOnlySet<DateTime> holidays,
        IReadOnlyDictionary<DateTime, LeaveType> leaves)
    {
        decimal worked = 0, holiday = 0, paidLeave = 0, unpaidLeave = 0;
        var counts = new Dictionary<LeaveType, int>();
        var holidayDates = new List<DateTime>();
        var leaveDates = new List<(DateTime Date, LeaveType Type)>();

        for (var d = from.Date; d <= to.Date; d = d.AddDays(1))
        {
            if (attendancePaidDays.TryGetValue(d, out var paid)) { worked += paid; continue; }
            if (d.DayOfWeek == DayOfWeek.Sunday) continue;
            if (holidays.Contains(d)) { holiday++; holidayDates.Add(d); continue; }
            if (leaves.TryGetValue(d, out var type))
            {
                if (IsPaid(type)) paidLeave++; else unpaidLeave++;
                counts[type] = counts.TryGetValue(type, out var c) ? c + 1 : 1;
                leaveDates.Add((d, type));
            }
        }
        return new PayrollDayBreakdown(worked, holiday, paidLeave, unpaidLeave, counts, holidayDates, leaveDates);
    }
}
