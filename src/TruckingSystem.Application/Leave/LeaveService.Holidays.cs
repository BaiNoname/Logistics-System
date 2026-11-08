using Microsoft.EntityFrameworkCore;
using TruckingSystem.Domain.Entities;

namespace TruckingSystem.Application.Leave;

public partial class LeaveService
{
    public async Task<List<HolidayDto>> GetHolidaysAsync(int year)
    {
        var start = new DateTime(year, 1, 1);
        var end = start.AddYears(1);
        return await db.PublicHolidays.AsNoTracking()
            .Where(h => h.Date >= start && h.Date < end)
            .OrderBy(h => h.Date).Select(h => new HolidayDto(h.Id, h.Date, h.Name)).ToListAsync();
    }

    public Task<LeaveResult> AddHolidayAsync(AddHolidayRequest request, string actor) => WriteAsync(async () =>
    {
        var name = (request.Name ?? "").Trim();
        if (name.Length == 0 || name.Length > 100) return Fail("Tên ngày lễ bắt buộc, tối đa 100 ký tự.");
        var date = DateTime.SpecifyKind(request.Date.Date, DateTimeKind.Unspecified);
        if (await db.PublicHolidays.AnyAsync(h => h.Date == date))
            return Fail($"Ngày {date:dd/MM/yyyy} đã có trong lịch nghỉ lễ.");
        if ((await ApprovedMonthsAsync(new[] { date })).Count > 0)
            return Fail($"Tháng {date:MM/yyyy} đã chốt lương.");
        var affected = await db.LeaveRecords.AsNoTracking().Where(l => l.LeaveDate == date)
            .Select(l => l.Employee.Code + " – " + l.Employee.FullName).Distinct().ToListAsync();
        if (affected.Count > 0)
            return Fail($"Ngày {date:dd/MM/yyyy} đã có đơn nghỉ của: {string.Join(", ", affected)}. Xóa các đơn nghỉ này trước.");

        var holiday = new PublicHoliday { Date = date, Name = name, CreatedBy = actor };
        db.PublicHolidays.Add(holiday);
        Audit(actor, "AddHoliday", "PublicHoliday", null, new { Date = date, Name = name });
        var warnings = date.DayOfWeek == DayOfWeek.Sunday
            ? new List<string> { "Ngày đã chọn là Chủ nhật; nhớ nhập thêm ngày nghỉ bù nếu có." } : null;
        return new(true, "Đã thêm ngày lễ.", warnings);
    });

    public Task<LeaveResult> DeleteHolidayAsync(int id, string actor) => WriteAsync(async () =>
    {
        var holiday = await db.PublicHolidays.FindAsync(id);
        if (holiday is null) return Fail("Không tìm thấy ngày lễ.");
        var date = holiday.Date.Date;
        if ((await ApprovedMonthsAsync(new[] { date })).Count > 0)
            return Fail($"Tháng {date:MM/yyyy} đã chốt lương.");
        db.PublicHolidays.Remove(holiday);
        Audit(actor, "DeleteHoliday", "PublicHoliday", id, new { holiday.Date, holiday.Name });
        return new(true, "Đã xóa ngày lễ.");
    });
}
