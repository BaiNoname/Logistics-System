using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_TransportOrderManagement;

namespace TruckingSystem.Desktop.GUI_TransportOrderManagement
{
    /// <summary>GHI NHẬN BỔ SUNG PHÍ LƯU KHO do khách chậm nhận hàng mà nhân viên quên "Đổi hạn giao" —
    /// dùng được cả khi đơn đã Hoàn tất / Đã hủy. KHÔNG đổi hạn giao (tỉ lệ giao đúng hạn giữ nguyên).
    /// Chỉ cần chọn ngày cuối hàng nằm kho do khách + lý do: server tính phí từ ngày SAU hạn giao tới ngày đó,
    /// mỗi ngày = m³ hàng thực sự còn trong kho × đơn giá kho (ngày nào hàng đã xuất / đã tính rồi thì bỏ qua).
    /// Giao diện nằm ở RecordStorageFeeForm.Designer.cs.</summary>
    public partial class RecordStorageFeeForm : BaseForm
    {
        private readonly ApiServices? _api;
        private readonly TransportOrderDetailItem? _order;

        /// <summary>Câu thông báo server trả về khi ghi nhận thành công (có diễn giải số tiền).</summary>
        public string? ResultMessage { get; private set; }

        public RecordStorageFeeForm()
        {
            InitializeComponent();
            Anim.FadeIn(this);
            dtEndDate.ValueChanged += (_, _) => UpdateHint();
        }

        public RecordStorageFeeForm(ApiServices api, TransportOrderDetailItem order) : this()
        {
            _api = api;
            _order = order;
            Text = $"Ghi nhận phí lưu kho bổ sung — đơn {order.Code}";
            var deadline = order.CommittedDeliveryDate?.Date;
            lblInfo.Text = $"Đơn {order.Code} ({order.CustomerName}) · Trạng thái: {TruckingSystem.Desktop.GUI_Shared.ReportFormat.OrderStatus(order.Status)} · " +
                           $"Kho: {order.WarehouseName} · Hạn giao: {(deadline is { } d ? d.ToString("dd/MM/yyyy") : "chưa có")}\n" +
                           "Dùng khi khách chậm nhận hàng mà lúc đó chưa \"Đổi hạn giao\". Hạn giao của đơn GIỮ NGUYÊN.";
            dtEndDate.Value = deadline is { } dl && dl.AddDays(1) <= DateTime.Today ? DateTime.Today : (deadline ?? DateTime.Today).AddDays(1);
            UpdateHint();
        }

        private void UpdateHint()
        {
            if (_order?.CommittedDeliveryDate is not DateTime deadline)
            {
                lblHint.Text = "⚠️ Đơn chưa có hạn giao — không xác định được từ ngày nào tính phí.";
                btnSave.Enabled = false;
                return;
            }
            var from = deadline.Date.AddDays(1);
            var to = dtEndDate.Value.Date;
            if (to < from)
            {
                lblHint.Text = $"⚠️ Ngày phải từ {from:dd/MM/yyyy} trở đi (sau hạn giao {deadline:dd/MM/yyyy}).";
                btnSave.Enabled = false;
                return;
            }
            btnSave.Enabled = true;
            lblHint.Text = $"💰 Sẽ xét {(to - from).Days + 1} ngày: {from:dd/MM/yyyy} → {to:dd/MM/yyyy}. Mỗi ngày hàng của đơn có nằm kho " +
                           "(kể cả ngày xuất kho) tính = m³ nằm kho × đơn giá kho; ngày hàng đã đi rồi hoặc đã tính ở lần trước thì không tính.";
        }

        private async void BtnSave_Click(object? sender, EventArgs e)
        {
            if (_api is null || _order is null) return;
            var reason = txtReason.Text.Trim();
            if (reason.Length < 5)
            {
                ShowError("Phải ghi lý do (ít nhất 5 ký tự).");
                txtReason.Focus();
                return;
            }
            btnSave.Enabled = false;
            try
            {
                var result = await _api.TransportOrders.RecordStorageFeeAsync(_order.Id, dtEndDate.Value.Date, reason);
                if (result.Success)
                {
                    ResultMessage = result.Message;
                    DialogResult = DialogResult.OK;
                    Close();
                    return;
                }
                ShowError(result.Message ?? "Không ghi nhận được.");
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
