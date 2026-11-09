using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_TripManagement;
using TruckingSystem.Desktop.SERVICE_TripManagement;

namespace TruckingSystem.Desktop.GUI_TripManagement
{
    /// <summary>Đổi xe và/hoặc tài xế cho chuyến CHƯA CHẠY — gỡ chuyến bị kẹt (xe hỏng, giấy
    /// tờ / bằng lái hết hạn, tài xế nghỉ...). Server đánh giá theo đúng điều kiện Chia xe; ô chọn CHỈ HIỆN
    /// xe / tài xế DÙNG ĐƯỢC (không bày cả danh sách không chọn được). Xe / tài xế đang gán mà không còn dùng
    /// được thì báo lý do ở dòng gợi ý. Chặng Lấy hàng + Giao hàng chưa chạy của cùng xe trong đơn được đổi CÙNG lúc.</summary>
    public partial class ReassignTripForm : BaseForm
    {
        private TripReassignApiService? _service;
        private int _tripId;
        private ReassignOptionsItem? _options;
        private bool _busy;

        public ReassignTripForm()
        {
            InitializeComponent();
            TruckingSystem.Desktop.Common.DialogFit.Enable(this, root); // vừa màn hình mọi máy, bảng không bị ép mất chữ
            TruckingSystem.Desktop.Common.DialogKeys.Enable(this, btnSave); // Esc = đóng, Enter = Lưu
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

                // CHỈ xe / tài xế DÙNG ĐƯỢC; mục đang gán thêm chữ "(đang gán)" để biết đang là ai.
                var vehicles = Usable(_options.Vehicles, _options.CurrentVehicleId);
                var drivers = Usable(_options.Drivers, _options.CurrentDriverEmployeeId);
                cboVehicle.DataSource = vehicles;
                cboDriver.DataSource = drivers;
                cboVehicle.SelectedItem = vehicles.FirstOrDefault(v => v.Id == _options.CurrentVehicleId) ?? vehicles.FirstOrDefault();
                cboDriver.SelectedItem = drivers.FirstOrDefault(d => d.Id == _options.CurrentDriverEmployeeId) ?? drivers.FirstOrDefault();
                cboVehicle.Enabled = vehicles.Count > 0;
                cboDriver.Enabled = drivers.Count > 0;

                // Xe / tài xế ĐANG GÁN không còn dùng được -> nói rõ vì sao phải đổi.
                var notes = new List<string>();
                if (_options.Vehicles.FirstOrDefault(v => v.Id == _options.CurrentVehicleId) is { Eligible: false } cv)
                    notes.Add($"Xe đang gán {cv.Text} không dùng được: {cv.Reason}");
                if (_options.Drivers.FirstOrDefault(d => d.Id == _options.CurrentDriverEmployeeId) is { Eligible: false } cd)
                    notes.Add($"Tài xế đang gán {cd.Text} không dùng được: {cd.Reason}");
                if (vehicles.Count == 0) notes.Add("Hiện KHÔNG có xe nào chạy được chuyến này (xe bận, bảo trì, giấy tờ hết hạn, không đủ tải...).");
                if (drivers.Count == 0) notes.Add("Hiện KHÔNG có tài xế nào chạy được chuyến này (bận, nghỉ, bằng lái không hợp...).");
                ShowStatus(notes.Count > 0 ? "⚠️ " + string.Join("\n⚠️ ", notes)
                        : $"{vehicles.Count} xe và {drivers.Count} tài xế chạy được chuyến này.",
                    notes.Count > 0 ? Theme.Warning : Theme.TextSecondary);
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
            // Hạng bằng không lái được xe đang chọn -> báo NGAY (trước đây phải bấm Lưu mới bị từ chối).
            string? licenseBlock = v is not null && d?.CannotDrive is { } cd && cd.TryGetValue(v.Id, out var why) ? why : null;
            btnSave.Enabled = !_busy && v is { Eligible: true } && d is { Eligible: true } && !unchanged && licenseBlock is null;
            lblHint.Text = v is null || d is null ? "Không đổi được: thiếu xe hoặc tài xế chạy được chuyến này."
                : licenseBlock is not null ? $"⛔ Tài xế này không lái được xe đang chọn: {licenseBlock} — chọn xe hoặc tài xế khác."
                : unchanged ? "Chọn xe hoặc tài xế khác với hiện tại."
                : d.Reason is not null ? "⚠ " + d.Reason + " — cần đăng ký trước khi xuất phát."
                : "";
            lblHint.ForeColor = v is null || d is null || licenseBlock is not null ? Theme.Danger : Theme.Warning;
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
                Toast.Show((Control?)Owner ?? this, "✅ " + (result.Message ?? "Đã đổi xe / tài xế."));
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

        /// <summary>Chỉ mục dùng được; mục đang gán đánh dấu "(đang gán)" (bản sao — không sửa dữ liệu gốc).</summary>
        private static List<ReassignOptionItem> Usable(IEnumerable<ReassignOptionItem> items, int currentId)
            => items.Where(i => i.Eligible)
                .Select(i => new ReassignOptionItem
                {
                    Id = i.Id,
                    Eligible = true,
                    Reason = i.Reason,
                    CannotDrive = i.CannotDrive,
                    Text = i.Id == currentId ? i.Text + "  (đang gán)" : i.Text,
                })
                .OrderByDescending(i => i.Id == currentId)
                .ToList();

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
