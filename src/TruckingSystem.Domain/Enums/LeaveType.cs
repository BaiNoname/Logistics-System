namespace TruckingSystem.Domain.Enums;

/// <summary>Loại ngày nghỉ. Thuộc tính từng loại do <c>LeaveRules</c> quy định (không lưu DB).</summary>
public enum LeaveType
{
    AnnualLeave,
    PersonalWithQuota,
    SickWithQuota,
    OwnMarriage,
    ChildMarriage,
    CloseBereavement,
    RelativeEvent,
    WorkAccident,
    Maternity,
    PaternityLeave,
    SickInsurance,
    UnpaidPersonal
}

/// <summary>Nhóm loại nghỉ (dùng để gom hiển thị).</summary>
public enum LeaveGroup { AnnualQuota, PaidSpecial, UnpaidInsurance, Unpaid }
