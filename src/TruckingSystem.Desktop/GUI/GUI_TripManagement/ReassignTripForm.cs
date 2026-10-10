using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_TripManagement;
using TruckingSystem.Desktop.SERVICE_TripManagement;

namespace TruckingSystem.Desktop.GUI_TripManagement
{
    /// <summary>Đổi xe và/hoặc tài xế cho chuyến CHƯA CHẠY — gỡ chuyến bị kẹt (xe hỏng, giấy
    /// tờ / bằng lái hết hạn, tài xế nghỉ...). Danh sách do SERVER đánh giá theo đúng điều
    /// kiện Chia xe: ✅ dùng được, ⛔ không dùng được (kèm lý do). Chặng Lấy hàng + Giao hàng
    /// chưa chạy của cùng xe trong đơn được đổi CÙNG lúc.</summary>
    public partial class ReassignTripForm : BaseForm
    {
        private TripReassignApiService? _service;
        private int _tripId;
        private ReassignOptionsItem? _options;
        private bool _busy;

        public ReassignTripForm()
        {
            InitializeComponent();
        }

        public ReassignTripForm(ApiServices api, int tripId, string tripLabel) : this()
        {
            _service = new TripReassignApiService(api.Connection);
            _tripId = tripId;
            lblTitle.Text = $"🔁 Đổi xe / tài xế — {tripLabel}";
            Text = $"Đổi xe / tài xế — chuyến #{tripId}";
            Load += async (_, _) => await LoadAsync();
            cboVehicle.SelectedIndexChanged += (_, _) => UpdateHint();
            cboDriver.SelectedIndexChanged += (_, _) => UpdateHint();
        }

        private async Task LoadAsync()
        {
            if (_service is null) return;
            try
            {
                _options = await _service.GetOptionsAsync(_tripId);
                if (_options.Message is not null)
                {
                    ShowStatus("⚠️ " + _options.Message, Theme.Warning);
                    btnSave.Enabled = cboVehicle.Enabled = cboDriver.Enabled = false;
                    return;
                }

                lblInfo.Text = _options.AffectedTripIds.Count > 1
                    ? $"Sẽ đổi CÙNG LÚC các chuyến chưa chạy của xe này trong đơn: #{string.Join(", #", _options.AffectedTripIds)}."
                    : $"Đổi cho chuyến #{_tripId}.";

                // Xe/tài xế dùng được lên đầu; không dùng được vẫn hiện (kèm lý do) để biết vì sao.
                var vehicles = _options.Vehicles.OrderByDescending(v => v.Eligible).ToList();
                var drivers = _options.Drivers.OrderByDescending(d => d.Eligible).ToList();
                cboVehicle.DataSource = vehicles;
                cboDriver.DataSource = drivers;
                cboVehicle.SelectedItem = vehicles.FirstOrDefault(v => v.Id == _options.CurrentVehicleId);
                cboDriver.SelectedItem = drivers.FirstOrDefault(d => d.Id == _options.CurrentDriverEmployeeId);

                var okVehicles = vehicles.Count(v => v.Eligible);
                var okDrivers = drivers.Count(d => d.Eligible);
                ShowStatus($"{okVehicles} xe và {okDrivers} tài xế dùng được (✅). Mục ⛔ kèm lý do không dùng được.", Theme.TextSecondary);
                UpdateHint();
            }
            catch (Exception ex)
            {
                ShowStatus("Lỗi tải dữ liệu: " + ex.Message, Theme.Danger);
            }
        }

        private void UpdateHint()
        {
            if (_options is null) return;
            var v = cboVehicle.SelectedItem as ReassignOptionItem;
            var d = cboDriver.SelectedItem as ReassignOptionItem;
            var unchanged = v?.Id == _options.CurrentVehicleId && d?.Id == _options.CurrentDriverEmployeeId;
            btnSave.Enabled = v is { Eligible: true } && d is { Eligible: true } && !unchanged;
            lblHint.Text = v is { Eligible: false } ? "⛔ Xe: " + v.Reason
                : d is { Eligible: false } ? "⛔ Tài xế: " + d.Reason
                : unchanged ? "Chọn xe hoặc tài xế khác với hiện tại."
                : d?.Reason is not null ? "⚠ " + d.Reason + " — cần đăng ký trước khi xuất phát."
                : "";
            lblHint.ForeColor = v is { Eligible: false } || d is { Eligible: false } ? Theme.Danger : Theme.Warning;
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            if (_service is null || _busy) return;
            if (cboVehicle.SelectedItem is not ReassignOptionItem v || cboDriver.SelectedItem is not ReassignOptionItem d) return;

            _busy = true;
            btnSave.Enabled = false;
            try
            {
                var result = await _service.ReassignAsync(_tripId, v.Id, d.Id);
                if (!result.Success)
                {
                    ShowStatus("⚠️ " + result.Message, Theme.Warning);
                    return;
                }
                MessageBox.Show(this, result.Message ?? "Đã đổi xe / tài xế.", "Đã đổi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                ShowStatus("Lỗi: " + ex.Message, Theme.Danger);
            }
            finally
            {
                _busy = false;
                if (!IsDisposed) UpdateHint();
            }
        }

        private void BtnClose_Click(object? sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void ShowStatus(string message, Color color)
        {
            lblStatus.ForeColor = color;
            lblStatus.Text = message;
        }
    }
}
