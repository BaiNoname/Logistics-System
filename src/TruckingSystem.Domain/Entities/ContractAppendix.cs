using TruckingSystem.Domain.Common;

namespace TruckingSystem.Domain.Entities;

public class ContractAppendix : BaseEntity
{
    public static void Validate(string? code, int contractId, DateTime issueDate, DateTime effectiveDate, int tariffRuleId)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Trim().Length > 100)
            throw new ArgumentException("Mã phụ lục phải có từ 1 đến 100 ký tự.");
        if (contractId <= 0) throw new ArgumentException("Vui lòng chọn hợp đồng.");
        if (issueDate == default) throw new ArgumentException("Vui lòng chọn ngày lập.");
        if (effectiveDate == default) throw new ArgumentException("Vui lòng chọn ngày có hiệu lực.");
        if (tariffRuleId <= 0) throw new ArgumentException("Vui lòng chọn chi tiết bảng giá.");
    }

    public string Code { get; set; } = string.Empty;
    public int ContractId { get; set; }
    public Contract Contract { get; set; } = null!;
    public DateTime IssueDate { get; set; }
    public DateTime EffectiveDate { get; set; }
    public int TariffRuleId { get; set; }
    public TariffRule TariffRule { get; set; } = null!;
}
