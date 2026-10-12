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

        public OrderCostDetailForm()
        {
            // Giao diện tĩnh (tiêu đề, vùng thẻ tổng hợp, bảng, nút Đóng) nằm ở
            // OrderCostDetailForm.Designer.cs; ở đây chỉ đổ dữ liệu theo từng đơn.
            InitializeComponent();
            Theme.StyleGrid(grid);
        }

        public OrderCostDetailForm(ApiServices api, int orderId) : this()
        {
            _api = api;
            _orderId = orderId;
            Load += async (_, _) => await LoadAsync();
        }

        private void BtnClose_Click(object? sender, EventArgs e) => Close();

        private async Task LoadAsync()
        {
            if (_api is null) return;
            try
            {
                var d = await _api.Reports.GetOrderCostDetailAsync(_orderId);
                var profit = d.Revenue - d.TotalCost;

                Text = $"Chi phí đơn {d.OrderCode}";
                lblTitle.Text = $"💰 Chi phí đơn {d.OrderCode}";
                lblSub.Text = $"Khách hàng: {d.CustomerName}   •   Trạng thái: {ReportFormat.OrderStatus(d.Status)}";

                summary.Controls.Clear();
                summary.Controls.Add(SummaryCard("Doanh thu", ReportFormat.FullMoney(d.Revenue), Theme.Primary));
                summary.Controls.Add(SummaryCard("Tổng chi phí", ReportFormat.FullMoney(d.TotalCost), Theme.Warning));
                summary.Controls.Add(SummaryCard(profit < 0 ? "Lỗ" : "Lãi", ReportFormat.FullMoney(profit),
                    profit < 0 ? Theme.Danger : Theme.Success));
                if (d.Status == "Cancelled")
                    summary.Controls.Add(SummaryCard("Ghi chú", "Đơn đã hủy — chi phí là tiền mất", Theme.Danger));

                lblByCategory.Text = d.Lines.Count == 0
                    ? "Đơn này chưa ghi nhận khoản chi nào (nhiên liệu, phát sinh, sự cố)."
                    : "Theo loại:  " + string.Join("   •   ", d.Lines
                        .GroupBy(l => l.Category)
                        .OrderByDescending(g => g.Sum(x => x.Amount))
                        .Select(g => $"{g.Key}: {ReportFormat.FullMoney(g.Sum(x => x.Amount))} ({ReportFormat.Percent(g.Sum(x => x.Amount), d.TotalCost)})"));

                grid.DataSource = d.Lines
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

                foreach (DataGridViewColumn col in grid.Columns)
                    col.HeaderText = col.Name.Replace('_', ' ');
                if (grid.Columns.Contains("Mô_tả")) grid.Columns["Mô_tả"]!.FillWeight = 220;
                if (grid.Columns.Contains("Số_tiền"))
                {
                    var c = grid.Columns["Số_tiền"]!;
                    c.DefaultCellStyle.Format = "N0";
                    c.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    c.HeaderText = "Số tiền (đ)";
                }
            }
            catch (Exception ex)
            {
                lblTitle.Text = "Không tải được chi tiết chi phí";
                lblSub.Text = ex.Message;
                lblSub.ForeColor = Theme.Danger;
            }
        }

        private void Grid_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (grid.Columns[e.ColumnIndex].Name == "Loại_chi_phí" && e.Value is string cat)
            {
                e.CellStyle!.ForeColor = cat.Contains("Sự cố", StringComparison.OrdinalIgnoreCase) ? Theme.Danger
                    : cat.Contains("Nhiên liệu", StringComparison.OrdinalIgnoreCase) ? Theme.Primary
                    : Theme.Warning;
                e.CellStyle.Font = Theme.FontBold;
            }
        }

        /// <summary>Thẻ tổng hợp (dựng theo dữ liệu đơn, thêm vào FlowLayoutPanel "summary" của
        /// Designer). Không dùng tọa độ cứng: thẻ là TableLayoutPanel tự co theo chữ; các khoảng
        /// cách nhỏ được quy đổi theo DPI bằng LogicalToDeviceUnits.</summary>
        private Control SummaryCard(string title, string value, Color accent)
        {
            var card = new TableLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Theme.CardBg,
                ColumnCount = 2,
                RowCount = 2,
                MinimumSize = new Size(LogicalToDeviceUnits(230), 0),
                Margin = new Padding(0, 0, LogicalToDeviceUnits(12), LogicalToDeviceUnits(8)),
                Padding = new Padding(0),
            };
            card.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, LogicalToDeviceUnits(4)));
            card.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            card.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            card.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var strip = new Panel { Dock = DockStyle.Fill, BackColor = accent, Margin = new Padding(0), Size = new Size(LogicalToDeviceUnits(4), 0) };
            card.Controls.Add(strip, 0, 0);
            card.SetRowSpan(strip, 2);
            card.Controls.Add(new Label
            {
                Text = title,
                Font = Theme.FontSmall,
                ForeColor = Theme.TextSecondary,
                AutoSize = true,
                Margin = new Padding(LogicalToDeviceUnits(10), LogicalToDeviceUnits(8), LogicalToDeviceUnits(12), 0),
            }, 1, 0);
            card.Controls.Add(new Label
            {
                Text = value,
                Font = Theme.FontBold,
                ForeColor = accent,
                AutoSize = true,
                Margin = new Padding(LogicalToDeviceUnits(10), LogicalToDeviceUnits(4), LogicalToDeviceUnits(12), LogicalToDeviceUnits(10)),
            }, 1, 1);
            return card;
        }
    }
}
