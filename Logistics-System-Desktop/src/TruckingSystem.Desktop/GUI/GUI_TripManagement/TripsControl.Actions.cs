using System.Drawing;
using System.Windows.Forms;
using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_TripManagement;

namespace TruckingSystem.Desktop.GUI_TripManagement
{
    /// <summary>THAO TÁC của màn Chuyến — bố cục gọn kiểu phần mềm doanh nghiệp (TMS / ERP):
    ///  • Thanh trên chỉ còn: Làm mới · nút BƯỚC TIẾP THEO đúng trạng thái chuyến đang chọn (Bắt đầu / Hoàn tất /
    ///    Duyệt + Từ chối; Đổi xe / Trả khách tại kho khi chuyến chưa chạy — trạng thái khác thì ẩn) · Xem lộ trình ·
    ///    2 nút nhóm thả xuống "⛽ Chi phí và nhiên liệu ▾" và "ℹ️ Thông tin và công cụ ▾" (luôn thấy được).
    ///  • CHUỘT PHẢI vào 1 dòng (hoặc phím menu / Shift+F10) = menu đủ mọi thao tác, gom nhóm:
    ///      Xem lộ trình | Vận hành | Chi phí & nhiên liệu ▸ | Thông tin & công cụ ▸ | Sao chép mã chuyến.
    ///    Thao tác không hợp lệ với trạng thái chuyến thì mờ đi (cùng điều kiện với trước đây).
    ///  • Nhấp đúp = Xem lộ trình · F5 = Làm mới.
    ///  • Thẻ "Chuyến đang chọn" (UserControl SelectedTripBar) ngay trên bảng: thông tin + việc cần làm tiếp.
    /// Các nút cũ vẫn giữ (Designer) làm nguồn trạng thái bật/tắt + xử lý — chỉ không còn bày hết lên thanh.</summary>
    public partial class TripsControl
    {
        private readonly SelectedTripBar _selectedBar = new();
        private readonly ContextMenuStrip _rowMenu = new();
        private Button? _btnCost, _btnInfo;

        /// <summary>Mỗi mục menu gắn với 1 nút gốc: bật/tắt theo nút đó; Visible (tuỳ chọn) để ẩn hẳn mục
        /// không liên quan (VD "Trả khách tại kho" chỉ hiện với chuyến Trả hàng).</summary>
        private readonly List<(ToolStripMenuItem Item, Button Source, Func<TripListItem?, bool>? Visible)> _menuBindings = new();

        private void SetupActions()
        {
            // 1) Thẻ "Chuyến đang chọn" ngay trên bảng.
            KpiStrip.InsertRow(root, _selectedBar, root.GetRow(grid));
            _selectedBar.ShowEmpty();

            // 2) Thanh nút: chỉ giữ các nút chính; các việc phụ gom vào 2 nút nhóm thả xuống (cũng có trong menu chuột phải).
            foreach (var b in new[] { btnCargoPhotos, btnFuel, btnFuelEstimate, btnExpenses, btnIncidents,
                                      btnQuickEstimate, btnSyncCalendar, btnDriverInfo })
                toolbar.Controls.Remove(b);
            BuildRowMenu();
            _btnCost = GroupButton("⛽ Chi phí && nhiên liệu ▾", AddCostItems, "Ghi xăng (kèm so sánh với ước tính), chi phí khác, sự cố của chuyến đang chọn");
            _btnInfo = GroupButton("ℹ️ Thông tin && công cụ ▾", AddInfoItems, "Ảnh hàng, link cho tài xế, lịch sử thay đổi");
            // Thanh nút chỉ giữ Làm mới + các VIỆC CHÍNH theo trạng thái (Bắt đầu / Hoàn tất / Duyệt / Từ chối / Đổi xe /
            // Trả tại kho). "Xem lộ trình" = nhấp đúp; "Chi phí & nhiên liệu", "Thông tin & công cụ" = chuột phải
            // (2 nút nhóm ▾ trước đây mở ra đúng danh sách của chuột phải -> bỏ cho đỡ trùng).
            toolbar.Controls.Remove(btnViewRoute);

            // 3) Menu chuột phải.
            grid.ContextMenuStrip = _rowMenu;
            _rowMenu.Opening += (_, e) =>
            {
                SyncRowMenu();
                if (GetSelectedTripId() is null) e.Cancel = true; // chưa chọn chuyến -> không mở menu trống
            };
            // Chuột phải chọn luôn dòng dưới con trỏ (mặc định WinForms giữ dòng cũ -> thao tác nhầm chuyến).
            grid.CellMouseDown += (_, e) =>
            {
                if (e.Button != MouseButtons.Right || e.RowIndex < 0) return;
                var col = Math.Max(0, e.ColumnIndex);
                if (grid.Rows[e.RowIndex].Cells[col].Visible) grid.CurrentCell = grid.Rows[e.RowIndex].Cells[col];
                else grid.Rows[e.RowIndex].Selected = true;
            };
            grid.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) BtnViewRoute_Click(s, e); };
            grid.KeyDown += (_, e) =>
            {
                if (e.KeyCode == Keys.F5) { e.Handled = true; _ = LoadAsync(); }
            };
        }

        private void BuildRowMenu()
        {
            var route = Add(_rowMenu.Items, "🗺️  Xem lộ trình", btnViewRoute, BtnViewRoute_Click);
            route.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            route.ShortcutKeyDisplayString = "Nhấp đúp";
            _rowMenu.Items.Add(new ToolStripSeparator());

            Header(_rowMenu.Items, "VẬN HÀNH");
            Add(_rowMenu.Items, "▶️  Bắt đầu chuyến", btnStart, BtnStart_Click, t => t?.Status == "Planned");
            Add(_rowMenu.Items, "✅  Hoàn tất chuyến", btnComplete, BtnComplete_Click, t => t?.Status == "InProgress");
            Add(_rowMenu.Items, "✅  Duyệt xác nhận", btnApprove, BtnApprove_Click, t => t?.Status == "PendingConfirmation");
            Add(_rowMenu.Items, "❌  Từ chối", btnReject, BtnReject_Click, t => t?.Status == "PendingConfirmation");
            Add(_rowMenu.Items, "🔁  Đổi xe / tài xế", btnReassign, BtnReassign_Click);
            Add(_rowMenu.Items, "🏬  Trả khách tại kho", btnReturnAtWarehouse, BtnReturnAtWarehouse_Click, t => t?.LegType == "Return");
            _rowMenu.Items.Add(new ToolStripSeparator());

            var cost = new ToolStripMenuItem("⛽  Chi phí && nhiên liệu");
            AddCostItems(cost.DropDownItems);
            _rowMenu.Items.Add(cost);

            var info = new ToolStripMenuItem("ℹ️  Thông tin && công cụ");
            AddInfoItems(info.DropDownItems);
            _rowMenu.Items.Add(info);
            _rowMenu.Items.Add(new ToolStripSeparator());

            var copy = new ToolStripMenuItem("📋  Sao chép mã chuyến");
            copy.Click += (_, _) =>
            {
                if (GetSelectedTripId() is not int id) return;
                var trip = _trips.FirstOrDefault(t => t.Id == id);
                Clipboard.SetText(trip is null ? $"#{id}" : $"#{id} — đơn {trip.OrderCode}");
                lblStatus.Text = $"📋 Đã sao chép mã chuyến #{id}.";
                lblStatus.ForeColor = Theme.Success;
            };
            _rowMenu.Items.Add(copy);
            // "Làm mới danh sách" bỏ khỏi chuột phải — đã có nút 🔄 Làm mới và phím F5.
            ModernMenu.Apply(_rowMenu);
        }

        private void AddCostItems(ToolStripItemCollection items)
        {
            // "Ước tính nhiên liệu" gộp vào cửa sổ Ghi xăng (dòng so sánh ước tính / thực tế) — bớt 1 mục.
            Add(items, "⛽  Ghi xăng / xem lịch sử đổ xăng", btnFuel, BtnFuel_Click);
            items.Add(new ToolStripSeparator());
            Add(items, "💰  Chi phí khác (cầu đường, bốc xếp...)", btnExpenses, BtnExpenses_Click);
            Add(items, "⚠️  Sự cố", btnIncidents, BtnIncidents_Click);
        }

        private void AddInfoItems(ToolStripItemCollection items)
        {
            Add(items, "📷  Ảnh hàng tài xế chụp", btnCargoPhotos, BtnCargoPhotos_Click);
            Add(items, "📱  Lấy link / mã chuyến cho tài xế", btnDriverInfo, BtnDriverInfo_Click);
            // km / thời gian ước tính đã tự tính lúc Chia xe và hiện ở thẻ "Chuyến đang chọn" -> chỉ hiện mục này khi
            // lúc chia xe chưa tính được (VD mạng / VietMap lỗi) để tính lại.
            Add(items, "📏  Tính lại km / thời gian (lúc chia xe chưa tính được)", btnQuickEstimate, BtnQuickEstimate_Click,
                t => t is { HasEstimate: false });
            // "Đồng bộ Google Calendar": server tự đồng bộ khi chia xe / đổi xe / đổi trạng thái -> bỏ khỏi menu
            // (nút kỹ thuật, nhân viên không cần biết).
            items.Add(new ToolStripSeparator());
            var history = new ToolStripMenuItem("🕘  Lịch sử thay đổi (ai làm gì, lúc nào)");
            history.Click += (_, _) => ShowHistory();
            items.Add(history);
        }

        /// <summary>🕘 Nhật ký của chuyến đang chọn + nhiên liệu, chi phí, sự cố, phiếu nhập/xuất của chuyến.</summary>
        private void ShowHistory()
        {
            if (_api is null || GetSelectedTripId() is not int id) return;
            using var f = new TruckingSystem.Desktop.GUI_AuditLogManagement.EntityHistoryForm(_api, "Trip", id, $"chuyến #{id}");
            f.ShowDialog(FindForm());
        }

        /// <summary>Nút trên thanh + menu thả xuống RIÊNG của nút (cùng các mục với nhóm trong menu chuột phải),
        /// mở ngay sát dưới nút, rộng tối thiểu bằng nút. (Bản trước mượn menu con của menu chuột phải -> WinForms
        /// đặt sai chỗ và vẽ kiểu menu mặc định.)</summary>
        private Button GroupButton(string text, Action<ToolStripItemCollection> addItems, string tip)
        {
            var menu = new ContextMenuStrip();
            addItems(menu.Items);
            ModernMenu.Apply(menu);
            var b = new Button
            {
                Text = text,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Color.White,
                ForeColor = Theme.TextPrimary,
                Cursor = Cursors.Hand,
                FlatStyle = FlatStyle.Flat,
                Font = btnRefresh.Font,
                Margin = btnRefresh.Margin,
                MinimumSize = btnRefresh.MinimumSize,
                Padding = btnRefresh.Padding,
                UseVisualStyleBackColor = false,
                Enabled = false,
            };
            b.FlatAppearance.BorderColor = Theme.BorderColor;
            b.FlatAppearance.MouseOverBackColor = Theme.GridAltRow;
            b.Click += (_, _) =>
            {
                SyncRowMenu();
                menu.MinimumSize = new Size(b.Width, 0);
                menu.Show(b, new Point(0, b.Height + b.LogicalToDeviceUnits(2)));
            };
            new ToolTip().SetToolTip(b, tip);
            return b;
        }

        private static void Header(ToolStripItemCollection items, string text)
            => items.Add(new ToolStripLabel(text)
            {
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = Theme.TextSecondary,
                Margin = new Padding(8, 2, 0, 0),
            });

        private ToolStripMenuItem Add(ToolStripItemCollection items, string text, Button source, EventHandler handler,
            Func<TripListItem?, bool>? visible = null)
        {
            var item = new ToolStripMenuItem(text);
            item.Click += (s, e) => { if (source.Enabled) handler(s, e); };
            items.Add(item);
            _menuBindings.Add((item, source, visible));
            return item;
        }

        /// <summary>Bật/tắt + ẩn/hiện các mục menu theo đúng trạng thái nút gốc (UpdateActionButtons tính sẵn).
        /// Mục chưa làm được thì mờ (kèm gợi ý khi rê chuột), nhóm vẫn mở được để thấy đủ chức năng.</summary>
        private void SyncRowMenu()
        {
            var trip = GetSelectedTripId() is int id ? _trips.FirstOrDefault(t => t.Id == id) : null;
            foreach (var (item, source, visible) in _menuBindings)
            {
                item.Visible = visible?.Invoke(trip) ?? true;
                item.Enabled = source.Enabled;
                item.ToolTipText = source.Enabled ? "" : "Chưa làm được với trạng thái hiện tại của chuyến";
            }
            // Nhóm luôn mở được (thấy đủ các mục, mục chưa làm được thì mờ) — để nhân viên biết chức năng nằm ở đâu.
            // (KHÔNG đọc item.Visible ở đây: với menu chưa mở, Visible luôn trả false -> bản trước làm cả nhóm mờ.)
        }

        /// <summary>Thanh trên: chỉ hiện nút BƯỚC TIẾP THEO hợp với trạng thái chuyến đang chọn.</summary>
        private void UpdatePrimaryButtons(TripListItem? trip)
        {
            var st = trip?.Status;
            btnStart.Visible = st == "Planned";
            btnComplete.Visible = st == "InProgress";
            btnApprove.Visible = btnReject.Visible = st == "PendingConfirmation";
            // Gỡ chuyến kẹt: chỉ hiện khi chuyến CHƯA CHẠY; "Trả khách tại kho" chỉ với chuyến Trả hàng.
            btnReassign.Visible = st == "Planned";
            btnReturnAtWarehouse.Visible = st == "Planned" && trip?.LegType == "Return";
            if (_btnCost != null) _btnCost.Enabled = trip is not null;
            if (_btnInfo != null) _btnInfo.Enabled = trip is not null;
        }
    }
}
