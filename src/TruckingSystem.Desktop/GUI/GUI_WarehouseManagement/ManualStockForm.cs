using System.Drawing;
using System.Windows.Forms;
using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_TransportOrderManagement;
using TruckingSystem.Desktop.DTO_WarehouseManagement;

namespace TruckingSystem.Desktop.GUI_WarehouseManagement
{
    /// <summary>
    /// Nhập/Xuất kho THỦ CÔNG — dùng cho trường hợp KHÔNG qua Trip (hàng dư, hàng
    /// trả về, sửa sai lệch tồn kho...). Khác luồng tự động (gắn Trip) đã có sẵn —
    /// đây là "lối tắt" để Dispatcher/nhân viên kho chủ động ghi nhận khi cần.
    /// CHO PHÉP chọn tay vị trí nhập (không bắt buộc để hệ thống tự Best-Fit) —
    /// danh sách vị trí tô màu theo % đầy giống hệt sơ đồ kho chính.
    /// </summary>
    public partial class ManualStockForm : BaseForm
    {
        private ApiServices? _api;
        private int _warehouseId;
        private List<TransportOrderListItem> _orders = new();
        private List<StorageBinItem> _availableBins = new();

        public ManualStockForm()
        {
            InitializeComponent();
            cboOrder.SelectedIndexChanged += async (_, _) => await LoadCargoLinesAsync();
            cboCargoLine.SelectedIndexChanged += async (_, _) => await LoadAvailableBinsAsync();
            Theme.StyleGrid(gridHistory);
            Theme.StyleGrid(gridBins);
            gridBins.CellFormatting += GridBins_CellFormatting;
        }

        public ManualStockForm(ApiServices api, int warehouseId) : this()
        {
            _api = api;
            _warehouseId = warehouseId;
            Load += async (_, _) => { await LoadOrdersAsync(); await LoadHistoryAsync(); };
        }

        private async Task LoadOrdersAsync()
        {
            if (_api is null) return;
            try
            {
                _orders = await _api.TransportOrders.GetTransportOrdersAsync();
                cboOrder.DataSource = _orders;
                cboOrder.DisplayMember = nameof(TransportOrderListItem.DisplayText);
                cboOrder.ValueMember = nameof(TransportOrderListItem.Id);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Lỗi tải danh sách đơn: " + ex.Message;
            }
        }

        private async Task LoadCargoLinesAsync()
        {
            if (_api is null || cboOrder.SelectedValue is not int orderId) return;
            try
            {
                var lines = await _api.Warehouses.GetCargoLinesForOrderAsync(orderId);
                cboCargoLine.DataSource = lines;
                cboCargoLine.DisplayMember = nameof(CargoLineOptionItem.CargoType);
                cboCargoLine.ValueMember = nameof(CargoLineOptionItem.Id);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Lỗi tải dòng hàng: " + ex.Message;
            }
        }

        /// <summary>Tải danh sách vị trí còn trống đúng khu vực — hiện kèm dòng
        /// "🤖 Tự động (Best-Fit)" ở đầu để người dùng vẫn có thể để hệ thống tự chọn.</summary>
        private async Task LoadAvailableBinsAsync()
        {
            if (_api is null || cboCargoLine.SelectedValue is not int cargoLineId) { gridBins.DataSource = null; return; }
            try
            {
                _availableBins = await _api.Warehouses.GetAvailableBinsForCargoAsync(_warehouseId, cargoLineId);

                var rows = new List<object>
                {
                    new { Id = 0, Vị_trí = "🤖 Tự động (Best-Fit)", Đã_dùng = "", Còn_trống_m3 = "", Cách_cửa_m = "" }
                };
                rows.AddRange(_availableBins.Select(b => (object)new
                {
                    Id = b.Id,
                    Vị_trí = b.Code,
                    Đã_dùng = $"{(b.CapacityM3 == 0 ? 0 : b.CurrentUsedM3 / b.CapacityM3 * 100):0}%",
                    Còn_trống_m3 = (b.CapacityM3 - b.CurrentUsedM3).ToString("0.00"),
                    Cách_cửa_m = b.DistanceFromDoorMeters.ToString("0.0"),
                }));
                gridBins.DataSource = rows;
                if (gridBins.Columns["Id"] != null) gridBins.Columns["Id"]!.Visible = false;
                if (gridBins.Rows.Count > 0) gridBins.Rows[0].Selected = true; // mặc định chọn "Tự động"
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Lỗi tải danh sách vị trí: " + ex.Message;
            }
        }

        /// <summary>Tô màu từng dòng theo % đầy — GIỐNG HỆT màu trên sơ đồ kho
        /// chính (xanh/vàng/đỏ), để người chọn thấy ngay vị trí nào còn nhiều chỗ.</summary>
        private void GridBins_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (gridBins.Rows[e.RowIndex].DataBoundItem is null) return;
            var binId = (int)gridBins.Rows[e.RowIndex].Cells["Id"].Value;
            if (binId == 0) return; // dòng "Tự động" giữ màu mặc định

            var bin = _availableBins.FirstOrDefault(b => b.Id == binId);
            if (bin is null) return;

            var pct = bin.CapacityM3 == 0 ? 0 : (double)(bin.CurrentUsedM3 / bin.CapacityM3);
            gridBins.Rows[e.RowIndex].DefaultCellStyle.BackColor = pct switch
            {
                >= 0.85 => Color.FromArgb(254, 226, 226), // đỏ nhạt
                >= 0.5 => Color.FromArgb(254, 243, 199),  // vàng nhạt
                _ => Color.FromArgb(220, 252, 231),        // xanh nhạt
            };
        }

        private async Task LoadHistoryAsync()
        {
            if (_api is null) return;
            try
            {
                var history = await _api.Warehouses.GetTransactionHistoryAsync(_warehouseId);
                gridHistory.DataSource = history.Select(h => new
                {
                    Loại = h.Type,
                    Thời_gian = h.OccurredAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm"),
                    Đơn_hàng = h.OrderCode,
                    Loại_hàng = h.CargoType,
                    Số_lượng = h.Qty,
                    Người_thực_hiện = h.ByEmployeeName,
                }).ToList();
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Lỗi tải lịch sử: " + ex.Message;
            }
        }

        private async void BtnReceive_Click(object? sender, EventArgs e) => await SubmitAsync(isReceive: true);
        private async void BtnIssue_Click(object? sender, EventArgs e) => await SubmitAsync(isReceive: false);

        private async Task SubmitAsync(bool isReceive)
        {
            if (_api is null) return;
            if (cboOrder.SelectedValue is not int orderId || cboCargoLine.SelectedValue is not int cargoLineId)
            {
                lblStatus.Text = "Vui lòng chọn đủ đơn hàng và dòng hàng.";
                return;
            }
            if (numQty.Value <= 0)
            {
                lblStatus.Text = "Số lượng phải lớn hơn 0.";
                return;
            }

            // Vị trí đã chọn tay (nếu có) — chỉ áp dụng cho chiều NHẬP; dòng "Tự
            // động" (Id=0) hoặc chưa chọn gì thì để null (hệ thống tự Best-Fit)
            int? chosenBinId = null;
            if (isReceive && gridBins.CurrentRow?.Cells["Id"].Value is int selectedId && selectedId != 0)
                chosenBinId = selectedId;

            try
            {
                if (isReceive)
                {
                    var result = await _api.Warehouses.ReceiveManualAsync(_warehouseId, orderId, cargoLineId, numQty.Value, chosenBinId);
                    lblStatus.Text = result.Success
                        ? $"✅ Đã nhập kho — vị trí {result.BinCode ?? "(chưa xếp được)"}"
                        : "⚠️ " + result.Message;
                    lblStatus.ForeColor = result.Success ? Theme.Success : Theme.Warning;
                }
                else
                {
                    var result = await _api.Warehouses.IssueManualAsync(_warehouseId, orderId, cargoLineId, numQty.Value);
                    lblStatus.Text = result.Success ? "✅ Đã xuất kho." : "⚠️ " + result.Message;
                    lblStatus.ForeColor = result.Success ? Theme.Success : Theme.Warning;
                }
                await LoadHistoryAsync();
                await LoadAvailableBinsAsync(); // vẽ lại danh sách NGAY để thấy màu/vị trí vừa đổi
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Lỗi: " + ex.Message;
                lblStatus.ForeColor = Theme.Danger;
            }
        }
    }
}