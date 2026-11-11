using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_TripManagement;

namespace TruckingSystem.Desktop.GUI_TripManagement
{
    /// <summary>GOM CHUYẾN THEO ĐƠN (mẫu "cách A" đã duyệt):
    ///  • Mỗi đơn 1 DÒNG ĐẦU NHÓM, xếp thẳng theo cột: ▼ + ô màu + mã đơn | "x/y xong: ✓ Lấy → ✓ Kho → ▶ Giao" |
    ///    👉 VIỆC TIẾP THEO (chỉ khi còn việc) | nhãn trạng thái ĐƠN | hạn xấu nhất.
    ///  • Màu theo TRẠNG THÁI ĐƠN (4 màu): xám chưa bắt đầu · vàng đang thực hiện · xanh lá hoàn tất · đỏ đã hủy.
    ///  • Các chuyến của đơn nằm ngay dưới (├ / └), cùng DẢI MÀU trạng thái đơn bên trái -> nhìn là biết thuộc đơn nào.
    ///  • Bấm ▼ (hoặc nhấp đúp / Enter / ← →) ở dòng đơn để thu gọn / mở. Đơn đã chạy xong hết tự thu gọn lần đầu thấy.
    ///  • Bấm dòng đơn -> thẻ phía trên hiện thông tin CẢ ĐƠN; bấm dòng chuyến -> thẻ của chuyến.
    /// Chỉ đổi phần hiển thị — thao tác, bộ lọc, chuột phải, phím tắt giữ nguyên.</summary>
    public partial class TripsControl
    {
        /// <summary>1 dòng của bảng: dòng ĐƠN (Là_dòng_đơn) hoặc dòng CHUYẾN.</summary>
        internal sealed class TripGridRow
        {
            public int? Id { get; init; }
            public string Chuyến { get; init; } = "";
            public string Chặng { get; init; } = "";
            public string Xe_và_tài_xế { get; init; } = "";
            public string Xuất_phát { get; init; } = "";
            public string Trạng_thái { get; init; } = "";
            public string Hạn { get; init; } = "";
            public string Nhóm { get; init; } = "";
            public bool Là_dòng_đơn { get; init; }
        }

        private enum StepState { Pending, Now, Wait, Done }

        /// <summary>Thông tin tính sẵn của 1 đơn (dòng đầu nhóm + thẻ đơn).</summary>
        private sealed class OrderGroup
        {
            public string Code = "";
            public List<TripListItem> Trips = new();
            public Color Color;
            public int Done, Total;
            public List<(string Text, StepState State)> Chain = new();
            public string? Deadline;
            public GridDecor.Tone? DeadlineTone;
            public string Next = "";
            public string StatusText = "";
            public GridDecor.Tone StatusTone = GridDecor.Slate;
            public bool Blocked, AllDone;
            public int? NextTripId;
        }

        private readonly HashSet<string> _collapsed = new();
        private readonly HashSet<string> _seenOrders = new();
        private Dictionary<string, OrderGroup> _groups = new();
        private Action? _cardAction;
        private string? _cardOrder;

        // 4 MÀU THEO TRẠNG THÁI ĐƠN (thay cho mỗi đơn 1 màu): xám = chưa bắt đầu · vàng = đang thực hiện ·
        // xanh lá = hoàn tất · đỏ = đã hủy. Trễ hạn vẫn báo riêng bằng nhãn đỏ ở cột Hạn.
        private static readonly Color StGray = ColorTranslator.FromHtml("#94A3B8");
        private static readonly Color StAmber = ColorTranslator.FromHtml("#F59E0B");
        private static readonly Color StGreen = ColorTranslator.FromHtml("#16A34A");
        private static readonly Color StRed = ColorTranslator.FromHtml("#DC2626");
        private static readonly Color GroupBack = ColorTranslator.FromHtml("#F8FAFC");
        private static readonly Color GroupSelected = ColorTranslator.FromHtml("#E0F2FE");
        private static readonly Color GroupLine = ColorTranslator.FromHtml("#E2E8F0");

        private Font? _gBold, _gText, _gChip;

        private Color OrderColor(string code) => _groups.TryGetValue(code, out var g) ? g.Color : StGray;

        private static int LegOrder(TripListItem t) => t.LegType switch { "Pickup" => 0, "Delivery" => 1, "Return" => 2, _ => 3 };

        private static string LegShort(string leg) => leg switch { "Pickup" => "Lấy", "Delivery" => "Giao", "Return" => "Trả", _ => leg };

        private static GridDecor.Tone? DeadlineTone(string level) => level switch
        {
            "Ok" or "DoneOnTime" => GridDecor.Green,
            "Warning" => GridDecor.Amber,
            "Late" or "DoneLate" => GridDecor.Red,
            _ => null,
        };

        private static GridDecor.Tone LegTone(string leg) => leg switch
        {
            "Pickup" => GridDecor.Teal,
            "Delivery" => GridDecor.Blue,
            _ => GridDecor.Violet,
        };

        // ------------------------------------------------------------ cài đặt

        private void SetupGrouping()
        {
            _gBold = new Font("Segoe UI", 10F, FontStyle.Bold);
            _gText = new Font("Segoe UI", 9F);
            _gChip = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            Disposed += (_, _) => { _gBold?.Dispose(); _gText?.Dispose(); _gChip?.Dispose(); };

            grid.DataBindingComplete += (_, _) =>
            {
                foreach (var name in new[] { "Nhóm", "Là_dòng_đơn" })
                    if (grid.Columns[name] is { } c) c.Visible = false;
                var h = LogicalToDeviceUnits(42);
                foreach (DataGridViewRow r in grid.Rows)
                    if (r.DataBoundItem is TripGridRow { Là_dòng_đơn: true }) r.Height = h;
            };
            grid.RowPrePaint += Grid_GroupRowPrePaint;
            grid.RowPostPaint += Grid_ChildRowPostPaint;
            // Bấm vào mũi tên ▼/▶ ở đầu dòng đơn -> thu gọn / mở.
            grid.MouseDown += (_, e) =>
            {
                if (e.Button != MouseButtons.Left) return;
                var hit = grid.HitTest(e.X, e.Y);
                if (hit.RowIndex >= 0 && GroupCodeAt(hit.RowIndex) is string code && e.X < LogicalToDeviceUnits(30))
                    BeginInvoke(new Action(() => ToggleGroup(code)));
            };
            grid.KeyDown += (_, e) =>
            {
                if (CurrentGroupCode() is not string code) return;
                if (e.KeyCode == Keys.Left && !_collapsed.Contains(code)) { e.Handled = true; ToggleGroup(code); }
                else if (e.KeyCode == Keys.Right && _collapsed.Contains(code)) { e.Handled = true; ToggleGroup(code); }
            };
            _selectedBar.ActionClicked += (_, _) => _cardAction?.Invoke();
            _selectedBar.LinkClicked += (_, _) => { if (_cardOrder is string c) SelectGroup(c); };
        }

        // ------------------------------------------------------------ dữ liệu bảng

        /// <summary>Dựng các dòng của bảng: dòng đơn + các chuyến (đã lọc) của đơn.</summary>
        private List<TripGridRow> BuildGroupedRows(List<TripListItem> filtered, int? keepId)
        {
            _groups = new Dictionary<string, OrderGroup>();
            // Đang tìm kiếm -> mở hết để thấy chuyến khớp; chuyến cần chọn sẵn (từ thông báo) -> mở đơn của nó.
            var searching = !string.IsNullOrWhiteSpace(filterBar.SearchText) || _pendingOnly;
            if (keepId is int k && _trips.FirstOrDefault(t => t.Id == k) is { } keep) _collapsed.Remove(keep.OrderCode);

            var rows = new List<TripGridRow>();
            foreach (var grp in filtered.GroupBy(t => t.OrderCode))
            {
                var code = grp.Key;
                var info = BuildOrderGroup(code);
                _groups[code] = info;
                if (_seenOrders.Add(code) && info.AllDone) _collapsed.Add(code); // đơn đã xong: thu gọn lần đầu
                rows.Add(new TripGridRow { Nhóm = code, Là_dòng_đơn = true });
                if (_collapsed.Contains(code) && !searching) continue;
                var kids = grp.OrderBy(LegOrder).ThenBy(t => t.Id).ToList();
                for (var i = 0; i < kids.Count; i++)
                {
                    var t = kids[i];
                    rows.Add(new TripGridRow
                    {
                        Id = t.Id,
                        Chuyến = $"{(i == kids.Count - 1 ? "└" : "├")}  #{t.Id}",
                        Chặng = TranslateLegType(t.LegType),
                        Xe_và_tài_xế = $"{t.VehiclePlate} · {t.DriverName}",
                        Xuất_phát = ScreenKit.Day(t.PlannedStartDate),
                        Trạng_thái = StatusText(t),
                        Hạn = DeadlineShort(t),
                        Nhóm = code,
                    });
                }
            }
            return rows;
        }

        private OrderGroup BuildOrderGroup(string code)
        {
            var all = _trips.Where(t => t.OrderCode == code).OrderBy(LegOrder).ThenBy(t => t.Id).ToList();
            var live = all.Where(t => t.Status != "Cancelled").ToList();
            var g = new OrderGroup { Code = code, Trips = all };
            g.Total = live.Count;
            g.Done = live.Count(t => t.Status == "Completed");
            g.AllDone = live.Count > 0 && g.Done == live.Count;
            (g.StatusText, g.StatusTone, g.Color) =
                live.Count == 0 ? ("Đã hủy", GridDecor.Red, StRed)
                : g.AllDone ? ("Hoàn tất", GridDecor.Green, StGreen)
                : live.Any(t => t.Status != "Planned") ? ("Đang thực hiện", GridDecor.Amber, StAmber)
                : ("Chưa bắt đầu", GridDecor.Slate, StGray);

            // Chuỗi chặng: Lấy → (Kho) → Giao → Trả; cùng chặng nhiều chuyến thì đánh số "Giao 1", "Giao 2".
            var legCount = live.GroupBy(t => t.LegType).ToDictionary(x => x.Key, x => x.Count());
            var legSeen = new Dictionary<string, int>();
            var hasPickup = live.Any(t => t.LegType == "Pickup");
            var hasDelivery = live.Any(t => t.LegType == "Delivery");
            var hasWarehouse = live.Any(t => t.LegType == "Pickup" && !string.IsNullOrWhiteSpace(t.WarehouseName));
            var khoAdded = false;
            foreach (var t in live)
            {
                if (!khoAdded && hasPickup && hasDelivery && hasWarehouse && t.LegType != "Pickup")
                {
                    var pickupsDone = live.Where(p => p.LegType == "Pickup").All(p => p.Status == "Completed");
                    var waitingReceipt = live.Any(IsWaitingForReceipt);
                    g.Chain.Add(("Kho", waitingReceipt ? StepState.Wait : pickupsDone ? StepState.Done : StepState.Pending));
                    khoAdded = true;
                }
                legSeen[t.LegType] = legSeen.TryGetValue(t.LegType, out var n) ? n + 1 : 1;
                var label = LegShort(t.LegType) + (legCount[t.LegType] > 1 ? $" {legSeen[t.LegType]}" : "");
                var state = t.Status switch
                {
                    "Completed" => StepState.Done,
                    "InProgress" => StepState.Now,
                    "PendingConfirmation" => StepState.Wait,
                    _ => StepState.Pending,
                };
                g.Chain.Add((label, state));
            }

            // Hạn của đơn: lấy mức xấu nhất trong các chuyến.
            var worst = all.Select(t => t.DeadlineLevel)
                .OrderBy(l => l switch { "Late" => 0, "DoneLate" => 1, "Warning" => 2, "Ok" => 3, "DoneOnTime" => 4, _ => 9 })
                .FirstOrDefault();
            if (worst is not null && DeadlineTone(worst) is { } tone)
            {
                g.DeadlineTone = tone;
                g.Deadline = DeadlineShort(new TripListItem { DeadlineLevel = worst });
            }

            // Việc tiếp theo của CẢ ĐƠN — ưu tiên việc đang chờ người ở văn phòng làm.
            TripListItem? pick;
            if ((pick = live.FirstOrDefault(t => t.Status == "PendingConfirmation")) is not null)
                (g.Next, g.NextTripId) = ($"Duyệt chuyến #{pick.Id} — tài xế đã báo xong", pick.Id);
            else if ((pick = live.FirstOrDefault(IsWaitingForReceipt)) is not null)
                (g.Next, g.Blocked, g.NextTripId) = ("Nhập kho hàng lấy về (màn Kho → 📦 Nhập hàng từ chuyến)", true, pick.Id);
            else if ((pick = live.FirstOrDefault(IsWaitingForIssue)) is not null)
                (g.Next, g.Blocked, g.NextTripId) = ($"Kho xuất hàng cho chuyến #{pick.Id} (màn Kho → 📤 Xuất kho theo chuyến)", true, pick.Id);
            else if ((pick = live.FirstOrDefault(t => t.Status == "InProgress")) is not null)
                (g.Next, g.NextTripId) = ($"Chuyến #{pick.Id} đang chạy — chờ xe tới nơi", pick.Id);
            else if ((pick = live.Where(t => t.Status == "Planned" && !IsWaitingForPickup(t))
                         .OrderBy(t => t.PlannedStartDate ?? DateTime.MaxValue).FirstOrDefault()) is not null)
                (g.Next, g.NextTripId) = ($"Bắt đầu chuyến #{pick.Id}" + (pick.PlannedStartDate is DateTime d ? $" (xuất phát {ScreenKit.Day(d)})" : ""), pick.Id);
            else if (g.AllDone)
                g.Next = "Đã chạy xong tất cả các chuyến"; // chỉ hiện ở thẻ đơn, dòng đơn để trống cho đỡ rối
            else if (live.Count == 0)
                g.Next = "Các chuyến của đơn đã hủy";
            return g;
        }

        private TripGridRow? RowItem(int row)
            => row >= 0 && row < grid.Rows.Count ? grid.Rows[row].DataBoundItem as TripGridRow : null;

        private string? GroupCodeAt(int row) => RowItem(row) is { Là_dòng_đơn: true } r ? r.Nhóm : null;

        private string? CurrentGroupCode() => grid.CurrentRow is null ? null : GroupCodeAt(grid.CurrentRow.Index);

        private void ToggleGroup(string code)
        {
            if (!_collapsed.Remove(code)) _collapsed.Add(code);
            BindGrid();
            SelectGroupRow(code);
        }

        /// <summary>Chọn dòng đơn (mở nhóm nếu đang thu gọn thì không cần — dòng đơn luôn hiện).</summary>
        private void SelectGroup(string code)
        {
            if (!SelectGroupRow(code)) return;
            grid.Focus();
        }

        private bool SelectGroupRow(string code)
        {
            foreach (DataGridViewRow r in grid.Rows)
                if (r.DataBoundItem is TripGridRow { Là_dòng_đơn: true } item && item.Nhóm == code && grid.Columns["Chuyến"] is { } col)
                {
                    grid.CurrentCell = r.Cells[col.Index];
                    UpdateActionButtons();
                    return true;
                }
            return false;
        }

        private void SelectTripRow(int id)
        {
            if (_trips.FirstOrDefault(t => t.Id == id) is { } t && _collapsed.Remove(t.OrderCode)) BindGrid();
            foreach (DataGridViewRow r in grid.Rows)
                if (r.Cells["Id"].Value is int rid && rid == id) { grid.CurrentCell = r.Cells["Chuyến"]; break; }
            grid.Focus();
        }

        // ------------------------------------------------------------ vẽ

        private int D(int v) => LogicalToDeviceUnits(v);

        /// <summary>Khoảng ngang của 1 cột đang hiện (để dòng đơn xếp chữ THẲNG theo cột của bảng).</summary>
        private (int X, int Right)? ColumnSpan(string name)
        {
            if (grid.Columns[name] is not { Visible: true } c) return null;
            var r = grid.GetColumnDisplayRectangle(c.Index, false);
            return r.Width > 0 ? (r.X, r.Right) : null;
        }

        /// <summary>Dòng ĐƠN: tự vẽ cả dòng, xếp THẲNG THEO CỘT của bảng:
        ///   cột Chuyến: ▼ + ô màu trạng thái + mã đơn · cột Chặng: số chuyến + chuỗi chặng (chữ xám) ·
        ///   cột Xe + Xuất phát: 👉 việc tiếp theo (chỉ khi còn việc) · cột Trạng thái: nhãn trạng thái ĐƠN · cột Hạn: hạn xấu nhất.</summary>
        private void Grid_GroupRowPrePaint(object? sender, DataGridViewRowPrePaintEventArgs e)
        {
            if (GroupCodeAt(e.RowIndex) is not string code || !_groups.TryGetValue(code, out var info)
                || _gBold is null || _gText is null || _gChip is null) return;
            var g = e.Graphics;
            var row = grid.Rows[e.RowIndex];
            var rb = e.RowBounds;
            var width = grid.DisplayRectangle.Width;
            var selected = (e.State & DataGridViewElementStates.Selected) != 0;
            var hover = row.DefaultCellStyle.BackColor;
            var back = selected ? GroupSelected : !hover.IsEmpty ? hover : GroupBack;
            using (var b = new SolidBrush(back)) g.FillRectangle(b, 0, rb.Y, width, rb.Height);
            using (var p = new Pen(GroupLine)) g.DrawLine(p, 0, rb.Bottom - 1, width, rb.Bottom - 1);

            var state = g.Save();
            g.SetClip(new Rectangle(0, rb.Y, width, rb.Height));
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var cy = rb.Y + rb.Height / 2;
            var collapsed = _collapsed.Contains(code) && string.IsNullOrWhiteSpace(filterBar.SearchText) && !_pendingOnly;
            var pad = D(10);

            // Cột Chuyến: ▼ ■ mã đơn
            var c1 = ColumnSpan("Chuyến") ?? (0, D(240));
            var chevW = D(22);
            TextRenderer.DrawText(g, collapsed ? "▶" : "▼", _gText, new Rectangle(c1.X + D(6), rb.Y, chevW, rb.Height), Theme.TextSecondary,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            var x = c1.X + D(6) + chevW + D(4);
            using (var path = Anim.RoundRect(new RectangleF(x, cy - D(6), D(12), D(12)), D(3)))
            using (var b = new SolidBrush(info.Color)) g.FillPath(b, path);
            x += D(20);
            Cell(g, code, _gBold, x, c1.Right - pad, rb, Theme.TextPrimary);

            // Cột Chặng: "2 chuyến: ✓ Lấy → ✓ Kho → ▶ Giao" (chữ xám, không tô màu từng chặng cho đỡ rối)
            if (ColumnSpan("Chặng") is { } c2)
            {
                var chain = string.Join("  →  ", info.Chain.Select(s => (s.State switch
                {
                    StepState.Done => "✓ ", StepState.Now => "▶ ", StepState.Wait => "⏳ ", _ => "○ ",
                }) + s.Text));
                var head = info.Total == 0 ? "0 chuyến" : $"{info.Done}/{info.Total} xong";
                Cell(g, chain.Length == 0 ? head : $"{head}:  {chain}", _gText, c2.X + pad, c2.Right - pad, rb, Theme.TextSecondary);
            }

            // Cột Xe + Xuất phát: việc tiếp theo — chỉ khi đơn còn việc phải làm.
            if (!info.AllDone && info.Total > 0 && !string.IsNullOrWhiteSpace(info.Next)
                && ColumnSpan("Xe_và_tài_xế") is { } c3)
            {
                var end = ColumnSpan("Xuất_phát")?.Right ?? c3.Right;
                Cell(g, "👉 " + info.Next, _gBold, c3.X + pad, end - pad, rb, info.Blocked ? Theme.Warning : Theme.Primary);
            }

            // Cột Trạng thái: trạng thái của ĐƠN (cùng màu với ô vuông + dải màu các chuyến).
            if (ColumnSpan("Trạng_thái") is { } c4) Chip(g, info.StatusText, info.StatusTone, c4.X + pad, cy, c4.Right - pad);
            // Cột Hạn: hạn xấu nhất của đơn.
            if (info.Deadline is not null && info.DeadlineTone is not null && ColumnSpan("Hạn") is { } c5)
                Chip(g, info.Deadline, info.DeadlineTone, c5.X + pad, cy, c5.Right - pad);

            g.Restore(state);
            e.Handled = true; // không vẽ các ô (đều trống) của dòng đơn
        }

        /// <summary>Dòng CHUYẾN: dải màu của đơn ở mép trái.</summary>
        private void Grid_ChildRowPostPaint(object? sender, DataGridViewRowPostPaintEventArgs e)
        {
            if (RowItem(e.RowIndex) is not { Là_dòng_đơn: false } item || !_groups.TryGetValue(item.Nhóm, out var info)) return;
            using var b = new SolidBrush(info.Color);
            e.Graphics.FillRectangle(b, D(3), e.RowBounds.Y, D(4), e.RowBounds.Height);
        }

        /// <summary>Chữ 1 dòng trong khoảng [x, right], thiếu chỗ thì cắt "…".</summary>
        private static void Cell(Graphics g, string text, Font font, int x, int right, Rectangle rb, Color color)
        {
            if (right - x < 8) return;
            TextRenderer.DrawText(g, text, font, new Rectangle(x, rb.Y, right - x, rb.Height), color,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis
                | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
        }

        /// <summary>Nhãn bo tròn (giống nhãn ở các dòng chuyến), không vượt quá <paramref name="right"/>.</summary>
        private void Chip(Graphics g, string text, GridDecor.Tone tone, int x, int cy, int right)
        {
            var w = Math.Min(TextRenderer.MeasureText(g, text, _gChip!, Size.Empty, TextFormatFlags.NoPadding).Width + D(18), right - x);
            if (w < D(24)) return;
            var h = D(22);
            var r = new Rectangle(x, cy - h / 2, w, h);
            using (var path = Anim.RoundRect(r, h / 2f))
            using (var b = new SolidBrush(tone.Back)) g.FillPath(b, path);
            TextRenderer.DrawText(g, text, _gChip!, r, tone.Fore,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis
                | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
        }

        // ------------------------------------------------------------ thẻ thông tin

        /// <summary>Thẻ của 1 CHUYẾN.</summary>
        private TripCardInfo BuildTripCard(TripListItem t, string nextStep, bool blocked)
        {
            var st = t.Status;
            var info = new TripCardInfo
            {
                Title = $"Chuyến #{t.Id}",
                LinkPrefix = "thuộc đơn",
                LinkText = $"{t.OrderCode} ›",
                From = string.IsNullOrWhiteSpace(t.FromName) ? null : t.FromName,
                To = string.IsNullOrWhiteSpace(t.ToName) ? null : t.ToName,
                Next = nextStep,
                Blocked = blocked,
                Done = st == "Completed",
                Accent = OrderColor(t.OrderCode),
            };
            info.Pills.Add((TranslateLegType(t.LegType), LegTone(t.LegType)));
            var stText = StatusText(t);
            info.Pills.Add((stText, GridDecor.StatusTone(stText) ?? GridDecor.Slate));
            if (DeadlineTone(t.DeadlineLevel) is { } dl) info.Pills.Add((DeadlineShort(t), dl));
            if (t.EstimatedDistanceKm is decimal km && km > 0)
                info.RouteMid = $"📏 ~{km:0.#} km" + (t.EstimatedDurationMinutes is int min && min > 0
                    ? (min < 60 ? $" · ⏱ ~{min} phút" : $" · ⏱ ~{min / 60} giờ {min % 60} phút") : "");

            info.Boxes.Add(("Xe và tài xế", new List<(string, string)>
            {
                ("Xe", "🚚 " + (string.IsNullOrWhiteSpace(t.VehiclePlate) ? "—" : t.VehiclePlate)),
                ("Tài xế", "👤 " + (string.IsNullOrWhiteSpace(t.DriverName) ? "—" : t.DriverName)),
            }));
            var time = new List<(string, string)>
            {
                ("Xuất phát", t.PlannedStartDate is DateTime s
                    ? (ScreenKit.Day(s).Contains('/') ? $"{s:dd/MM/yyyy}" : $"{ScreenKit.Day(s)} ({s:dd/MM/yyyy})") : "—"),
            };
            if (t.LatestStartDate is DateTime latest && st == "Planned") time.Add(("Xuất phát muộn nhất", $"{latest:dd/MM/yyyy}"));
            time.Add(("Hạn giao của đơn", t.CommittedDeliveryDate is DateTime due ? $"{due:dd/MM/yyyy}" : "—"));
            if (!string.IsNullOrWhiteSpace(t.DeadlineStatus) && t.DeadlineStatus != "—") time.Add(("Tình trạng", t.DeadlineStatus));
            info.Boxes.Add(("Thời gian", time));
            var goods = new List<(string, string)>
            {
                ("Số lượng", $"📦 {t.TotalQtyPlanned:#,##0.##}"),
                ("Kho", string.IsNullOrWhiteSpace(t.WarehouseName) ? "Giao thẳng" : "🏬 " + t.WarehouseName),
            };
            if (st != "Cancelled" && _progress.ContainsKey(t.Id)) goods.Add(("Tiến độ kho", WarehouseText(t)));
            info.Boxes.Add(("Hàng và kho", goods));

            // Nút chính cạnh "việc tiếp theo" — cùng xử lý với nút trên thanh.
            (info.ActionText, _cardAction) = st switch
            {
                "Planned" when btnStart.Enabled => ("▶️ Bắt đầu chuyến", (Action)(() => BtnStart_Click(this, EventArgs.Empty))),
                "InProgress" => ("✅ Hoàn tất chuyến", (Action)(() => BtnComplete_Click(this, EventArgs.Empty))),
                "PendingConfirmation" => ("✅ Duyệt", (Action)(() => BtnApprove_Click(this, EventArgs.Empty))),
                _ => ("🗺️ Xem lộ trình", (Action)(() => BtnViewRoute_Click(this, EventArgs.Empty))),
            };
            _cardOrder = t.OrderCode;
            return info;
        }

        /// <summary>Thẻ của CẢ ĐƠN (khi chọn dòng đơn).</summary>
        private void ShowOrderCard(string code)
        {
            if (!_groups.TryGetValue(code, out var g)) { _selectedBar.ShowEmpty(); return; }
            var live = g.Trips.Where(t => t.Status != "Cancelled").ToList();
            var first = live.FirstOrDefault() ?? g.Trips.FirstOrDefault();
            var pickup = live.FirstOrDefault(t => t.LegType == "Pickup");
            var delivery = live.LastOrDefault(t => t.LegType == "Delivery");
            var warehouse = live.Select(t => t.WarehouseName).FirstOrDefault(w => !string.IsNullOrWhiteSpace(w));
            var info = new TripCardInfo
            {
                Title = $"Đơn {code}",
                FromCaption = "NGƯỜI GỬI",
                ToCaption = "NGƯỜI NHẬN",
                From = (pickup ?? first)?.FromName,
                To = (delivery ?? live.LastOrDefault())?.ToName,
                RouteMid = warehouse is null ? "giao thẳng" : $"qua 🏬 {warehouse}",
                Next = "Việc tiếp theo của đơn: " + g.Next,
                Blocked = g.Blocked,
                Done = g.AllDone,
                Accent = g.Color,
            };
            info.Pills.Add((g.StatusText, g.StatusTone));
            info.Pills.Add(($"{g.Done}/{g.Total} chuyến xong", GridDecor.Slate));
            if (g.Deadline is not null && g.DeadlineTone is not null) info.Pills.Add((g.Deadline, g.DeadlineTone));

            var legs = g.Trips.Take(4).Select(t => ($"#{t.Id} · {LegShort(t.LegType)}", StatusText(t))).ToList();
            if (g.Trips.Count > 4) legs.Add(("…", $"và {g.Trips.Count - 4} chuyến khác"));
            info.Boxes.Add(("Các chuyến của đơn", legs));
            var due = g.Trips.Select(t => t.CommittedDeliveryDate).FirstOrDefault(d => d is not null);
            var nextStart = live.Where(t => t.Status == "Planned" && t.PlannedStartDate is not null)
                .OrderBy(t => t.PlannedStartDate).FirstOrDefault();
            var time = new List<(string, string)> { ("Hạn giao", due is DateTime d ? $"{d:dd/MM/yyyy}" : "—") };
            if (nextStart?.PlannedStartDate is DateTime ns) time.Add(("Xuất phát tiếp theo", $"#{nextStart.Id} · {ScreenKit.Day(ns)}"));
            var late = live.Count(t => t.DeadlineLevel == "Late");
            if (late > 0) time.Add(("Chuyến trễ", $"🔴 {late}"));
            info.Boxes.Add(("Thời gian", time));
            info.Boxes.Add(("Hàng và kho", new List<(string, string)>
            {
                ("Số lượng", $"📦 {(pickup ?? first)?.TotalQtyPlanned ?? 0:#,##0.##}"),
                ("Kho", warehouse is null ? "Giao thẳng" : "🏬 " + warehouse),
            }));

            if (g.NextTripId is int id)
            {
                info.ActionText = $"Chọn chuyến #{id}";
                _cardAction = () => SelectTripRow(id);
            }
            else _cardAction = null;
            _cardOrder = null;
            _selectedBar.ShowCard(info);
        }
    }
}
