using TruckingSystem.Domain.Common;

namespace TruckingSystem.Domain.Entities;

public class FuelPriceHistory : BaseEntity
{
    public string FuelType { get; set; } = string.Empty; // Dầu DO, Xăng RON95...
    public decimal Price { get; set; }
    public DateTime EffectiveDate { get; set; }

    // Tùy chọn — null nếu chỉ ghi giá thị trường chung, có giá trị nếu ghi giá
    // theo đúng 1 nhà cung cấp cụ thể.
    public int? FuelProviderId { get; set; }
    public FuelProvider? FuelProvider { get; set; }
}
