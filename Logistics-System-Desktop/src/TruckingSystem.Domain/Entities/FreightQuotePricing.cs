namespace TruckingSystem.Domain.Entities;

public record FreightQuoteLegInput(decimal DistanceKm, decimal WeightKg, bool IsEmpty,
    bool IsFragile, bool IsHazmat);

public record FreightQuoteLegPrice(decimal DistanceCharge, decimal WeightCharge,
    decimal FragileCharge, decimal HazmatCharge)
{
    public decimal Total => DistanceCharge + WeightCharge + FragileCharge + HazmatCharge;
}

/// <summary>Cước tham khảo theo từng chặng; không thay đổi nghiệp vụ hóa đơn.</summary>
public static class FreightQuotePricing
{
    public static IReadOnlyList<FreightQuoteLegPrice> Calculate(
        IReadOnlyList<FreightQuoteLegInput> legs, TariffRule tariff, SurchargePolicy? policy = null)
    {
        ArgumentNullException.ThrowIfNull(legs);
        ArgumentNullException.ThrowIfNull(tariff);
        if (legs.Count is < 1 or > 10)
            throw new ArgumentException("Hành trình cần từ 1 đến 10 chặng.");
        TariffRule.Validate(tariff.VehicleType, tariff.PricePerKm, tariff.PricePerTon,
            tariff.SurchargeFragile, tariff.SurchargeHazmat);
        var result = new List<FreightQuoteLegPrice>(legs.Count);
        decimal total = 0;
        try
        {
            foreach (var leg in legs)
            {
                if (leg is null || leg.DistanceKm <= 0 || leg.WeightKg < 0)
                    throw new ArgumentException("Quãng đường phải lớn hơn 0 và khối lượng không được âm.");
                if (leg.IsEmpty && (leg.WeightKg != 0 || leg.IsFragile || leg.IsHazmat))
                    throw new ArgumentException("Chặng xe rỗng không được có hàng hoặc phụ phí hàng hóa.");
                if (!leg.IsEmpty && leg.WeightKg <= 0)
                    throw new ArgumentException("Vui lòng cung cấp khối lượng hàng của từng chặng.");
                var price = new FreightQuoteLegPrice(
                    Round(leg.DistanceKm * tariff.PricePerKm),
                    Round(leg.WeightKg / 1000m * tariff.PricePerTon),
                    policy is null && leg.IsFragile ? tariff.SurchargeFragile : 0,
                    policy is null && leg.IsHazmat ? tariff.SurchargeHazmat : 0);
                total += price.Total;
                if (total > Invoice.MaximumAmount)
                    throw new ArgumentException("Tổng cước vượt giới hạn cho phép.");
                result.Add(price);
            }
            if (policy != null)
            {
                var basis = total;
                var fragile = true;
                var hazmat = true;
                for (var i = 0; i < legs.Count; i++)
                {
                    var price = result[i];
                    if (fragile && legs[i].IsFragile)
                    {
                        price = price with { FragileCharge = SurchargePolicy.Charge(basis, policy.FragilePercent) };
                        fragile = false;
                    }
                    if (hazmat && legs[i].IsHazmat)
                    {
                        price = price with { HazmatCharge = SurchargePolicy.Charge(basis, policy.HazmatPercent) };
                        hazmat = false;
                    }
                    result[i] = price;
                }
                if (result.Sum(p => p.Total) > Invoice.MaximumAmount)
                    throw new ArgumentException("Tổng cước vượt giới hạn cho phép.");
            }
        }
        catch (OverflowException)
        {
            throw new ArgumentException("Số liệu hoặc tổng cước vượt giới hạn cho phép.");
        }
        return result;
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
