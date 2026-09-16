using TruckingSystem.Desktop.Common;

namespace TruckingSystem.Desktop.GUI_InvoiceManagement;

public class InvoicesControl : BaseUserControl
{
    private readonly ApiServices _api;
    private readonly DataGridView _grid = new() { Top = 76, Left = 0, Height = 560 };
    private readonly Label _lblStatus = new() { Font = Theme.FontSmall, ForeColor = Theme.TextSecondary, Top = 44, Left = 0, AutoSize = true };

    public InvoicesControl(ApiServices api)
    {
        _api = api;
        BackColor = Theme.ContentBg;

        var title = new Label { Text = "Hóa đơn", Font = Theme.FontTitle, ForeColor = Theme.TextPrimary, Top = 0, Left = 0, AutoSize = true };
        var note = new Label
        {
            Text = "(Chức năng tạo hóa đơn tự động đang trong quá trình hoàn thiện — hiện chỉ hiển thị danh sách)",
            Font = Theme.FontSmall, ForeColor = Theme.Warning, Top = 4, Left = 220, AutoSize = true,
        };
        var btnRefresh = Theme.MakeSecondaryButton("🔄 Làm mới", 110);
        btnRefresh.Top = 0; btnRefresh.Left = 900;
        btnRefresh.Click += async (_, _) => await LoadAsync();

        Theme.StyleGrid(_grid);
        Resize += (_, _) => { _grid.Width = Width; _grid.Height = Height - _grid.Top; };

        Controls.Add(_grid);
        Controls.Add(_lblStatus);
        Controls.Add(btnRefresh);
        Controls.Add(note);
        Controls.Add(title);

        Load += async (_, _) => await LoadAsync();
    }

    private async Task LoadAsync()
    {
        _lblStatus.Text = "Đang tải...";
        try
        {
            var invoices = await _api.Invoices.GetInvoicesAsync();
            _grid.DataSource = invoices.Select(i => new
            {
                Mã_đơn = i.OrderCode,
                Khách_hàng = i.CustomerName,
                Tổng_tiền = i.TotalAmount.ToString("N0") + " đ",
                Trạng_thái = TranslateStatus(i.Status),
                Ngày_xuất = i.IssueDate.ToString("dd/MM/yyyy"),
            }).ToList();
            _lblStatus.Text = $"Tổng {invoices.Count} hóa đơn";
        }
        catch (Exception ex)
        {
            _lblStatus.Text = "Lỗi: " + ex.Message;
            _lblStatus.ForeColor = Theme.Danger;
        }
    }

    private static string TranslateStatus(string s) => s switch
    {
        "Draft" => "Nháp", "Sent" => "Đã gửi", "PartiallyPaid" => "Trả 1 phần",
        "Paid" => "Đã thanh toán", "Overdue" => "Quá hạn", "Cancelled" => "Đã hủy", _ => s,
    };
}
