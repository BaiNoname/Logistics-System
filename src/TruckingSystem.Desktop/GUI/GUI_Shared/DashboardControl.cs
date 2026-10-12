using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_ReportManagement;

namespace TruckingSystem.Desktop.GUI_Shared
{
    /// <summary>Màn Tổng quan & Lãi/Lỗ — đo lường doanh thu, chi phí và lãi/lỗ
    /// theo kỳ, để thấy rõ tiền đi đâu:
    ///   - Thẻ số liệu tài chính: doanh thu, chi phí, lãi/lỗ, biên lợi nhuận...
    ///   - Thẻ vận hành: số đơn, đơn hủy, chi phí mất do hủy đơn, đơn chưa xuất HĐ
    ///   - Biểu đồ doanh thu/chi phí theo tháng + cơ cấu chi phí
    ///   - Bảng lãi/lỗ TỪNG ĐƠN (lỗ nặng nhất lên đầu) — double-click 1 đơn để
    ///     xem chi tiết từng khoản chi (nhiên liệu, phát sinh, sự cố).
    /// Mọi số liệu gắn theo đơn hàng, lọc theo NGÀY TẠO ĐƠN.</summary>
    public partial class DashboardControl : BaseUserControl
    {
        private ApiServices? _api;
        private ProfitReport? _report;
        private int _loadId; // đếm lượt tải báo cáo — bỏ kết quả của lượt cũ

        private static readonly Font KpiValueFont = new("Segoe UI", 18, FontStyle.Bold);
        private static readonly Font KpiIconFont = new("Segoe UI Emoji", 13);
        private static readonly Color[] Palette =
        {
            ColorTranslator.FromHtml("#2563EB"), ColorTranslator.FromHtml("#D97706"), ColorTranslator.FromHtml("#DC2626"),
            ColorTranslator.FromHtml("#16A34A"), ColorTranslator.FromHtml("#7C3AED"), ColorTranslator.FromHtml("#0891B2"),
            ColorTranslator.FromHtml("#DB2777"), ColorTranslator.FromHtml("#65A30D"), ColorTranslator.FromHtml("#64748B"),
        };

        public DashboardControl()
        {
            // Khung tĩnh (tiêu đề, thanh lọc, hàng thẻ, biểu đồ, bảng) nằm ở DashboardControl.Designer.cs
            // — bố cục TableLayoutPanel/FlowLayoutPanel tự co giãn, không còn tính tọa độ bằng code.
            InitializeComponent();
            _cboPeriod.SelectedIndex = 1;
            ApplyPeriod();
            Theme.StyleGrid(_grid);
            Theme.StyleGrid(_gridFuel);
            Load += async (_, _) => await LoadAsync();
        }

        public DashboardControl(ApiServices api) : this()
        {
            _api = api;
        }

        // ================= Thanh lọc =================

        private async void CboPeriod_SelectedIndexChanged(object? sender, EventArgs e)
        {
            ApplyPeriod();
            if (_cboPeriod.SelectedIndex != 4) await LoadAsync(); // "Tùy chọn": chọn ngày xong mới bấm Xem báo cáo
        }

        private async void BtnRefresh_Click(object? sender, EventArgs e) => await LoadAsync();

        private void ApplyPeriod()
        {
            var today = DateTime.Today;
            var custom = _cboPeriod.SelectedIndex == 4;
            _dtpFrom.Enabled = _dtpTo.Enabled = custom;
            if (custom) return;

            _dtpTo.Value = today;
            _dtpFrom.Value = _cboPeriod.SelectedIndex switch
            {
                0 => new DateTime(today.Year, today.Month, 1),
                1 => new DateTime(today.Year, today.Month, 1).AddMonths(-2),
                2 => new DateTime(today.Year, 1, 1),
                _ => new DateTime(2020, 1, 1),
            };
        }

        // ================= Tải dữ liệu =================

        private async Task LoadAsync()
        {
            if (_api is null) return;

            var from = _dtpFrom.Value.Date;
            var to = _dtpTo.Value.Date;
            if (to < from)
            {
                _lblStatus.Text = "⚠️ Ngày kết thúc phải sau ngày bắt đầu.";
                _lblStatus.ForeColor = Theme.Danger;
                return;
            }

            if (!_api.Connection.IsInRole("Admin", "Dispatcher", "Accountant"))
            {
                _lblStatus.Text = "Báo cáo tài chính chỉ dành cho Quản trị / Điều phối / Kế toán.";
                _lblStatus.ForeColor = Theme.Warning;
                return;
            }

            // Đổi kỳ báo cáo liên tục: chỉ hiện kết quả của lượt tải MỚI NHẤT — lượt cũ trả về
            // muộn không được vẽ đè số liệu của kỳ khác.
            var loadId = ++_loadId;
            _btnRefresh.Enabled = false;
            _lblStatus.Text = "Đang tải báo cáo...";
            _lblStatus.ForeColor = Theme.TextSecondary;
            try
            {
                var report = await _api.Reports.GetProfitReportAsync(from, to);
                if (loadId != _loadId || IsDisposed) return;
                _report = report;

                // Thông tin vận hành tức thời (không phụ thuộc kỳ) — lỗi cũng không sao.
                int availableVehicles = 0, totalVehicles = 0;
                // Danh sách xe chỉ Quản trị / Điều phối được xem (Kế toán gọi sẽ bị 403
                // -> thẻ hiện "0/0" sai sự thật) -> vai trò khác bỏ qua, thẻ hiện "—".
                if (_api.Connection.IsInRole("Admin", "Dispatcher"))
                    try
                    {
                        var vehicles = await _api.Vehicles.GetVehiclesAsync();
                        totalVehicles = vehicles.Count;
                        // "Sẵn sàng" = Rảnh VÀ giấy tờ hợp lệ — đúng xe mà Chia xe dùng được
                        // (trước đây đếm cả xe Rảnh nhưng hết Đăng kiểm).
                        availableVehicles = vehicles.Count(v => v.Status == "Available" && v.DocumentBlockReason is null);
                    }
                    catch { }
                if (loadId != _loadId || IsDisposed) return;

                RenderKpis(_report, availableVehicles, totalVehicles);
                RenderGrid(_report);
                RenderFuelControl(_report);
                _chartMonthly.Invalidate();
                _chartCosts.Invalidate();

                _lblStatus.Text = $"{_report.TotalOrders} đơn tạo từ {from:dd/MM/yyyy} đến {to:dd/MM/yyyy}";
            }
            catch (Exception ex)
            {
                if (loadId != _loadId || IsDisposed) return;
                _lblStatus.Text = "Không tải được báo cáo: " + ex.Message;
                _lblStatus.ForeColor = Theme.Danger;
            }
            finally
            {
                if (loadId == _loadId && !IsDisposed) _btnRefresh.Enabled = true;
            }
        }

        // ================= Thẻ số liệu =================

        private void RenderKpis(ProfitReport r, int availableVehicles, int totalVehicles)
        {
            _kpiFinance.Controls.Clear();
            _kpiFinance.Controls.Add(MakeCard("💰", "Doanh thu (theo hóa đơn)", ReportFormat.Money(r.Revenue), Theme.Primary, Theme.TextPrimary,
                $"Đã thu: {ReportFormat.Money(r.Collected)}", ReportFormat.FullMoney(r.Revenue)));
            _kpiFinance.Controls.Add(MakeCard("💸", "Tổng chi phí", ReportFormat.Money(r.TotalCost), Theme.Warning, Theme.TextPrimary,
                $"Xăng {ReportFormat.Money(r.FuelCost)} · Khác {ReportFormat.Money(r.ExpenseCost + r.IncidentCost)}", ReportFormat.FullMoney(r.TotalCost)));

            var isProfit = r.Profit >= 0;
            var profitColor = isProfit ? Theme.Success : Theme.Danger;
            _kpiFinance.Controls.Add(MakeCard(isProfit ? "📈" : "📉", isProfit ? "Lãi" : "Lỗ", ReportFormat.Money(r.Profit), profitColor, profitColor,
                r.MarginPercent.HasValue ? $"Biên lợi nhuận: {r.MarginPercent:0.#}%" : "Chưa có doanh thu để tính biên", ReportFormat.FullMoney(r.Profit)));
            _kpiFinance.Controls.Add(MakeCard("⛽", "Chi phí nhiên liệu", ReportFormat.Money(r.FuelCost), Theme.Primary, Theme.TextPrimary,
                r.TripsUsingFuelEstimate > 0
                    ? $"{r.TripsUsingFuelEstimate} chuyến tạm tính theo định mức"
                    : $"{ReportFormat.Percent(r.FuelCost, r.TotalCost)} tổng chi phí",
                $"{ReportFormat.FullMoney(r.FuelCost)} = thực đổ (hóa đơn) + tạm tính theo định mức GPS cho chuyến chưa nhập hóa đơn"));
            _kpiFinance.Controls.Add(MakeCard("⚠️", "Sự cố + phát sinh", ReportFormat.Money(r.IncidentCost + r.ExpenseCost), Theme.Danger, Theme.TextPrimary,
                $"Riêng sự cố: {ReportFormat.Money(r.IncidentCost)}", ReportFormat.FullMoney(r.IncidentCost + r.ExpenseCost)));

            _kpiOps.Controls.Clear();
            _kpiOps.Controls.Add(MakeCard("📦", "Đơn trong kỳ", r.TotalOrders.ToString(), Theme.Primary, Theme.TextPrimary,
                $"Hoàn tất: {r.DoneOrders}", null));
            _kpiOps.Controls.Add(MakeCard("❌", "Đơn bị hủy", r.CancelledOrders.ToString(), Theme.Danger,
                r.CancelledOrders > 0 ? Theme.Danger : Theme.TextPrimary,
                $"{ReportFormat.Percent(r.CancelledOrders, r.TotalOrders)} số đơn", null));
            _kpiOps.Controls.Add(MakeCard("🕳️", "Chi phí mất do hủy đơn", ReportFormat.Money(r.CancelledOrdersCost), Theme.Danger,
                r.CancelledOrdersCost > 0 ? Theme.Danger : Theme.TextPrimary,
                "Xe đã chạy/trả hàng, không thu được", ReportFormat.FullMoney(r.CancelledOrdersCost)));
            _kpiOps.Controls.Add(MakeCard("🧾", "Đơn xong chưa xuất HĐ", r.UninvoicedDoneOrders.ToString(), Theme.Warning,
                r.UninvoicedDoneOrders > 0 ? Theme.Warning : Theme.TextPrimary,
                r.UninvoicedDoneOrders > 0 ? "Doanh thu chưa được ghi nhận" : "Đã xuất đủ hóa đơn", null));
            _kpiOps.Controls.Add(MakeCard("🚛", "Xe sẵn sàng (hiện tại)",
                totalVehicles > 0 ? $"{availableVehicles}/{totalVehicles}" : "—", Theme.Success, Theme.TextPrimary,
                totalVehicles > 0 ? "Rảnh + giấy tờ còn hạn (không phụ thuộc kỳ)" : "Không có quyền xem danh sách xe", null));
        }

        /// <summary>Thẻ số liệu (sinh theo dữ liệu nên tạo bằng code). Bên trong là TableLayoutPanel
        /// (biểu tượng + nhãn / giá trị / dòng phụ) — không tọa độ cứng; bề ngang/cao thẻ nhân theo
        /// DPI (LogicalToDeviceUnits) và FlowLayoutPanel chứa thẻ tự xuống dòng khi hẹp.</summary>
        private Panel MakeCard(string icon, string label, string value, Color accent, Color valueColor, string sub, string? tooltip)
        {
            int S(int v) => LogicalToDeviceUnits(v);

            var card = new TableLayoutPanel
            {
                Size = new Size(S(220), S(110)),
                Margin = new Padding(0, 0, S(14), S(10)),
                Padding = new Padding(S(14), S(8), S(8), S(6)),
                BackColor = Theme.CardBg,
                ColumnCount = 2,
                RowCount = 3,
            };
            card.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            card.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            card.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            card.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            card.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            card.Paint += (_, e) =>
            {
                using var pen = new Pen(Theme.BorderColor);
                e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                using var brush = new SolidBrush(accent);
                e.Graphics.FillRectangle(brush, 0, 0, S(4), card.Height);
            };

            var lblIcon = new Label { Text = icon, Font = KpiIconFont, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 0, S(6), 0) };
            var lblLabel = new Label { Text = label, Font = Theme.FontSmall, ForeColor = Theme.TextSecondary, AutoSize = true, Anchor = AnchorStyles.Left, Margin = Padding.Empty };
            var lblValue = new Label { Text = value, Font = KpiValueFont, ForeColor = valueColor, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, S(2), 0, 0) };
            var lblSub = new Label
            {
                Text = sub,
                Font = Theme.FontSmall,
                ForeColor = Theme.TextSecondary,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.BottomLeft,
                AutoEllipsis = true,
                Margin = Padding.Empty,
            };
            card.Controls.Add(lblIcon, 0, 0);
            card.Controls.Add(lblLabel, 1, 0);
            card.Controls.Add(lblValue, 0, 1);
            card.SetColumnSpan(lblValue, 2);
            card.Controls.Add(lblSub, 0, 2);
            card.SetColumnSpan(lblSub, 2);

            if (tooltip is not null)
            {
                var tip = new ToolTip();
                tip.SetToolTip(card, tooltip);
                foreach (Control c in card.Controls) tip.SetToolTip(c, tooltip);
            }
            return card;
        }

        // ================= Bảng lãi/lỗ từng đơn =================

        private static readonly string[] MoneyColumns = { "Doanh_thu", "Nhiên_liệu", "Phát_sinh", "Sự_cố", "Tổng_chi_phí", "Lãi_Lỗ" };

        private void RenderGrid(ProfitReport r)
        {
            _grid.DataSource = r.Orders.Select(o => new
            {
                o.OrderId,
                Mã_đơn = o.OrderCode,
                Khách_hàng = o.CustomerName,
                Ngày_tạo = o.CreatedAt.ToString("dd/MM/yyyy"),
                Trạng_thái = ReportFormat.OrderStatus(o.Status),
                Doanh_thu = o.Revenue,
                Nhiên_liệu = o.FuelCost,
                Phát_sinh = o.ExpenseCost,
                Sự_cố = o.IncidentCost,
                Tổng_chi_phí = o.TotalCost,
                Lãi_Lỗ = o.Profit,
                Ghi_chú = NoteFor(o),
            }).ToList();
        }

        /// <summary>Giải thích những đơn trông như "lỗ" nhưng thật ra chưa chốt
        /// sổ — tránh hiểu nhầm khi đọc bảng.</summary>
        private static string NoteFor(OrderProfitItem o)
        {
            if (o.Status == "Cancelled") return o.TotalCost > 0 ? "Đơn hủy — chi phí mất trắng" : "Đơn hủy";
            if (o.Status == "Done" && !o.HasInvoice) return "⚠️ Chưa xuất hóa đơn";
            if (o.Status != "Done") return "Đang xử lý — chưa chốt";
            return "";
        }

        private void Grid_DataBindingComplete(object? sender, DataGridViewBindingCompleteEventArgs e)
        {
            if (_grid.Columns["OrderId"] is { } idCol) idCol.Visible = false;
            foreach (var name in MoneyColumns)
            {
                if (_grid.Columns[name] is not { } col) continue;
                col.DefaultCellStyle.Format = "N0";
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
        }

        private void Grid_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            var name = _grid.Columns[e.ColumnIndex].Name;
            if (name == "Lãi_Lỗ" && e.Value is decimal profit)
            {
                e.CellStyle!.ForeColor = profit < 0 ? Theme.Danger : profit > 0 ? Theme.Success : Theme.TextSecondary;
                e.CellStyle.Font = Theme.FontBold;
            }
            else if (name == "Ghi_chú" && e.Value is string note && note.Length > 0)
            {
                e.CellStyle!.ForeColor = note.StartsWith("⚠️") ? Theme.Warning : note.StartsWith("Đơn hủy") ? Theme.Danger : Theme.TextSecondary;
            }
        }

        private void Grid_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (_api is null || e.RowIndex < 0) return;
            if (_grid.Rows[e.RowIndex].Cells["OrderId"].Value is not int orderId) return;

            using var f = new OrderCostDetailForm(_api, orderId);
            f.ShowDialog(FindForm());
        }

        // ================= Kiểm soát nhiên liệu =================

        private void RenderFuelControl(ProfitReport r)
        {
            var rows = r.FuelControl ?? new List<FuelControlRowItem>();
            var flagged = rows.Count(x => x.Flagged);

            _kpiFuel.Controls.Clear();
            _kpiFuel.Controls.Add(MakeCard("🗺️", "Dự toán (VietMap)", ReportFormat.Money(r.FuelPlannedCost), Theme.Primary, Theme.TextPrimary,
                "Km tuyến dự kiến × định mức", ReportFormat.FullMoney(r.FuelPlannedCost)));
            _kpiFuel.Controls.Add(MakeCard("📡", "Định mức (GPS thực tế)", ReportFormat.Money(r.FuelNormCost), Theme.Primary, Theme.TextPrimary,
                // "Lệch dự toán" chỉ so trên các chuyến hoàn tất có ĐỦ cả dự toán lẫn định mức
                // (2 tổng ở trên có thể gồm 2 tập chuyến khác nhau).
                r.FuelComparedTripCount > 0
                    ? $"Lệch dự toán: {DeviationText(r.FuelNormCostCompared, r.FuelPlannedCostCompared)} ({r.FuelComparedTripCount} chuyến đủ số liệu)"
                    : "Lệch dự toán: chưa có chuyến hoàn tất đủ số liệu",
                ReportFormat.FullMoney(r.FuelNormCost)));
            _kpiFuel.Controls.Add(MakeCard("🧾", "Thực đổ (hóa đơn)", ReportFormat.Money(r.FuelActualCost), Theme.Warning, Theme.TextPrimary,
                r.TripsUsingFuelEstimate > 0 ? $"{r.TripsUsingFuelEstimate} chuyến chưa nhập hóa đơn" : "Đã nhập đủ hóa đơn",
                ReportFormat.FullMoney(r.FuelActualCost)));
            _kpiFuel.Controls.Add(MakeCard("🚩", "Chuyến lệch > 15%", $"{flagged}/{rows.Count}", Theme.Danger,
                flagged > 0 ? Theme.Danger : Theme.TextPrimary,
                flagged > 0 ? "Xem dòng tô đỏ bên dưới" : "Không có chuyến bất thường", null));

            _gridFuel.DataSource = rows.Select(x => new
            {
                Chuyến = $"#{x.TripId}",
                Đơn = x.OrderCode,
                Chặng = x.LegType,
                Xe = x.PlateNumber,
                Ngày = x.TripDate.ToString("dd/MM/yyyy"),
                Km_kế_hoạch = x.PlannedKm,
                Km_GPS = x.GpsKm,
                Lệch_km = x.KmDeviationPct.HasValue ? $"{x.KmDeviationPct:+0.#;-0.#;0}%" : "",
                Dự_toán = x.PlannedCost,
                Định_mức_GPS = x.NormCost,
                Thực_đổ = x.HasActual ? x.ActualCost : (decimal?)null,
                Lệch_tiền = x.CostDeviationPct.HasValue ? $"{x.CostDeviationPct:+0.#;-0.#;0}%" : "",
                Ghi_chú = FuelNote(x),
                Cờ = x.Flagged,
            }).ToList();
        }

        private static string DeviationText(decimal value, decimal baseline)
            => baseline > 0 ? $"{(value - baseline) / baseline * 100:+0.#;-0.#;0}%" : "—";

        private static string FuelNote(FuelControlRowItem x)
        {
            if (!string.IsNullOrEmpty(x.Note)) return "⚠️ " + x.Note;
            var notes = new List<string>();
            // Chuyến CHƯA hoàn tất: km GPS / hóa đơn còn đang tăng -> chưa so lệch (trước đây
            // chuyến đang chạy bị ghi "Km GPS thiếu", "Đổ ít hơn định mức" dù chưa chạy xong).
            if (x.Status != "Completed")
                return x.Status == "Cancelled" ? "Chuyến đã hủy — chỉ tính chi phí đã phát sinh" : "Chuyến chưa hoàn tất — chưa so lệch";
            if (x.KmDeviationPct is > 15) notes.Add("Chạy dài hơn tuyến dự kiến");
            if (x.KmDeviationPct is < -15) notes.Add("Km GPS thiếu (mất sóng?)");
            if (x.CostDeviationPct is > 15) notes.Add("Đổ nhiều hơn định mức");
            if (x.CostDeviationPct is < -15) notes.Add("Đổ ít hơn định mức (chưa nhập đủ hóa đơn?)");
            if (!x.HasActual) notes.Add("Chưa nhập hóa đơn xăng");
            return string.Join(" · ", notes);
        }

        private static readonly string[] FuelMoneyColumns = { "Dự_toán", "Định_mức_GPS", "Thực_đổ" };

        private void GridFuel_DataBindingComplete(object? sender, DataGridViewBindingCompleteEventArgs e)
        {
            if (_gridFuel.Columns["Cờ"] is { } flagCol) flagCol.Visible = false;
            foreach (var name in FuelMoneyColumns)
            {
                if (_gridFuel.Columns[name] is not { } col) continue;
                col.DefaultCellStyle.Format = "N0";
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
            foreach (var name in new[] { "Km_kế_hoạch", "Km_GPS", "Lệch_km", "Lệch_tiền" })
                if (_gridFuel.Columns[name] is { } col) col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            foreach (DataGridViewRow row in _gridFuel.Rows)
                if (row.Cells["Cờ"].Value is true)
                    row.DefaultCellStyle.BackColor = ColorTranslator.FromHtml("#FEE2E2");
        }

        private void GridFuel_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            var name = _gridFuel.Columns[e.ColumnIndex].Name;
            if ((name == "Lệch_km" || name == "Lệch_tiền") && e.Value is string s && s.Length > 0)
            {
                var number = decimal.TryParse(s.TrimEnd('%').Replace("+", ""), System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.CurrentCulture, out var v) ? v : 0;
                e.CellStyle!.ForeColor = Math.Abs(number) > 15 ? Theme.Danger : Theme.TextSecondary;
                if (Math.Abs(number) > 15) e.CellStyle.Font = Theme.FontBold;
            }
            else if (name == "Ghi_chú" && e.Value is string note && note.StartsWith("⚠️"))
            {
                e.CellStyle!.ForeColor = Theme.Warning;
            }
        }

        // ================= Biểu đồ (vẽ tay, không cần thư viện ngoài) =================

        private void ChartMonthly_Paint(object? sender, PaintEventArgs e) => PaintMonthlyChart(e.Graphics, _chartMonthly.ClientRectangle);

        private void ChartCosts_Paint(object? sender, PaintEventArgs e) => PaintCostChart(e.Graphics, _chartCosts.ClientRectangle);

        private static void DrawFrame(Graphics g, Rectangle rect, string title)
        {
            using var borderPen = new Pen(Theme.BorderColor);
            g.DrawRectangle(borderPen, 0, 0, rect.Width - 1, rect.Height - 1);
            using var titleBrush = new SolidBrush(Theme.TextPrimary);
            g.DrawString(title, Theme.FontBold, titleBrush, 12, 10);
        }

        private static void DrawEmpty(Graphics g, Rectangle rect, string text)
        {
            using var brush = new SolidBrush(Theme.TextSecondary);
            var size = g.MeasureString(text, Theme.FontNormal);
            g.DrawString(text, Theme.FontNormal, brush, (rect.Width - size.Width) / 2, (rect.Height - size.Height) / 2);
        }

        private static void DrawLegend(Graphics g, float x, float y, Color color, string text)
        {
            using var brush = new SolidBrush(color);
            g.FillRectangle(brush, x, y + 3, 12, 12);
            using var textBrush = new SolidBrush(Theme.TextSecondary);
            g.DrawString(text, Theme.FontSmall, textBrush, x + 16, y);
        }

        /// <summary>Cột đôi theo tháng: xanh = doanh thu, cam = chi phí. Nhãn tháng
        /// màu đỏ nếu tháng đó chi phí vượt doanh thu (lỗ).</summary>
        private void PaintMonthlyChart(Graphics g, Rectangle rect)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            DrawFrame(g, rect, "📈 Doanh thu & chi phí theo tháng");
            DrawLegend(g, rect.Width - 210, 12, Theme.Primary, "Doanh thu");
            DrawLegend(g, rect.Width - 110, 12, Theme.Warning, "Chi phí");

            var data = _report?.Monthly ?? new List<MonthlyProfitItem>();
            if (data.Count == 0) { DrawEmpty(g, rect, "Chưa có dữ liệu trong kỳ này"); return; }

            const int left = 74, top = 46;
            int right = rect.Width - 16, bottom = rect.Height - 34;
            float plotW = right - left, plotH = bottom - top;
            if (plotW <= 0 || plotH <= 0) return;

            var max = data.Max(m => Math.Max(m.Revenue, m.Cost));
            if (max <= 0) max = 1;

            using var gridPen = new Pen(Theme.BorderColor);
            using var axisBrush = new SolidBrush(Theme.TextSecondary);
            for (int i = 0; i <= 4; i++)
            {
                var y = bottom - plotH * i / 4f;
                g.DrawLine(gridPen, left, y, right, y);
                var label = ReportFormat.Money(max * i / 4);
                var size = g.MeasureString(label, Theme.FontSmall);
                g.DrawString(label, Theme.FontSmall, axisBrush, left - size.Width - 4, y - size.Height / 2);
            }

            var groupW = plotW / data.Count;
            var barW = Math.Min(28f, groupW * 0.32f);
            using var revBrush = new SolidBrush(Theme.Primary);
            using var costBrush = new SolidBrush(Theme.Warning);
            using var lossBrush = new SolidBrush(Theme.Danger);
            for (int i = 0; i < data.Count; i++)
            {
                var m = data[i];
                var cx = left + groupW * i + groupW / 2;
                var hRev = (float)(m.Revenue / max) * plotH;
                var hCost = (float)(m.Cost / max) * plotH;
                g.FillRectangle(revBrush, cx - barW - 2, bottom - hRev, barW, hRev);
                g.FillRectangle(costBrush, cx + 2, bottom - hCost, barW, hCost);

                var label = MonthLabel(m.Month);
                var size = g.MeasureString(label, Theme.FontSmall);
                g.DrawString(label, Theme.FontSmall, m.Cost > m.Revenue ? lossBrush : axisBrush, cx - size.Width / 2, bottom + 6);
            }
        }

        private static string MonthLabel(string yyyyMM)
            => yyyyMM.Length == 7 ? $"T{int.Parse(yyyyMM[5..])}/{yyyyMM[2..4]}" : yyyyMM;

        /// <summary>Thanh ngang theo từng loại chi phí, lớn nhất lên đầu, kèm số tiền
        /// và % trên tổng chi phí.</summary>
        private void PaintCostChart(Graphics g, Rectangle rect)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            DrawFrame(g, rect, "🧾 Cơ cấu chi phí");

            var data = _report?.CostBreakdown ?? new List<CostCategoryItem>();
            if (data.Count == 0) { DrawEmpty(g, rect, "Chưa có khoản chi nào trong kỳ này"); return; }

            var total = data.Sum(x => x.Amount);
            var max = data.Max(x => x.Amount);
            if (max <= 0) max = 1;

            const int labelW = 160, amountW = 170, top = 48, rowH = 30;
            var barLeft = 14 + labelW;
            var barMaxW = rect.Width - barLeft - amountW - 12;
            if (barMaxW <= 20) return;

            var rows = Math.Min(data.Count, Math.Max(1, (rect.Height - top - 8) / rowH));
            using var textBrush = new SolidBrush(Theme.TextPrimary);
            using var subBrush = new SolidBrush(Theme.TextSecondary);
            using var ellipsis = new StringFormat { Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap, LineAlignment = StringAlignment.Center };

            for (int i = 0; i < rows; i++)
            {
                var item = data[i];
                var y = top + i * rowH;
                g.DrawString(item.Category, Theme.FontSmall, textBrush, new RectangleF(14, y, labelW - 6, rowH - 6), ellipsis);

                var barW = Math.Max(2f, (float)(item.Amount / max) * barMaxW);
                using var barBrush = new SolidBrush(Palette[i % Palette.Length]);
                g.FillRectangle(barBrush, barLeft, y + 4, barW, rowH - 14);

                var amountText = $"{ReportFormat.FullMoney(item.Amount)} ({ReportFormat.Percent(item.Amount, total)})";
                g.DrawString(amountText, Theme.FontSmall, subBrush, new RectangleF(barLeft + barMaxW + 8, y, amountW, rowH - 6), ellipsis);
            }

            if (rows < data.Count)
            {
                g.DrawString($"… và {data.Count - rows} loại chi phí khác", Theme.FontSmall, subBrush, 14, top + rows * rowH);
            }
        }
    }

    /// <summary>Khung biểu đồ vẽ tay (nền trắng, chống nháy, vẽ lại khi đổi cỡ). Khai báo ở
    /// DashboardControl.Designer.cs; phần vẽ nằm ở DashboardControl (PaintMonthlyChart /
    /// PaintCostChart).</summary>
    public sealed class DashboardChartPanel : Panel
    {
        public DashboardChartPanel()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
            BackColor = Theme.CardBg;
        }
    }
}
