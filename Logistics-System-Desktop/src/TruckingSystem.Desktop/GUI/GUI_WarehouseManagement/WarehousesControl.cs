using System.Drawing;
using System.Windows.Forms;
using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_TransportOrderManagement;
using TruckingSystem.Desktop.DTO_WarehouseManagement;

namespace TruckingSystem.Desktop.GUI_WarehouseManagement
{
    /// <summary>
    /// Logic — vẽ SƠ ĐỒ TỔNG THỂ đúng tỉ lệ thật của kho: mỗi WarehouseZone được
    /// đặt ĐÚNG vị trí (PosX, PosY) và kích thước (WidthMeters, HeightMeters)
    /// thật trong không gian kho (Warehouse.LengthMeters x WidthMeters), thay vì
    /// hiện từng thẻ Zone rời rạc như bản trước — giờ nhìn được TOÀN CẢNH kho,
    /// biết chính xác khu nào nằm ở đâu trong thực tế.
    /// </summary>
    public partial class WarehousesControl : BaseUserControl
    {
        private const decimal PixelsPerMeter = 25; // tỉ lệ vẽ — 1 mét thật = 25 pixel LOGIC (màn 100%)
        /// <summary>Pixel THẬT cho 1 mét ở DPI hiện tại (150% -> 37,5 px) — sơ đồ trông y hệt
        /// nhau trên laptop phóng to và màn PC 100% (khu/ô vẽ bằng code không được Windows tự nhân DPI).</summary>
        private decimal Ppm => PixelsPerMeter * DeviceDpi / 96m;
        private bool _currentHasDoor; // kho đang xem đã khai báo cửa chưa (để hiện khoảng cách trong tooltip)

        private ApiServices? _api;
        // Đếm lượt tải: đổi kho nhanh -> lượt tải CŨ trả về muộn KHÔNG được vẽ đè (trước đây
        // sơ đồ có thể lẫn khu của 2 kho).
        private int _zonesLoadId;

        public WarehousesControl()
        {
            // Bố cục (thanh nút gồm cả "📦 Hàng chờ nhập kho", sơ đồ Dock Fill, khung thử Best-Fit)
            // nằm hết trong WarehousesControl.Designer.cs — tự co giãn, không còn Resize tính tay.
            InitializeComponent();
            // Từ ngữ thống nhất: "kệ" (đặt bằng code, không sửa Designer).
            lblTestTitle.Text = "🧪  Thử gợi ý kệ (Best-Fit) — CHỈ GỢI Ý, không nhập kho";
            lblCargoLineId.Text = "Chọn đơn hàng → dòng hàng cần thử xếp kệ:";
            btnSuggest.Text = "🔍  Gợi ý kệ";

            // Chống giật/nhòe hình khi vẽ động nhiều Panel lồng nhau (sơ đồ Zone/Bin) —
            // Panel không cho set DoubleBuffered trực tiếp từ ngoài (protected),
            // phải dùng Reflection để bật.
            typeof(System.Windows.Forms.Control).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                null, zonesPanel, new object[] { true });

            cboWarehouse.SelectedIndexChanged += async (_, _) => { await LoadZonesAsync(); await UpdatePendingCountAsync(); };

            // Kéo cửa sổ sang màn hình khác DPI -> vẽ lại sơ đồ theo tỉ lệ pixel mới.
            DpiChangedAfterParent += (_, _) => _ = LoadZonesAsync();

            SetupModernLook();
            Load += async (_, _) =>
            {
                await LoadWarehousesAsync();
                OpenPendingReceiptsIfRequested();
            };
        }

        private string? _openPendingReceiptsFor;

        /// <summary>Mở từ thông báo "📦 Chuyến #id đã về kho X — chờ nhập kho": tải xong danh sách
        /// kho thì chọn đúng kho X và mở luôn "📦 Nhập hàng từ chuyến". Tên kho rỗng (thông báo cũ)
        /// -> giữ kho đang chọn.</summary>
        /// <param name="tripIssues">true: mở "📤 Xuất kho theo chuyến" (thông báo "chờ kho … xuất hàng") thay cho Nhập hàng.</param>
        public void OpenPendingReceiptsOnLoad(string? warehouseName, bool tripIssues = false)
        {
            _openPendingReceiptsFor = warehouseName ?? "";
            _openTripIssues = tripIssues;
        }

        private bool _openTripIssues;

        private void OpenPendingReceiptsIfRequested()
        {
            if (_openPendingReceiptsFor is not string name) return;
            _openPendingReceiptsFor = null;
            if (name.Length > 0 && cboWarehouse.DataSource is IEnumerable<WarehouseListItem> list)
            {
                var match = list.FirstOrDefault(w => string.Equals(w.Name, name, StringComparison.OrdinalIgnoreCase));
                if (match != null) cboWarehouse.SelectedItem = match;
            }
            var issues = _openTripIssues;
            BeginInvoke(new Action(() =>
            {
                if (issues) BtnTripIssues_Click(this, EventArgs.Empty);
                else BtnPendingReceipts_Click(this, EventArgs.Empty);
            }));
        }

        // ------------------------------------------------------------ thẻ số liệu kho + hiệu ứng ô kệ

        private readonly List<Panel> _binCells = new(); // các ô kệ đang vẽ — để chạy hiệu ứng "dâng hàng"
        private double _binFill = 1;                     // 0 → 1: phần đã dùng của ô dâng dần lên khi vẽ sơ đồ
        private System.Windows.Forms.Timer? _binFillTimer;
        private Button? _btnManage;

        /// <summary>Bố cục gọn (góp ý GVHD):
        ///  • Bỏ hàng 5 thẻ số liệu — số chờ nhập / chờ xuất đã hiện ngay trên 2 nút "(n chờ)"; sức chứa gom vào dòng
        ///    trạng thái phía trên sơ đồ.
        ///  • Thanh nút chỉ giữ VIỆC HẰNG NGÀY của nhân viên kho: Nhập hàng từ chuyến · Xuất kho theo chuyến ·
        ///    Nhận hàng khách mang tới · Tồn kho & lịch sử. Việc QUẢN TRỊ kho (thêm / sửa / xóa kho, khu vực)
        ///    gom vào 1 nút thả xuống "⚙️ Quản lý kho ▾".
        ///  • Bỏ khung "Kiểm tra Best-Fit" (chỉ để thử thuật toán) — sơ đồ kho được rộng chỗ hơn.</summary>
        private void SetupModernLook()
        {
            // Việc quản trị kho -> menu thả xuống (các nút gốc vẫn giữ để dùng lại xử lý + trạng thái bật/tắt).
            foreach (var b in new[] { btnCreateWarehouse, btnEditWarehouse, btnManageZones, btnDeleteWarehouse })
                toolbar.Controls.Remove(b);
            var manage = new ContextMenuStrip();
            var create = MenuItem("➕  Thêm kho mới", BtnCreateWarehouse_Click);
            var edit = MenuItem("✏️  Sửa thông tin kho đang chọn (kích thước, cửa, phí lưu kho)", BtnEditWarehouse_Click);
            manage.Items.Add(create);
            manage.Items.Add(edit);
            manage.Items.Add(MenuItem("🏗️  Quản lý khu vực và kệ", BtnManageZones_Click));
            var sep = new ToolStripSeparator();
            manage.Items.Add(sep);
            var delete = MenuItem("🗑️  Xóa kho đang chọn", BtnDeleteWarehouse_Click);
            delete.ForeColor = Theme.Danger;
            manage.Items.Add(delete);
            _adminItems.AddRange(new ToolStripItem[] { create, edit, sep, delete });
            ModernMenu.Apply(manage);
            _btnManage = ToolbarButton("⚙️ Quản lý kho ▾");
            _btnManage.Click += (_, _) =>
            {
                manage.MinimumSize = new Size(_btnManage.Width, 0);
                manage.Show(_btnManage, new Point(0, _btnManage.Height + _btnManage.LogicalToDeviceUnits(2)));
            };
            toolbar.Controls.Add(_btnManage);

            // Bỏ khung "Kiểm tra thuật toán Best-Fit" (chỉ để thử, làm rối màn) — Best-Fit vẫn chạy thật khi nhập kho.
            testPanel.Visible = false;

            // Thanh SỨC CHỨA (vẽ tay) ngay trên sơ đồ: "Đã dùng 58%" + thanh tiến độ + số liệu + chú thích màu ô.
            _summary.Paint += (_, e) => PaintSummary(e.Graphics);
            _summary.Resize += (_, _) => _summary.Invalidate();
            _summary.Height = LogicalToDeviceUnits(44);
            KpiStrip.InsertRow(root, _summary, root.GetRow(lblStatus));

            // Cửa sổ thấp: giữ sơ đồ kho cao tối thiểu ~320 px, thiếu chỗ thì cả màn cuộn xuống được.
            SetupTaskTiles(); // 3 ô "việc hôm nay" (WarehousesControl.Tiles.cs)
            ScreenKit.Help(this, "warehouses", "Việc hằng ngày ở kho trung chuyển: nhập hàng xe về, xuất hàng lên xe giao, nhận hàng khách mang tới.",
                new[] { "Chọn kho ở ô \"Chọn kho\"", "Bấm 1 trong 3 ô việc phía trên (ô xanh = đang có việc chờ)", "Rê chuột lên 1 kệ trên sơ đồ để xem hàng bên trong; bấm để sửa" },
                "Esc đóng cửa sổ đang mở · Enter = nút chính của cửa sổ");
            ScrollFit.Enable(this, root, zonesPanel, LogicalToDeviceUnits(320));
        }

        private static ToolStripMenuItem MenuItem(string text, EventHandler handler)
        {
            var item = new ToolStripMenuItem(text);
            item.Click += handler;
            return item;
        }

        private Button ToolbarButton(string text)
        {
            var b = new Button
            {
                Text = text,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Color.White,
                ForeColor = Theme.TextPrimary,
                Cursor = Cursors.Hand,
                FlatStyle = FlatStyle.Flat,
                Font = btnEditWarehouse.Font,
                Margin = btnEditWarehouse.Margin,
                MinimumSize = btnEditWarehouse.MinimumSize,
                Padding = btnEditWarehouse.Padding,
                UseVisualStyleBackColor = false,
            };
            b.FlatAppearance.BorderColor = Theme.BorderColor;
            b.FlatAppearance.MouseOverBackColor = Theme.GridAltRow;
            return b;
        }

        // ------------------------------------------------------------ màu & thanh sức chứa

        private static readonly Color FloorColor = ColorTranslator.FromHtml("#F8FAFC");
        private static readonly Color FloorDot = ColorTranslator.FromHtml("#CBD5E1");
        private static readonly Color WallColor = ColorTranslator.FromHtml("#475569");
        private static readonly Color DoorColor = ColorTranslator.FromHtml("#F97316");
        private static readonly Color BinBorder = ColorTranslator.FromHtml("#CBD5E1");
        private static readonly Color BinHover = ColorTranslator.FromHtml("#2563EB");
        private static readonly Color FillLow = ColorTranslator.FromHtml("#34D399");   // còn nhiều chỗ
        private static readonly Color FillMid = ColorTranslator.FromHtml("#FBBF24");   // quá nửa
        private static readonly Color FillHigh = ColorTranslator.FromHtml("#F87171");  // gần đầy (>= 85%)
        private static readonly Color EmptyStripe = ColorTranslator.FromHtml("#EEF2F6");
        private static readonly Font BinCodeFont = new("Segoe UI", 8.5f, FontStyle.Bold);
        private static readonly Font BinPctFont = new("Segoe UI", 8f);
        private static readonly Font ZoneTitleFont = new("Segoe UI", 9f, FontStyle.Bold);

        private static Color FillColor(double pct) => pct >= 0.85 ? FillHigh : pct >= 0.5 ? FillMid : FillLow;

        /// <summary>Loại khu -> (màu thanh tiêu đề, màu nền nhạt, màu viền).</summary>
        private static (Color Head, Color Tint, Color Border) ZoneColors(string zoneType) => zoneType switch
        {
            "Fragile" => (ColorTranslator.FromHtml("#D97706"), ColorTranslator.FromHtml("#FFFBEB"), ColorTranslator.FromHtml("#FDE68A")),
            "Hazmat" => (ColorTranslator.FromHtml("#DC2626"), ColorTranslator.FromHtml("#FEF2F2"), ColorTranslator.FromHtml("#FECACA")),
            _ => (ColorTranslator.FromHtml("#2563EB"), ColorTranslator.FromHtml("#EFF6FF"), ColorTranslator.FromHtml("#BFDBFE")),
        };

        private readonly Panel _summary = new DoubleBufferedPanel { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 8), BackColor = Color.Transparent };
        private int _sumBins, _sumFull, _sumEmpty;
        private decimal _sumUsed, _sumCap;

        /// <summary>Sức chứa của kho đang xem (tính từ dữ liệu sơ đồ vừa tải) -> thanh sức chứa.</summary>
        private void UpdateCapacityKpis(List<StorageBinItem> bins)
        {
            _sumBins = bins.Count;
            _sumCap = bins.Sum(b => b.CapacityM3);
            _sumUsed = bins.Sum(b => Math.Min(b.CurrentUsedM3, b.CapacityM3));
            _sumFull = bins.Count(b => b.CapacityM3 > 0 && b.CurrentUsedM3 / b.CapacityM3 >= 0.85m);
            _sumEmpty = bins.Count(b => b.CurrentUsedM3 <= 0);
            _summary.Invalidate();
        }

        private void PaintSummary(Graphics g)
        {
            int S(int v) => LogicalToDeviceUnits(v);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            var r = new Rectangle(0, 0, _summary.Width - 1, _summary.Height - 1);
            if (r.Width <= 0 || r.Height <= 0) return;
            using (var card = RoundRect(r, S(8)))
            {
                using var bg = new SolidBrush(Color.White);
                using var pen = new Pen(Theme.BorderColor);
                g.FillPath(bg, card);
                g.DrawPath(pen, card);
            }
            var flags = TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine;
            var x = S(16);
            if (_sumBins == 0)
            {
                TextRenderer.DrawText(g, "Kho chưa có kệ nào — vào ⚙️ Quản lý kho ▾ để thêm khu và kệ.", Theme.FontNormal,
                    new Rectangle(x, 0, r.Width - x, r.Height), Theme.TextSecondary, flags);
                return;
            }
            var ratio = _sumCap <= 0 ? 0.0 : (double)(_sumUsed / _sumCap);

            var head = $"Đã dùng {ratio:P0}";
            TextRenderer.DrawText(g, head, Theme.FontBold, new Rectangle(x, 0, S(200), r.Height), Theme.TextPrimary, flags);
            x += TextRenderer.MeasureText(g, head, Theme.FontBold, Size.Empty, flags).Width + S(10);

            // thanh tiến độ
            var bar = new Rectangle(x, r.Height / 2 - S(6), S(200), S(12));
            using (var track = RoundRect(bar, S(6)))
            {
                using var tb = new SolidBrush(ColorTranslator.FromHtml("#E2E8F0"));
                g.FillPath(tb, track);
                var fw = (int)(bar.Width * Math.Min(1.0, ratio));
                if (fw > 0)
                {
                    var state = g.Save();
                    g.SetClip(track);
                    using var fb = new System.Drawing.Drawing2D.LinearGradientBrush(bar, FillLow, FillColor(ratio), 0f);
                    g.FillRectangle(fb, bar.X, bar.Y, fw, bar.Height);
                    g.Restore(state);
                }
            }
            x = bar.Right + S(14);

            // số liệu
            void Part(string text, Color color, Font font)
            {
                TextRenderer.DrawText(g, text, font, new Rectangle(x, 0, r.Width, r.Height), color, flags);
                x += TextRenderer.MeasureText(g, text, font, Size.Empty, flags).Width;
            }
            Part($"{_sumUsed:0.#} / {_sumCap:0.#} m³ · {_sumBins} kệ", Theme.TextSecondary, Theme.FontNormal);
            if (_sumFull > 0) { Part(" · ", Theme.TextSecondary, Theme.FontNormal); Part($"{_sumFull} ô gần đầy", Theme.Danger, Theme.FontBold); }
            Part($" · {_sumEmpty} ô trống", Theme.TextSecondary, Theme.FontNormal);

            // chú thích màu (chỉ vẽ khi còn đủ chỗ bên phải)
            var items = new (string Text, Color Color, bool Empty)[]
                { ("Trống", EmptyStripe, true), ("Còn nhiều chỗ", FillLow, false), ("Quá nửa", FillMid, false), ("Gần đầy", FillHigh, false) };
            var widths = items.Select(i => S(18) + TextRenderer.MeasureText(g, i.Text, Theme.FontSmall, Size.Empty, flags).Width + S(12)).ToList();
            var lx = r.Width - S(12) - widths.Sum();
            if (lx < x + S(20)) return;
            for (var i = 0; i < items.Length; i++)
            {
                var sw = new Rectangle(lx, r.Height / 2 - S(7), S(14), S(14));
                using (var swb = new SolidBrush(items[i].Color)) g.FillRectangle(swb, sw);
                if (items[i].Empty)
                {
                    using var dash = new Pen(ColorTranslator.FromHtml("#94A3B8")) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash };
                    g.DrawRectangle(dash, sw);
                }
                TextRenderer.DrawText(g, items[i].Text, Theme.FontSmall, new Rectangle(lx + S(18), 0, widths[i], r.Height), Theme.TextSecondary, flags);
                lx += widths[i];
            }
        }

        private static System.Drawing.Drawing2D.GraphicsPath RoundRect(Rectangle r, int radius)
        {
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            var d = Math.Max(1, Math.Min(radius * 2, Math.Min(r.Width, r.Height)));
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        /// <summary>Panel bật sẵn chống nháy (vẽ tay nhiều lớp).</summary>
        private sealed class DoubleBufferedPanel : Panel
        {
            public DoubleBufferedPanel()
            {
                DoubleBuffered = true;
                ResizeRedraw = true;
            }
        }

        /// <summary>Phần đã dùng của các ô kệ dâng từ 0 lên đúng % (~0,7 giây) mỗi lần vẽ sơ đồ.</summary>
        private void AnimateBinFill()
        {
            _binFillTimer?.Dispose();
            _binFill = 0;
            _binFillTimer = Anim.Tween(this, 700, p =>
            {
                _binFill = p;
                foreach (var c in _binCells) if (!c.IsDisposed) c.Invalidate();
            }, () => _binFillTimer = null);
        }

        public WarehousesControl(ApiServices api) : this()
        {
            _api = api;
            // Thu gọn theo vai trò (mẫu D2): Nhân viên kho chỉ thấy việc hằng ngày + khu vực & kệ; thêm / sửa / xóa kho,
            // phí lưu kho là việc của Quản trị / Điều phối (chỉ ẩn trên giao diện — server vẫn kiểm tra quyền như cũ).
            if (!api.Connection.IsInRole("Admin", "Dispatcher"))
                foreach (var it in _adminItems) it.Visible = false;
        }

        private readonly List<ToolStripItem> _adminItems = new();

        /// <summary>🙋 Nhận hàng KHÁCH TỰ MANG TỚI kho (đơn "Khách tự mang tới").</summary>
        private void BtnManualStock_Click(object? sender, EventArgs e) => OpenManualStock(ManualStockMode.Receive);

        /// <summary>📋 Tồn kho theo đơn + lịch sử nhập/xuất (xuất tay chỉ cho hàng sót của đơn Hoàn tất / Đã hủy).</summary>
        private void BtnIssueStock_Click(object? sender, EventArgs e) => OpenManualStock(ManualStockMode.Issue);

        private void OpenManualStock(ManualStockMode mode)
        {
            if (_api is null || cboWarehouse.SelectedValue is not int warehouseId)
            {
                Toast.Show(this, "👆 " + "Vui lòng chọn 1 kho trước.", ToastKind.Info);
                return;
            }
            using var f = new ManualStockForm(_api, warehouseId, mode);
            f.ShowDialog(FindForm());
            _ = LoadZonesAsync(); // vẽ lại sơ đồ ngay — có thể vừa nhập/xuất làm đổi % các ô
        }

        /// <summary>Hàng của chuyến Lấy hàng đã về kho nhưng chưa lên kệ — nhập thủ công hoặc Best-Fit.</summary>
        private void BtnPendingReceipts_Click(object? sender, EventArgs e)
        {
            if (_api is null || cboWarehouse.SelectedItem is not WarehouseListItem warehouse)
            {
                Toast.Show(this, "👆 " + "Vui lòng chọn 1 kho trước.", ToastKind.Info);
                return;
            }
            using var f = new PendingReceiptsForm(_api, warehouse.Id, warehouse.Name);
            f.ShowDialog(FindForm());
            if (f.ReceivedAny)
            {
                _ = LoadZonesAsync(); // vẽ lại % các ô vừa xếp hàng vào
            }
            _ = UpdatePendingCountAsync();
        }

        /// <summary>📤 Xuất kho theo chuyến giao — soạn hàng, bốc lên xe, xác nhận xuất (bắt buộc trước khi xe chạy).</summary>
        private void BtnTripIssues_Click(object? sender, EventArgs e)
        {
            if (_api is null || cboWarehouse.SelectedItem is not WarehouseListItem warehouse)
            {
                Toast.Show(this, "👆 " + "Vui lòng chọn 1 kho trước.", ToastKind.Info);
                return;
            }
            using var f = new TripIssuesForm(_api, warehouse.Id, warehouse.Name);
            f.ShowDialog(FindForm());
            if (f.ChangedAny)
            {
                _ = LoadZonesAsync(); // vẽ lại % các ô vừa lấy hàng ra / xếp lại
            }
            _ = UpdatePendingCountAsync();
        }

        private int _pendingCountLoadId;

        /// <summary>Nút "📦 Nhập hàng từ chuyến" hiện luôn SỐ DÒNG đang chờ nhập của kho đang chọn —
        /// biết ngay có việc cần làm, không phải mở ra mới thấy.</summary>
        private async Task UpdatePendingCountAsync()
        {
            const string baseText = "📦 Nhập hàng từ chuyến";
            if (_api is null || cboWarehouse.SelectedValue is not int warehouseId) { btnPendingReceipts.Text = baseText; return; }
            var loadId = ++_pendingCountLoadId;
            try
            {
                var lines = await new TruckingSystem.Desktop.SERVICE_WarehouseManagement.PendingReceiptApiService(_api.Connection).GetPendingAsync(warehouseId);
                if (IsDisposed || loadId != _pendingCountLoadId) return;
                btnPendingReceipts.Text = lines.Count > 0 ? $"{baseText} ({lines.Count} chờ)" : baseText;
                var inTrips = lines.Select(l => l.TripId).Distinct().Count();
                // Số chuyến giao SẴN SÀNG xuất (hàng đã nhập kho đủ, chờ kho soạn + bốc lên xe).
                var issueLines = await new TruckingSystem.Desktop.SERVICE_WarehouseManagement.TripIssueApiService(_api.Connection).GetAsync(warehouseId);
                if (IsDisposed || loadId != _pendingCountLoadId) return;
                var ready = issueLines.Where(l => l.CanIssue).Select(l => l.TripId).Distinct().Count();
                btnTripIssues.Text = ready > 0 ? $"📤 Xuất kho theo chuyến ({ready} chờ)" : "📤 Xuất kho theo chuyến";
                var today = DateTime.Today;
                var urgent = issueLines.Where(l => l.CanIssue && l.LatestStart is DateTime ls && ls.Date <= today).Select(l => l.TripId).Distinct().Count();
                UpdateTiles(inTrips, ready, urgent);
            }
            catch
            {
                if (!IsDisposed && loadId == _pendingCountLoadId) btnPendingReceipts.Text = baseText; // lỗi đếm không chặn màn Kho
            }
        }

        private void BtnManageZones_Click(object? sender, EventArgs e)
        {
            if (_api is null || cboWarehouse.SelectedItem is not WarehouseListItem warehouse)
            {
                Toast.Show(this, "👆 " + "Vui lòng chọn 1 kho trước.", ToastKind.Info);
                return;
            }
            // 1 cửa sổ 2 cột: khu bên trái, sơ đồ + kệ bên phải (thay cho mở lồng Quản lý khu → Vị trí lưu trữ).
            using var f = new WarehouseLayoutForm(_api, warehouse);
            f.ShowDialog(FindForm());
            _ = LoadZonesAsync(); // vẽ lại sơ đồ ngay khi đóng form — có thể vừa thêm khu/vị trí mới
        }

        private async void BtnEditWarehouse_Click(object? sender, EventArgs e)
        {
            if (_api is null || cboWarehouse.SelectedItem is not WarehouseListItem warehouse)
            {
                Toast.Show(this, "👆 " + "Vui lòng chọn 1 kho trước.", ToastKind.Info);
                return;
            }
            using var f = new EditWarehouseForm(_api, warehouse);
            if (f.ShowDialog(FindForm()) == DialogResult.OK)
            {
                await LoadWarehousesAsync();
                cboWarehouse.SelectedValue = warehouse.Id;
            }
        }

        private async void BtnDeleteWarehouse_Click(object? sender, EventArgs e)
        {
            if (_api is null || cboWarehouse.SelectedValue is not int warehouseId) return;
            if (cboWarehouse.SelectedItem is not WarehouseListItem warehouse) return;

            if (!Ask.Confirm(this, $"Xóa kho \"{warehouse.Name}\"?", "Toàn bộ khu và kệ bên trong (nếu chưa có hàng) bị xóa theo. Không hoàn tác được.", "🗑 Xóa kho", danger: true)) return;

            try
            {
                var result = await _api.Warehouses.DeleteWarehouseAsync(warehouseId);
                if (!result.Success)
                {
                    Ask.Fail(this, result.Message ?? "Không thể xóa kho.");
                    return;
                }
                await LoadWarehousesAsync();
            }
            catch (Exception ex)
            {
                Ask.Fail(this, "Lỗi: " + ex.Message);
            }
        }

        private async void BtnCreateWarehouse_Click(object? sender, EventArgs e)
        {
            if (_api is null) return;
            using var f = new CreateWarehouseForm(_api);
            if (f.ShowDialog(FindForm()) != DialogResult.OK) return;

            await LoadWarehousesAsync();
            // Tự động chọn luôn kho vừa tạo — mặc định thấy ngay sơ đồ trống của
            // kho mới thay vì phải tự tìm lại trong danh sách.
            if (cboWarehouse.DataSource is List<WarehouseListItem> list && list.Count > 0)
                cboWarehouse.SelectedValue = list.Max(w => w.Id);
        }

        /// <summary>Khung thử Best-Fit đã BỎ khỏi giao diện (thuật toán đã chạy thật ở Nhập hàng từ chuyến / Nhận hàng
        /// khách mang tới). Hàm rỗng giữ lại vì Designer còn gắn sự kiện nút cũ — xóa khung testPanel trong Designer
        /// thì xóa luôn hàm này.</summary>
        private void BtnSuggest_Click(object? sender, EventArgs e) { }

        private async Task LoadWarehousesAsync()
        {
            if (_api is null) return;
            try
            {
                var warehouses = await _api.Warehouses.GetWarehousesAsync();
                cboWarehouse.DataSource = warehouses;
                cboWarehouse.DisplayMember = nameof(WarehouseListItem.Name);
                cboWarehouse.ValueMember = nameof(WarehouseListItem.Id);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Lỗi tải danh sách kho: " + ex.Message;
                lblStatus.ForeColor = Theme.Danger;
            }
        }

        private async Task LoadZonesAsync()
        {
            if (_api is null || cboWarehouse.SelectedValue is not int warehouseId) return;
            if (cboWarehouse.SelectedItem is not WarehouseListItem warehouse) return;

            var loadId = ++_zonesLoadId;
            lblStatus.Text = "Đang tải sơ đồ kho...";
            ClearMap();
            try
            {
                // Kích thước canvas = đúng kích thước thật của kho (mét) x tỉ lệ vẽ.
                // Nếu kho chưa nhập kích thước (kho cũ, dữ liệu thiếu) thì dùng tạm
                // 1 kích thước mặc định hợp lý để vẫn hiển thị được, không vỡ layout.
                var lengthM = warehouse.LengthMeters ?? 30;
                var widthM = warehouse.WidthMeters ?? 20;
                var canvasWidth = (int)(lengthM * Ppm);
                var canvasHeight = (int)(widthM * Ppm);

                // AutoScrollMinSize thay vì đổi Size trực tiếp — giữ đúng cơ chế
                // cuộn của Panel khi nội dung lớn hơn khung nhìn thực tế.
                // LỀ quanh mặt sàn: tường nằm NGOÀI vùng đặt khu (khu ở x = 0, y = 0 không bị tường / cửa đè), phía có
                // cửa chừa thêm chỗ cho nhãn "🚪 CỬA" nằm ngoài tường.
                var (padL, padT, padR, padB) = MapMargins(warehouse.DoorSide);
                _ox = padL;
                _oy = padT;
                zonesPanel.AutoScrollMinSize = new Size(canvasWidth + padL + padR, canvasHeight + padT + padB);

                // Tải HẾT dữ liệu trước rồi mới vẽ — và chỉ vẽ nếu đây vẫn là lượt tải mới nhất.
                var zones = await _api.Warehouses.GetZonesAsync(warehouseId);
                // Tải ô của MỌI khu CÙNG LÚC (trước đây lần lượt từng khu: kho 6 khu = chờ 6 lượt nối nhau).
                var binLists = await Task.WhenAll(zones.Select(z => _api.Warehouses.GetBinsAsync(z.Id)));
                var binsByZone = zones.Select((z, i) => (Zone: z, Bins: binLists[i])).ToList();
                if (loadId != _zonesLoadId || IsDisposed) return;
                ClearMap();
                _binCells.Clear();
                UpdateCapacityKpis(binsByZone.SelectMany(z => z.Bins).ToList());
                // Left/Top của control con trong Panel AutoScroll tính theo KHUNG NHÌN (đã trừ phần
                // đang cuộn) -> đưa thanh cuộn về gốc trước khi đặt khu/ô theo tọa độ canvas, vẽ xong
                // mới trả lại vị trí cuộn cũ (không thì sơ đồ vẽ lại bị lệch đúng bằng đoạn đã cuộn).
                var scroll = zonesPanel.AutoScrollPosition;
                zonesPanel.AutoScrollPosition = Point.Empty;
                foreach (var (zone, bins) in binsByZone)
                    zonesPanel.Controls.Add(BuildZonePanel(zone, bins));

                // Vẽ TƯỜNG kho (khung viền) và CỬA kho lên sơ đồ
                AddWarehouseWalls(canvasWidth, canvasHeight);
                _currentHasDoor = AddDoorMarker(warehouse, canvasWidth, canvasHeight);
                zonesPanel.AutoScrollPosition = new Point(-scroll.X, -scroll.Y);
                AnimateBinFill();

                var missingSize = warehouse.LengthMeters is null || warehouse.WidthMeters is null;
                var warnings = new List<string>();
                if (missingSize) warnings.Add("⚠️ kho chưa khai báo kích thước thật, đang hiện tạm theo tỉ lệ mặc định");
                if (!_currentHasDoor) warnings.Add("⚠️ kho chưa khai báo vị trí cửa — chưa ưu tiên gần/xa cửa được (vào Sửa kho để khai báo)");
                lblStatus.Text = $"{zones.Count} khu vực — bấm vào khu / kệ để sửa, rê chuột vào kệ để xem hàng bên trong" +
                                 string.Concat(warnings.Select(w => "   " + w));
                lblStatus.ForeColor = warnings.Count > 0 ? Theme.Warning : Theme.TextSecondary;
            }
            catch (Exception ex)
            {
                if (loadId != _zonesLoadId || IsDisposed) return;
                lblStatus.Text = "Lỗi: " + ex.Message;
                lblStatus.ForeColor = Theme.Danger;
            }
        }

        /// <summary>Vẽ MẶT SÀN + TƯỜNG kho: sàn xám rất nhạt có lưới chấm (mỗi chấm cách nhau 1 m — dễ hình dung
        /// kích thước), tường nét liền xám đậm bao quanh. Nằm DƯỚI các khu vực.</summary>
        private void AddWarehouseWalls(int canvasWidth, int canvasHeight)
        {
            var wall = WallThickness;
            // Tấm sàn = mặt sàn + 1 vòng tường bao NGOÀI (tường không lấn vào chỗ đặt khu vực).
            var floor = new DoubleBufferedPanel
            {
                Left = _ox - wall,
                Top = _oy - wall,
                Width = canvasWidth + 2 * wall,
                Height = canvasHeight + 2 * wall,
                BackColor = FloorColor,
            };
            floor.Paint += (_, e) =>
            {
                var g = e.Graphics;
                var step = (float)Ppm;
                if (step >= 8)
                {
                    using var dot = new SolidBrush(FloorDot);
                    var size = Math.Max(2, LogicalToDeviceUnits(2));
                    for (var x = wall + step; x < floor.Width - wall - 1; x += step)
                        for (var y = wall + step; y < floor.Height - wall - 1; y += step)
                            g.FillRectangle(dot, x - size / 2f, y - size / 2f, size, size);
                }
                using var pen = new Pen(WallColor, wall);
                g.DrawRectangle(pen, wall / 2f, wall / 2f, floor.Width - wall, floor.Height - wall);
            };
            zonesPanel.Controls.Add(floor);
            floor.SendToBack(); // nằm DƯỚI các khu vực, không che mất khu vực nào
        }

        private int _ox, _oy; // gốc tọa độ mặt sàn trong khung sơ đồ (sau lề + tường)

        private static readonly string DoorText = "🚪 CỬA";

        private Size DoorLabelSize()
            => TextRenderer.MeasureText(DoorText, Theme.FontBold) + new Size(LogicalToDeviceUnits(14), LogicalToDeviceUnits(4));

        /// <summary>Lề trái / trên / phải / dưới của sơ đồ: khoảng trống + tường, phía có cửa thêm chỗ cho nhãn cửa.</summary>
        private (int L, int T, int R, int B) MapMargins(string? doorSide)
        {
            var baseM = LogicalToDeviceUnits(16) + WallThickness;
            var lbl = DoorLabelSize();
            var gap = LogicalToDeviceUnits(6);
            return (baseM + (doorSide == "Left" ? lbl.Width + gap : 0),
                    baseM + (doorSide == "Top" ? lbl.Height + gap : 0),
                    baseM + (doorSide == "Right" ? lbl.Width + gap : 0),
                    baseM + (doorSide == "Bottom" ? lbl.Height + gap : 0));
        }

        /// <summary>1 ToolTip dùng chung cho mọi khu / ô / cửa trên sơ đồ (đủ lâu để đọc danh sách hàng bên trong).</summary>
        private readonly ToolTip _mapTip = new() { AutoPopDelay = 15000, InitialDelay = 300 };

        /// <summary>Xóa sơ đồ cũ VÀ GIẢI PHÓNG các khu / ô cũ. Trước đây chỉ Controls.Clear() — các ô cũ không được giải phóng,
        /// mỗi lần đổi kho / tải lại / sửa ô lại tích thêm hàng trăm cửa sổ con + ToolTip -> dùng lâu app chậm dần.</summary>
        private void ClearMap()
        {
            _mapTip.RemoveAll();
            _binCells.Clear();
            foreach (var c in zonesPanel.Controls.Cast<Control>().ToList()) c.Dispose();
            zonesPanel.Controls.Clear();
        }

        private int WallThickness => LogicalToDeviceUnits(8);

        /// <summary>Vẽ CỬA kho đúng vị trí trên tường (thanh màu cam dày, rộng ~3 m)
        /// kèm nhãn "🚪 CỬA" nằm phía trong kho, cạnh cửa. Trả về false nếu kho chưa
        /// khai báo cửa hoặc chưa có kích thước thật.</summary>
        private bool AddDoorMarker(WarehouseListItem warehouse, int canvasWidth, int canvasHeight)
        {
            if (warehouse.DoorSide is null || warehouse.DoorOffsetMeters is null
                || warehouse.LengthMeters is null || warehouse.WidthMeters is null)
                return false;

            var thickness = WallThickness; // cửa = KHOẢNG HỞ trên tường (cùng bề dày tường)
            var doorColor = DoorColor;
            var doorLength = Math.Max(LogicalToDeviceUnits(40), (int)(3 * Ppm));
            var offsetPx = (int)(warehouse.DoorOffsetMeters.Value * Ppm);

            // Cửa = khoảng hở NẰM TRÊN TƯỜNG (tường ở ngoài mặt sàn) — không đè lên khu vực nào.
            var along = Math.Clamp(offsetPx - doorLength / 2, 0, Math.Max(0, (warehouse.DoorSide is "Left" or "Right" ? canvasHeight : canvasWidth) - doorLength));
            var rect = warehouse.DoorSide switch
            {
                "Top" => new Rectangle(_ox + along, _oy - thickness, doorLength, thickness),
                "Bottom" => new Rectangle(_ox + along, _oy + canvasHeight, doorLength, thickness),
                "Left" => new Rectangle(_ox - thickness, _oy + along, thickness, doorLength),
                _ => new Rectangle(_ox + canvasWidth, _oy + along, thickness, doorLength), // Right
            };

            var tipText = $"🚪 Cửa kho — tường {WarehouseDoorHelper.WallName(warehouse.DoorSide)}, cách góc {warehouse.DoorOffsetMeters:0.#} m";
            var door = new Panel { Bounds = rect, BackColor = FloorColor };
            var vertical = warehouse.DoorSide is "Left" or "Right";
            door.Paint += (_, e) =>
            {
                // 2 vạch cam ở 2 mép khoảng hở = khung cửa
                using var b = new SolidBrush(doorColor);
                var k = LogicalToDeviceUnits(3);
                if (vertical) { e.Graphics.FillRectangle(b, 0, 0, door.Width, k); e.Graphics.FillRectangle(b, 0, door.Height - k, door.Width, k); }
                else { e.Graphics.FillRectangle(b, 0, 0, k, door.Height); e.Graphics.FillRectangle(b, door.Width - k, 0, k, door.Height); }
            };
            _mapTip.SetToolTip(door, tipText);
            zonesPanel.Controls.Add(door);
            door.BringToFront();

            var label = new Label
            {
                Text = DoorText,
                AutoSize = true,
                Font = Theme.FontBold,
                ForeColor = Color.White,
                BackColor = doorColor,
                Padding = new Padding(LogicalToDeviceUnits(6), LogicalToDeviceUnits(1), LogicalToDeviceUnits(6), LogicalToDeviceUnits(1)),
            };
            zonesPanel.Controls.Add(label);
            // Nhãn nằm NGOÀI tường (phía lề đã chừa sẵn) -> không che khu / ô bên trong.
            var size = label.PreferredSize;
            var gap = LogicalToDeviceUnits(4);
            label.Location = warehouse.DoorSide switch
            {
                "Top" => new Point(Math.Max(0, rect.X + rect.Width / 2 - size.Width / 2), rect.Top - size.Height - gap),
                "Bottom" => new Point(Math.Max(0, rect.X + rect.Width / 2 - size.Width / 2), rect.Bottom + gap),
                "Left" => new Point(Math.Max(0, rect.Left - size.Width - gap), rect.Y + rect.Height / 2 - size.Height / 2),
                _ => new Point(rect.Right + gap, rect.Y + rect.Height / 2 - size.Height / 2),
            };
            _mapTip.SetToolTip(label, tipText);
            label.BringToFront();
            return true;
        }

        /// <summary>Vẽ 1 khu vực (Zone) ĐÚNG vị trí + kích thước thật (PosX, PosY,
        /// WidthMeters, HeightMeters, quy đổi ra pixel theo PixelsPerMeter) —
        /// khác bản trước chỉ hiện thẻ rời rạc không theo tỉ lệ nào cả. Bên trong
        /// vẫn vẽ lưới StorageBin theo Row/Column như cũ, nhưng giờ CO GIÃN vừa
        /// đúng khung của Zone thay vì cỡ ô cố định 90px.
        ///
        /// 2 CẢI TIẾN mới:
        ///  1. Mỗi ô tô màu ĐÚNG TỈ LỆ % đã dùng (VD: dùng 60% -> 60% ô màu đỏ
        ///     dâng từ dưới lên, 40% còn lại vẫn xanh) — thay vì trước đây cả ô
        ///     chỉ 1 màu duy nhất theo ngưỡng, khó phân biệt "còn bao nhiêu %".
        ///  2. Click trực tiếp vào ô/khu vực trên sơ đồ ĐỂ MỞ NGAY form Sửa —
        ///     không cần vòng qua "Quản lý khu vực" → double-click → "Quản lý
        ///     vị trí" → chọn dòng → "Sửa vị trí" như trước, nhanh hơn nhiều.</summary>
        private Panel BuildZonePanel(WarehouseZoneItem zone, List<StorageBinItem> bins)
        {
            // Mọi khoảng cách cố định tính theo pixel LOGIC rồi nhân DPI (LogicalToDeviceUnits).
            var headerHeight = LogicalToDeviceUnits(26);
            var padding = LogicalToDeviceUnits(6);
            var radius = LogicalToDeviceUnits(8);
            var ppm = Ppm;
            var (head, tint, border) = ZoneColors(zone.ZoneType);
            var zoneCap = bins.Sum(b => b.CapacityM3);
            var zonePct = zoneCap <= 0 ? 0.0 : (double)(bins.Sum(b => Math.Min(b.CurrentUsedM3, b.CapacityM3)) / zoneCap);

            var zonePanel = new DoubleBufferedPanel
            {
                Left = _ox + (int)(zone.PosX * ppm),
                Top = _oy + (int)(zone.PosY * ppm),
                Width = Math.Max(LogicalToDeviceUnits(60), (int)(zone.WidthMeters * ppm)),
                Height = Math.Max(LogicalToDeviceUnits(60), (int)(zone.HeightMeters * ppm)),
                BackColor = FloorColor, // 4 góc bo lộ ra màu sàn
                Cursor = Cursors.Hand,
            };
            // Nền nhạt theo loại khu + thanh tiêu đề màu: "📦 Khu A ........ 46%".
            zonePanel.Paint += (_, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                var r = new Rectangle(0, 0, zonePanel.Width - 1, zonePanel.Height - 1);
                using var body = RoundRect(r, radius);
                using (var tb = new SolidBrush(tint)) g.FillPath(tb, body);
                var state = g.Save();
                g.SetClip(body);
                using (var hb = new SolidBrush(head)) g.FillRectangle(hb, 0, 0, zonePanel.Width, headerHeight);
                g.Restore(state);
                using (var pen = new Pen(border, LogicalToDeviceUnits(1))) g.DrawPath(pen, body);

                var flags = TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis;
                var pctText = bins.Count > 0 ? $"{zonePct:P0}" : "";
                var pctW = pctText.Length > 0 ? TextRenderer.MeasureText(g, pctText, ZoneTitleFont, Size.Empty, flags).Width + LogicalToDeviceUnits(12) : 0;
                TextRenderer.DrawText(g, $"{ZoneIcon(zone.ZoneType)} {zone.Name}", ZoneTitleFont,
                    new Rectangle(LogicalToDeviceUnits(8), 0, zonePanel.Width - LogicalToDeviceUnits(16) - pctW, headerHeight), Color.White, flags);
                if (pctW > 0 && zonePanel.Width > pctW + LogicalToDeviceUnits(60))
                {
                    var pill = new Rectangle(zonePanel.Width - pctW - LogicalToDeviceUnits(6), LogicalToDeviceUnits(4), pctW, headerHeight - LogicalToDeviceUnits(8));
                    using var pp = RoundRect(pill, pill.Height / 2);
                    using var pb = new SolidBrush(Color.FromArgb(64, Color.White));
                    g.FillPath(pb, pp);
                    TextRenderer.DrawText(g, pctText, ZoneTitleFont, pill, Color.White,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
                }
            };
            // Click vào NỀN khu vực (không phải vào 1 ô vị trí cụ thể) -> mở Sửa khu vực
            zonePanel.Click += async (_, _) => await OpenEditZoneAsync(zone);
            _mapTip.SetToolTip(zonePanel, $"{zone.Name} — đã dùng {zonePct:P0}\n(Bấm để sửa khu vực)");

            if (bins.Count == 0) return zonePanel;

            foreach (var bin in bins)
            {
                var pct = bin.CapacityM3 == 0 ? 0.0 : Math.Min(1.0, (double)(bin.CurrentUsedM3 / bin.CapacityM3));

                // Vẽ ĐÚNG tọa độ + kích thước thật (mét) của vị trí, TƯƠNG ĐỐI so với góc trên-trái của khu vực.
                var cell = new DoubleBufferedPanel
                {
                    Left = padding + (int)(bin.PosX * ppm),
                    Top = headerHeight + (int)(bin.PosY * ppm),
                    Width = Math.Max(LogicalToDeviceUnits(4), (int)(bin.WidthMeters * ppm)),
                    Height = Math.Max(LogicalToDeviceUnits(4), (int)(bin.HeightMeters * ppm)),
                    BackColor = tint, // góc bo lộ ra màu nền khu
                    Cursor = Cursors.Hand,
                };
                var hover = false;
                cell.MouseEnter += (_, _) => { hover = true; cell.Invalidate(); };
                cell.MouseLeave += (_, _) => { hover = false; cell.Invalidate(); };

                // Ô nền TRẮNG; phần ĐÃ DÙNG dâng từ DƯỚI LÊN đúng tỉ lệ % (màu dịu theo mức đầy); ô trống kẻ sọc nhạt.
                // Chữ tối (mã ô + %) ở giữa — dễ đọc trên mọi màu.
                cell.Paint += (_, e) =>
                {
                    var g = e.Graphics;
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    var small = cell.Width < LogicalToDeviceUnits(14) || cell.Height < LogicalToDeviceUnits(14);
                    var r = new Rectangle(0, 0, cell.Width - 1, cell.Height - 1);
                    using var shape = RoundRect(r, small ? 1 : LogicalToDeviceUnits(5));
                    using (var white = new SolidBrush(Color.White)) g.FillPath(white, shape);

                    var state = g.Save();
                    g.SetClip(shape);
                    if (pct <= 0)
                    {
                        using var stripe = new System.Drawing.Drawing2D.HatchBrush(System.Drawing.Drawing2D.HatchStyle.WideUpwardDiagonal, EmptyStripe, Color.White);
                        g.FillRectangle(stripe, r);
                    }
                    else
                    {
                        var usedHeight = (int)(cell.Height * pct * _binFill); // dâng dần lên khi vừa vẽ sơ đồ
                        using var fill = new SolidBrush(FillColor(pct));
                        g.FillRectangle(fill, 0, cell.Height - usedHeight, cell.Width, usedHeight);
                    }
                    g.Restore(state);

                    using (var pen = new Pen(hover ? BinHover : BinBorder, LogicalToDeviceUnits(hover ? 2 : 1)))
                    {
                        if (pct <= 0 && !hover) pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                        g.DrawPath(pen, shape);
                    }

                    // Chữ: đủ chỗ thì 2 dòng (mã + %), ít chỗ thì chỉ mã ô, quá nhỏ thì chỉ nhìn màu.
                    var flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine;
                    var pctText = pct <= 0 ? "Trống" : $"{pct:P0}";
                    var codeColor = pct <= 0 ? ColorTranslator.FromHtml("#94A3B8") : Theme.TextPrimary;
                    if (cell.Width >= LogicalToDeviceUnits(36) && cell.Height >= LogicalToDeviceUnits(34))
                    {
                        var mid = cell.Height / 2;
                        TextRenderer.DrawText(g, bin.Code, BinCodeFont, new Rectangle(0, mid - LogicalToDeviceUnits(16), cell.Width, LogicalToDeviceUnits(16)), codeColor, flags);
                        TextRenderer.DrawText(g, pctText, BinPctFont, new Rectangle(0, mid, cell.Width, LogicalToDeviceUnits(15)), ColorTranslator.FromHtml("#334155"), flags);
                    }
                    else if (cell.Width >= LogicalToDeviceUnits(30) && cell.Height >= LogicalToDeviceUnits(18))
                        TextRenderer.DrawText(g, bin.Code, BinCodeFont, r, codeColor, flags | TextFormatFlags.EndEllipsis);
                };

                // Click vào Ô -> mở Sửa vị trí ngay
                cell.Click += async (_, _) => await OpenEditBinAsync(bin, zone);
                _binCells.Add(cell);

                var tooltip = _mapTip; // 1 ToolTip dùng chung cho cả sơ đồ (trước đây mỗi ô tạo 1 cái, không bao giờ giải phóng)
                var tooltipText = $"{bin.Code}\nĐã dùng: {bin.CurrentUsedM3:#,##0.##}/{bin.CapacityM3:#,##0.##} m³ ({pct:P0})\nTải tối đa: {(bin.MaxWeightKg > 0 ? bin.MaxWeightKg.ToString("#,##0.##") + " kg" : "không giới hạn")}\n" +
                    (_currentHasDoor ? $"🚪 Cách cửa: {bin.DistanceFromDoorMeters:0.#} m (tự tính)\n" : "") + "\n" +
                    (bin.Contents.Count == 0
                        ? "(Trống — chưa có hàng)"
                        : "📦 Hàng bên trong:\n" + string.Join("\n", bin.Contents.Select(c =>
                            $"• Đơn {c.OrderCode} — {c.CustomerName}\n   {c.CargoType}: {c.QtyStored:0.##} đơn vị (nhập {c.ReceivedAt.ToLocalTime():dd/MM HH:mm})"))) +
                    "\n\n(Bấm để sửa)";
                tooltip.SetToolTip(cell, tooltipText);

                zonePanel.Controls.Add(cell);
            }
            return zonePanel;
        }

        /// <summary>Mở form Sửa khu vực ngay từ sơ đồ — vẽ lại sơ đồ ngay sau khi lưu.</summary>
        private async Task OpenEditZoneAsync(WarehouseZoneItem zone)
        {
            if (_api is null) return;
            using var f = new EditZoneForm(_api, zone, cboWarehouse.SelectedValue is int whId ? whId : 0);
            if (f.ShowDialog(FindForm()) == DialogResult.OK) await LoadZonesAsync();
        }

        /// <summary>Mở form Sửa vị trí ngay từ sơ đồ — vẽ lại sơ đồ ngay sau khi lưu
        /// (thấy ngay màu/tỉ lệ mới nếu vừa đổi sức chứa).</summary>
        private async Task OpenEditBinAsync(StorageBinItem bin, WarehouseZoneItem zone)
        {
            if (_api is null) return;
            using var f = new EditBinForm(_api, bin, zone.Name, zone.WidthMeters, zone.HeightMeters, zone.Id);
            if (f.ShowDialog(FindForm()) == DialogResult.OK) await LoadZonesAsync();
        }

        private static string ZoneIcon(string zoneType) => zoneType switch { "Fragile" => "🔷", "Hazmat" => "☣️", _ => "📦" };
    }
}
