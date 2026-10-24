using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_TransportOrderManagement;

namespace TruckingSystem.Desktop.GUI_TransportOrderManagement
{
    /// <summary>ĐỔI HẠN GIAO của đơn Đã xác nhận / Đang chạy — khách xin dời ngày, hai bên thỏa
    /// thuận lùi hạn (kẹt đường, thời tiết...), hoặc BỔ SUNG hạn cho đơn cũ chưa có hạn.
    /// Bắt buộc lý do; server không cho chọn ngày đã qua và ghi nhật ký hạn cũ -> hạn mới.
    /// Gợi ý tham khảo: số ngày chạy ước tính của tuyến (cùng cách tính với Chia xe).
    /// Giao diện nằm ở ChangeDeadlineForm.Designer.cs.</summary>
    public partial class ChangeDeadlineForm : BaseForm
    {
        private readonly ApiServices? _api;
        private readonly TransportOrderDetailItem? _order;

        /// <summary>Câu thông báo server trả về khi đổi thành công.</summary>
        public string? ResultMessage { get; private set; }

        public ChangeDeadlineForm()
        {
            InitializeComponent();
            Anim.FadeIn(this);
            // Bên yêu cầu BẮT BUỘC chọn (không chọn sẵn): khách xin dời trễ hơn -> khách chịu phí lưu kho.
            cboRequester.Items.Add(new ComboOption("Customer", "👤 Khách hàng (tính phí lưu kho)"));
            cboRequester.Items.Add(new ComboOption("Company", "🏢 Công ty (không tính phí)"));
            _feeTimer.Tick += async (_, _) => { _feeTimer.Stop(); await LoadFeePreviewAsync(); };
            cboRequester.SelectedIndexChanged += (_, _) => RequestFeePreview();
            dtNewDate.ValueChanged += (_, _) => RequestFeePreview();
            FormClosed += (_, _) => _feeTimer.Dispose();
        }

        private readonly System.Windows.Forms.Timer _feeTimer = new() { Interval = 350 };
        private int _feeVersion;

        private void RequestFeePreview()
        {
            _feeTimer.Stop();
            _feeTimer.Start();
        }

        /// <summary>Phí lưu kho ƯỚC TÍNH theo hạn mới + bên yêu cầu (server tính: m³ hàng đang trong kho ×
        /// số ngày dời × đơn giá kho) — để báo khách ngay trước khi lưu.</summary>
        private async Task LoadFeePreviewAsync()
        {
            if (_api is null || _order is null) return;
            if (cboRequester.SelectedItem is not ComboOption requester)
            {
                lblFee.Text = "Chọn bên yêu cầu: khách xin dời trễ hơn hạn cũ thì tính phí lưu kho cho số ngày dời thêm.";
                lblFee.ForeColor = Theme.TextSecondary;
                return;
            }
            var version = ++_feeVersion;
            lblFee.Text = "💰 Đang tính phí lưu kho...";
            lblFee.ForeColor = Theme.TextSecondary;
            try
            {
                var p = await _api.TransportOrders.PreviewStorageFeeAsync(_order.Id, dtNewDate.Value.Date, requester.Value);
                if (IsDisposed || version != _feeVersion) return;
                lblFee.Text = (p.EstimatedFee > 0 ? "💰 " : "ℹ️ ") + p.Note;
                lblFee.ForeColor = p.EstimatedFee > 0 ? Theme.Warning : Theme.TextSecondary;
            }
            catch (Exception ex)
            {
                if (!IsDisposed && version == _feeVersion) lblFee.Text = "Không tính được phí lưu kho: " + ex.Message;
            }
        }

        public ChangeDeadlineForm(ApiServices api, TransportOrderDetailItem order) : this()
        {
            _api = api;
            _order = order;
            Text = $"Đổi hạn giao — đơn {order.Code}";
            lblCurrent.Text = order.CommittedDeliveryDate is { } due
                ? $"Hạn giao hiện tại: {due:dd/MM/yyyy}"
                : "Hạn giao hiện tại: ⚠️ chưa có (đơn cũ) — chọn hạn giao để bổ sung.";
            dtNewDate.Value = order.CommittedDeliveryDate is { } d && d.Date >= DateTime.Today ? d.Date : DateTime.Today.AddDays(1);
            Load += async (_, _) => { await LoadEstimateAsync(); await LoadFeePreviewAsync(); };
        }

        /// <summary>Tham khảo: tuyến chạy khoảng bao nhiêu ngày (đơn chưa chia xe -> giao sớm nhất ngày nào).</summary>
        private async Task LoadEstimateAsync()
        {
            if (_api is null || _order is null) return;
            try
            {
                var days = await _api.TransportOrders.EstimateRouteDaysAsync(
                    _order.PickupMethod == "SelfDelivery" ? null : _order.PickupLocationId,
                    _order.DropoffLocationId, _order.WarehouseId, _order.PickupMethod);
                if (IsDisposed) return;
                lblHint.Text = _order.Trips.Count == 0
                    ? $"🕒 Tuyến ước tính ~{days} ngày chạy — nếu chia xe hôm nay, giao sớm nhất {DateTime.Today.AddDays(days - 1):dd/MM/yyyy}."
                    : $"🕒 Tham khảo: toàn tuyến ước tính ~{days} ngày chạy. Tình trạng hạn của từng chuyến sẽ tự tính lại theo hạn mới.";
            }
            catch
            {
                if (!IsDisposed) lblHint.Text = "Không ước tính được số ngày chạy của tuyến (thiếu tọa độ / lỗi bản đồ) — vẫn đổi hạn bình thường.";
            }
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            if (_api is null || _order is null) return;
            var newDate = dtNewDate.Value.Date;
            var reason = txtReason.Text.Trim();
            if (newDate < DateTime.Today)
            {
                ShowError("Hạn giao mới không được là ngày đã qua.");
                return;
            }
            if (cboRequester.SelectedItem is not ComboOption requester)
            {
                ShowError("Vui lòng chọn Bên yêu cầu đổi hạn (Khách hàng / Công ty).");
                cboRequester.Focus();
                return;
            }
            if (reason.Length < 5)
            {
                ShowError("Vui lòng ghi lý do đổi hạn giao (ít nhất 5 ký tự) — VD \"Khách đề nghị dời ngày nhận\".");
                txtReason.Focus();
                return;
            }

            btnSave.Enabled = false;
            try
            {
                var result = await _api.TransportOrders.ChangeDeadlineAsync(_order.Id, newDate, reason, requester.Value);
                if (result.Success)
                {
                    ResultMessage = result.Message;
                    DialogResult = DialogResult.OK;
                    Close();
                    return;
                }
                ShowError(result.Message ?? "Không đổi được hạn giao.");
            }
            catch (Exception ex)
            {
                ShowError("Lỗi: " + ex.Message);
            }
            finally
            {
                if (!IsDisposed) btnSave.Enabled = true;
            }
        }

        private void ShowError(string message)
        {
            lblStatus.Text = "⚠️ " + message;
            lblStatus.ForeColor = Theme.Danger;
        }
    }
}
