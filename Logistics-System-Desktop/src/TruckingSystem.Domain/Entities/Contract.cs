using TruckingSystem.Domain.Common;
using TruckingSystem.Domain.Enums;

namespace TruckingSystem.Domain.Entities;

public class Contract : BaseEntity
{
    public static ContractStatus GetStatus(DateTime startDate, DateTime? endDate, DateTime onDate)
        => onDate.Date < startDate.Date ? ContractStatus.Upcoming
            : endDate.HasValue && onDate.Date > endDate.Value.Date ? ContractStatus.Expired
            : ContractStatus.Active;

    public static void ValidatePeriod(DateTime startDate, DateTime? endDate, ContractType? type = null, DateTime? minimumStartDate = null)
    {
        if (startDate == default)
            throw new ArgumentException("Vui lòng nhập ngày bắt đầu hợp đồng.");
        if (minimumStartDate.HasValue && startDate.Date < minimumStartDate.Value.Date)
            throw new ArgumentException("Ngày bắt đầu hợp đồng mới không được trong quá khứ.");
        if (type == ContractType.OneTime && !endDate.HasValue)
            throw new ArgumentException("Hợp đồng vận chuyển một lần phải có ngày kết thúc.");
        if (endDate.HasValue && endDate.Value.Date < startDate.Date)
            throw new ArgumentException("Ngày kết thúc không được trước ngày bắt đầu.");
    }

    public static void ValidateDiscount(ContractType type, decimal? discountPercent)
    {
        if (!Enum.IsDefined(type)) throw new ArgumentException("Loại hợp đồng không hợp lệ.");
        if (type == ContractType.OneTime && discountPercent is not null and not 0)
            throw new ArgumentException("Hợp đồng một lần không áp dụng ưu đãi.");
        if (type == ContractType.LongTerm && !discountPercent.HasValue)
            throw new ArgumentException("Vui lòng nhập phần trăm ưu đãi cho hợp đồng dài hạn.");
        if (discountPercent is < 0 or > 100 ||
            (discountPercent.HasValue && decimal.Round(discountPercent.Value, 2) != discountPercent.Value))
            throw new ArgumentException("Ưu đãi phải từ 0 đến 100%, tối đa 2 chữ số thập phân.");
    }

    public static void ValidateLateDeliveryDiscount(decimal percent)
    {
        if (percent is < 0 or > 100 || decimal.Round(percent, 2) != percent)
            throw new ArgumentException("Giảm giá khi giao trễ phải từ 0 đến 100%, tối đa 2 chữ số thập phân.");
    }

    public ContractType? Type { get; set; }
    public decimal? DiscountPercent { get; set; }
    public decimal LateDeliveryDiscountPercent { get; set; }
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
