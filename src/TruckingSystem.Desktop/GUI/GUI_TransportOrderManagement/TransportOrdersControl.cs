using System.Windows.Forms;
using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_TransportOrderManagement;

namespace TruckingSystem.Desktop.GUI_TransportOrderManagement;

public class TransportOrdersControl : BaseUserControl
{
    private readonly ApiServices _api;
    private readonly DataGridView _grid = new() { Top = 76, Left = 0, Height = 560 };
    private readonly Label _lblStatus = new() { Font = Theme.FontSmall, ForeColor = Theme.TextSecondary, Top = 44, Left = 0, AutoSize = true };
    private List<TransportOrderListItem> _orders = new();

    public TransportOrdersControl(ApiServices api)
    {
        _api = api;
        BackColor = Theme.ContentBg;

        var title = new Label { Text = "Đơn vận chuyển", Font = Theme.FontTitle, ForeColor = Theme.TextPrimary, Top = 0, Left = 0, AutoSize = true };
        var btnRefresh = Theme.MakeSecondaryButton("🔄 Làm mới", 110);
        var btnAllocate = Theme.MakeSecondaryButton("🚚 Chia xe", 130);
        var btnCreate = Theme.MakePrimaryButton("➕ Tạo đơn mới", 150);
        btnRefresh.Top = 0; btnRefresh.Left = 640;
        btnAllocate.Top = 0; btnAllocate.Left = 760;
        btnCreate.Top = 0; btnCreate.Left = 900;

        btnRefresh.Click += async (_, _) => await LoadAsync();
        btnCreate.Click += async (_, _) =>
        {
            using var f = new CreateOrderForm(_api);
            if (f.ShowDialog() == DialogResult.OK) await LoadAsync();
        };
        btnAllocate.Click += async (_, _) => await AllocateSelectedAsync();

        Theme.StyleGrid(_grid);
        // Chỉ hiện các cột dễ đọc, ẩn cột Id đi (vẫn giữ trong DataSource để lấy khi cần)
        _grid.DataBindingComplete += (_, _) =>
        {
            if (_grid.Columns["Id"] != null) _grid.Columns["Id"]!.Visible = false;
        };
        Resize += (_, _) => { _grid.Width = Width; _grid.Height = Height - _grid.Top; };
        _grid.CellDoubleClick += async (_, _) => await OpenDetailAsync();

        Controls.Add(_grid);
        Controls.Add(_lblStatus);
        Controls.Add(btnRefresh);
        Controls.Add(btnAllocate);
        Controls.Add(btnCreate);
        Controls.Add(title);

        Load += async (_, _) => await LoadAsync();
    }

    private async Task LoadAsync()
    {
        _lblStatus.Text = "Đang tải...";
        try
        {
            _orders = await _api.TransportOrders.GetTransportOrdersAsync();
            _grid.DataSource = _orders.Select(o => new
            {
                o.Id,
                Mã_đơn = o.Code,
                Khách_hàng = o.CustomerName,
                Trạng_thái = TranslateStatus(o.Status),
                Tiến_độ = $"{o.ProgressPercent}%",
                Điểm_lấy = o.PickupPoint,
                Điểm_giao = o.DropoffPoint,
            }).ToList();
            _lblStatus.Text = $"Tổng {_orders.Count} đơn hàng — chọn 1 dòng rồi bấm \"Chia xe\" để chạy thuật toán Greedy";
            _lblStatus.ForeColor = Theme.TextSecondary;
        }
        catch (Exception ex)
        {
            _lblStatus.Text = "Lỗi: " + ex.Message;
            _lblStatus.ForeColor = Theme.Danger;
        }
    }

    private async Task OpenDetailAsync()
    {
        if (_grid.CurrentRow is null) return;
        var orderId = (int)_grid.CurrentRow.Cells["Id"].Value;

        using var f = new OrderDetailForm(_api, orderId);
        f.ShowDialog();
        await LoadAsync(); // làm mới danh sách phòng khi trạng thái đơn vừa đổi (do vừa Chia xe)
    }

    private async Task AllocateSelectedAsync()
    {
        if (_grid.CurrentRow is null)
        {
            MessageBox.Show("Vui lòng chọn 1 đơn hàng trong danh sách trước.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var orderId = (int)_grid.CurrentRow.Cells["Id"].Value;
        var orderCode = _orders.First(o => o.Id == orderId).Code;

        try
        {
            var result = await _api.TransportOrders.AllocateVehiclesAsync(orderId);

            if (!result.Trips.Any())
            {
                MessageBox.Show(result.Message ?? "Không có xe nào rảnh để chia.", "Không thể chia xe", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var lines = result.Trips.Select(a => $"  • Xe {a.VehiclePlate} — chở {a.QtyAssigned} (Chuyến #{a.TripId})");
            var summary = $"Đơn {orderCode}\n" +
                          $"Tổng cần chở: {result.TotalQtyToShip}  |  Đã gán: {result.TotalQtyAssigned}\n\n" +
                          $"Thuật toán Greedy đã chia thành {result.Trips.Count} chuyến:\n" +
                          string.Join("\n", lines) +
                          (result.Message != null ? $"\n\n⚠️ {result.Message}" : "");

            MessageBox.Show(summary, "Kết quả chia xe (Greedy)", MessageBoxButtons.OK,
                result.Message != null ? MessageBoxIcon.Warning : MessageBoxIcon.Information);

            await LoadAsync(); // làm mới để thấy trạng thái đơn vừa đổi thành "Đang vận chuyển"
        }
        catch (Exception ex)
        {
            MessageBox.Show("Lỗi khi chia xe: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static string TranslateStatus(string s) => s switch
    {
        "Draft" => "Nháp", "Confirmed" => "Đã xác nhận", "InProgress" => "Đang vận chuyển",
        "Done" => "Hoàn tất", "Cancelled" => "Đã hủy", _ => s,
    };
}
