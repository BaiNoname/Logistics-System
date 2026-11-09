using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_ReportManagement;

namespace TruckingSystem.Desktop.GUI_Shared
{
    /// <summary>Màn TỔNG QUAN — chia 3 TRANG CON để mỗi người chỉ nhìn phần của mình, không bị rối:
    ///   • 📊 Tổng quan (mặc định): 4 thẻ chính (Doanh thu · Chi phí · Lãi/Lỗ · Giao đúng hạn, kèm so với kỳ trước)
    ///     + khung "⚠️ Cần chú ý" (bấm 1 dòng là mở đúng chỗ cần xử lý) + biểu đồ doanh thu/chi phí theo tháng.
    ///   • 💰 Tài chính: thẻ chi phí chi tiết, cơ cấu chi phí, bảng lãi/lỗ từng đơn (mặc định 10 đơn lỗ nhiều nhất).
    ///   • ⛽ Nhiên liệu: so xăng dự kiến / theo km thực chạy / đã đổ, bảng chuyến (chuyến bất thường lên đầu).
    /// Kế toán mở màn là vào thẳng trang Tài chính. Cách tính số liệu KHÔNG đổi (vẫn lấy từ báo cáo lãi/lỗ của API),
    /// chỉ sắp xếp lại cách hiển thị. Mọi số liệu gắn theo đơn hàng, lọc theo NGÀY TẠO ĐƠN.
    /// Bố cục trang con dựng bằng code (BuildLayout) trên các control có sẵn ở Designer — không cần mở Designer.</summary>
    public partial class DashboardControl : BaseUserControl
    {
        private ApiServices? _api;
        private ProfitReport? _report;
        private ProfitReport? _prev;     // kỳ trước (để so sánh ▲▼) — null nếu không tải được
        private int _loadId; // đếm lượt tải báo cáo — bỏ kết quả của lượt cũ

        private static readonly Font KpiValueFont = new("Segoe UI", 18, FontStyle.Bold);
        private static readonly Font KpiBigValueFont = new("Segoe UI", 22, FontStyle.Bold);
        private static readonly Font KpiIconFont = new("Segoe UI Emoji", 13);
        private static readonly Font LinkFont = new("Segoe UI", 10, FontStyle.Underline);
        private static readonly Color[] Palette =
        {
            ColorTranslator.FromHtml("#2563EB"), ColorTranslator.FromHtml("#D97706"), ColorTranslator.FromHtml("#DC2626"),
            ColorTranslator.FromHtml("#16A34A"), ColorTranslator.FromHtml("#7C3AED"), ColorTranslator.FromHtml("#0891B2"),
            ColorTranslator.FromHtml("#DB2777"), ColorTranslator.FromHtml("#65A30D"), ColorTranslator.FromHtml("#64748B"),
        };

        // ---- trang con
        private const string PageOverview = "ov", PageFinance = "fin", PageFuel = "fuel";
        private readonly UnderlineTabs _pages = new();   // tab gạch chân = ĐỔI TRANG (khác hẳn ô chọn kỳ)
        private readonly ComboBox _periodBox = new() { DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10F), Width = 190, Margin = new Padding(0, 4, 10, 4) };
        private static readonly (string Key, string Text)[] PeriodOptions =
            { ("m", "Tháng này"), ("pm", "Tháng trước"), ("q", "3 tháng gần nhất"), ("y", "Năm nay"), ("c", "📅 Tự chọn ngày…") };
        private readonly FlowLayoutPanel _kpiMain = new() { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Dock = DockStyle.Fill, WrapContents = true, Margin = new Padding(0, 0, 0, 6) };
        private readonly FlowLayoutPanel _attention = new()
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            BackColor = Color.White,
            Padding = new Padding(16, 10, 16, 10),
            Margin = new Padding(0, 0, 0, 14),
        };
        private readonly LinkLabel _lnkAllOrders = new() { AutoSize = true, Margin = new Padding(12, 6, 0, 0), Font = Theme.FontSmall };
        private readonly ToolTip _tip = new() { AutoPopDelay = 20000 };
        private readonly List<(Control Control, string? Page, int Height)> _rows = new();
        private string _period = "q";
        private bool _showAllOrders;
        // Lọc nhanh từ khung "Cần chú ý" (VD đơn hoàn tất bị lỗ) — cộng thêm với bộ lọc của bảng; bấm "Bỏ lọc" để tắt.
        private Func<OrderProfitItem, bool>? _preset;
        private string? _presetText;

        public DashboardControl()
        {
            InitializeComponent();
            Theme.StyleGrid(_grid);
            Theme.StyleGrid(_gridFuel);
            GridLayout.FitColumns(_grid);
            GridLayout.FitColumns(_gridFuel);

            // Bộ lọc riêng cho 2 bảng dài (lọc ngay trên số liệu đã tải, không gọi lại API).
            _ordersFilter.AddChoice("status", "Trạng thái", Array.Empty<string>());
            _ordersFilter.AddChoice("customer", "Khách hàng", Array.Empty<string>());
            _ordersFilter.AddChoice("profit", "Kết quả", new[] { ProfitLoss, ProfitGain, ProfitZero });
            _ordersFilter.AddChoice("delivery", "Giao hàng", new[] { DeliveryOnTime, DeliveryLate, DeliveryNone });
            _ordersFilter.AddChoice("invoice", "Hóa đơn", new[] { InvoiceMissing, InvoiceDone });
            _ordersFilter.FilterChanged += (_, _) => BindOrdersGrid();

            _fuelFilter.AddChoice("state", "Tình trạng", new[] { FuelFlagged, FuelNormal, FuelOpen });
            _fuelFilter.AddChoice("actual", "Hóa đơn xăng", new[] { FuelHasActual, FuelNoActual });
            _fuelFilter.AddChoice("plate", "Xe", Array.Empty<string>());
            _fuelFilter.FilterChanged += (_, _) => BindFuelGrid();

            BuildFilterBar();
            BuildLayout();
            ApplyPeriod();
            Load += async (_, _) =>
            {
                // Kế toán quan tâm tiền -> mở thẳng trang Tài chính; các vai trò khác mở trang Tổng quan.
                if (_api is not null && _api.Connection.IsInRole("Accountant") && !_api.Connection.IsInRole("Admin", "Dispatcher"))
                    _pages.SelectTab(PageFinance, raiseEvent: false);
                ApplyPage();
                await LoadAsync();
            };
        }

        public DashboardControl(ApiServices api) : this()
        {
            _api = api;
        }

        // ================= Bố cục (dựng bằng code) =================

        /// <summary>Thanh chọn kỳ: "Xem số liệu của: [3 tháng gần nhất ▾]" (1 ô chọn — trước đây là hàng nút giống hệt hàng
        /// tab trang, dễ nhầm) + khoảng ngày + giờ cập nhật ở dòng trạng thái + nút 🔄. Ô chọn ngày chỉ hiện khi "Tự chọn ngày…".</summary>
        private void BuildFilterBar()
        {
            _cboPeriod.Visible = false;
            lblPeriod.Text = "Xem số liệu của:";
            lblPeriod.Visible = true;
            _periodBox.Width = LogicalToDeviceUnits(190);
            foreach (var (_, text) in PeriodOptions) _periodBox.Items.Add(text);
            _periodBox.SelectedIndex = 2; // 3 tháng gần nhất
            _periodBox.SelectedIndexChanged += async (_, _) =>
            {
                _period = PeriodOptions[Math.Max(0, _periodBox.SelectedIndex)].Key;
                ApplyPeriod();
                if (_period != "c") await LoadAsync(); // "Tự chọn ngày": chọn ngày xong mới bấm Xem
            };
            // Ô chọn kỳ nằm ở đầu trang (chỉ hiện khi cửa sổ hẹp — rộng thì là hàng nút chọn kỳ), xem DashboardControl.Modern.cs.
            ButtonStyles.Secondary(_btnRefresh);
        }

        /// <summary>Xếp lại các khối có sẵn thành 3 trang con. Mỗi khối là 1 hàng của dashLayout, gắn với 1 trang
        /// (null = luôn hiện). Đổi trang: khối của trang khác ẩn đi và hàng của nó co về 0.</summary>
        private void BuildLayout()
        {
            BuildHeader();

            _pages.Add(PageOverview, "Tổng quan", "Số liệu chính và những việc cần chú ý");
            _pages.Add(PageFinance, "Tài chính", "Chi phí chi tiết, lãi / lỗ từng đơn");
            _pages.Add(PageFuel, "Nhiên liệu", "So sánh xăng dự kiến – theo km thực chạy – đã đổ");
            _pages.Margin = new Padding(0, 4, 0, 12);
            _pages.Changed += (_, _) => ApplyPage();

            _attention.Paint += (_, e) =>
            {
                using var pen = new Pen(Theme.BorderColor);
                e.Graphics.DrawRectangle(pen, 0, 0, _attention.Width - 1, _attention.Height - 1);
                using var bar = new SolidBrush(Theme.Warning);
                e.Graphics.FillRectangle(bar, 0, 0, LogicalToDeviceUnits(4), _attention.Height);
            };

            // Bảng lãi/lỗ: tiêu đề ngắn + ⓘ (giải thích khi rê chuột) + link "Xem tất cả".
            _lblGridTitle.Text = "📋 Lãi / lỗ từng đơn";
            _lnkAllOrders.LinkClicked += (_, _) =>
            {
                if (_preset is not null) { _preset = null; _presetText = null; }
                else _showAllOrders = !_showAllOrders;
                BindOrdersGrid();
            };
            var ordersHeader = HeaderRow(_lblGridTitle,
                "Mỗi dòng là 1 đơn: doanh thu (theo hóa đơn) trừ chi phí (xăng, phát sinh, sự cố).\n" +
                "Đơn lỗ nhiều nhất lên đầu. Nhấp đúp 1 đơn để xem chi tiết từng khoản chi.\n" +
                "Đơn đang chạy hoặc chưa xuất hóa đơn thì chưa có doanh thu nên có thể tạm hiện lỗ.", _lnkAllOrders);

            _lblFuelTitle.Text = "⛽ Xăng từng chuyến";
            var fuelHeader = HeaderRow(_lblFuelTitle,
                "Xăng dự kiến = km tuyến đường lúc lập chuyến × mức tiêu hao của xe × giá xăng.\n" +
                "Xăng theo km thực chạy = km xe thật sự chạy × mức tiêu hao × giá xăng ngày chạy (bảng giá ở màn Nhiên liệu).\n" +
                "Dòng tô đỏ: lệch quá 15% — km thực chạy dài hơn dự kiến (chạy vòng / sai tuyến) hoặc tiền đổ nhiều hơn mức đáng lẽ (hao hụt).\n" +
                "Chuyến chưa nhập hóa đơn xăng thì lãi/lỗ tạm tính theo km thực chạy.", null);

            // Gỡ khỏi bố cục cũ (1 trang dài) rồi xếp lại.
            dashLayout.SuspendLayout();
            dashLayout.Controls.Clear();
            chartsRow.Controls.Clear();
            dashLayout.RowStyles.Clear();
            dashLayout.RowCount = 0;
            _chartMonthly.Margin = new Padding(0, 0, 0, 16);
            _chartCosts.Margin = new Padding(0, 0, 0, 16);
            _grid.Margin = new Padding(0, 0, 0, 12);

            AddRow(_header, null);
            AddRow(_filterBar, null); // chỉ hiện khi "Tự chọn ngày" (ô từ ngày / đến ngày + nút Xem)
            AddRow(_pages, null);
            // 📊 Tổng quan: thẻ chia đều + (biểu đồ tháng | Cần chú ý)
            AddRow(_mainCards, PageOverview);
            AddRow(_ovRow, PageOverview, 330);
            _ovRowIndex = dashLayout.RowCount - 1;
            // 💰 Tài chính
            AddRow(_finCards, PageFinance);
            AddRow(_chartCosts, PageFinance, 250);
            AddRow(ordersHeader, PageFinance);
            AddRow(_ordersFilter, PageFinance);
            AddRow(_grid, PageFinance, 440);
            // ⛽ Nhiên liệu
            AddRow(_fuelCards, PageFuel);
            AddRow(fuelHeader, PageFuel);
            AddRow(_fuelFilter, PageFuel);
            AddRow(_gridFuel, PageFuel, 420);
            dashLayout.ResumeLayout(true);
            BuildOverviewRow();
        }

        private void AddRow(Control c, string? page, int height = 0)
        {
            var r = dashLayout.RowCount++;
            dashLayout.RowStyles.Add(height > 0 ? new RowStyle(SizeType.Absolute, height) : new RowStyle(SizeType.AutoSize));
            dashLayout.Controls.Add(c, 0, r);
            _rows.Add((c, page, height));
        }

        /// <summary>Hàng tiêu đề: chữ + biểu tượng ⓘ (rê chuột xem giải thích) + control phụ (VD link "Xem tất cả").</summary>
        private FlowLayoutPanel HeaderRow(Label title, string help, Control? extra)
        {
            var row = new FlowLayoutPanel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Dock = DockStyle.Fill, WrapContents = true, Margin = new Padding(0, 0, 0, 4) };
            title.Anchor = AnchorStyles.Left;
            title.Margin = new Padding(0, 0, 4, 4);
            var info = new Label { Text = "ⓘ", AutoSize = true, Font = new Font("Segoe UI", 12F), ForeColor = Theme.Primary, Cursor = Cursors.Help, Margin = new Padding(0, 2, 0, 0) };
            _tip.SetToolTip(info, help);
            _tip.SetToolTip(title, help);
            row.Controls.Add(title);
            row.Controls.Add(info);
            if (extra is not null) row.Controls.Add(extra);
            return row;
        }

        private void ApplyPage()
        {
            var page = _pages.Selected ?? PageOverview;
            dashLayout.SuspendLayout();
            for (var i = 0; i < _rows.Count; i++)
            {
                var (c, p, h) = _rows[i];
                var show = (p is null || p == page) && (c != _filterBar || _period == "c");
                c.Visible = show;
                dashLayout.RowStyles[i] = h > 0 ? new RowStyle(SizeType.Absolute, show ? h : 0) : new RowStyle(SizeType.AutoSize);
            }
            dashLayout.ResumeLayout(true);
            _root.AutoScrollPosition = new Point(0, 0);
        }

        /// <summary>Chuyển trang con rồi cuộn tới control (dùng cho các dòng "Cần chú ý").</summary>
        private void GoTo(string page, Control target)
        {
            _pages.SelectTab(page); // raise Changed -> ApplyPage
            BeginInvoke(new Action(() => _root.ScrollControlIntoView(target)));
        }

        // ================= Thanh lọc =================

        private void CboPeriod_SelectedIndexChanged(object? sender, EventArgs e) { } // ô xổ cũ đã ẩn (thay bằng nút nhanh)

        private async void BtnRefresh_Click(object? sender, EventArgs e) => await LoadAsync();

        private void ApplyPeriod()
        {
            var today = DateTime.Today;
            var custom = _period == "c";
            lblFrom.Visible = lblTo.Visible = _dtpFrom.Visible = _dtpTo.Visible = custom;
            if (_rows.Count > 0) ApplyPage(); // hàng chọn ngày chỉ hiện khi "Tự chọn ngày"
            _dtpFrom.Enabled = _dtpTo.Enabled = custom;
            _btnRefresh.Text = custom ? "🔄 Xem" : "🔄 Làm mới";
            if (custom) ButtonStyles.Primary(_btnRefresh); else ButtonStyles.Secondary(_btnRefresh);
            if (custom) return;

            var monthStart = new DateTime(today.Year, today.Month, 1);
            (_dtpFrom.Value, _dtpTo.Value) = _period switch
            {
                "m" => (monthStart, today),
                "pm" => (monthStart.AddMonths(-1), monthStart.AddDays(-1)),
                "q" => (monthStart.AddMonths(-2), today),
                _ => (new DateTime(today.Year, 1, 1), today),
            };
        }

        /// <summary>Kỳ trước để so sánh: lùi đúng 1 tháng / 3 tháng / 1 năm; "Tùy chọn" lùi đúng số ngày của kỳ.</summary>
        private (DateTime From, DateTime To, string Label) PreviousRange(DateTime from, DateTime to) => _period switch
        {
            "m" or "pm" => (from.AddMonths(-1), to.AddMonths(-1), "tháng trước"),
            "q" => (from.AddMonths(-3), from.AddDays(-1), "3 tháng trước đó"),
            "y" => (from.AddYears(-1), to.AddYears(-1), "năm trước"),
            _ => (from.AddDays(-((to - from).Days + 1)), from.AddDays(-1), "kỳ trước"),
        };

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
                var prevRange = PreviousRange(from, to);
                var mainTask = _api.Reports.GetProfitReportAsync(from, to);
                var prevTask = _api.Reports.GetProfitReportAsync(prevRange.From, prevRange.To);
                var report = await mainTask;
                ProfitReport? prev = null;
                try { prev = await prevTask; } catch { } // kỳ trước lỗi thì chỉ mất dòng so sánh
                if (loadId != _loadId || IsDisposed) return;
                _report = report;
                _prev = prev;

                // Thông tin xe tức thời (không phụ thuộc kỳ) — lỗi cũng không sao.
                // Danh sách xe chỉ Quản trị / Điều phối được xem (Kế toán gọi sẽ bị 403) -> vai trò khác bỏ qua.
                int availableVehicles = 0, totalVehicles = 0, blockedVehicles = 0;
                if (_api.Connection.IsInRole("Admin", "Dispatcher"))
                    try
                    {
                        var vehicles = await _api.Vehicles.GetVehiclesAsync();
                        totalVehicles = vehicles.Count;
                        // "Sẵn sàng" = Rảnh VÀ giấy tờ hợp lệ — đúng xe mà Chia xe dùng được.
                        availableVehicles = vehicles.Count(v => v.Status == "Available" && v.DocumentBlockReason is null);
                        blockedVehicles = vehicles.Count(v => v.DocumentBlockReason is not null);
                    }
                    catch { }
                if (loadId != _loadId || IsDisposed) return;

                RenderMainKpis(_report, _prev, prevRange.Label);
                RenderFinanceKpis(_report);
                RenderGrid(_report);
                RenderFuelControl(_report);
                RenderAttention(_report, availableVehicles, totalVehicles, blockedVehicles);
                dashLayout.PerformLayout(); // hàng thẻ vừa dựng lại -> xếp lại chiều cao 1 lần (CardGrid không tự ép giữa chừng)
                ApplyOverviewLayout();
                _chartMonthly.Invalidate();
                _chartCosts.Invalidate();

                _lblStatus.Text = $"{from:dd/MM/yyyy} – {to:dd/MM/yyyy} (theo ngày tạo đơn) · {_report.TotalOrders} đơn · cập nhật lúc {DateTime.Now:HH:mm}";
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

        // ================= Trang Tổng quan: 4 thẻ chính =================

        private void RenderMainKpis(ProfitReport r, ProfitReport? p, string prevLabel)
        {
            ClearCards(_mainCards);
            _mainCards.Controls.Add(MakeCard("💰", "Doanh thu", ReportFormat.Money(r.Revenue), Theme.Primary, Theme.TextPrimary,
                Delta(r.Revenue, p?.Revenue, prevLabel, higherIsBetter: true, out var c1) ?? $"Đã thu {ReportFormat.Money(r.Collected)}",
                $"{ReportFormat.FullMoney(r.Revenue)} (theo hóa đơn) — đã thu {ReportFormat.FullMoney(r.Collected)}", big: true, subColor: c1,
                link: "Xem từng đơn", onClick: ShowAllOrders));
            _mainCards.Controls.Add(MakeCard("💸", "Chi phí", ReportFormat.Money(r.TotalCost), Theme.Warning, Theme.TextPrimary,
                Delta(r.TotalCost, p?.TotalCost, prevLabel, higherIsBetter: false, out var c2) ?? $"Xăng {ReportFormat.Money(r.FuelCost)}",
                $"{ReportFormat.FullMoney(r.TotalCost)} = xăng {ReportFormat.FullMoney(r.FuelCost)} + phát sinh / sự cố {ReportFormat.FullMoney(r.ExpenseCost + r.IncidentCost)}",
                big: true, subColor: c2, link: "Xem cơ cấu chi phí", onClick: () => GoTo(PageFinance, _chartCosts)));

            var isProfit = r.Profit >= 0;
            var profitColor = isProfit ? Theme.Success : Theme.Danger;
            _mainCards.Controls.Add(MakeCard(isProfit ? "📈" : "📉", isProfit ? "Lãi" : "Lỗ", ReportFormat.Money(r.Profit), profitColor, profitColor,
                r.MarginPercent.HasValue ? $"Biên lợi nhuận {r.MarginPercent:0.#}%" : "Chưa có doanh thu",
                ReportFormat.FullMoney(r.Profit) + (p is not null ? $" — {prevLabel}: {ReportFormat.FullMoney(p.Profit)}" : ""), big: true,
                link: "Xem đơn bị lỗ", onClick: () => ShowOrders("đơn hoàn tất bị lỗ", o => o.Status == "Done" && o.Profit < 0)));

            // Tỉ lệ giao ĐÚNG HẠN: đơn hoàn tất có hạn giao, ngày xong chặng Giao cuối ≤ hạn giao.
            var judged = r.OnTimeOrders + r.LateOrders;
            var prevJudged = (p?.OnTimeOrders ?? 0) + (p?.LateOrders ?? 0);
            _mainCards.Controls.Add(MakeCard("⏱️", "Giao đúng hạn",
                judged > 0 ? ReportFormat.Percent(r.OnTimeOrders, judged) : "—",
                Theme.Success, r.LateOrders > 0 ? Theme.Warning : Theme.TextPrimary,
                judged == 0 ? "Chưa có đơn hoàn tất"
                    : prevJudged > 0 ? $"Trễ {r.LateOrders} đơn · {prevLabel} {ReportFormat.Percent(p!.OnTimeOrders, prevJudged)}"
                    : $"Đúng hạn {r.OnTimeOrders} · Trễ {r.LateOrders} đơn",
                "Đơn hoàn tất trong kỳ có hạn giao: so ngày giao xong với hạn giao" +
                (r.OrdersWithoutDeadline > 0 ? $" ({r.OrdersWithoutDeadline} đơn cũ không có hạn giao — không tính)" : ""), big: true,
                link: "Xem đơn giao trễ", onClick: () => ShowOrders("đơn giao trễ hạn", o => DeliveryGroup(o) == DeliveryLate)));
        }

        /// <summary>Xóa thẻ cũ VÀ giải phóng (Controls.Clear() không giải phóng -> đổi kỳ nhiều lần app nặng dần).</summary>
        private static void ClearCards(Control host)
        {
            foreach (var c in host.Controls.Cast<Control>().ToList()) c.Dispose();
            host.Controls.Clear();
        }

        /// <summary>"▲ 12% so với tháng trước" — màu xanh nếu tốt lên, đỏ nếu xấu đi. null nếu không so được.</summary>
        private static string? Delta(decimal cur, decimal? prev, string prevLabel, bool higherIsBetter, out Color? color)
        {
            color = null;
            if (prev is not decimal pv || pv == 0) return null;
            var pct = (cur - pv) / Math.Abs(pv) * 100;
            if (Math.Abs(pct) < 0.05m) return $"Bằng {prevLabel}";
            var up = pct > 0;
            color = up == higherIsBetter ? Theme.Success : Theme.Danger;
            return $"{(up ? "▲" : "▼")} {Math.Abs(pct):0.#}% so với {prevLabel}";
        }

        // ================= Trang Tổng quan: khung "Cần chú ý" =================

        private void RenderAttention(ProfitReport r, int availableVehicles, int totalVehicles, int blockedVehicles)
        {
            // Danh sách thẻ (AttentionView — DashboardControl.Modern.cs): việc đỏ (gấp) xếp trên, vàng xếp dưới; bấm cả dòng để mở.
            var items = new List<AttentionView.Item>();
            void Item(string icon, string text, int count, bool danger, Action go) => items.Add(new AttentionView.Item(icon, text, count, danger, go));

            var doneLoss = r.Orders.Count(o => o.Status == "Done" && o.Profit < 0);
            var flagged = r.FuelControl?.Count(x => x.Flagged) ?? 0;
            var cancelledWithCost = r.Orders.Count(o => o.Status == "Cancelled" && o.TotalCost > 0);

            if (r.LateOrders > 0)
                Item("⏱️", "Đơn giao trễ hạn", r.LateOrders, true, () => ShowOrders("đơn giao trễ hạn", o => DeliveryGroup(o) == DeliveryLate));
            if (doneLoss > 0)
                Item("📉", "Đơn hoàn tất bị lỗ", doneLoss, true, () => ShowOrders("đơn hoàn tất bị lỗ", o => o.Status == "Done" && o.Profit < 0));
            if (cancelledWithCost > 0)
                Item("🕳️", $"Đơn hủy sau khi xe đã chạy — mất {ReportFormat.Money(r.CancelledOrdersCost)}", cancelledWithCost, true,
                    () => ShowOrders("đơn hủy mất chi phí", o => o.Status == "Cancelled" && o.TotalCost > 0));
            if (flagged > 0)
                Item("🚩", "Chuyến tốn xăng bất thường (lệch > 15%)", flagged, true, () => ShowFuel("state", FuelFlagged));
            if (blockedVehicles > 0)
                Item("📄", "Xe hết hạn / thiếu giấy tờ — không chạy được", blockedVehicles, true, () => OpenMenu("Đội xe"));
            if (r.UninvoicedDoneOrders > 0)
                Item("🧾", "Đơn đã xong chưa xuất hóa đơn", r.UninvoicedDoneOrders, false,
                    () => ShowOrders("đơn đã xong chưa xuất hóa đơn", o => o.Status == "Done" && !o.HasInvoice));
            if (r.TripsUsingFuelEstimate > 0)
                Item("⛽", "Chuyến chưa nhập hóa đơn xăng", r.TripsUsingFuelEstimate, false, () => ShowFuel("actual", FuelNoActual));

            _attnView.SetData(items, availableVehicles, totalVehicles);
        }

        /// <summary>Thẻ Doanh thu -> bảng lãi / lỗ TẤT CẢ đơn trong kỳ (bỏ lọc).</summary>
        private void ShowAllOrders()
        {
            _showAllOrders = true;
            _preset = null;
            _presetText = null;
            _ordersFilter.SetOnly(null, null);
            GoTo(PageFinance, _grid);
        }

        private void ShowOrders(string text, Func<OrderProfitItem, bool> preset)
        {
            _showAllOrders = true;
            _preset = preset;
            _presetText = text;
            _ordersFilter.SetOnly(null, null); // các ô lọc khác về "Tất cả" -> tự vẽ lại bảng
            GoTo(PageFinance, _grid);
        }

        private void ShowFuel(string key, string value)
        {
            _fuelFilter.SetOnly(key, value);
            GoTo(PageFuel, _gridFuel);
        }

        /// <summary>Mở 1 màn khác như bấm mục ở menu trái (tìm nút có chữ kết thúc bằng <paramref name="navText"/>).</summary>
        private void OpenMenu(string navText)
        {
            var btn = FindButton(FindForm(), navText);
            if (btn is null)
            {
                MessageBox.Show(FindForm(), $"Tài khoản của bạn không có quyền vào màn \"{navText}\".", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            // Chạy SAU khi xử lý xong cú bấm hiện tại: mở màn mới sẽ giải phóng màn Tổng quan này.
            // Gọi thẳng MainForm: mục con đang ẩn (nhóm đóng / menu thu gọn) thì PerformClick() không chạy gì.
            if (FindForm() is MainForm main) main.BeginInvoke(new Action(() => main.OpenNavButton(btn)));
            else FindForm()?.BeginInvoke(new Action(btn.PerformClick));
        }

        private static Button? FindButton(Control? parent, string navText)
        {
            if (parent is null) return null;
            foreach (Control c in parent.Controls)
            {
                if (c is Button b && b.Text.TrimEnd().EndsWith(navText)) return b;
                if (c is DashboardControl) continue; // không tìm trong chính màn này
                if (FindButton(c, navText) is { } found) return found;
            }
            return null;
        }

        // ================= Trang Tài chính: thẻ chi phí =================

        private void RenderFinanceKpis(ProfitReport r)
        {
            ClearCards(_finCards);
            _finCards.Controls.Add(MakeCard("⛽", "Chi phí xăng", ReportFormat.Money(r.FuelCost), Theme.Primary, Theme.TextPrimary,
                $"{ReportFormat.Percent(r.FuelCost, r.TotalCost)} tổng chi phí",
                $"{ReportFormat.FullMoney(r.FuelCost)} = xăng đã đổ (hóa đơn) + tạm tính theo km thực chạy cho chuyến chưa nhập hóa đơn"));
            _finCards.Controls.Add(MakeCard("⚠️", "Sự cố + phát sinh", ReportFormat.Money(r.IncidentCost + r.ExpenseCost), Theme.Danger, Theme.TextPrimary,
                $"Riêng sự cố: {ReportFormat.Money(r.IncidentCost)}", ReportFormat.FullMoney(r.IncidentCost + r.ExpenseCost)));
            _finCards.Controls.Add(MakeCard("❌", "Đơn bị hủy", r.CancelledOrders.ToString(), Theme.Danger,
                r.CancelledOrders > 0 ? Theme.Danger : Theme.TextPrimary,
                r.CancelledOrdersCost > 0 ? $"Mất {ReportFormat.Money(r.CancelledOrdersCost)} chi phí" : $"Trên {r.TotalOrders} đơn trong kỳ",
                "Chi phí mất do hủy đơn: xe đã chạy / trả hàng nhưng không thu được tiền — " + ReportFormat.FullMoney(r.CancelledOrdersCost)));
            _finCards.Controls.Add(MakeCard("🧾", "Chưa xuất hóa đơn", r.UninvoicedDoneOrders.ToString(), Theme.Warning,
                r.UninvoicedDoneOrders > 0 ? Theme.Warning : Theme.TextPrimary,
                r.UninvoicedDoneOrders > 0 ? "Đơn đã xong, chưa tính vào doanh thu" : "Đã xuất đủ hóa đơn", null));
        }

        /// <summary>Thẻ số liệu kiểu mới (DashCard — DashboardControl.Modern.cs): ô biểu tượng + nhãn, số to + đơn vị nhỏ,
        /// nhãn màu so sánh, dòng "Xem … →" (cả thẻ bấm được). Rê chuột: chú thích số đầy đủ.</summary>
        private Control MakeCard(string icon, string label, string value, Color accent, Color valueColor, string sub, string? tooltip,
            bool big = false, Color? subColor = null, string? link = null, Action? onClick = null)
        {
            var card = new DashCard
            {
                IconText = icon, Label = label, Value = value, ValueColor = valueColor, Accent = accent,
                Sub = sub, SubColor = subColor, Link = onClick is null ? null : link, Clicked = onClick,
            };
            if (tooltip is not null) _tip.SetToolTip(card, tooltip);
            return card;
        }

        // ================= Bảng lãi/lỗ từng đơn =================

        private const int OrdersPreview = 10;
        private static readonly string[] MoneyColumns = { "Doanh_thu", "Chi_phí", "Lãi_Lỗ" };

        private const string ProfitLoss = "📉 Lỗ";
        private const string ProfitGain = "📈 Lãi";
        private const string ProfitZero = "Hòa vốn";
        private const string DeliveryOnTime = "Đúng hạn";
        private const string DeliveryLate = "Trễ hạn";
        private const string DeliveryNone = "Chưa giao / không có hạn";
        private const string InvoiceMissing = "⚠️ Đơn xong chưa xuất HĐ";
        private const string InvoiceDone = "Đã xuất HĐ";

        private static string ProfitGroup(OrderProfitItem o) => o.Profit < 0 ? ProfitLoss : o.Profit > 0 ? ProfitGain : ProfitZero;
        private static string DeliveryGroup(OrderProfitItem o)
            => o.DeliveryResult is null ? DeliveryNone : o.DeliveryResult == "Đúng hạn" ? DeliveryOnTime : DeliveryLate;

        private void RenderGrid(ProfitReport r)
        {
            _showAllOrders = false;
            _preset = null;
            _presetText = null;
            _ordersFilter.SetChoiceOptions("status", r.Orders.Select(o => ReportFormat.OrderStatus(o.Status)));
            _ordersFilter.SetChoiceOptions("customer", r.Orders.Select(o => o.CustomerName));
            BindOrdersGrid();
        }

        private bool OrdersFiltering()
            => _ordersFilter.SearchText.Length > 0
               || new[] { "status", "customer", "profit", "delivery", "invoice" }.Any(k => _ordersFilter.Choice(k) is not null);

        private void BindOrdersGrid()
        {
            var all = _report?.Orders ?? new List<OrderProfitItem>();
            var rows = all.Where(o =>
                    _ordersFilter.Is("status", ReportFormat.OrderStatus(o.Status)) &&
                    _ordersFilter.Is("customer", o.CustomerName) &&
                    _ordersFilter.Is("profit", ProfitGroup(o)) &&
                    _ordersFilter.Is("delivery", DeliveryGroup(o)) &&
                    (_ordersFilter.Choice("invoice") is not string inv
                        || (inv == InvoiceMissing ? o.Status == "Done" && !o.HasInvoice : o.HasInvoice)) &&
                    _ordersFilter.MatchesSearch(o.OrderCode, o.CustomerName) &&
                    (_preset is null || _preset(o)))
                .OrderBy(o => o.Profit)
                .ToList();
            _ordersFilter.SetCount(rows.Count, all.Count);

            // Mặc định chỉ hiện 10 đơn lỗ nhiều nhất cho gọn; đang lọc / tìm thì hiện hết kết quả.
            var limited = !_showAllOrders && !OrdersFiltering() && rows.Count > OrdersPreview;
            _lnkAllOrders.Text = _preset is not null ? $"Đang xem: {_presetText} ({rows.Count}) · Bỏ lọc"
                : limited ? $"Đang hiện {OrdersPreview} đơn lỗ nhiều nhất · Xem tất cả ({rows.Count})"
                : _showAllOrders && !OrdersFiltering() && rows.Count > OrdersPreview ? $"Thu gọn ({OrdersPreview} đơn)" : "";
            _lnkAllOrders.Visible = _lnkAllOrders.Text.Length > 0;
            if (limited) rows = rows.Take(OrdersPreview).ToList();

            _grid.DataSource = rows.Select(o => new
            {
                o.OrderId,
                Mã_đơn = o.OrderCode,
                Khách_hàng = o.CustomerName,
                Ngày_tạo = o.CreatedAt.ToString("dd/MM/yyyy"),
                Trạng_thái = ReportFormat.OrderStatus(o.Status),
                Doanh_thu = o.Revenue,
                Chi_phí = o.TotalCost,
                Lãi_Lỗ = o.Profit,
                Ghi_chú = NoteFor(o),
            }).ToList();
        }

        /// <summary>Giải thích những đơn trông như "lỗ" nhưng thật ra chưa chốt sổ, và đơn giao trễ.
        /// (Chi phí từng khoản: nhấp đúp 1 đơn.)</summary>
        private static string NoteFor(OrderProfitItem o)
        {
            if (o.Status == "Cancelled") return o.TotalCost > 0 ? "Đơn hủy — chi phí mất trắng" : "Đơn hủy";
            if (o.Status != "Done") return "Đang xử lý — chưa chốt";
            var late = o.DeliveryResult is string d && d.StartsWith("Trễ");
            if (!o.HasInvoice) return late ? "⚠️ Chưa xuất hóa đơn · Giao trễ" : "⚠️ Chưa xuất hóa đơn";
            return late ? "Giao trễ hạn" : "";
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
                e.CellStyle!.ForeColor = note.StartsWith("⚠️") ? Theme.Warning
                    : note.StartsWith("Đơn hủy") || note.Contains("trễ") ? Theme.Danger : Theme.TextSecondary;
            }
        }

        private void Grid_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (_api is null || e.RowIndex < 0) return;
            if (_grid.Rows[e.RowIndex].Cells["OrderId"].Value is not int orderId) return;

            using var f = new OrderCostDetailForm(_api, orderId);
            f.ShowDialog(FindForm());
        }

        // ================= Trang Nhiên liệu =================

        private const string FuelFlagged = "🚩 Lệch > 15%";
        private const string FuelNormal = "Hoàn tất, không lệch";
        private const string FuelOpen = "Chưa hoàn tất / đã hủy";
        private const string FuelHasActual = "Đã nhập";
        private const string FuelNoActual = "Chưa nhập";

        private static string FuelState(FuelControlRowItem x)
            => x.Flagged ? FuelFlagged : x.Status == "Completed" ? FuelNormal : FuelOpen;

        private void RenderFuelControl(ProfitReport r)
        {
            var rows = r.FuelControl ?? new List<FuelControlRowItem>();
            var flagged = rows.Count(x => x.Flagged);

            ClearCards(_fuelCards);
            _fuelCards.Controls.Add(MakeCard("🗺️", "Xăng dự kiến", ReportFormat.Money(r.FuelPlannedCost), Theme.Primary, Theme.TextPrimary,
                "Theo km tuyến đường lúc lập chuyến", ReportFormat.FullMoney(r.FuelPlannedCost)));
            _fuelCards.Controls.Add(MakeCard("📡", "Xăng theo km thực chạy", ReportFormat.Money(r.FuelNormCost), Theme.Primary, Theme.TextPrimary,
                // "Chênh dự kiến" chỉ so trên các chuyến hoàn tất có ĐỦ cả 2 số (2 tổng ở trên có thể gồm 2 tập chuyến khác nhau).
                r.FuelComparedTripCount > 0
                    ? $"Chênh dự kiến {DeviationText(r.FuelNormCostCompared, r.FuelPlannedCostCompared)} ({r.FuelComparedTripCount} chuyến)"
                    : "Chưa có chuyến xong đủ số liệu để so",
                ReportFormat.FullMoney(r.FuelNormCost)));
            _fuelCards.Controls.Add(MakeCard("🧾", "Xăng đã đổ (hóa đơn)", ReportFormat.Money(r.FuelActualCost), Theme.Warning, Theme.TextPrimary,
                r.TripsUsingFuelEstimate > 0 ? $"{r.TripsUsingFuelEstimate} chuyến chưa nhập hóa đơn" : "Đã nhập đủ hóa đơn",
                ReportFormat.FullMoney(r.FuelActualCost)));
            _fuelCards.Controls.Add(MakeCard("🚩", "Chuyến bất thường", $"{flagged}/{rows.Count}", Theme.Danger,
                flagged > 0 ? Theme.Danger : Theme.TextPrimary,
                flagged > 0 ? "Lệch quá 15% — tô đỏ, nằm đầu bảng" : "Không có chuyến bất thường", null));

            _fuelFilter.SetChoiceOptions("plate", rows.Select(x => x.PlateNumber));
            BindFuelGrid();
        }

        private void BindFuelGrid()
        {
            var all = _report?.FuelControl ?? new List<FuelControlRowItem>();
            var rows = all.Where(x =>
                    _fuelFilter.Is("state", FuelState(x)) &&
                    _fuelFilter.Is("actual", x.HasActual ? FuelHasActual : FuelNoActual) &&
                    _fuelFilter.Is("plate", x.PlateNumber) &&
                    _fuelFilter.MatchesSearch($"#{x.TripId}", x.OrderCode, x.PlateNumber, FuelNote(x)))
                .OrderByDescending(x => x.Flagged).ThenByDescending(x => x.TripDate) // chuyến bất thường lên đầu
                .ToList();
            _fuelFilter.SetCount(rows.Count, all.Count);
            _gridFuel.DataSource = rows.Select(x => new
            {
                Chuyến = $"#{x.TripId}",
                Đơn = x.OrderCode,
                Xe = x.PlateNumber,
                Ngày = x.TripDate.ToString("dd/MM/yyyy"),
                Km_dự_kiến = x.PlannedKm,
                Km_thực_chạy = x.GpsKm,
                Lệch_km = x.KmDeviationPct.HasValue ? $"{x.KmDeviationPct:+0.#;-0.#;0}%" : "",
                Xăng_đáng_lẽ = x.NormCost,
                Xăng_đã_đổ = x.HasActual ? x.ActualCost : (decimal?)null,
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
            // Chuyến CHƯA hoàn tất: km / hóa đơn còn đang tăng -> chưa so lệch.
            if (x.Status != "Completed")
                return x.Status == "Cancelled" ? "Chuyến đã hủy — chỉ tính chi phí đã phát sinh" : "Chuyến chưa hoàn tất — chưa so lệch";
            if (x.KmDeviationPct is > 15) notes.Add("Chạy dài hơn tuyến dự kiến");
            if (x.KmDeviationPct is < -15) notes.Add("Km thực chạy thiếu (mất tín hiệu định vị?)");
            if (x.CostDeviationPct is > 15) notes.Add("Đổ nhiều hơn mức đáng lẽ");
            if (x.CostDeviationPct is < -15) notes.Add("Đổ ít hơn mức đáng lẽ (chưa nhập đủ hóa đơn?)");
            if (!x.HasActual) notes.Add("Chưa nhập hóa đơn xăng");
            return string.Join(" · ", notes);
        }

        private static readonly string[] FuelMoneyColumns = { "Xăng_đáng_lẽ", "Xăng_đã_đổ" };

        private void GridFuel_DataBindingComplete(object? sender, DataGridViewBindingCompleteEventArgs e)
        {
            if (_gridFuel.Columns["Cờ"] is { } flagCol) flagCol.Visible = false;
            foreach (var name in FuelMoneyColumns)
            {
                if (_gridFuel.Columns[name] is not { } col) continue;
                col.DefaultCellStyle.Format = "N0";
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
            foreach (var name in new[] { "Km_dự_kiến", "Km_thực_chạy", "Lệch_km", "Lệch_tiền" })
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

        private void ChartMonthly_Paint(object? sender, PaintEventArgs e) => PaintMonthlyChartModern(e.Graphics, _chartMonthly.ClientRectangle);

        private void ChartCosts_Paint(object? sender, PaintEventArgs e) => PaintCostChart(e.Graphics, _chartCosts.ClientRectangle);

        /// <summary>Khung biểu đồ: thẻ trắng bo góc (DrawCard) — tiêu đề bỏ biểu tượng đầu cho gọn.</summary>
        private void DrawFrame(Graphics g, Rectangle rect, string title)
        {
            var space = title.IndexOf(' ');
            DrawCard(g, rect, space > 0 && !char.IsLetterOrDigit(title[0]) ? title[(space + 1)..] : title);
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
            DrawFrame(g, rect, "📈 Doanh thu và chi phí theo tháng");
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
                using (var barPath = Anim.RoundRect(new RectangleF(barLeft, y + 4, barW, rowH - 14), (rowH - 14) / 2f))
                    g.FillPath(barBrush, barPath);

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
            ResizeRedraw = true; // nền theo màu trang (thẻ trắng bo góc tự vẽ trong DrawCard)
        }
    }
}
