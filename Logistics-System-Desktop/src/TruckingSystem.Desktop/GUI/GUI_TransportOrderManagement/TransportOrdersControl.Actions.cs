using System.Drawing;
using System.Windows.Forms;
using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_TransportOrderManagement;
using TruckingSystem.Desktop.GUI_RouteManagement;
using TruckingSystem.Desktop.GUI_TripManagement;

namespace TruckingSystem.Desktop.GUI_TransportOrderManagement
{
    /// <summary>THAO TÁC của màn Đơn vận chuyển — cùng kiểu với màn Chuyến (gọn, đúng việc):
    ///  • Thanh trên: Làm mới · Tạo đơn mới · nút BƯỚC TIẾP THEO theo trạng thái đơn đang chọn (Nháp: Sửa đơn +
    ///    Xác nhận; Đã xác nhận chưa chia xe: Chia xe) · Xem chi tiết · 2 nút nhóm thả xuống
    ///    "📅 Hạn giao và phí lưu kho ▾" và "🧰 Công cụ khác ▾".
    ///  • CHUỘT PHẢI vào 1 đơn = menu đủ mọi thao tác (gom nhóm, thao tác không hợp trạng thái thì mờ).
    ///  • Nhấp đúp = Xem chi tiết · F5 = Làm mới.
    ///  • Thẻ "Đơn đang chọn" ngay trên bảng — CÙNG KIỂU thẻ màn Chuyến (SelectedTripBar): dòng đầu + đường đi
    ///    (lấy hàng → kho → giao) + 3 ô (Khách và hợp đồng · Thời gian · Hàng và chuyến) + việc cần làm kèm NÚT CHÍNH.
    /// Mọi điều kiện bật/tắt giống màn Chi tiết đơn; server vẫn kiểm tra lại khi thực hiện.</summary>
    public partial class TransportOrdersControl
    {
        /// <summary>1 thao tác trên đơn: chữ, điều kiện bấm được, (tuỳ chọn) điều kiện hiện, việc cần chạy.</summary>
        private sealed record OrderAction(string Text, Func<TransportOrderListItem, bool> CanRun,
            Func<TransportOrderListItem, Task> Run, Func<TransportOrderListItem, bool>? Show = null);

        private readonly SelectedTripBar _card = new() { Hint = "🖱 Chuột phải: mọi thao tác · Nhấp đúp: xem chi tiết đơn" };
        private Func<Task>? _cardAction; // nút chính trên thẻ
        private readonly ContextMenuStrip _rowMenu = new();
        private readonly List<(ToolStripMenuItem Item, OrderAction Action)> _menuBindings = new();
        private Button? _btnEdit, _btnConfirm, _btnDetail, _btnDeadline, _btnTools;

        private OrderAction _actDetail = null!, _actEdit = null!, _actConfirm = null!, _actAllocate = null!,
            _actDeadline = null!, _actFee = null!, _actRoute = null!,
            _actCopyCode = null!, _actCopyLink = null!, _actCancel = null!, _actHistory = null!;

        private TransportOrderListItem? SelectedOrder
            => grid.CurrentRow?.Cells["Id"]?.Value is int id ? _orders.FirstOrDefault(o => o.Id == id) : null;

        private void SetupActions()
        {
            DefineActions();

            // 1) Thẻ "Đơn đang chọn" ngay trên bảng.
            KpiStrip.InsertRow(root, _card, root.GetRow(grid));
            _card.ShowEmpty("Bấm 1 dòng trong bảng bên dưới để xem thông tin và việc cần làm của đơn.", "Chưa chọn đơn");
            _card.ActionClicked += async (_, _) =>
            {
                if (_cardAction is null) return;
                try { await _cardAction(); }
                catch (Exception ex) { Ask.Fail(this, "Lỗi: " + ex.Message); }
            };

            // 2) Thanh nút: Làm mới · Tạo đơn · [Sửa đơn · Xác nhận | Chia xe] · Xem chi tiết · 2 nhóm thả xuống.
            _btnEdit = ToolbarButton("✏️ Sửa đơn", _actEdit, primary: false);
            _btnConfirm = ToolbarButton("✅ Xác nhận đơn", _actConfirm, primary: true);
            _btnDetail = ToolbarButton("📄 Xem chi tiết", _actDetail, primary: false);
            _btnDeadline = GroupButton("📅 Hạn giao và phí lưu kho ▾", AddDeadlineItems);
            _btnTools = GroupButton("🧰 Công cụ khác ▾", AddToolItems);
            // Thanh nút chỉ giữ việc KHÔNG gắn 1 đơn (Tạo đơn, Làm mới) + 1 NÚT VIỆC CHÍNH theo trạng thái đơn đang chọn
            // (nháp -> Xác nhận; đã xác nhận chưa chia -> Chia xe). Xem chi tiết / Sửa / Hạn giao / Công cụ: nhấp đúp hoặc
            // chuột phải (trước đây lặp ở cả thanh nút, chuột phải và cửa sổ chi tiết).
            toolbar.Controls.Add(_btnConfirm);
            toolbar.Controls.SetChildIndex(btnAllocate, toolbar.Controls.Count - 1);


            // 3) Menu chuột phải.
            BuildRowMenu();
            grid.ContextMenuStrip = _rowMenu;
            _rowMenu.Opening += (_, e) =>
            {
                SyncMenu();
                if (SelectedOrder is null) e.Cancel = true;
            };
            grid.CellMouseDown += (_, e) =>
            {
                if (e.Button != MouseButtons.Right || e.RowIndex < 0) return;
                var col = Math.Max(0, e.ColumnIndex);
                if (grid.Rows[e.RowIndex].Cells[col].Visible) grid.CurrentCell = grid.Rows[e.RowIndex].Cells[col];
                else grid.Rows[e.RowIndex].Selected = true;
            };
            grid.KeyDown += (_, e) => { if (e.KeyCode == Keys.F5) { e.Handled = true; _ = LoadAsync(); } };
        }

        private void DefineActions()
        {
            _actDetail = new("📄  Xem chi tiết đơn", _ => true, _ => OpenDetailAsync());
            _actEdit = new("✏️  Sửa đơn", o => o.Status == "Draft", EditAsync);
            _actConfirm = new("✅  Xác nhận đơn", o => o.Status == "Draft", ConfirmAsync, o => o.Status == "Draft");
            _actAllocate = new("🚚  Chia xe tự động", o => !_allocating && o.Status == "Confirmed" && o.ActiveTripCount == 0,
                _ => AllocateSelectedAsync(), o => o.Status is "Draft" or "Confirmed");
            _actDeadline = new("📅  Đổi hạn giao", o => o.Status is "Confirmed" or "InProgress", ChangeDeadlineAsync);
            _actFee = new("💰  Xem phí lưu kho (chi tiết từng ngày)", o => o.Status != "Draft", StorageFeeAsync);
            _actRoute = new("🗺️  Xem lộ trình (lấy → kho → giao)", _ => true, ViewRouteAsync);
            _actCopyCode = new("📋  Sao chép mã đơn", _ => true, o =>
            {
                Clipboard.SetText(o.Code);
                SetStatus($"📋 Đã sao chép mã đơn {o.Code}.", Theme.Success);
                return Task.CompletedTask;
            });
            _actCopyLink = new("🔗  Sao chép link theo dõi cho khách", o => !string.IsNullOrEmpty(o.PublicTrackingToken), o =>
            {
                var url = $"{_api!.Connection.BaseUrl}/track.html?token={o.PublicTrackingToken}";
                Clipboard.SetText(url);
                SetStatus("🔗 Đã sao chép link theo dõi: " + url, Theme.Success);
                return Task.CompletedTask;
            });
            _actHistory = new("🕘  Lịch sử thay đổi (ai làm gì, lúc nào)", _ => true, o =>
            {
                using var f = new TruckingSystem.Desktop.GUI_AuditLogManagement.EntityHistoryForm(_api!, "TransportOrder", o.Id, "đơn " + o.Code);
                f.ShowDialog(FindForm());
                return Task.CompletedTask;
            });
            _actCancel = new("❌  Hủy đơn", o => o.Status is not ("Done" or "Cancelled"), CancelAsync);
        }

        private void BuildRowMenu()
        {
            var detail = AddItem(_rowMenu.Items, _actDetail);
            detail.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            detail.ShortcutKeyDisplayString = "Nhấp đúp";
            _rowMenu.Items.Add(new ToolStripSeparator());
            AddItem(_rowMenu.Items, _actEdit);
            AddItem(_rowMenu.Items, _actConfirm);
            AddItem(_rowMenu.Items, _actAllocate);
            _rowMenu.Items.Add(new ToolStripSeparator());
            var deadline = new ToolStripMenuItem("📅  Hạn giao và phí lưu kho");
            AddDeadlineItems(deadline.DropDownItems);
            _rowMenu.Items.Add(deadline);
            var tools = new ToolStripMenuItem("🧰  Công cụ khác");
            AddToolItems(tools.DropDownItems);
            _rowMenu.Items.Add(tools);
            _rowMenu.Items.Add(new ToolStripSeparator());
            var cancel = AddItem(_rowMenu.Items, _actCancel);
            cancel.ForeColor = Theme.Danger;
            // "Làm mới danh sách" bỏ khỏi chuột phải — đã có nút 🔄 Làm mới và phím F5.
            ModernMenu.Apply(_rowMenu);
        }

        private void AddDeadlineItems(ToolStripItemCollection items)
        {
            AddItem(items, _actDeadline);
            items.Add(new ToolStripSeparator());
            AddItem(items, _actFee);
            // "➕ Ghi nhận phí lưu kho bổ sung" bỏ khỏi menu — đã có nút cùng tên trong cửa sổ "Xem phí lưu kho"
            // (xem phí rồi mới ghi bổ sung, đỡ trùng 2 chỗ mở cùng 1 cửa sổ).
        }

        private void AddToolItems(ToolStripItemCollection items)
        {
            AddItem(items, _actRoute);
            AddItem(items, _actHistory);
            AddItem(items, _actCopyLink);
            AddItem(items, _actCopyCode);
        }

        private ToolStripMenuItem AddItem(ToolStripItemCollection items, OrderAction action)
        {
            var item = new ToolStripMenuItem(action.Text);
            item.Click += async (_, _) => await RunAsync(action);
            items.Add(item);
            _menuBindings.Add((item, action));
            return item;
        }

        private bool _running; // đang chạy 1 thao tác (VD Chia xe mất vài giây) -> bấm lại lần nữa thì bỏ qua

        private async Task RunAsync(OrderAction action)
        {
            if (_running || _api is null || SelectedOrder is not { } order || !action.CanRun(order)) return;
            _running = true;
            try { await action.Run(order); }
            catch (Exception ex)
            {
                Ask.Fail(this, "Lỗi: " + ex.Message);
            }
            finally { _running = false; }
        }

        private void SyncMenu()
        {
            var o = SelectedOrder;
            foreach (var (item, action) in _menuBindings)
            {
                item.Visible = o is null || action.Show?.Invoke(o) != false;
                item.Enabled = o is not null && action.CanRun(o);
                item.ToolTipText = item.Enabled ? "" : "Chưa làm được với trạng thái hiện tại của đơn";
            }
        }

        private Button ToolbarButton(string text, OrderAction action, bool primary)
        {
            var b = NewButton(text, primary);
            b.Click += async (_, _) => await RunAsync(action);
            return b;
        }

        private Button GroupButton(string text, Action<ToolStripItemCollection> addItems)
        {
            var menu = new ContextMenuStrip();
            addItems(menu.Items);
            ModernMenu.Apply(menu);
            var b = NewButton(text, primary: false);
            b.Click += (_, _) =>
            {
                SyncMenu();
                menu.MinimumSize = new Size(b.Width, 0);
                menu.Show(b, new Point(0, b.Height + b.LogicalToDeviceUnits(2)));
            };
            return b;
        }

        private Button NewButton(string text, bool primary)
        {
            var b = new Button
            {
                Text = text,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = primary ? Theme.Primary : Color.White,
                ForeColor = primary ? Color.White : Theme.TextPrimary,
                Cursor = Cursors.Hand,
                FlatStyle = FlatStyle.Flat,
                Font = btnRefresh.Font,
                Margin = btnRefresh.Margin,
                MinimumSize = btnRefresh.MinimumSize,
                Padding = btnRefresh.Padding,
                UseVisualStyleBackColor = false,
                Enabled = false,
            };
            b.FlatAppearance.BorderSize = primary ? 0 : 1;
            b.FlatAppearance.BorderColor = Theme.BorderColor;
            b.FlatAppearance.MouseOverBackColor = primary ? Theme.PrimaryDark : Theme.GridAltRow;
            return b;
        }

        /// <summary>Thanh trên + thẻ "Đơn đang chọn" theo đúng trạng thái đơn đang chọn (gọi từ UpdateActionButtons).</summary>
        private void UpdateSelectionUi(TransportOrderListItem? o)
        {
            var draft = o?.Status == "Draft";
            if (_btnEdit != null) { _btnEdit.Visible = draft; _btnEdit.Enabled = draft; }
            if (_btnConfirm != null) { _btnConfirm.Visible = draft; _btnConfirm.Enabled = draft; }
            btnAllocate.Visible = o?.Status == "Confirmed" && o.ActiveTripCount == 0;
            foreach (var b in new[] { _btnDetail, _btnDeadline, _btnTools })
                if (b != null) b.Enabled = o is not null;

            if (o is null)
            {
                _cardAction = null;
                _card.ShowEmpty(_orders.Count == 0
                    ? "Chưa có đơn nào (hoặc bộ lọc không khớp đơn nào) — bấm ➕ Tạo đơn mới."
                    : "Bấm 1 dòng trong bảng bên dưới để xem thông tin và việc cần làm của đơn.", "Chưa chọn đơn");
                return;
            }
            var overdue = DeadlineGroup(o) == DeadlineOverdue;
            var next = o.Status switch
            {
                "Draft" => "Nháp — kiểm tra lại với khách rồi bấm ✅ Xác nhận đơn (còn sửa được bằng ✏️ Sửa đơn)",
                "Confirmed" when o.ActiveTripCount == 0 => "Đã xác nhận — bấm 🚚 Chia xe để tạo chuyến",
                "Confirmed" => $"Đã chia xe ({o.ActiveTripCount} chuyến) — chờ xe xuất phát, theo dõi ở màn Chuyến",
                "InProgress" => $"Đang vận chuyển {o.ProgressPercent:0.##}% — theo dõi ở màn Chuyến",
                "Done" => "Hoàn tất — chỉ xem lại / phí lưu kho",
                "Cancelled" => "Đã hủy — chỉ xem lại",
                _ => "",
            };
            if (overdue) next = "⏰ ĐÃ QUÁ HẠN GIAO — " + next;
            _card.ShowCard(BuildOrderCard(o, next, overdue));
        }

        /// <summary>Thẻ đơn (cùng kiểu thẻ màn Chuyến).</summary>
        private TripCardInfo BuildOrderCard(TransportOrderListItem o, string next, bool overdue)
        {
            var status = TranslateStatus(o.Status);
            var done = o.Status is "Done" or "Cancelled";
            var info = new TripCardInfo
            {
                Title = $"Đơn {o.Code}",
                FromCaption = "LẤY HÀNG TẠI",
                ToCaption = "GIAO TỚI",
                From = string.IsNullOrWhiteSpace(o.PickupPoint) ? "?" : o.PickupPoint,
                To = string.IsNullOrWhiteSpace(o.DropoffPoint) ? "?" : o.DropoffPoint,
                RouteMid = string.IsNullOrWhiteSpace(o.WarehouseName) ? "giao thẳng" : $"qua 🏬 {o.WarehouseName}",
                Next = next,
                Blocked = overdue || o.OverdueStorageDays > 0 || o.Status == "Confirmed" && o.ActiveTripCount == 0,
                Done = o.Status == "Done",
                Accent = StatusColor(o.Status),
            };
            info.Pills.Add((status, status == "Đã hủy" ? GridDecor.Red : GridDecor.StatusTone(status) ?? GridDecor.Slate));
            if (!done && o.CommittedDeliveryDate is not null && DeadlineGroup(o) is DeadlineOverdue or DeadlineSoon)
                info.Pills.Add(("Hạn " + ScreenKit.Deadline(o.CommittedDeliveryDate), DeadlineGroup(o) == DeadlineOverdue ? GridDecor.Red : GridDecor.Amber));
            if (o.OverdueStorageDays > 0) info.Pills.Add(("📦 Nằm kho quá hạn", GridDecor.Red));

            info.Boxes.Add(("Khách và hợp đồng", new List<(string, string)>
            {
                ("Khách", o.CustomerName),
                ("Hợp đồng", o.ContractCode is { Length: > 0 } c ? "📑 " + c : "Chưa có hợp đồng"),
            }));
            info.Boxes.Add(("Thời gian", new List<(string, string)>
            {
                ("Hạn giao", o.CommittedDeliveryDate is DateTime due ? $"{due:dd/MM/yyyy}" : "Chưa có"),
                ("Tình trạng", o.CommittedDeliveryDate is null ? "—" : ScreenKit.Deadline(o.CommittedDeliveryDate, done)),
            }));
            var goods = new List<(string, string)>
            {
                ("Chuyến", o.ActiveTripCount > 0 ? $"🚚 {o.ActiveTripCount} chuyến" : "Chưa chia xe"),
                ("Đã giao", $"{o.ProgressPercent:0.##}%"),
            };
            if (o.QtyOnHand > 0) goods.Add(("Đang nằm kho", $"📦 {o.QtyOnHand:#,##0.##}"));
            info.Boxes.Add(("Hàng và chuyến", goods));

            // Nút chính cạnh "việc cần làm" — cùng xử lý với nút / menu sẵn có.
            (info.ActionText, _cardAction) = o.Status switch
            {
                "Draft" => ("✅ Xác nhận đơn", (Func<Task>)(() => RunAsync(_actConfirm))),
                "Confirmed" when o.ActiveTripCount == 0 => ("🚚 Chia xe", (Func<Task>)(() => RunAsync(_actAllocate))),
                "Confirmed" or "InProgress" => ("🛣️ Mở màn Chuyến xe", (Func<Task>)OpenTripsScreen),
                _ => ("📄 Xem chi tiết", (Func<Task>)(() => RunAsync(_actDetail))),
            };
            return info;
        }

        private Task OpenTripsScreen()
        {
            var main = Application.OpenForms.OfType<TruckingSystem.Desktop.GUI_Shared.MainForm>().FirstOrDefault();
            if (main is not null && !main.OpenNavByName("Chuyến xe"))
                Toast.Show(this, "Tài khoản của bạn không có quyền vào màn Chuyến xe.", ToastKind.Info);
            return Task.CompletedTask;
        }

        private void SetStatus(string text, Color color)
        {
            lblStatus.Text = text;
            lblStatus.ForeColor = color;
        }

        // ------------------------------------------------------------ các thao tác

        private async Task EditAsync(TransportOrderListItem o)
        {
            using var f = new CreateOrderForm(_api!, o.Id);
            if (f.ShowDialog(FindForm()) == DialogResult.OK) await LoadAsync();
        }

        private async Task ConfirmAsync(TransportOrderListItem o)
        {
            if (!Ask.Confirm(this, $"Xác nhận đơn {o.Code}?",
                    "Sau khi xác nhận sẽ KHÔNG sửa được đơn nữa (chỉ có thể hủy). Bước tiếp theo: chia xe.", "✅ Xác nhận đơn")) return;
            var result = await _api!.TransportOrders.ConfirmOrderAsync(o.Id);
            await LoadAsync(); // đơn có thể vừa bị người khác đổi -> nút theo dữ liệu mới
            SetStatus(result.Success ? $"✅ Đã xác nhận đơn {o.Code} — bước tiếp: 🚚 Chia xe." : "⚠️ " + result.Message,
                result.Success ? Theme.Success : Theme.Warning);
        }

        private async Task CancelAsync(TransportOrderListItem o)
        {
            if (!Ask.Confirm(this, $"Hủy đơn {o.Code}?", "Đơn bị hủy hẳn, không hoàn tác được.",
                    "❌ Hủy đơn", danger: true, cancelText: "Giữ đơn")) return;
            var result = await _api!.TransportOrders.CancelOrderAsync(o.Id);
            await LoadAsync();
            if (result.Success && !string.IsNullOrEmpty(result.Message)) // cảnh báo quan trọng (VD xe lấy hàng đang chạy) -> giữ hộp thoại
                MessageBox.Show(FindForm(), result.Message, "Đã hủy đơn — cần lưu ý", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            SetStatus(result.Success ? $"❌ Đã hủy đơn {o.Code}." : "⚠️ " + result.Message,
                result.Success ? Theme.TextSecondary : Theme.Warning);
        }

        private async Task ChangeDeadlineAsync(TransportOrderListItem o)
        {
            var detail = await _api!.TransportOrders.GetOrderDetailAsync(o.Id);
            using var f = new ChangeDeadlineForm(_api, detail);
            if (f.ShowDialog(FindForm()) != DialogResult.OK) return;
            await LoadAsync();
            SetStatus("✅ " + (f.ResultMessage ?? "Đã đổi hạn giao.").Replace("\n", "  "), Theme.Success);
        }

        private async Task StorageFeeAsync(TransportOrderListItem o)
        {
            var detail = await _api!.TransportOrders.GetOrderDetailAsync(o.Id);
            using var f = new StorageFeeForm(_api, detail);
            f.ShowDialog(FindForm());
            if (f.ChangedAny) await LoadAsync();
        }

        private async Task ViewRouteAsync(TransportOrderListItem o)
        {
            var detail = await _api!.TransportOrders.GetOrderDetailAsync(o.Id);
            var stops = OrderDetailForm.BuildRouteStops(detail);
            if (stops.Count < 2)
            {
                Ask.Fail(this,
                    "Chưa đủ tọa độ để vẽ lộ trình (cần kho trung chuyển và điểm giao có tọa độ" +
                    (detail.PickupMethod == "SelfDelivery" ? "" : ", cùng điểm lấy hàng") + ").");
                return;
            }
            using var f = new RouteMapForm(_api, detail.Code, stops);
            f.ShowDialog(FindForm());
        }
    }
}
