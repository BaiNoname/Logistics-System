using System.Drawing;
using System.Windows.Forms;
using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_TripManagement;

namespace TruckingSystem.Desktop.GUI_TripManagement;

/// <summary>Danh sách TOÀN BỘ chuyến (Trip) trong hệ thống — khác OrderDetailForm
/// chỉ hiện Trip của 1 đơn cụ thể. Cho phép Dispatcher đổi trạng thái thủ công
/// (Planned→InProgress→Completed) vì chưa tích hợp GPS thật để tự động.</summary>
public partial class TripsControl : BaseUserControl
{
    private ApiServices? _api;
    private List<TripListItem> _trips = new();

    public TripsControl()
    {
        InitializeComponent();
        Theme.StyleGrid(grid);
        Resize += (_, _) => { grid.Width = Width; grid.Height = Height - grid.Top; };
        grid.DataBindingComplete += (_, _) => { if (grid.Columns["Id"] != null) grid.Columns["Id"]!.Visible = false; };
        Load += async (_, _) => await LoadAsync();
    }

    public TripsControl(ApiServices api) : this()
    {
        _api = api;
    }

    private void BtnRefresh_Click(object? sender, EventArgs e) => _ = LoadAsync();

    private async void BtnStart_Click(object? sender, EventArgs e)
    {
        var tripId = GetSelectedTripId();
        if (tripId is null) { ShowNoSelection(); return; }

        try
        {
            var result = await _api!.Trips.StartTripAsync(tripId.Value);
            if (!result.Success)
            {
                MessageBox.Show(result.Message ?? "Không thể bắt đầu chuyến.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            await LoadAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void BtnComplete_Click(object? sender, EventArgs e)
    {
        var tripId = GetSelectedTripId();
        if (tripId is null) { ShowNoSelection(); return; }

        var confirm = MessageBox.Show(
            "Xác nhận chuyến này đã hoàn tất (xe đã tới nơi, hàng đã giao/nhận xong)?",
            "Xác nhận hoàn tất chuyến", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (confirm != DialogResult.Yes) return;

        try
        {
            var result = await _api!.Trips.CompleteTripAsync(tripId.Value);
            if (!result.Success)
            {
                MessageBox.Show(result.Message ?? "Không thể hoàn tất chuyến.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            await LoadAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void BtnFuel_Click(object? sender, EventArgs e)
    {
        var tripId = GetSelectedTripId();
        if (tripId is null) { ShowNoSelection(); return; }

        using var f = new RecordFuelForm(_api!, tripId.Value);
        if (f.ShowDialog() == DialogResult.OK)
        {
            MessageBox.Show("Đã ghi nhận đổ xăng thành công.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private int? GetSelectedTripId()
    {
        if (grid.CurrentRow is null) return null;
        return (int)grid.CurrentRow.Cells["Id"].Value;
    }

    private static void ShowNoSelection()
        => MessageBox.Show("Vui lòng chọn 1 chuyến trong danh sách trước.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

    private async Task LoadAsync()
    {
        if (_api is null) return;
        lblStatus.Text = "Đang tải...";
        try
        {
            _trips = await _api.Trips.GetTripsAsync();
            grid.DataSource = _trips.Select(t => new
            {
                t.Id,
                Mã_đơn = t.OrderCode,
                Biển_số_xe = t.VehiclePlate,
                Tài_xế = t.DriverName,
                Chặng = TranslateLegType(t.LegType),
                Trạng_thái = TranslateStatus(t.Status),
                Số_lượng = t.TotalQtyPlanned,
            }).ToList();
            lblStatus.Text = $"Tổng {_trips.Count} chuyến — chọn 1 dòng rồi bấm Bắt đầu/Hoàn tất để đổi trạng thái";
        }
        catch (Exception ex)
        {
            lblStatus.Text = "Lỗi: " + ex.Message;
            lblStatus.ForeColor = Theme.Danger;
        }
    }

    private static string TranslateLegType(string s) => s switch
    {
        "Pickup" => "🔽 Lấy hàng",
        "Delivery" => "🔼 Giao hàng",
        _ => s,
    };

    private static string TranslateStatus(string s) => s switch
    {
        "Planned" => "Đã lên kế hoạch",
        "InProgress" => "Đang chạy",
        "Completed" => "Hoàn tất",
        "Cancelled" => "Đã hủy",
        _ => s,
    };
}