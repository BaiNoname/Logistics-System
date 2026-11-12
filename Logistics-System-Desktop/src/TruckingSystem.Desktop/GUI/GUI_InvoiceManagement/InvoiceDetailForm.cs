using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_InvoiceManagement;
using System.Globalization;

namespace TruckingSystem.Desktop.GUI_InvoiceManagement;

public partial class InvoiceDetailForm : BaseForm
{
    public InvoiceDetailForm()
    {
        InitializeComponent();
        Theme.StyleGrid(gridLines);
        Theme.StyleGrid(gridPayments);
        gridLines.DefaultCellStyle.FormatProvider = gridPayments.DefaultCellStyle.FormatProvider = CultureInfo.GetCultureInfo("vi-VN");
    }

    public InvoiceDetailForm(InvoiceDetail invoice) : this()
    {
        Text = $"Chi tiết hóa đơn #{invoice.Id}";
        lblTitle.Text = $"Hóa đơn #{invoice.Id} — {invoice.OrderCode}";
        var status = invoice.Status == "Paid" ? "Đã thanh toán" : "Chưa thanh toán";
        lblInvoiceInfo.Text = $"{status}  •  Ngày lập: {(invoice.IssueDate == default ? "Chưa có dữ liệu" : invoice.IssueDate.AddHours(7).ToString("dd/MM/yyyy HH:mm"))}"
            + $"  •  Hạn thanh toán: {invoice.DueDate?.ToString("dd/MM/yyyy") ?? "Chưa đặt"}";
        lblInvoiceInfo.ForeColor = invoice.Status == "Paid" ? Theme.Success : Theme.Warning;
        lblCustomer.Text = $"KHÁCH HÀNG #{invoice.CustomerId}\n{invoice.CustomerName}"
            + $"\nMã số thuế: {Display(invoice.CustomerTaxCode)}\nĐiện thoại: {Display(invoice.CustomerPhone)}\nĐịa chỉ: {Display(invoice.CustomerAddress)}";
        lblOrderInfo.Text = $"ĐƠN VẬN CHUYỂN: {invoice.OrderCode}\nHợp đồng: {Display(invoice.ContractCode)}"
            + $"\nNhận hàng: {Display(invoice.PickupPoint)}\nGiao hàng: {Display(invoice.DropoffPoint)}";
        gridLines.DataSource = invoice.Lines;
        lblNote.Text = invoice.Lines.Count == 0
            ? "Hóa đơn này chưa có dữ liệu chi tiết các khoản phí đã lưu."
            : $"{invoice.Lines.Count} khoản phí đã lưu khi lập hóa đơn. Thông tin khách hàng và đơn vận chuyển hiển thị theo hồ sơ hiện tại.";
        gridPayments.DataSource = invoice.Payments.Select(p => new
        {
            p.Id,
            PaymentDate = p.PaymentDate.AddHours(7),
            Method = p.Method switch { "Cash" => "Tiền mặt", "BankTransfer" => "Chuyển khoản", "Card" => "Thẻ", _ => "Khác" },
            p.Amount
        }).ToList();
        var vi = CultureInfo.GetCultureInfo("vi-VN");
        lblPaymentSummary.Text = invoice.Payments.Count == 0
            ? "Chưa có giao dịch thu tiền được ghi nhận. Đánh dấu đã thanh toán không tự tạo giao dịch thu tiền."
            : $"{invoice.Payments.Count} giao dịch • Tổng tiền đã ghi nhận: {invoice.Payments.Sum(p => p.Amount).ToString("N2", vi)} đ";
        lblTotal.Text = $"Tổng hóa đơn: {invoice.TotalAmount.ToString("N2", vi)} đ";
    }

    private static string Display(string? value) => string.IsNullOrWhiteSpace(value) ? "Chưa có thông tin" : value;
}
