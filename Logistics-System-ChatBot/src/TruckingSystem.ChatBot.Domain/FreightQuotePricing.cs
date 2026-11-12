namespace TruckingSystem.Domain.Entities;

/// <summary>Một lô hàng cần báo giá: km từng chặng (lấy hàng nếu có, giao hàng), tổng khối lượng, loại hàng.</summary>
public record FreightQuoteInput(IReadOnlyList<decimal> LegKm, decimal WeightKg, bool IsFragile, bool IsHazmat);

/// <summary>Một khoản tiền trong báo giá: tên, cách tính (đã định dạng vi-VN), số tiền.</summary>
public record FreightQuoteLine(string Name, string Detail, decimal Amount);

/// <summary>Cước tham khảo cho chatbot; cùng quy tắc với hóa đơn nhưng không thay đổi nghiệp vụ hóa đơn.</summary>
public static class FreightQuotePricing
{
    private static readonly System.Globalization.CultureInfo Vi = System.Globalization.CultureInfo.GetCultureInfo("vi-VN");

    /// <summary>
    /// Báo giá một lô hàng giống hóa đơn hệ thống: tổng km các chặng × giá/km, khối lượng × giá/tấn (một lần),
    /// phụ phí dễ vỡ/nguy hiểm theo % trên (cước km + cước tấn) hoặc phí cố định khi chưa có bảng phụ phí chung.
    /// </summary>
    public static IReadOnlyList<FreightQuoteLine> Calculate(FreightQuoteInput input, TariffRule tariff, SurchargePolicy? policy = null)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(tariff);
        if (input.LegKm is null || input.LegKm.Count is < 1 or > 2)
            throw new ArgumentException("Báo giá cần 1 hoặc 2 chặng.");
        if (input.LegKm.Any(km => km <= 0)) throw new ArgumentException("Quãng đường mỗi chặng phải lớn hơn 0.");
        if (input.WeightKg <= 0) throw new ArgumentException("Vui lòng cung cấp khối lượng hàng.");
        TariffRule.Validate(tariff.VehicleType, tariff.PricePerKm, tariff.PricePerTon,
            tariff.SurchargeFragile, tariff.SurchargeHazmat);
        var lines = new List<FreightQuoteLine>();
        try
        {
            var km = input.LegKm.Sum();
            var tons = input.WeightKg / 1000m;
            var distance = Round(checked(km * tariff.PricePerKm));
            var weight = Round(checked(tons * tariff.PricePerTon));
            Add("Cước theo km", string.Format(Vi, "{0:N2} km × {1:N0} đ", km, tariff.PricePerKm), distance);
            Add("Cước theo tấn", string.Format(Vi, "{0:0.###} tấn × {1:N0} đ", tons, tariff.PricePerTon), weight);
            var basis = distance + weight;
            if (input.IsFragile) Fee("Phụ phí dễ vỡ", policy?.FragilePercent, tariff.SurchargeFragile);
            if (input.IsHazmat) Fee("Phụ phí nguy hiểm", policy?.HazmatPercent, tariff.SurchargeHazmat);
            if (lines.Sum(l => l.Amount) > Invoice.MaximumAmount)
                throw new ArgumentException("Tổng cước vượt giới hạn cho phép.");

            void Fee(string name, decimal? percent, decimal legacy)
            {
                if (percent is decimal p) Add(name, string.Format(Vi, "{0:0.##}% × {1:N0} đ", p, basis), SurchargePolicy.Charge(basis, p));
                else Add(name, "Phí cố định theo loại xe", legacy);
            }
        }
        catch (OverflowException)
        {
            throw new ArgumentException("Số liệu hoặc tổng cước vượt giới hạn cho phép.");
        }
        return lines;

        void Add(string name, string detail, decimal amount)
        {
            if (amount != 0) lines.Add(new(name, detail, amount));
        }
    }

    public static decimal ToKilograms(decimal weight, string unit)
    {
        if (weight < 0) throw new ArgumentException("Khối lượng không được âm.");
        try
        {
            return unit?.Trim().ToLowerInvariant() switch
            {
                "kg" => weight,
                "tấn" or "tan" or "ton" => checked(weight * 1000m),
                _ => throw new ArgumentException("Đơn vị khối lượng phải là kg hoặc tấn.")
            };
        }
        catch (OverflowException)
        {
            throw new ArgumentException("Khối lượng vượt giới hạn cho phép.");
        }
    }

    private static decimal Round(decimal amount)
        => decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
}
