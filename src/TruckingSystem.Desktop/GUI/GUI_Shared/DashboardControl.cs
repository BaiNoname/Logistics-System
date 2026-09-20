using System.Drawing;
using System.Windows.Forms;
using TruckingSystem.Desktop.Common;

namespace TruckingSystem.Desktop.GUI_Shared;

/// <summary>Logic — khung tĩnh (tiêu đề, panel chứa thẻ KPI) nằm ở
/// DashboardControl.Designer.cs. Nội dung 6 thẻ KPI vẫn tạo động vì số liệu
/// (và cả số lượng thẻ) phụ thuộc dữ liệu tải về lúc chạy — không cố định.</summary>
public partial class DashboardControl : BaseUserControl
{
    private ApiServices? _api;

    public DashboardControl()
    {
        InitializeComponent();
        Load += async (_, _) => await LoadAsync();
    }

    public DashboardControl(ApiServices api) : this()
    {
        _api = api;
    }

    private async Task LoadAsync()
    {
        if (_api is null) return;
        try
        {
            var orders = await _api.TransportOrders.GetTransportOrdersAsync();
            var vehicles = await _api.Vehicles.GetVehiclesAsync();
            var customers = await _api.Customers.GetCustomersAsync();
            var invoices = await _api.Invoices.GetInvoicesAsync();

            var inProgress = orders.Count(o => o.Status == "InProgress");
            var done = orders.Count(o => o.Status == "Done");
            var availableVehicles = vehicles.Count(v => v.Status == "Available");

            kpiPanel.Controls.Clear();
            kpiPanel.Controls.Add(MakeKpiCard("📦", "Tổng đơn hàng", orders.Count.ToString(), Theme.Primary));
            kpiPanel.Controls.Add(MakeKpiCard("🚚", "Đang giao", inProgress.ToString(), Theme.Warning));
            kpiPanel.Controls.Add(MakeKpiCard("✅", "Đã hoàn tất", done.ToString(), Theme.Success));
            kpiPanel.Controls.Add(MakeKpiCard("🚛", "Xe sẵn sàng", $"{availableVehicles}/{vehicles.Count}", Theme.Primary));
            kpiPanel.Controls.Add(MakeKpiCard("👥", "Khách hàng", customers.Count.ToString(), Theme.Primary));
            kpiPanel.Controls.Add(MakeKpiCard("🧾", "Hóa đơn", invoices.Count.ToString(), Theme.Primary));

            lblLoading.Visible = false;
        }
        catch (Exception ex)
        {
            lblLoading.Text = "Không tải được dữ liệu: " + ex.Message;
            lblLoading.ForeColor = Theme.Danger;
        }
    }

    private static Panel MakeKpiCard(string icon, string label, string value, Color accentColor)
    {
        var card = new Panel { Width = 190, Height = 200, Margin = new Padding(0, 0, 16, 0), BackColor = Theme.CardBg };
        card.Paint += (_, e) =>
        {
            using var pen = new Pen(Theme.BorderColor);
            e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
            using var brush = new SolidBrush(accentColor);
            e.Graphics.FillRectangle(brush, 0, 0, 4, card.Height);
        };

        var stack = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoSize = false,
            Left = 18, Top = 14, Width = card.Width - 30, Height = card.Height - 24,
        };

        var lblIcon = new Label { Text = icon, Font = new Font("Segoe UI Emoji", 18), AutoSize = true, Margin = new Padding(0, 0, 0, 6) };
        var lblValue = new Label { Text = value, Font = Theme.FontKpiNumber, ForeColor = Theme.TextPrimary, AutoSize = true, Margin = new Padding(0, 0, 0, 6) };
        var lblLabel = new Label { Text = label, Font = Theme.FontSmall, ForeColor = Theme.TextSecondary, AutoSize = true, Margin = new Padding(0) };

        stack.Controls.Add(lblIcon);
        stack.Controls.Add(lblValue);
        stack.Controls.Add(lblLabel);
        card.Controls.Add(stack);
        return card;
    }
}
