using TruckingSystem.Domain.Common;

namespace TruckingSystem.Domain.Entities;

public class Contract : BaseEntity
{
    public static ContractStatus GetStatus(DateTime startDate, DateTime? endDate, DateTime onDate)
        => onDate.Date < startDate.Date ? ContractStatus.Upcoming
            : endDate.HasValue && onDate.Date > endDate.Value.Date ? ContractStatus.Expired
            : ContractStatus.Active;

    public static void ValidatePeriod(DateTime startDate, DateTime? endDate)
    {
        if (startDate == default)
            throw new ArgumentException("Vui lòng nhập ngày bắt đầu hợp đồng.");
        if (endDate.HasValue && endDate.Value.Date < startDate.Date)
            throw new ArgumentException("Ngày kết thúc không được trước ngày bắt đầu.");
    }

    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public string Code { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Terms { get; set; }

    public int? PriceListId { get; set; }
    public PriceList? PriceList { get; set; }
    public int? TariffRuleId { get; set; }
    public TariffRule? TariffRule { get; set; }
}

public enum ContractStatus { Upcoming, Active, Expired }
