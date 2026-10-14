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
            Load += async (_, _) => await LoadEstimateAsync();
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
            if (reason.Length < 5)
            {
                ShowError("Vui lòng ghi lý do đổi hạn giao (ít nhất 5 ký tự) — VD \"Khách đề nghị dời ngày nhận\".");
                txtReason.Focus();
                return;
            }

            btnSave.Enabled = false;
            try
            {
                var result = await _api.TransportOrders.ChangeDeadlineAsync(_order.Id, newDate, reason);
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
