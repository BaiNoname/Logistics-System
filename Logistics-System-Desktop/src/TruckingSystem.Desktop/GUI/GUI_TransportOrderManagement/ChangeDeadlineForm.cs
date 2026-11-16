using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_TransportOrderManagement;

namespace TruckingSystem.Desktop.GUI_TransportOrderManagement
{
    /// <summary>ĐỔI HẠN GIAO của đơn Đã xác nhận / Đang chạy — khách xin dời ngày, hai bên thỏa
    /// thuận lùi hạn (kẹt đường, thời tiết...), hoặc BỔ SUNG hạn cho đơn cũ chưa có hạn.
    /// Bắt buộc lý do; server không cho chọn ngày đã qua và ghi nhật ký hạn cũ -> hạn mới.
    /// Gợi ý tham khảo: số ngày chạy ước tính của tuyến (cùng cách tính với Chia xe).
    /// Bên yêu cầu quyết định HẠN TÍNH TRỄ: khách dời -> hạn mới là mốc tính đúng hạn; công ty dời -> vẫn tính trễ theo hạn cũ.
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
            TruckingSystem.Desktop.Common.DialogFit.Enable(this, root); // vừa màn hình mọi máy, bảng không bị ép mất chữ
            Anim.FadeIn(this);
            // Bên yêu cầu BẮT BUỘC chọn (không chọn sẵn): quyết định HẠN TÍNH TRỄ (mốc xét giao đúng hạn / trễ).
            cboRequester.Items.Add(new ComboOption("Customer", "👤 Khách hàng (hạn mới là mốc tính đúng hạn)"));
            cboRequester.Items.Add(new ComboOption("Company", "🏢 Công ty (vẫn tính trễ theo hạn cũ)"));
            cboRequester.SelectedIndexChanged += (_, _) => UpdateRequesterHint();
            dtNewDate.ValueChanged += (_, _) => UpdateRequesterHint();
        }

        /// <summary>Dòng nhắc đổi theo bên yêu cầu: khách dời -> hạn mới thành mốc tính đúng hạn; công ty dời -> vẫn
        /// tính trễ theo hạn cũ. Đổi hạn KHÔNG tính phí riêng (phí lưu kho tự tính theo ngày hàng nằm kho).</summary>
        private void UpdateRequesterHint()
        {
            if (_order is null) return;
            const string feeNote = "\nℹ️ Đổi hạn giao không tính phí riêng — phí lưu kho tự tính theo số ngày hàng nằm kho (miễn phí vài ngày đầu theo kho).";
            var newDate = dtNewDate.Value.Date;
            var basis = (_order.LateBasisDate ?? _order.CommittedDeliveryDate)?.Date;
            switch ((cboRequester.SelectedItem as ComboOption)?.Value)
            {
                case "Customer":
                    lblFee.Text = $"👤 Khách dời hạn: hạn tính trễ đổi thành {newDate:dd/MM}. Giao trước hoặc đúng {newDate:dd/MM} là đúng hạn." + feeNote;
                    lblFee.ForeColor = Theme.Success;
                    break;
                case "Company" when basis is DateTime b && newDate > b:
                    lblFee.Text = $"🏢 Công ty dời hạn: hạn mới chỉ để báo khách. Giao sau {b:dd/MM} vẫn tính là giao trễ " +
                                  $"(VD giao {b.AddDays(2):dd/MM} = trễ 2 ngày)." + feeNote;
                    lblFee.ForeColor = Theme.Warning;
                    break;
                case "Company":
                    lblFee.Text = $"🏢 Công ty đổi hạn: giao trước hoặc đúng {newDate:dd/MM} là đúng hạn." + feeNote;
                    lblFee.ForeColor = Theme.TextSecondary;
                    break;
                default:
                    lblFee.Text = "Chọn bên yêu cầu: khách dời thì hạn mới là mốc tính đúng hạn; công ty dời thì vẫn tính trễ theo hạn cũ.";
                    lblFee.ForeColor = Theme.TextSecondary;
                    break;
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
            if (order.HasLateBasis)
                lblCurrent.Text += $"   ·   Hạn tính trễ: {order.LateBasisDate:dd/MM/yyyy} (công ty đã dời hạn)";
            UpdateRequesterHint();
            Load += async (_, _) => await LoadEstimateAsync();
        }

        /// <summary>Mở sẵn cho việc "báo khách & dời hạn": điền ngày gợi ý, chọn sẵn bên yêu cầu, gợi ý lý do (vẫn sửa được).</summary>
        public void Preset(DateTime? newDate, string requester, string? reason = null)
        {
            if (newDate is DateTime d)
            {
                var v = d.Date < dtNewDate.MinDate ? dtNewDate.MinDate : d.Date > dtNewDate.MaxDate ? dtNewDate.MaxDate : d.Date;
                dtNewDate.Value = v;
            }
            cboRequester.SelectedItem = cboRequester.Items.Cast<ComboOption>().FirstOrDefault(o => o.Value == requester);
            UpdateRequesterHint();
            if (!string.IsNullOrWhiteSpace(reason) && string.IsNullOrWhiteSpace(txtReason.Text)) txtReason.Text = reason;
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
