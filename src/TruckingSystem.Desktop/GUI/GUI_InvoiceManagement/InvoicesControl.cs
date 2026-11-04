using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_InvoiceManagement;
using System.Globalization;

namespace TruckingSystem.Desktop.GUI_InvoiceManagement
{
    public partial class InvoicesControl : BaseUserControl
    {
        private ApiServices? _api;
        private bool _busy;
        private List<InvoiceListItem> _invoices = new();
        private static readonly CultureInfo Vi = CultureInfo.GetCultureInfo("vi-VN");
        private readonly Button _details = Theme.MakeSecondaryButton("Xem chi tiết");

        public InvoicesControl()
        {
            InitializeComponent();
            Theme.StyleGrid(grid);
            GridDecor.Pills(grid, "Trạng_thái", (text, _) => GridDecor.StatusTone(text));
            filterBar.AddChoice("status", "Trạng thái", new[]
                { "Nháp", "Đã phát hành", "Thanh toán một phần", "Đã thanh toán", "Quá hạn", "Đã hủy" });
            filterBar.AddDate("from", "Lập từ ngày");
            filterBar.AddDate("to", "Đến ngày");
            filterBar.FilterChanged += (_, _) => BindGrid();
            ScrollFit.Enable(this, root, grid);
            toolbar.Controls.Add(_details);
            _details.Click += async (_, _) => await ShowDetailsAsync();
            grid.CellDoubleClick += async (_, e) => { if (e.RowIndex >= 0) await ShowDetailsAsync(); };
            grid.SelectionChanged += (_, _) => UpdateActions();
            UpdateActions();
            Load += async (_, _) => await LoadAsync();
        }

        public InvoicesControl(ApiServices api) : this()
        {
            _api = api;
        }

        private void BtnRefresh_Click(object? sender, EventArgs e) => _ = LoadAsync();

        private async void BtnCreate_Click(object? sender, EventArgs e)
        {
            if (_api is null || _busy) return;
            using var form = new CreateInvoiceForm(_api);
            if (form.ShowDialog(this) == DialogResult.OK)
                await LoadAsync();
        }

        private void UpdateActions()
        {
            btnRefresh.Enabled = btnCreate.Enabled = !_busy;
            filterBar.Enabled = !_busy;
            _details.Enabled = !_busy && grid.CurrentRow is not null;
        }

        private async Task ShowDetailsAsync()
        {
            if (_api is null || _busy || grid.CurrentRow?.Cells["Mã_hóa_đơn"].Value is not int id) return;
            _busy = true;
            UpdateActions();
            try
            {
                var invoice = await _api.Invoices.GetDetailAsync(id)
                    ?? throw new InvalidOperationException("Không tìm thấy hóa đơn. Vui lòng làm mới danh sách.");
                if (IsDisposed) return;
                using var form = new InvoiceDetailForm(invoice);
                form.ShowDialog(this);
            }
            catch (Exception ex)
            {
                if (IsDisposed) return;
                lblStatus.Text = "Lỗi: " + ex.Message;
                lblStatus.ForeColor = Theme.Danger;
            }
            finally
            {
                _busy = false;
                if (!IsDisposed) UpdateActions();
            }
        }

        private async Task LoadAsync()
        {
            if (_api is null || _busy) return;
            _busy = true;
            UpdateActions();
            lblStatus.Text = "Đang tải...";
            lblStatus.ForeColor = Theme.TextSecondary;
            try
            {
                var invoices = await _api.Invoices.GetInvoicesAsync();
                if (IsDisposed) return;
                _invoices = invoices;
                UpdateSummary();
                BindGrid();
            }
            catch (Exception ex)
            {
                if (IsDisposed) return;
                lblStatus.Text = "Lỗi: " + ex.Message;
                lblStatus.ForeColor = Theme.Danger;
            }
            finally
            {
                _busy = false;
                if (!IsDisposed) UpdateActions();
            }
        }

        private void UpdateSummary()
        {
            var today = DateTime.UtcNow.AddHours(7);
            var month = _invoices.Where(i => i.IssueDate.AddHours(7).Year == today.Year
                && i.IssueDate.AddHours(7).Month == today.Month).ToList();
            lblSummaryPeriod.Text = $"Thống kê tháng {today:MM/yyyy} · Không thay đổi theo bộ lọc danh sách";
            lblMonthCount.Text = $"{month.Count:N0} hóa đơn";
            lblRevenue.Text = month.Where(i => i.Status != "Cancelled").Sum(i => i.TotalAmount).ToString("N2", Vi) + " đ";
            lblUnpaid.Text = $"{month.Count(i => i.Status is not ("Paid" or "Cancelled")):N0} hóa đơn";
            lblOverdue.Text = $"Trong đó quá hạn: {month.Count(i => i.Status == "Overdue"):N0}";
            lblPaid.Text = $"{month.Count(i => i.Status == "Paid"):N0} hóa đơn";
        }

        private void BindGrid()
        {
            var from = filterBar.Date("from");
            var to = filterBar.Date("to");
            var invalidRange = from.HasValue && to.HasValue && from > to;
            var selectedId = grid.CurrentRow?.Cells["Mã_hóa_đơn"].Value;
            var rows = _invoices.Where(i => !invalidRange
                && filterBar.MatchesSearch(i.Id, i.OrderCode, i.CustomerName)
                && filterBar.Is("status", TranslateStatus(i.Status))
                && (!from.HasValue || i.IssueDate.AddHours(7).Date >= from.Value)
                && (!to.HasValue || i.IssueDate.AddHours(7).Date <= to.Value)).ToList();
            grid.DataSource = rows.Select(i => new
            {
                Mã_hóa_đơn = i.Id,
                Mã_đơn = i.OrderCode,
                Khách_hàng = i.CustomerName,
                Tổng_tiền = i.TotalAmount.ToString("N2", Vi) + " đ",
                Trạng_thái = TranslateStatus(i.Status),
                Ngày_lập = i.IssueDate.AddHours(7).ToString("dd/MM/yyyy"),
            }).ToList();
            if (grid.Columns["Mã_hóa_đơn"] is { } idColumn) idColumn.Visible = false;
            foreach (DataGridViewRow row in grid.Rows)
                if (Equals(row.Cells["Mã_hóa_đơn"].Value, selectedId)) grid.CurrentCell = row.Cells["Mã_đơn"];
            filterBar.SetCount(rows.Count, _invoices.Count);
            lblStatus.ForeColor = invalidRange ? Theme.Danger : Theme.TextSecondary;
            lblStatus.Text = invalidRange ? "Ngày bắt đầu phải trước hoặc bằng ngày kết thúc."
                : rows.Count == 0 ? "Không có hóa đơn phù hợp. Thử đổi điều kiện hoặc chọn Xóa lọc."
                : $"Hiển thị {rows.Count}/{_invoices.Count} hóa đơn · Chọn Xem chi tiết hoặc nhấp đúp vào hóa đơn.";
            UpdateActions();
        }

        private static string TranslateStatus(string s) => s switch
        {
            "Draft" => "Nháp", "Sent" => "Đã phát hành", "PartiallyPaid" => "Thanh toán một phần",
            "Paid" => "Đã thanh toán", "Overdue" => "Quá hạn", "Cancelled" => "Đã hủy", _ => s,
        };
    }
}
