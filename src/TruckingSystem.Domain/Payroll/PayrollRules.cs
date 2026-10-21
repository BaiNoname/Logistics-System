namespace TruckingSystem.Domain.Payroll;

public static class PayrollRules
{
    public static (DateTime From, DateTime To) PeriodRange(int year, int month)
    {
        var to = new DateTime(year, month, 25);
        return (to.AddMonths(-1).AddDays(1), to);
    }

    public const decimal MaxAmount = 1_000_000_000_000m;
    public static decimal RoundMoney(decimal amount) => decimal.Round(amount, 0, MidpointRounding.AwayFromZero);
    public static bool ValidMoney(decimal amount) => amount > 0 && amount <= MaxAmount && amount == RoundMoney(amount);
    public static bool ValidQuantity(decimal value) => value >= 0 && value == decimal.Round(value, 2);

    public static decimal Prorate(decimal amount, decimal standardDays, decimal paidDays)
    {
        if (amount < 0 || standardDays <= 0 || paidDays < 0 || paidDays > standardDays)
            throw new ArgumentOutOfRangeException(nameof(paidDays), "Mức lương hoặc công không hợp lệ.");
        return RoundMoney(amount / standardDays * paidDays);
    }

    public static string PaymentStatus(decimal netPay, decimal paidAmount)
        => paidAmount >= netPay ? "Paid" : paidAmount > 0 ? "PartiallyPaid" : "Unpaid";
}
