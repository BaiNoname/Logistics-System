using System.Drawing;
using System.Windows.Forms;
using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_WarehouseManagement;

namespace TruckingSystem.Desktop.GUI_WarehouseManagement
{
    /// <summary>⚙️ KHU VỰC & KỆ của 1 kho — 1 cửa sổ 2 cột (mẫu UX ⑦ đã duyệt), thay cho mở lồng 3 tầng
    /// (Quản lý khu vực → Vị trí lưu trữ → form thêm / sửa):
    ///  • Trái: danh sách khu (tên · loại · số kệ) + "➕ Thêm khu". Chuột phải / nhấp đúp 1 khu: Sửa · Xóa.
    ///  • Phải: sơ đồ nhỏ cả kho (khu đang chọn tô đậm, kệ của khu vẽ bên trong) + bảng kệ của khu + "➕ Thêm kệ".
    ///    Chuột phải / nhấp đúp 1 kệ: Sửa · Xóa.
    /// Form thêm / sửa khu, kệ vẫn là form cũ (đã có sơ đồ xem trước). Xóa dùng hộp hỏi lại chữ đỏ.
    /// Tạo bằng code (không có Designer).</summary>
    public sealed class WarehouseLayoutForm : BaseForm
    {
        private readonly ApiServices _api;
        private readonly WarehouseListItem _warehouse;
        private List<WarehouseZoneItem> _zones = new();
        private List<StorageBinItem> _bins = new();
        private readonly ListBox _zoneList = new() { Dock = DockStyle.Fill, DrawMode = DrawMode.OwnerDrawFixed, BorderStyle = BorderStyle.None, IntegralHeight = false };
        private readonly LayoutPreview _map = new() { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 8) };
        private readonly DataGridView _binGrid = new() { Dock = DockStyle.Fill };
        private readonly Label _binTitle = new() { AutoSize = true, Font = new Font("Segoe UI", 11F, FontStyle.Bold), Anchor = AnchorStyles.Left };
        private readonly Button _addZone = new() { Text = "➕ Thêm khu", AutoSize = true };
        private readonly Button _addBin = new() { Text = "➕ Thêm kệ", AutoSize = true };
        private readonly Label _status = new() { AutoSize = true, ForeColor = Theme.TextSecondary, Anchor = AnchorStyles.Left };
        private int _binLoadId;

        /// <summary>Có thêm / sửa / xóa ít nhất 1 lần -> màn Kho vẽ lại sơ đồ.</summary>
        public bool ChangedAny { get; private set; }

        public WarehouseLayoutForm(ApiServices api, WarehouseListItem warehouse)
        {
            _api = api;
            _warehouse = warehouse;
            int S(int v) => LogicalToDeviceUnits(v);
            Text = $"Khu vực và kệ — {warehouse.Name}";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(S(1180), S(720));
            MinimumSize = new Size(S(900), S(560));
            BackColor = Theme.ContentBg;
            Font = new Font("Segoe UI", 10F);

            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2, Padding = new Padding(S(18), S(14), S(18), S(8)) };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 68));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            var size = warehouse.LengthMeters is { } l && warehouse.WidthMeters is { } w ? $" ({l:0.##} × {w:0.##} m)" : "";
            var title = new Label { Text = $"⚙️ Khu vực và kệ — {warehouse.Name}{size}", AutoSize = true, Font = Theme.FontSectionTitle, Margin = new Padding(0, 0, 0, S(10)) };
            root.Controls.Add(title, 0, 0);
            root.SetColumnSpan(title, 2);

            // ---- Cột trái: khu vực ----
            var left = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1, BackColor = Color.White, Padding = new Padding(S(10)), Margin = new Padding(0, 0, S(12), 0) };
            left.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            left.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            var leftHead = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 2, Margin = new Padding(0, 0, 0, S(8)) };
            leftHead.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            leftHead.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            leftHead.Controls.Add(new Label { Text = "Khu vực", AutoSize = true, Font = new Font("Segoe UI", 11F, FontStyle.Bold), Anchor = AnchorStyles.Left }, 0, 0);
            leftHead.Controls.Add(_addZone, 1, 0);
            left.Controls.Add(leftHead, 0, 0);
            left.Controls.Add(_zoneList, 0, 1);
            _zoneList.ItemHeight = S(46);
            _zoneList.DrawItem += DrawZone;
            _zoneList.SelectedIndexChanged += async (_, _) => await LoadBinsAsync();
            _zoneList.DoubleClick += async (_, _) => await EditZoneAsync();
            _zoneList.MouseDown += (_, e) =>
            {
                if (e.Button != MouseButtons.Right) return;
                var i = _zoneList.IndexFromPoint(e.Location);
                if (i >= 0) _zoneList.SelectedIndex = i;
            };
            _zoneList.ContextMenuStrip = BuildMenu(("✏️ Sửa khu", async () => await EditZoneAsync()), ("🗑️ Xóa khu…", async () => await DeleteZoneAsync()));
            root.Controls.Add(left, 0, 1);

            // ---- Cột phải: sơ đồ + kệ ----
            var right = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1, BackColor = Color.White, Padding = new Padding(S(10)), Margin = Padding.Empty };
            right.RowStyles.Add(new RowStyle(SizeType.Percent, 48));
            right.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            right.RowStyles.Add(new RowStyle(SizeType.Percent, 52));
            right.Controls.Add(_map, 0, 0);
            var binHead = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 2, Margin = new Padding(0, 0, 0, S(6)) };
            binHead.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            binHead.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            binHead.Controls.Add(_binTitle, 0, 0);
            binHead.Controls.Add(_addBin, 1, 0);
            right.Controls.Add(binHead, 0, 1);
            right.Controls.Add(_binGrid, 0, 2);
            Theme.StyleGrid(_binGrid);
            GridLayout.FitColumns(_binGrid);
            _binGrid.DataBindingComplete += (_, _) => { if (_binGrid.Columns["Id"] is { } c) c.Visible = false; };
            _binGrid.CellDoubleClick += async (_, e) => { if (e.RowIndex >= 0) await EditBinAsync(); };
            _binGrid.CellMouseDown += (_, e) =>
            {
                if (e.Button == MouseButtons.Right && e.RowIndex >= 0 && e.ColumnIndex >= 0)
                    _binGrid.CurrentCell = _binGrid.Rows[e.RowIndex].Cells[e.ColumnIndex];
            };
            _binGrid.ContextMenuStrip = BuildMenu(("✏️ Sửa kệ", async () => await EditBinAsync()), ("🗑️ Xóa kệ…", async () => await DeleteBinAsync()));
            root.Controls.Add(right, 1, 1);

            // ---- Chân cửa sổ ----
            foreach (var b in new[] { _addZone, _addBin }) { ButtonStyles.Secondary(b); b.Margin = Padding.Empty; }
            _addZone.Click += async (_, _) => await AddZoneAsync();
            _addBin.Click += async (_, _) => await AddBinAsync();
            var close = new Button { Text = "Đóng", AutoSize = true, MinimumSize = new Size(S(90), S(36)), DialogResult = DialogResult.Cancel };
            ButtonStyles.Secondary(close);
            CancelButton = close;
            Controls.Add(root);
            DialogFit.Enable(this, root); // laptop màn nhỏ: thu cửa sổ cho vừa, thiếu chỗ thì cuộn
            var footer = ButtonStyles.Footer(this, null, close);
            footer.Controls.Add(_status, 0, 0);
            DialogKeys.Enable(this);
            Load += async (_, _) => await LoadZonesAsync(null);
        }

        private ContextMenuStrip BuildMenu(params (string Text, Func<Task> Run)[] items)
        {
            var m = new ContextMenuStrip();
            foreach (var (text, run) in items)
            {
                var it = new ToolStripMenuItem(text);
                it.Click += async (_, _) => await run();
                if (text.Contains("Xóa")) it.ForeColor = Theme.Danger;
                m.Items.Add(it);
            }
            return m;
        }

        private WarehouseZoneItem? SelectedZone => _zoneList.SelectedItem as WarehouseZoneItem;

        private StorageBinItem? SelectedBin
            => _binGrid.CurrentRow?.Cells["Id"]?.Value is int id ? _bins.FirstOrDefault(b => b.Id == id) : null;

        private static string ZoneTypeText(string t) => t switch { "Fragile" => "dễ vỡ", "Hazmat" => "nguy hiểm", _ => "hàng thường" };
        private static string ZoneIcon(string t) => t switch { "Fragile" => "🔷", "Hazmat" => "☣️", _ => "📦" };

        private void DrawZone(object? sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= _zoneList.Items.Count || _zoneList.Items[e.Index] is not WarehouseZoneItem z) return;
            var selected = (e.State & DrawItemState.Selected) != 0;
            using (var bg = new SolidBrush(selected ? Color.FromArgb(239, 246, 255) : Color.White)) e.Graphics.FillRectangle(bg, e.Bounds);
            if (selected) using (var bar = new SolidBrush(Theme.Primary)) e.Graphics.FillRectangle(bar, e.Bounds.X, e.Bounds.Y + 4, LogicalToDeviceUnits(4), e.Bounds.Height - 8);
            using var bold = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            using var small = new Font("Segoe UI", 9F);
            var x = e.Bounds.X + LogicalToDeviceUnits(12);
            TextRenderer.DrawText(e.Graphics, $"{ZoneIcon(z.ZoneType)} {z.Name}", bold, new Point(x, e.Bounds.Y + LogicalToDeviceUnits(4)), Theme.TextPrimary);
            TextRenderer.DrawText(e.Graphics, $"{ZoneTypeText(z.ZoneType)} · {z.BinCount} kệ · {z.WidthMeters:0.##} × {z.HeightMeters:0.##} m",
                small, new Point(x, e.Bounds.Y + LogicalToDeviceUnits(25)), Theme.TextSecondary);
            using var line = new Pen(Theme.BorderColor);
            e.Graphics.DrawLine(line, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
        }

        private async Task LoadZonesAsync(int? keepZoneId)
        {
            try
            {
                keepZoneId ??= SelectedZone?.Id;
                _zones = await _api.Warehouses.GetZonesAsync(_warehouse.Id);
                if (IsDisposed) return;
                _zoneList.BeginUpdate();
                _zoneList.Items.Clear();
                foreach (var z in _zones) _zoneList.Items.Add(z);
                _zoneList.EndUpdate();
                var keep = _zones.FindIndex(z => z.Id == keepZoneId);
                if (_zones.Count > 0) _zoneList.SelectedIndex = keep >= 0 ? keep : 0;
                else { _bins = new(); BindBins(); DrawMap(); }
                _status.Text = _zones.Count == 0
                    ? "Kho chưa có khu nào — bấm \"➕ Thêm khu\" để bắt đầu."
                    : $"{_zones.Count} khu · {_zones.Sum(z => z.BinCount)} kệ — chuột phải hoặc nhấp đúp để sửa / xóa.";
            }
            catch (Exception ex)
            {
                if (!IsDisposed) Ask.Fail(this, "Lỗi tải khu vực: " + ex.Message);
            }
        }

        private async Task LoadBinsAsync()
        {
            var zone = SelectedZone;
            _addBin.Enabled = zone is not null;
            _binTitle.Text = zone is null ? "Kệ" : $"Kệ trong {zone.Name}";
            if (zone is null) return;
            var loadId = ++_binLoadId;
            try
            {
                var bins = await _api.Warehouses.GetBinsAsync(zone.Id) ?? new();
                if (IsDisposed || loadId != _binLoadId) return;
                _bins = bins;
                BindBins();
                DrawMap();
            }
            catch (Exception ex)
            {
                if (!IsDisposed) Ask.Fail(this, "Lỗi tải kệ: " + ex.Message);
            }
        }

        private void BindBins()
        {
            _binGrid.DataSource = _bins.OrderBy(b => b.Code).Select(b => new
            {
                b.Id,
                Mã_kệ = b.Code,
                Sức_chứa_m3 = b.CapacityM3,
                Đang_dùng = b.CapacityM3 <= 0 ? "—" : $"{b.CurrentUsedM3 / b.CapacityM3 * 100:0}%",
                Tải_tối_đa_kg = b.MaxWeightKg,
                Cách_cửa_m = b.DistanceFromDoorMeters,
                Đang_chứa = b.Contents.Count == 0 ? "Trống" : string.Join(", ", b.Contents.Select(c => $"{c.OrderCode} ({c.QtyStored:0.##})").Distinct()),
            }).ToList();
            if (_binGrid.Columns["Đang_chứa"] is { } c) c.FillWeight = 180;
        }

        /// <summary>Sơ đồ cả kho: mọi khu (khu đang chọn tô đậm) + kệ của khu đang chọn vẽ bên trong.</summary>
        private void DrawMap()
        {
            var areaW = _warehouse.LengthMeters ?? _zones.Select(z => z.PosX + z.WidthMeters).DefaultIfEmpty(30).Max();
            var areaH = _warehouse.WidthMeters ?? _zones.Select(z => z.PosY + z.HeightMeters).DefaultIfEmpty(20).Max();
            _map.SetArea(areaW, areaH, $"Sơ đồ kho — {areaW:0.##} × {areaH:0.##} m · bấm 1 khu ở bên trái để tô sáng");
            var sel = SelectedZone;
            var boxes = _zones.Select(z => new LayoutPreview.Box(z.Name,
                new RectangleF((float)z.PosX, (float)z.PosY, (float)z.WidthMeters, (float)z.HeightMeters),
                LayoutPreview.ZoneColor(z.ZoneType), z.Id == sel?.Id)).ToList();
            if (sel is not null)
                boxes.AddRange(_bins.Select(b => new LayoutPreview.Box(b.Code,
                    new RectangleF((float)(sel.PosX + b.PosX), (float)(sel.PosY + b.PosY), (float)b.WidthMeters, (float)b.HeightMeters),
                    LayoutPreview.BinColor(b.CurrentUsedM3, b.CapacityM3))));
            _map.SetBoxes(boxes);
            _map.SetCurrent(null, "", false);
        }

        // ================= Thao tác =================

        private async Task AddZoneAsync()
        {
            using var f = new CreateZoneForm(_api, _warehouse.Id);
            if (f.ShowDialog(this) != DialogResult.OK) return;
            ChangedAny = true;
            Toast.Show(this, "✅ Đã thêm khu.");
            await LoadZonesAsync(null);
            if (_zones.Count > 0) _zoneList.SelectedIndex = _zones.Count - 1;
        }

        private async Task EditZoneAsync()
        {
            if (SelectedZone is not { } z) return;
            using var f = new EditZoneForm(_api, z, _warehouse.Id);
            if (f.ShowDialog(this) != DialogResult.OK) return;
            ChangedAny = true;
            Toast.Show(this, $"✅ Đã lưu {z.Name}.");
            await LoadZonesAsync(z.Id);
        }

        private async Task DeleteZoneAsync()
        {
            if (SelectedZone is not { } z) return;
            if (!Ask.Confirm(this, $"Xóa {z.Name}?", $"Khu và {z.BinCount} kệ bên trong (nếu chưa có hàng) bị xóa hẳn, không hoàn tác được.",
                    "🗑️ Xóa khu", danger: true)) return;
            try
            {
                var result = await _api.Warehouses.DeleteZoneAsync(z.Id);
                if (!result.Success) { Ask.Fail(this, result.Message ?? "Không xóa được khu."); return; }
                ChangedAny = true;
                Toast.Show(this, $"🗑️ Đã xóa {z.Name}.");
                await LoadZonesAsync(null);
            }
            catch (Exception ex) { Ask.Fail(this, "Lỗi: " + ex.Message); }
        }

        private async Task AddBinAsync()
        {
            if (SelectedZone is not { } z) { Toast.Show(this, "👆 Chọn 1 khu ở bên trái trước.", ToastKind.Info); return; }
            using var f = new CreateBinForm(_api, z.Id, z.Name, z.WidthMeters, z.HeightMeters);
            if (f.ShowDialog(this) != DialogResult.OK) return;
            ChangedAny = true;
            Toast.Show(this, "✅ Đã thêm kệ.");
            await LoadZonesAsync(z.Id); // số kệ của khu đổi
        }

        private async Task EditBinAsync()
        {
            if (SelectedZone is not { } z || SelectedBin is not { } b) return;
            using var f = new EditBinForm(_api, b, z.Name, z.WidthMeters, z.HeightMeters, z.Id);
            if (f.ShowDialog(this) != DialogResult.OK) return;
            ChangedAny = true;
            Toast.Show(this, $"✅ Đã lưu kệ {b.Code}.");
            await LoadBinsAsync();
        }

        private async Task DeleteBinAsync()
        {
            if (SelectedZone is not { } z || SelectedBin is not { } b) return;
            if (!Ask.Confirm(this, $"Xóa kệ {b.Code}?", "Kệ bị xóa hẳn (chỉ xóa được kệ đang trống), không hoàn tác được.", "🗑️ Xóa kệ", danger: true)) return;
            try
            {
                var result = await _api.Warehouses.DeleteBinAsync(b.Id);
                if (!result.Success) { Ask.Fail(this, result.Message ?? "Không xóa được kệ."); return; }
                ChangedAny = true;
                Toast.Show(this, $"🗑️ Đã xóa kệ {b.Code}.");
                await LoadZonesAsync(z.Id);
            }
            catch (Exception ex) { Ask.Fail(this, "Lỗi: " + ex.Message); }
        }
    }
}
