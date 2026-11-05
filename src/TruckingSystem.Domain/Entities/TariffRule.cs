using TruckingSystem.Domain.Common;

namespace TruckingSystem.Domain.Entities;

public class TariffRule : BaseEntity
{
    public static IReadOnlyList<string> DefaultVehicleTypes { get; } = Array.AsReadOnly(new[]
    {
        "Xe van 1 tấn", "Xe tải 1,5 tấn", "Xe tải 2,5 tấn", "Xe tải 3 tấn", "Xe tải 5 tấn",
        "Xe tải 8 tấn", "Xe tải 15 tấn", "Đầu kéo container 20 feet", "Đầu kéo container 40 feet"
    });

    public const decimal MaximumPrice = 9999999999999999.99m;

    public static void Validate(string? vehicleType, decimal pricePerKm, decimal pricePerTon,
        decimal surchargeFragile, decimal surchargeHazmat)
    {
        if (string.IsNullOrWhiteSpace(vehicleType))
            throw new ArgumentException("Vui lòng nhập hoặc chọn loại xe.");
        foreach (var price in new[] { pricePerKm, pricePerTon, surchargeFragile, surchargeHazmat })
        {
            if (price < 0 || price > MaximumPrice)
                throw new ArgumentException("Đơn giá và phụ phí phải từ 0 đến 9.999.999.999.999.999,99.");
            if (decimal.Round(price, 2) != price)
                throw new ArgumentException("Đơn giá và phụ phí chỉ được có tối đa 2 chữ số thập phân.");
        }
    }

    public int PriceListId { get; set; }
    public PriceList PriceList { get; set; } = null!;

    public string VehicleType { get; set; } = string.Empty;
    public decimal PricePerKm { get; set; }
    public decimal PricePerTon { get; set; }
    public decimal SurchargeFragile { get; set; }
    public decimal SurchargeHazmat { get; set; }
}
