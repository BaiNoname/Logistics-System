namespace TruckingSystem.Domain.Payroll;

public static class PayrollRules
{
    public static (DateTime From, DateTime To) PeriodRange(int year, int month)
    {
        var from = new DateTime(year, month, 1);
        return (from, from.AddMonths(1).AddDays(-1));
    }

    public static int StandardDays(int year, int month)
    {
        var (from, to) = PeriodRange(year, month);
        var count = 0;
        for (var day = from; day <= to; day = day.AddDays(1))
            if (day.DayOfWeek != DayOfWeek.Sunday) count++;
        return count;
    }

    public static decimal DriverSalary(decimal basicSalary, decimal points, decimal targetPoints)
    {
        if (basicSalary <= 0 || points < 0 || targetPoints <= 0)
            throw new ArgumentOutOfRangeException(nameof(targetPoints));
        return RoundMoney(basicSalary * (points / targetPoints));
    }

    public static decimal TripPoints(decimal distanceKm, decimal vehicleFactor, decimal kmPerPoint = 200)
    {
        if (distanceKm <= 0 || vehicleFactor <= 0 || kmPerPoint <= 0)
            throw new ArgumentOutOfRangeException(nameof(distanceKm), "Km và hệ số KPI phải dương.");
        return (0.5m + distanceKm / kmPerPoint) * vehicleFactor;
    }

    public static string SalaryStatus(DateTime effectiveFrom, DateTime? effectiveTo, DateTime today)
        => today.Date < effectiveFrom.Date ? "Pending"
            : effectiveTo.HasValue && today.Date > effectiveTo.Value.Date ? "Expired" : "Active";

    public const decimal MaxAmount = 1_000_000_000_000m;
    public static decimal RoundMoney(decimal amount) => decimal.Round(amount, 0, MidpointRounding.AwayFromZero);
    public static bool ValidMoney(decimal amount) => amount > 0 && amount <= MaxAmount && amount == RoundMoney(amount);
    public static bool ValidQuantity(decimal value) => value >= 0 && value == decimal.Round(value, 2);

    public static decimal Prorate(decimal amount, decimal standardDays, decimal paidDays)
    {
        if (amount < 0 || standardDays <= 0 || paidDays < 0)
            throw new ArgumentOutOfRangeException(nameof(paidDays), "Mức lương hoặc công không hợp lệ.");
        return RoundMoney(amount / standardDays * paidDays);
    }

    public static string PaymentStatus(decimal netPay, decimal paidAmount)
        => paidAmount >= netPay ? "Paid" : paidAmount > 0 ? "PartiallyPaid" : "Unpaid";
}
