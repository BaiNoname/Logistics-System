using TruckingSystem.Domain.Common;
using TruckingSystem.Domain.Enums;

namespace TruckingSystem.Domain.Entities;

public class Invoice : BaseEntity
{
    public const decimal MaximumAmount = 9999999999999999.99m;

    public static Invoice Create(TransportOrder order, decimal amount)
    {
        if (order.Status != TransportOrderStatus.Done)
            throw new InvalidOperationException("Chỉ được lập hóa đơn cho đơn hàng đã hoàn thành.");
        if (amount <= 0 || amount > MaximumAmount || decimal.Round(amount, 2) != amount)
            throw new ArgumentException("Cước phí phải lớn hơn 0, không vượt 9.999.999.999.999.999,99 và có tối đa 2 chữ số thập phân.");

        return new Invoice
        {
            TransportOrderId = order.Id,
            CustomerId = order.CustomerId,
            TotalAmount = amount,
            Lines = new List<InvoiceLine>
            {
                new()
                {
                    Description = $"Cước vận chuyển đơn {order.Code}",
                    Quantity = 1,
                    UnitPrice = amount,
                    Amount = amount
                }
            }
        };
    }

    public int TransportOrderId { get; set; }
    public TransportOrder TransportOrder { get; set; } = null!;

    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public DateTime IssueDate { get; set; } = DateTime.UtcNow;
    public DateTime? DueDate { get; set; }
    public decimal TotalAmount { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;

    public ICollection<InvoiceLine> Lines { get; set; } = new List<InvoiceLine>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
