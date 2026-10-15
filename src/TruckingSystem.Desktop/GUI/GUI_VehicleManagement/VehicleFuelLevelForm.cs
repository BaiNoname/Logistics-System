using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_VehicleManagement;

namespace TruckingSystem.Desktop.GUI_VehicleManagement
{
    /// <summary>⛽ NHIÊN LIỆU TRONG BÌNH của 1 xe.
    /// Hiện có (ước tính) = Mức đo gần nhất + Đổ xăng sau mốc đo − Tiêu hao (km GPS × định mức L/100km).
    /// Ghi đổ xăng (màn Chuyến) tự cộng vào, xe chạy tự trừ theo GPS — không phải sửa tay. Định kỳ
    /// (hoặc khi đổ đầy bình) Vận hành nhập MỨC ĐO THỰC TẾ để đặt lại mốc, tránh sai số tích lũy.
    /// Giao diện nằm ở VehicleFuelLevelForm.Designer.cs.</summary>
    public partial class VehicleFuelLevelForm : BaseForm
    {
        private readonly ApiServices? _api;
        private readonly int _vehicleId;
        private VehicleFuelLevelItem? _level;

        public VehicleFuelLevelForm()
        {
            InitializeComponent();
            chkSetMeasured.CheckedChanged += (_, _) => numMeasured.Enabled = chkSetMeasured.Checked;
            btnFull.Click += (_, _) => { chkSetMeasured.Checked = true; numMeasured.Value = Math.Min(numMeasured.Maximum, numCapacity.Value); };
            numCapacity.ValueChanged += (_, _) => numMeasured.Maximum = Math.Max(numCapacity.Value, 1);
        }

        public VehicleFuelLevelForm(ApiServices api, int vehicleId, string plate) : this()
        {
            _api = api;
            _vehicleId = vehicleId;
            Text = $"Nhiên liệu — xe {plate}";
            lblTitle.Text = $"⛽ Nhiên liệu trong bình — xe {plate}";
            Load += async (_, _) => await LoadAsync();
        }

        private async Task LoadAsync()
        {
            if (_api is null) return;
            try
            {
                _level = await _api.Vehicles.GetFuelLevelAsync(_vehicleId);
                if (_level.TankCapacityLiters is decimal cap) numCapacity.Value = Math.Clamp(cap, numCapacity.Minimum, numCapacity.Maximum);
                numMeasured.Maximum = Math.Max(numCapacity.Value, 1);
                numMeasured.Value = Math.Clamp(_level.CurrentLiters ?? 0, 0, numMeasured.Maximum);
                // Chưa từng khai mức đo -> bắt buộc khai lần đầu.
                chkSetMeasured.Checked = _level.MeasuredLiters is null;
                numMeasured.Enabled = chkSetMeasured.Checked;
                RenderSummary();
            }
            catch (Exception ex)
            {
                ShowStatus("Lỗi tải dữ liệu: " + ex.Message, Theme.Danger);
            }
        }

        private void RenderSummary()
        {
            var l = _level;
            if (l is null) return;
            if (l.MeasuredLiters is null || l.MeasuredAt is null)
            {
                lblCurrent.Text = "Chưa khai báo mức nhiên liệu cho xe này.";
                lblCurrent.ForeColor = Theme.Warning;
                lblBreakdown.Text = "Nhập dung tích bình và mức đo hiện tại (VD đổ đầy bình thì mức = dung tích), bấm Lưu. " +
                                    "Từ đó hệ thống tự cộng khi ghi đổ xăng và tự trừ theo quãng đường GPS × định mức.";
                return;
            }
            var cap = l.TankCapacityLiters;
            var pct = cap is decimal c && c > 0 && l.CurrentLiters is decimal cur ? $" ({cur / c * 100:0}%)" : "";
            lblCurrent.Text = $"Hiện có (ước tính): ~{l.CurrentLiters:0.#} L" + (cap is decimal cc ? $" / {cc:0.#} L" : "") + pct;
            lblCurrent.ForeColor = l.CurrentLiters is decimal x && cap is decimal y && y > 0 && x / y <= 0.15m ? Theme.Danger : Theme.Success;
            var measuredAt = DateTime.SpecifyKind(l.MeasuredAt.Value, DateTimeKind.Utc).ToLocalTime();
            lblBreakdown.Text =
                $"= Mức đo {l.MeasuredLiters:0.#} L lúc {measuredAt:dd/MM/yyyy HH:mm}\n" +
                $"+ Đổ xăng sau mốc đo: {l.RefueledLiters:0.#} L\n" +
                $"− Tiêu hao ước tính: {l.ConsumedLiters:0.#} L (đi {l.ConsumedKm:0.#} km theo GPS × định mức " +
                (l.FuelConsumptionPer100Km is decimal n ? $"{n:0.#} L/100km)" : "— xe CHƯA khai định mức nên chưa trừ được)") +
                "\nĐịnh mức là ước tính — nên nhập lại mức đo thực tế định kỳ (hoặc mỗi lần đổ đầy bình).";
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            if (_api is null) return;
            var capacity = numCapacity.Value;
            decimal? measured = chkSetMeasured.Checked ? numMeasured.Value : null;
            if (measured > capacity)
            {
                ShowStatus("Mức đo không được lớn hơn dung tích bình.", Theme.Danger);
                return;
            }
            btnSave.Enabled = false;
            try
            {
                var result = await _api.Vehicles.UpdateFuelLevelAsync(_vehicleId, capacity, measured);
                if (!result.Success)
                {
                    ShowStatus("⚠️ " + result.Message, Theme.Danger);
                    return;
                }
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                ShowStatus("Lỗi: " + ex.Message, Theme.Danger);
            }
            finally
            {
                if (!IsDisposed) btnSave.Enabled = true;
            }
        }

        private void ShowStatus(string message, Color color)
        {
            lblStatus.Text = message;
            lblStatus.ForeColor = color;
        }
    }
}
