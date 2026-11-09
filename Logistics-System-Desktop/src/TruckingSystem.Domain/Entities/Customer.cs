using TruckingSystem.Domain.Common;
using TruckingSystem.Domain.Enums;

namespace TruckingSystem.Domain.Entities;

public class Customer : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? TaxCode { get; set; }
    public CustomerType CustomerType { get; set; } = CustomerType.Individual;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public decimal? CreditLimit { get; set; }

    // Tùy chọn — chỉ có giá trị nếu khách hàng có tài khoản đăng nhập (role
    // CustomerPortal) — dùng cho các tính năng khác trong tương lai, KHÔNG
    // dùng cho Google Calendar (xem GoogleCalendarId bên dưới).
    public string? UserId { get; set; }

    // Calendar ID của khách hàng — NHẬP TAY bởi nhân viên (không qua OAuth),
    // vì khách hàng là bên ngoài, không cần tài khoản trong hệ thống. Khách
    // tự share lịch của họ cho 1 email Service Account cố định, rồi gửi lại
    // Calendar ID này cho nhân viên nhập vào — làm 1 lần duy nhất.
    public string? GoogleCalendarId { get; set; }

    public ICollection<CustomerContact> Contacts { get; set; } = new List<CustomerContact>();
    public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
    public ICollection<SalesOrder> SalesOrders { get; set; } = new List<SalesOrder>();
    public ICollection<TransportOrder> TransportOrders { get; set; } = new List<TransportOrder>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}
