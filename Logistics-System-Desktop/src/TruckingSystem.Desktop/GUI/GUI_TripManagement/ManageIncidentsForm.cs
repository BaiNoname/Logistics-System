using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_TripManagement;

namespace TruckingSystem.Desktop.GUI_TripManagement
{
    /// <summary>Danh sách sự cố (tai nạn/hỏng xe/vi phạm/khác) đã ghi nhận cho
    /// 1 chuyến cụ thể — tài xế/Dispatcher ghi nhận khi có sự cố xảy ra.</summary>
    public partial class ManageIncidentsForm : BaseForm
    {
        private ApiServices? _api;
        private int _tripId;
        private List<IncidentReportItem> _incidents = new();

        public ManageIncidentsForm()
        {
            InitializeComponent();
            TruckingSystem.Desktop.Common.DialogFit.Enable(this, root); // vừa màn hình mọi máy, bảng không bị ép mất chữ
            TruckingSystem.Desktop.Common.DialogKeys.Enable(this); // Esc = đóng
            Theme.StyleGrid(grid);
        }

        public ManageIncidentsForm(ApiServices api, int tripId, string tripLabel) : this()
        {
            _api = api;
            _tripId = tripId;
            Text = $"Sự cố — {tripLabel}";
            Load += async (_, _) => await LoadAsync();
        }

        private async Task LoadAsync()
        {
            if (_api is null) return;
            lblStatus.Text = "Đang tải...";
            try
            {
                _incidents = await _api.Trips.GetIncidentsAsync(_tripId);
                grid.DataSource = _incidents.Select(i => new
                {
                    i.Id,
                    Loại_sự_cố = TranslateType(i.Type),
                    Mô_tả = i.Description ?? "—",
                    Thời_gian = i.Timestamp.ToLocalTime().ToString("dd/MM/yyyy HH:mm"),
                    Chi_phí_phát_sinh = i.Cost.HasValue ? i.Cost.Value.ToString("N0") + " đ" : "—",
                }).ToList();
                grid.DataBindingComplete += (_, _) => { if (grid.Columns["Id"] != null) grid.Columns["Id"]!.Visible = false; };
                lblStatus.Text = $"{_incidents.Count} sự cố đã ghi nhận";
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Lỗi: " + ex.Message;
                lblStatus.ForeColor = Theme.Danger;
            }
        }

        private async void BtnAddIncident_Click(object? sender, EventArgs e)
        {
            if (_api is null) return;
            using var f = new CreateIncidentForm(_api, _tripId);
            if (f.ShowDialog(FindForm()) == DialogResult.OK) await LoadAsync();
        }

        private async void BtnDeleteIncident_Click(object? sender, EventArgs e)
        {
            if (_api is null || grid.CurrentRow is null)
            {
                Toast.Show(this, "👆 " + "Vui lòng chọn 1 sự cố trước.", ToastKind.Info);
                return;
            }
            var id = (int)grid.CurrentRow.Cells["Id"].Value;

            if (!Ask.Confirm(this, "Xóa bản ghi sự cố này?", "Bản ghi bị xóa hẳn, không hoàn tác được.", "🗑 Xóa sự cố", danger: true)) return;

            try
            {
                var result = await _api.Trips.DeleteIncidentAsync(id);
                if (!result.Success)
                {
                    Ask.Fail(this, result.Message ?? "Không thể xóa.");
                    return;
                }
                await LoadAsync();
            }
            catch (Exception ex)
            {
                Ask.Fail(this, "Lỗi: " + ex.Message);
            }
        }

        private static string TranslateType(string s) => s switch
        {
            "Accident" => "🚨 Tai nạn",
            "Breakdown" => "🔧 Hỏng xe",
            "TrafficViolation" => "🚔 Vi phạm giao thông",
            _ => "❓ Khác",
        };
    }
}
