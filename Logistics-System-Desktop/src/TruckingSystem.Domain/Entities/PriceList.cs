using TruckingSystem.Domain.Common;

namespace TruckingSystem.Domain.Entities;

public class PriceList : BaseEntity
{
    public PriceListStatus StatusOn(DateTime today, int? activeId)
    {
        if (Status is PriceListStatus.Cancelled or PriceListStatus.Inactive) return Status;
        if (EffectiveDate.Date > today.Date) return PriceListStatus.Pending;
        return Id == activeId ? PriceListStatus.Active : PriceListStatus.Inactive;
    }

    public void EnsureEditable(DateTime today)
    {
        if (Status is not (PriceListStatus.Active or PriceListStatus.Pending) || EffectiveDate.Date <= today.Date)
            throw new InvalidOperationException("Chỉ được sửa hoặc hủy bảng giá chưa áp dụng.");
    }

    public static void Validate(string? name, DateTime effectiveDate, DateTime today)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Vui lòng nhập tên bảng giá.");
        if (effectiveDate == default)
            throw new ArgumentException("Vui lòng chọn ngày hiệu lực.");
        if (effectiveDate.Date < today.Date)
            throw new ArgumentException("Ngày hiệu lực không được trước ngày hiện tại.");
    }

    public string Name { get; set; } = string.Empty;
    public DateTime EffectiveDate { get; set; }
    public PriceListStatus Status { get; set; } = PriceListStatus.Active;

    public ICollection<TariffRule> TariffRules { get; set; } = new List<TariffRule>();
}

public enum PriceListStatus { Active = 0, Inactive = 1, Pending = 3, Cancelled = 4 }
