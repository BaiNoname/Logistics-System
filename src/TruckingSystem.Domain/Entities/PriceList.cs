using TruckingSystem.Domain.Common;

namespace TruckingSystem.Domain.Entities;

public class PriceList : BaseEntity
{
    public void EnsureEditable()
    {
        if (Status != PriceListStatus.Active)
            throw new InvalidOperationException("Không thể sửa bảng giá đã ngừng áp dụng.");
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

public enum PriceListStatus { Active, Inactive }
