using System.Drawing;
using System.Windows.Forms;
using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_ReportManagement;

namespace TruckingSystem.Desktop.GUI_Shared
{
    /// <summary>Chi tiết chi phí 1 đơn hàng — mở khi double-click 1 dòng ở bảng
    /// Lãi/Lỗ của màn Tổng quan. Liệt kê TỪNG khoản chi (nhiên liệu, chi phí
    /// phát sinh, sự cố) theo từng chuyến của đơn, kèm doanh thu và lãi/lỗ.</summary>
    public partial class OrderCostDetailForm : BaseForm
    {
        private readonly ApiServices? _api;
        private readonly int _orderId;

        private readonly Label _lblTitle = new() { Font = Theme.FontSectionTitle, ForeColor = Theme.TextPrimary, AutoSize = true, Left = 20, Top = 16 };
        private readonly Label _lblSub = new() { Font = Theme.FontSmall, ForeColor = Theme.TextSecondary, AutoSize = true, Left = 20, Top = 46 };
        private readonly FlowLayoutPanel _summary = new() { Left = 20, Top = 74, Height = 70, WrapContents = false };
        private readonly Label _lblByCategory = new() { Font = Theme.FontSmall, ForeColor = Theme.TextSecondary, AutoSize = true, Left = 20, Top = 152 };
        private readonly DataGridView _grid = new() { Left = 20, Top = 178 };
        private readonly Button _btnClose = Theme.MakeSecondaryButton("Đóng", 110, 34);

        public OrderCostDetailForm()
        {
            InitializeComponent();
            BuildLayout();
        }

        public OrderCostDetailForm(ApiServices api, int orderId) : this()
        {
            _api = api;
            _orderId = orderId;
            Load += async (_, _) => await LoadAsync();
        }

        private void BuildLayout()
        {
            Theme.StyleGrid(_grid);
            _grid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            _grid.Width = ClientSize.Width - 40;
            _grid.Height = ClientSize.Height - 178 - 64;
            _grid.CellFormatting += Grid_CellFormatting;

            _summary.Width = ClientSize.Width - 40;
            _summary.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            _btnClose.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            _btnClose.Left = ClientSize.Width - 20 - _btnClose.Width;
            _btnClose.Top = ClientSize.Height - 20 - _btnClose.Height;
            _btnClose.Click += (_, _) => Close();
            CancelButton = _btnClose;

            _lblTitle.Text = "Đang tải...";
            Controls.AddRange(new Control[] { _lblTitle, _lblSub, _summary, _lblByCategory, _grid, _btnClose });
        }

        private async Task LoadAsync()
        {
            if (_api is null) return;
            try
            {
                var d = await _api.Reports.GetOrderCostDetailAsync(_orderId);
                var profit = d.Revenue - d.TotalCost;

                Text = $"Chi phí đơn {d.OrderCode}";
                _lblTitle.Text = $"💰 Chi phí đơn {d.OrderCode}";
                _lblSub.Text = $"Khách hàng: {d.CustomerName}   •   Trạng thái: {ReportFormat.OrderStatus(d.Status)}";

                _summary.Controls.Clear();
                _summary.Controls.Add(SummaryCard("Doanh thu", ReportFormat.FullMoney(d.Revenue), Theme.Primary));
                _summary.Controls.Add(SummaryCard("Tổng chi phí", ReportFormat.FullMoney(d.TotalCost), Theme.Warning));
                _summary.Controls.Add(SummaryCard(profit < 0 ? "Lỗ" : "Lãi", ReportFormat.FullMoney(profit),
                    profit < 0 ? Theme.Danger : Theme.Success));
                if (d.Status == "Cancelled")
                    _summary.Controls.Add(SummaryCard("Ghi chú", "Đơn đã hủy — chi phí là tiền mất", Theme.Danger));

                _lblByCategory.Text = d.Lines.Count == 0
                    ? "Đơn này chưa ghi nhận khoản chi nào (nhiên liệu, phát sinh, sự cố)."
                    : "Theo loại:  " + string.Join("   •   ", d.Lines
                        .GroupBy(l => l.Category)
                        .OrderByDescending(g => g.Sum(x => x.Amount))
                        .Select(g => $"{g.Key}: {ReportFormat.FullMoney(g.Sum(x => x.Amount))} ({ReportFormat.Percent(g.Sum(x => x.Amount), d.TotalCost)})"));

                _grid.DataSource = d.Lines
                    .OrderBy(l => l.Date)
                    .Select(l => new
                    {
                        Chuyến = $"#{l.TripId} — {l.LegType}",
                        Trạng_thái = ReportFormat.TripStatus(l.TripStatus),
                        Loại_chi_phí = l.Category,
                        Mô_tả = l.Description,
                        Ngày = l.Date.ToString("dd/MM/yyyy HH:mm"),
                        Số_tiền = l.Amount,
                    })
                    .ToList();

                foreach (DataGridViewColumn col in _grid.Columns)
                    col.HeaderText = col.Name.Replace('_', ' ');
                if (_grid.Columns.Contains("Mô_tả")) _grid.Columns["Mô_tả"]!.FillWeight = 220;
                if (_grid.Columns.Contains("Số_tiền"))
                {
                    var c = _grid.Columns["Số_tiền"]!;
                    c.DefaultCellStyle.Format = "N0";
                    c.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    c.HeaderText = "Số tiền (đ)";
                }
            }
            catch (Exception ex)
            {
                _lblTitle.Text = "Không tải được chi tiết chi phí";
                _lblSub.Text = ex.Message;
                _lblSub.ForeColor = Theme.Danger;
            }
        }

        private void Grid_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (_grid.Columns[e.ColumnIndex].Name == "Loại_chi_phí" && e.Value is string cat)
            {
                e.CellStyle!.ForeColor = cat.Contains("Sự cố", StringComparison.OrdinalIgnoreCase) ? Theme.Danger
                    : cat.Contains("Nhiên liệu", StringComparison.OrdinalIgnoreCase) ? Theme.Primary
                    : Theme.Warning;
                e.CellStyle.Font = Theme.FontBold;
            }
        }

        private static Panel SummaryCard(string title, string value, Color accent)
        {
            var card = new Panel { Width = 230, Height = 62, BackColor = Theme.CardBg, Margin = new Padding(0, 0, 12, 0) };
            card.Controls.Add(new Panel { Dock = DockStyle.Left, Width = 4, BackColor = accent });
            card.Controls.Add(new Label { Text = title, Font = Theme.FontSmall, ForeColor = Theme.TextSecondary, AutoSize = true, Left = 14, Top = 8 });
            card.Controls.Add(new Label { Text = value, Font = Theme.FontBold, ForeColor = accent, AutoSize = true, Left = 14, Top = 30 });
            return card;
        }
    }
}
