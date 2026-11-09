using TruckingSystem.Domain.Common;

namespace TruckingSystem.Domain.Entities;

/// <summary>Nhà cung cấp nhiên liệu (Petrolimex, PVOil...) — gắn với FuelPriceHistory
/// để biết mức giá đó áp dụng theo nhà cung cấp nào, khớp đúng mô tả gốc "Fuel Provider/
/// Price History theo thời gian" từ hệ thống Odoo cũ.</summary>
public class FuelProvider : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public string? Address { get; set; }

    public ICollection<FuelPriceHistory> PriceHistories { get; set; } = new List<FuelPriceHistory>();
}
