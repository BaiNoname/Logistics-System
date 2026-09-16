using TruckingSystem.Desktop.Common;

namespace TruckingSystem.Desktop.GUI_Shared;

public class DashboardControl : BaseUserControl
{
    private readonly ApiServices _api;
    private readonly FlowLayoutPanel _kpiPanel = new() { Dock = DockStyle.Top, Height = 140, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
    private readonly Label _lblLoading = new() { Text = "Đang tải dữ liệu tổng quan...", Font = Theme.FontNormal, ForeColor = Theme.TextSecondary, AutoSize = true, Top = 160, Left = 4 };

    public DashboardControl(ApiServices api)
    {
        _api = api;
        BackColor = Theme.ContentBg;

        var title = new Label { Text = "Tổng quan hệ thống", Font = Theme.FontTitle, ForeColor = Theme.TextPrimary, AutoSize = true, Top = 0, Left = 0 };

        Controls.Add(_lblLoading);
        Controls.Add(_kpiPanel);
        Controls.Add(title);

        Load += async (_, _) => await LoadAsync();
    }

    private async Task LoadAsync()
    {
        try
        {
            var orders = await _api.TransportOrders.GetTransportOrdersAsync();
            var vehicles = await _api.Vehicles.GetVehiclesAsync();
            var customers = await _api.Customers.GetCustomersAsync();
            var invoices = await _api.Invoices.GetInvoicesAsync();

            var inProgress = orders.Count(o => o.Status == "InProgress");
            var done = orders.Count(o => o.Status == "Done");
            var availableVehicles = vehicles.Count(v => v.Status == "Available");

            _kpiPanel.Controls.Clear();
            _kpiPanel.Controls.Add(MakeKpiCard("📦", "Tổng đơn hàng", orders.Count.ToString(), Theme.Primary));
            _kpiPanel.Controls.Add(MakeKpiCard("🚚", "Đang giao", inProgress.ToString(), Theme.Warning));
            _kpiPanel.Controls.Add(MakeKpiCard("✅", "Đã hoàn tất", done.ToString(), Theme.Success));
            _kpiPanel.Controls.Add(MakeKpiCard("🚛", "Xe sẵn sàng", $"{availableVehicles}/{vehicles.Count}", Theme.Primary));
            _kpiPanel.Controls.Add(MakeKpiCard("👥", "Khách hàng", customers.Count.ToString(), Theme.Primary));
            _kpiPanel.Controls.Add(MakeKpiCard("🧾", "Hóa đơn", invoices.Count.ToString(), Theme.Primary));

            _lblLoading.Visible = false;
        }
        catch (Exception ex)
        {
            _lblLoading.Text = "Không tải được dữ liệu: " + ex.Message;
            _lblLoading.ForeColor = Theme.Danger;
        }
    }

    private static Panel MakeKpiCard(string icon, string label, string value, Color accentColor)
    {
        var card = new Panel { Width = 190, Height = 120, Margin = new Padding(0, 0, 16, 0), BackColor = Theme.CardBg };
        card.Paint += (_, e) =>
        {
            using var pen = new Pen(Theme.BorderColor);
            e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
            using var brush = new SolidBrush(accentColor);
            e.Graphics.FillRectangle(brush, 0, 0, 4, card.Height);
        };

        // Dùng FlowLayoutPanel(TopDown) thay vì Top/Left cố định — mỗi Label tự
        // AutoSize và tự xếp chồng theo thứ tự, không phụ thuộc tính toán pixel
        // thủ công nên không bị lệch/đè khi chạy trên màn hình DPI khác 100%
        // (lỗi cũ: control tạo ra SAU khi Form đã hiển thị không được WinForms
        // tự động scale theo DPI như các control khai báo sẵn trong constructor).
        var stack = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoSize = false,
            Left = 18,
            Top = 14,
            Width = card.Width - 30,
            Height = card.Height - 24,
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