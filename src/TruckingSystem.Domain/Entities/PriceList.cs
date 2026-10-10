using TruckingSystem.Domain.Common;

namespace TruckingSystem.Domain.Entities;

public class PriceList : BaseEntity
{
    public static void Validate(string? name, DateTime effectiveDate)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Vui lòng nhập tên bảng giá.");
        if (effectiveDate == default)
            throw new ArgumentException("Vui lòng chọn ngày hiệu lực.");
    }

    public string Name { get; set; } = string.Empty;
    public DateTime EffectiveDate { get; set; }

    public ICollection<TariffRule> TariffRules { get; set; } = new List<TariffRule>();
}
