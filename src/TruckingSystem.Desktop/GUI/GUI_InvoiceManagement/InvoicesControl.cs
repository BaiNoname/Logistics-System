using TruckingSystem.Desktop.Common;

namespace TruckingSystem.Desktop.GUI_InvoiceManagement;

public partial class InvoicesControl : BaseUserControl
{
    private ApiServices? _api;

    public InvoicesControl()
    {
        InitializeComponent();
        Theme.StyleGrid(grid);
        Resize += (_, _) => { grid.Width = Width; grid.Height = Height - grid.Top; };
        Load += async (_, _) => await LoadAsync();
    }

    public InvoicesControl(ApiServices api) : this()
    {
        _api = api;
    }

    private void BtnRefresh_Click(object? sender, EventArgs e) => _ = LoadAsync();

    private async Task LoadAsync()
    {
        if (_api is null) return;
        lblStatus.Text = "Đang tải...";
        try
        {
            var invoices = await _api.Invoices.GetInvoicesAsync();
            grid.DataSource = invoices.Select(i => new
            {
                Mã_đơn = i.OrderCode,
                Khách_hàng = i.CustomerName,
                Tổng_tiền = i.TotalAmount.ToString("N0") + " đ",
                Trạng_thái = TranslateStatus(i.Status),
                Ngày_xuất = i.IssueDate.ToString("dd/MM/yyyy"),
            }).ToList();
            lblStatus.Text = $"Tổng {invoices.Count} hóa đơn";
        }
        catch (Exception ex)
        {
            lblStatus.Text = "Lỗi: " + ex.Message;
            lblStatus.ForeColor = Theme.Danger;
        }
    }

    private static string TranslateStatus(string s) => s switch
    {
        "Draft" => "Nháp", "Sent" => "Đã gửi", "PartiallyPaid" => "Trả 1 phần",
        "Paid" => "Đã thanh toán", "Overdue" => "Quá hạn", "Cancelled" => "Đã hủy", _ => s,
    };
}
