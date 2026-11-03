using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_WarehouseManagement;

namespace TruckingSystem.Desktop.GUI_Shared
{
    /// <summary>SỐ VIỆC CHỜ ngay trên menu trái (chấm đỏ có số ở mép phải mục menu):
    ///  • 📦 Đơn vận chuyển: đơn nháp chờ xác nhận + đơn đã xác nhận chưa chia xe.
    ///  • 🛣️ Chuyến xe: chuyến tài xế báo xong, chờ duyệt.
    ///  • 🏬 Kho trung chuyển: chuyến đã về kho chờ nhập + chuyến giao sẵn sàng chờ xuất (mọi kho).
    /// Nhóm đang thu gọn thì nút nhóm hiện TỔNG số của các mục bên trong — biết ngay chỗ nào cần làm mà không phải mở
    /// từng màn. Chỉ đếm những gì tài khoản được xem. Cập nhật lúc mở app, mỗi 3 phút, và sau khi chuyển màn (lúc vừa làm
    /// xong việc ở màn trước). Lỗi đếm thì thôi, không báo làm phiền.</summary>
    public partial class MainForm
    {
        private readonly Dictionary<string, int> _menuCounts = new();
        private readonly Dictionary<string, string> _menuDetails = new(); // "Đơn vận chuyển" -> "2 đơn nháp chờ xác nhận · 1 đơn chờ chia xe"
        private readonly ToolTip _badgeTip = new() { AutoPopDelay = 15000, InitialDelay = 300 };
        private (string Warehouse, bool Issues)? _kboTarget; // kho đầu tiên đang có việc chờ: (tên, true = chờ xuất / false = chờ nhập)
        private readonly System.Windows.Forms.Timer _badgeTimer = new() { Interval = 3 * 60 * 1000 };
        private readonly System.Windows.Forms.Timer _badgeSoon = new() { Interval = 1500 };
        private bool _badgeLoading, _badgesReady;

        private static readonly Font BadgeFont = new("Segoe UI", 8F, FontStyle.Bold);

        private void SetupMenuBadges()
        {
            if (_api is null) return;
            foreach (var b in _navButtons) b.Paint += (_, e) => PaintBadge(e.Graphics, b, CountFor(b), arrowSpace: false);
            foreach (var h in _navHeaders) h.Paint += (_, e) => PaintBadge(e.Graphics, h, (string)h.Tag! == _openGroup && !_navCollapsed ? 0 : GroupCount((string)h.Tag!), arrowSpace: true);

            _badgeTimer.Tick += async (_, _) => await RefreshMenuBadgesAsync();
            _badgeSoon.Tick += async (_, _) => { _badgeSoon.Stop(); await RefreshMenuBadgesAsync(); };
            _badgeTimer.Start();
            _badgesReady = true;
            FormClosed += (_, _) => { _badgeTimer.Dispose(); _badgeSoon.Dispose(); _badgeTip.Dispose(); };
            _ = RefreshMenuBadgesAsync();
        }

        /// <summary>Gọi sau khi chuyển màn: đếm lại sau 1,5 giây (gộp nhiều lần bấm liên tiếp thành 1 lần đếm).</summary>
        private void RefreshMenuBadgesSoon()
        {
            if (_api is null || !_badgesReady) return;
            _badgeSoon.Stop();
            _badgeSoon.Start();
        }

        private int CountFor(Button b)
            => _menuCounts.FirstOrDefault(kv => b.Text.TrimEnd().EndsWith(kv.Key)).Value;

        private int GroupCount(string group)
            => _groupOf.Where(kv => kv.Value == group).Sum(kv => CountFor(kv.Key));

        private async Task RefreshMenuBadgesAsync()
        {
            if (_api is null || _badgeLoading || IsDisposed) return;
            _badgeLoading = true;
            try
            {
                var api = _api;
                var counts = new Dictionary<string, int>();
                var details = new Dictionary<string, string>();
                (string, bool)? kbo = null;
                static string Join(params (int N, string Text)[] parts)
                    => string.Join(" · ", parts.Where(p => p.N > 0).Select(p => $"{p.N} {p.Text}"));
                // NHANH: 1 lần gọi API chỉ đếm (đăng nhập xong thấy số gần như ngay). API cũ chưa có -> cách cũ bên dưới.
                WorkCountsItem? wc = null;
                try { wc = await api.Warehouses.GetWorkCountsAsync(); } catch { }
                if (wc is not null)
                {
                    if (wc.DraftOrders >= 0)
                    {
                        counts["Đơn vận chuyển"] = wc.DraftOrders + wc.OrdersWaitAllocation + wc.OrdersOverdueStorage;
                        details["Đơn vận chuyển"] = Join((wc.DraftOrders, "đơn nháp chờ xác nhận"), (wc.OrdersWaitAllocation, "đơn chờ chia xe"),
                            (wc.OrdersOverdueStorage, "đơn hàng nằm kho quá hạn, chưa xác định do ai"));
                        counts["Chuyến xe"] = wc.TripsWaitApproval;
                        details["Chuyến xe"] = Join((wc.TripsWaitApproval, "chuyến tài xế báo xong, chờ duyệt"));
                    }
                    var per = wc.Warehouses.Select(w => (w.Name, In: w.InTrips, Out: w.OutTrips)).ToList();
                    SetWarehouseCounts(per, counts, details, out kbo);
                    var urgent = wc.Warehouses.Sum(w => w.UrgentOut);
                    if (urgent > 0 && details.ContainsKey("Kho trung chuyển"))
                        details["Kho trung chuyển"] += $"\n⏰ {urgent} chuyến phải xuất kho hôm nay (hoặc đã quá mốc)";
                }
                else if (api.Connection.IsInRole("Admin", "Dispatcher"))
                {
                    var ordersTask = api.TransportOrders.GetTransportOrdersAsync();
                    var tripsTask = api.Trips.GetTripsAsync();
                    try
                    {
                        var orders = await ordersTask;
                        var drafts = orders.Count(o => o.Status == "Draft");
                        var waitAlloc = orders.Count(o => o.Status == "Confirmed" && o.ActiveTripCount == 0);
                        counts["Đơn vận chuyển"] = drafts + waitAlloc;
                        details["Đơn vận chuyển"] = Join((drafts, "đơn nháp chờ xác nhận"), (waitAlloc, "đơn chờ chia xe"));
                    }
                    catch { }
                    try
                    {
                        var wait = (await tripsTask).Count(t => t.Status == "PendingConfirmation");
                        counts["Chuyến xe"] = wait;
                        details["Chuyến xe"] = Join((wait, "chuyến tài xế báo xong, chờ duyệt"));
                    }
                    catch { }
                }
                if (wc is null && api.Connection.IsInRole("Admin", "Dispatcher", "WarehouseStaff"))
                {
                    try
                    {
                        var warehouses = await api.Warehouses.GetWarehousesAsync();
                        var receipts = new TruckingSystem.Desktop.SERVICE_WarehouseManagement.PendingReceiptApiService(api.Connection);
                        var issues = new TruckingSystem.Desktop.SERVICE_WarehouseManagement.TripIssueApiService(api.Connection);
                        var perWarehouse = await Task.WhenAll(warehouses.Select(async (WarehouseListItem w) =>
                        {
                            var r = await receipts.GetPendingAsync(w.Id);
                            var i = await issues.GetAsync(w.Id);
                            return (w.Name, In: r.Select(x => x.TripId).Distinct().Count(),
                                    Out: i.Where(x => x.CanIssue).Select(x => x.TripId).Distinct().Count());
                        }));
                        SetWarehouseCounts(perWarehouse.ToList(), counts, details, out kbo);
                    }
                    catch { }
                }
                if (IsDisposed) return;
                _menuCounts.Clear();
                foreach (var (k, v) in counts) _menuCounts[k] = v;
                _menuDetails.Clear();
                foreach (var (k, v) in details) _menuDetails[k] = v;
                _kboTarget = kbo;
                UpdateBadgeTips();
                foreach (var b in _navButtons) b.Invalidate();
                foreach (var h in _navHeaders) h.Invalidate();
            }
            finally { _badgeLoading = false; }
        }

        private static void SetWarehouseCounts(List<(string Name, int In, int Out)> perWarehouse,
            Dictionary<string, int> counts, Dictionary<string, string> details, out (string, bool)? kbo)
        {
            static string Join(params (int N, string Text)[] parts)
                => string.Join(" · ", parts.Where(p => p.N > 0).Select(p => $"{p.N} {p.Text}"));
            kbo = null;
            var inTotal = perWarehouse.Sum(x => x.In);
            var outTotal = perWarehouse.Sum(x => x.Out);
            counts["Kho trung chuyển"] = inTotal + outTotal;
            details["Kho trung chuyển"] = Join((inTotal, "chuyến đã về kho chờ nhập hàng"), (outTotal, "chuyến chờ kho xuất hàng"))
                + (perWarehouse.Count(x => x.In + x.Out > 0) > 1
                    ? " (" + string.Join(", ", perWarehouse.Where(x => x.In + x.Out > 0).Select(x => $"{x.Name}: {x.In + x.Out}")) + ")" : "");
            // Bấm mục Kho khi có số đỏ: mở thẳng kho đầu tiên có việc — ưu tiên NHẬP (hàng về trước), rồi XUẤT.
            var first = perWarehouse.FirstOrDefault(x => x.In > 0);
            if (first.Name is not null) kbo = (first.Name, false);
            else if (perWarehouse.FirstOrDefault(x => x.Out > 0) is { Name: not null } o) kbo = (o.Name, true);
        }

        /// <summary>Rê chuột lên mục / nhóm có số đỏ -> hiện số đó là việc gì.</summary>
        private void UpdateBadgeTips()
        {
            foreach (var b in _navButtons)
            {
                var key = _menuDetails.Keys.FirstOrDefault(k => b.Text.TrimEnd().EndsWith(k));
                var n = CountFor(b);
                _badgeTip.SetToolTip(b, key is not null && n > 0 ? $"{_menuDetails[key]}\n— bấm để xem đúng các việc này" : "");
            }
            foreach (var h in _navHeaders)
            {
                var g = (string)h.Tag!;
                var lines = _groupOf.Where(kv => kv.Value == g && CountFor(kv.Key) > 0)
                    .Select(kv => _menuDetails.FirstOrDefault(d => kv.Key.Text.TrimEnd().EndsWith(d.Key)))
                    .Where(d => d.Key is not null)
                    .Select(d => $"• {d.Key}: {d.Value}").ToList();
                _badgeTip.SetToolTip(h, lines.Count > 0 ? "Việc đang chờ:\n" + string.Join("\n", lines) : "");
            }
        }

        /// <summary>Bấm 1 mục menu ĐANG CÓ SỐ ĐỎ -> mở màn đã lọc sẵn đúng các việc chờ (null = mở bình thường).</summary>
        private Func<Control>? PendingBuild(Button b)
        {
            if (_api is null || CountFor(b) <= 0) return null;
            var api = _api;
            var text = b.Text.TrimEnd();
            if (text.EndsWith("Đơn vận chuyển"))
                return () => { var c = new TruckingSystem.Desktop.GUI_TransportOrderManagement.TransportOrdersControl(api); c.ShowPendingOnly(); return c; };
            if (text.EndsWith("Chuyến xe"))
                return () => { var c = new TruckingSystem.Desktop.GUI_TripManagement.TripsControl(api); c.ShowPendingOnly(); return c; };
            if (text.EndsWith("Kho trung chuyển") && _kboTarget is { } k)
                return () =>
                {
                    var c = new TruckingSystem.Desktop.GUI_WarehouseManagement.WarehousesControl(api);
                    c.OpenPendingReceiptsOnLoad(k.Warehouse, tripIssues: k.Issues);
                    return c;
                };
            return null;
        }

        /// <summary>Chấm đỏ bo tròn có số ở mép phải nút (nút nhóm: chừa chỗ cho mũi tên ▸ / ▾).</summary>
        private void PaintBadge(Graphics g, Button b, int count, bool arrowSpace)
        {
            if (count <= 0) return;
            var text = count > 99 ? "99+" : count.ToString();
            var size = TextRenderer.MeasureText(g, text, BadgeFont, Size.Empty, TextFormatFlags.NoPadding);
            var h = LogicalToDeviceUnits(_navCollapsed ? 16 : 18);
            var w = Math.Max(h, size.Width + LogicalToDeviceUnits(_navCollapsed ? 6 : 10));
            // Menu thu gọn: chấm số nhỏ ở góc trên phải biểu tượng.
            var rect = _navCollapsed
                ? new Rectangle(b.Width - w - LogicalToDeviceUnits(2), LogicalToDeviceUnits(2), w, h)
                : new Rectangle(b.Width - LogicalToDeviceUnits(arrowSpace ? 36 : 12) - w, (b.Height - h) / 2, w, h);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using (var path = DayScene.Round(rect.X, rect.Y, rect.Width, rect.Height, h / 2f))
            using (var bg = new SolidBrush(Theme.Danger))
                g.FillPath(bg, path);
            TextRenderer.DrawText(g, text, BadgeFont, rect, Color.White,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
        }
    }
}
