using TruckingSystem.Domain.Common;
using TruckingSystem.Domain.Enums;

namespace TruckingSystem.Domain.Entities;

/// <summary>
/// Bước "chốt đơn" ban đầu với khách, tương tự sale.order bên Odoo.
/// Xác nhận SalesOrder sẽ tự sinh 1 TransportOrder (Draft).
/// </summary>
public class SalesOrder : BaseEntity
{
    public string Code { get; set; } = string.Empty;

    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public int? ContractId { get; set; }
    public Contract? Contract { get; set; }

    public string PickupPoint { get; set; } = string.Empty;
    public string DropoffPoint { get; set; } = string.Empty;
    public TransportOrderStatus Status { get; set; } = TransportOrderStatus.Draft;

    public TransportOrder? TransportOrder { get; set; }
}
