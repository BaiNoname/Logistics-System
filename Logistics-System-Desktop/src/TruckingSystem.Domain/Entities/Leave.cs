using TruckingSystem.Domain.Common;
using TruckingSystem.Domain.Enums;

namespace TruckingSystem.Domain.Entities;

/// <summary>Ngày nghỉ lễ chung của công ty — mọi nhân viên được nghỉ hưởng lương ngày này.</summary>
public class PublicHoliday : BaseEntity
{
    public DateTime Date { get; set; }
    public string Name { get; set; } = "";
    public string CreatedBy { get; set; } = "";
}

/// <summary>1 ngày nghỉ của 1 nhân viên. Một lần nhập nhiều ngày liên tiếp dùng chung BatchId để thu hồi cả đợt.</summary>
public class LeaveRecord : BaseEntity
{
    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    public DateTime LeaveDate { get; set; }
    public LeaveType Type { get; set; }
    public Guid BatchId { get; set; }
    public string Note { get; set; } = "";
    public string CreatedBy { get; set; } = "";
}
