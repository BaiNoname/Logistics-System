using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_InvoiceManagement;

namespace TruckingSystem.Desktop.GUI_InvoiceManagement;

public partial class InvoiceDetailForm : BaseForm
{
    public InvoiceDetailForm()
    {
        InitializeComponent();
        Theme.StyleGrid(gridLines);
    }

    public InvoiceDetailForm(InvoiceDetail invoice) : this()
    {
        lblTitle.Text = $"Chi tiết hóa đơn — {invoice.OrderCode}";
        lblCustomer.Text = $"Khách hàng: {invoice.CustomerName}";
        gridLines.DataSource = invoice.Lines;
        lblNote.Text = invoice.Lines.Count == 0
            ? "Hóa đơn này chưa có dữ liệu chi tiết các khoản phí đã lưu."
            : "Các khoản phí đã lưu khi lập hóa đơn.";
        lblTotal.Text = $"Tổng hóa đơn: {invoice.TotalAmount:N2} đ";
    }
}
