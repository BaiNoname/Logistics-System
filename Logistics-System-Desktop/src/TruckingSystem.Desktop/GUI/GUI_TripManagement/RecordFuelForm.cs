using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_TripManagement;
using TruckingSystem.Desktop.SERVICE_TripManagement;

namespace TruckingSystem.Desktop.GUI_TripManagement
{
    /// <summary>Nhiên liệu của 1 chuyến:
    /// - Trên: LỊCH SỬ các lần đổ xăng (1 chuyến có thể đổ nhiều lần), tổng lít + tổng tiền,
    ///   xóa được lần ghi nhầm.
    /// - Dưới: thêm 1 lần đổ — CHỈ chọn hãng + nhập số lít; đơn giá tự lấy từ bảng giá
    ///   ở màn Nhiên liệu (giá mới nhất của hãng đó, đúng loại nhiên liệu xe dùng),
    ///   thành tiền = số lít × đơn giá.
    /// Chuyến chưa chạy / đã hủy vẫn mở được để XEM lịch sử, chỉ khóa phần thêm mới.</summary>
    public partial class RecordFuelForm : BaseForm
    {
        private sealed record ProviderOption(int? Id, string Name)
        {
            public override string ToString() => Name;
        }

        private ApiServices? _api;
        private TripFuelApiService? _fuel;
        private int _tripId;
        private FuelPriceQuoteItem? _quote;
        private int _quoteLoadId; // đổi nhà cung cấp nhanh: chỉ dùng đơn giá của lượt hỏi MỚI NHẤT
        private List<FuelLogItem> _logs = new();
        private bool _changed;
        private bool _busy;

        public RecordFuelForm()
        {
            InitializeComponent();
            TruckingSystem.Desktop.Common.DialogFit.Enable(this, root); // vừa màn hình mọi máy, bảng không bị ép mất chữ
            TruckingSystem.Desktop.Common.DialogKeys.Enable(this); // Esc = đóng
            Theme.StyleGrid(gridLogs);
            SetupGrid();
        }

        public RecordFuelForm(ApiServices api, int tripId) : this()
        {
            _api = api;
            _fuel = new TripFuelApiService(api.Connection);
            _tripId = tripId;
            lblTitle.Text = $"⛽ Nhiên liệu của chuyến #{tripId}";
            Text = $"Nhiên liệu — chuyến #{tripId}";

            Load += async (_, _) => await InitAsync();
            cboProvider.SelectedIndexChanged += async (_, _) => await LoadQuoteAsync();
            numLiters.ValueChanged += (_, _) => UpdateAmount();
        }

        private void SetupGrid()
        {
            // Các cột (colId..colCost, độ rộng tương đối, căn phải số) khai báo trong Designer.
            gridLogs.AutoGenerateColumns = false;
        }

        private async Task InitAsync()
        {
            if (_api is null || _fuel is null) return;
            try
            {
                // Hãng: các hãng ở màn Nhiên liệu (lấy từ DB), cuối cùng là "Giá thị
                // trường chung" (giá không gắn hãng, nếu có).
                var providers = await _api.Fuel.GetProvidersAsync() ?? new();
                cboProvider.Items.Clear();
                foreach (var p in providers.OrderBy(p => p.Name))
                    cboProvider.Items.Add(new ProviderOption(p.Id, p.Name));
                cboProvider.Items.Add(new ProviderOption(null, "🌐 Giá thị trường chung"));

                await LoadLogsAsync();
                cboProvider.SelectedIndex = 0; // -> tự gọi LoadQuoteAsync
            }
            catch (Exception ex)
            {
                ShowError("Lỗi tải dữ liệu: " + ex.Message);
            }
        }

        private async Task LoadLogsAsync()
        {
            if (_fuel is null) return;
            _logs = await _fuel.GetLogsAsync(_tripId);

            gridLogs.Rows.Clear();
            foreach (var log in _logs)
            {
                gridLogs.Rows.Add(log.Id,
                    log.TimestampLocal.ToString("dd/MM/yyyy HH:mm"),
                    log.StationName ?? "",
                    log.Liters.ToString("N2"),
                    $"{log.UnitPrice:N0} đ",
                    $"{log.Cost:N0} đ");
            }
            gridLogs.ClearSelection();

            lblTotal.Text = _logs.Count == 0
                ? "Chưa ghi lần đổ xăng nào cho chuyến này."
                : $"Tổng: {_logs.Count} lần đổ · {_logs.Sum(l => l.Liters):N2} lít · {_logs.Sum(l => l.Cost):N0} đ";
            btnDelete.Enabled = _logs.Count > 0;
            await AppendEstimateAsync();
        }

        /// <summary>So sánh với ƯỚC TÍNH ngay dưới dòng tổng (trước đây là mục riêng "Ước tính nhiên liệu" ở màn Chuyến):
        /// km thật từ GPS -> số lít / tiền ước tính theo định mức xe, so với tiền xăng đã ghi. Chưa đủ dữ liệu (chuyến chưa
        /// chạy, chưa có GPS, xe chưa khai báo định mức) thì không hiện gì thêm.</summary>
        private async Task AppendEstimateAsync()
        {
            if (_api is null) return;
            try
            {
                var est = await _api.Trips.GetFuelEstimateAsync(_tripId);
                if (IsDisposed || est?.EstimatedCost is not decimal estCost) return;
                // Không phán "tốn hơn / tiết kiệm" theo từng chuyến (đổ đầy bình thì xăng còn dùng cho chuyến sau).
                lblTotal.Text += $"\n📊 Chuyến này tiêu ~{est.EstimatedLiters:0.0} lít theo {est.ActualDistanceKm:0.0} km GPS · {estCost:N0} đ (tính vào đơn)";
            }
            catch
            {
                // Không lấy được ước tính không ảnh hưởng việc ghi xăng.
            }
        }

        /// <summary>Nhiên liệu trong bình của xe (ước tính) — để biết còn đổ thêm được bao nhiêu lít.</summary>
        private static string TankText(FuelPriceQuoteItem q)
        {
            if (q.TankCapacityLiters is not decimal cap || q.CurrentFuelLiters is not decimal cur) return "";
            return $"\n⛽ Bình xe đang có ~{cur:0.#} / {cap:0.#} L — còn đổ thêm được ~{Math.Max(0, cap - cur):0.#} lít (ước tính).";
        }

        private async Task LoadQuoteAsync()
        {
            if (_fuel is null || cboProvider.SelectedItem is not ProviderOption option) return;
            var loadId = ++_quoteLoadId;
            // Chưa có đơn giá của nhà cung cấp vừa chọn -> chưa cho Lưu (tránh hiện thành tiền theo giá cũ).
            btnSave.Enabled = false;
            try
            {
                var quote = await _fuel.GetPriceQuoteAsync(_tripId, option.Id);
                if (IsDisposed || loadId != _quoteLoadId) return; // đã chọn nhà cung cấp khác
                _quote = quote;

                lblInfo.Text = string.IsNullOrWhiteSpace(_quote.FuelType)
                    ? "Xe của chuyến này chưa khai báo loại nhiên liệu."
                    : $"Loại nhiên liệu của xe: {_quote.FuelType}";

                if (_quote.Found)
                {
                    var from = _quote.EffectiveDate.HasValue ? $", áp dụng từ {_quote.EffectiveDate.Value:dd/MM/yyyy}" : "";
                    lblPrice.Text = $"Đơn giá: {_quote.UnitPrice:N0} đ/lít — {_quote.SourceText}{from}" + TankText(_quote);
                    lblPrice.ForeColor = Theme.TextSecondary;
                }
                else
                {
                    lblPrice.Text = "Không tìm được đơn giá.";
                    lblPrice.ForeColor = Theme.Danger;
                }

                lblStatus.ForeColor = _quote.CanRecord && _quote.Found ? Theme.Warning : Theme.Danger;
                lblStatus.Text = _quote.Message ?? "";

                var canAdd = _quote.CanRecord && _quote.Found;
                numLiters.Enabled = txtStationName.Enabled = btnSave.Enabled = canAdd;
                cboProvider.Enabled = _quote.CanRecord;
                btnDelete.Enabled = _logs.Count > 0 && _quote.CanRecord;
                UpdateAmount();
            }
            catch (Exception ex)
            {
                if (IsDisposed || loadId != _quoteLoadId) return;
                ShowError("Lỗi lấy đơn giá: " + ex.Message);
            }
        }

        private void UpdateAmount()
        {
            if (_quote is null || !_quote.Found)
            {
                lblAmount.Text = "";
                return;
            }
            var amount = Math.Round(numLiters.Value * _quote.UnitPrice, 0);
            lblAmount.Text = numLiters.Value > 0
                ? $"Thành tiền: {amount:N0} đ  ({numLiters.Value:N2} lít × {_quote.UnitPrice:N0} đ)"
                : "Nhập số lít để tính thành tiền";
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            if (_fuel is null || _busy) return;
            if (numLiters.Value <= 0)
            {
                ShowError("Vui lòng nhập số lít lớn hơn 0.");
                return;
            }
            if (cboProvider.SelectedItem is not ProviderOption option) return;

            _busy = true;
            btnSave.Enabled = false;
            try
            {
                var note = txtStationName.Text.Trim();
                var result = await _fuel.RecordAsync(_tripId, numLiters.Value, option.Id, note.Length == 0 ? null : note);
                if (!result.Success)
                {
                    ShowError("⚠️ " + result.Message);
                    return;
                }

                _changed = true;
                numLiters.Value = 0;
                txtStationName.Clear();
                await LoadLogsAsync();
                lblStatus.ForeColor = Theme.Success;
                lblStatus.Text = "✅ Đã thêm lần đổ xăng.";
            }
            catch (Exception ex)
            {
                ShowError("Lỗi: " + ex.Message);
            }
            finally
            {
                _busy = false;
                btnSave.Enabled = _quote is { CanRecord: true, Found: true };
            }
        }

        private async void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (_fuel is null || _busy) return;
            if (gridLogs.CurrentRow?.Cells["colId"].Value is not int logId)
            {
                ShowError("Chọn 1 lần đổ trong danh sách để xóa.");
                return;
            }
            var log = _logs.FirstOrDefault(l => l.Id == logId);
            if (log is null) return;

            if (!Ask.Confirm(this, "Xóa lần đổ xăng này?", $"{log.Liters:N2} lít · {log.Cost:N0} đ · lúc {log.TimestampLocal:dd/MM/yyyy HH:mm}. Không hoàn tác được.", "🗑 Xóa lần đổ", danger: true)) return;

            _busy = true;
            try
            {
                var result = await _fuel.DeleteAsync(logId);
                if (!result.Success)
                {
                    ShowError("⚠️ " + result.Message);
                    return;
                }
                // Xóa không tính là "đã ghi nhận đổ xăng" (màn Chuyến chỉ báo thành công khi có THÊM mới)
                await LoadLogsAsync();
                lblStatus.ForeColor = Theme.Success;
                lblStatus.Text = "Đã xóa lần đổ xăng.";
            }
            catch (Exception ex)
            {
                ShowError("Lỗi: " + ex.Message);
            }
            finally
            {
                _busy = false;
            }
        }

        private void BtnClose_Click(object? sender, EventArgs e)
        {
            // OK chỉ khi có thêm/xóa — màn Chuyến dựa vào đây để báo "đã ghi nhận".
            DialogResult = _changed ? DialogResult.OK : DialogResult.Cancel;
            Close();
        }

        private void ShowError(string message)
        {
            lblStatus.ForeColor = Theme.Danger;
            lblStatus.Text = message;
        }
    }
}
