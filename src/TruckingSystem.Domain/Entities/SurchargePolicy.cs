namespace TruckingSystem.Domain.Entities;

public sealed class SurchargePolicy
{
    public DateOnly EffectiveDate { get; set; }
    public decimal FragilePercent { get; set; }
    public decimal HazmatPercent { get; set; }

    public void Validate(DateOnly today)
    {
        if (EffectiveDate < today)
            throw new ArgumentException("Ngày hiệu lực không được nằm trong quá khứ.");
        foreach (var rate in new[] { FragilePercent, HazmatPercent })
            if (rate <= 0 || rate > 100 || decimal.Round(rate, 2) != rate)
                throw new ArgumentException("Mỗi tỷ lệ phải lớn hơn 0, tối đa 100% và có tối đa 2 chữ số thập phân.");
    }

    public static decimal Charge(decimal basis, decimal percent)
        => decimal.Round(basis * percent / 100m, 2, MidpointRounding.AwayFromZero);
}
