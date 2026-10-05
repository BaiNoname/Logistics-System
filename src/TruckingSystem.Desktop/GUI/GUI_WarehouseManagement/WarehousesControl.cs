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
        private const decimal PixelsPerMeter = 25; // tỉ lệ vẽ — 1 mét thật = 25 pixel trên màn hình
        private bool _currentHasDoor; // kho đang xem đã khai báo cửa chưa (để hiện khoảng cách trong tooltip)

        private ApiServices? _api;

        public WarehousesControl()
        {
            InitializeComponent();

            // Chống giật/nhòe hình khi vẽ động nhiều Panel lồng nhau (sơ đồ Zone/Bin) —
            // Panel không cho set DoubleBuffered trực tiếp từ ngoài (protected),
            // phải dùng Reflection để bật.
            typeof(System.Windows.Forms.Control).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                null, zonesPanel, new object[] { true });

            cboWarehouse.SelectedIndexChanged += async (_, _) => await LoadZonesAsync();
            cboOrderSuggest.SelectedIndexChanged += async (_, _) => await LoadCargoLinesForSuggestAsync();
            cboCargoLineSuggest.SelectedIndexChanged += CboCargoLineSuggest_SelectedIndexChanged;

            Resize += (_, _) =>
            {
                testPanel.Width = Width;
                testPanel.Top = Height - 190;
                zonesPanel.Width = Width;
                zonesPanel.Height = testPanel.Top - zonesPanel.Top - 10;
            };

            Load += async (_, _) => { await LoadWarehousesAsync(); await LoadOrdersForSuggestAsync(); };
        }

        public WarehousesControl(ApiServices api) : this()
        {
            _api = api;
        }

        private void BtnManualStock_Click(object? sender, EventArgs e)
        {
            if (_api is null || cboWarehouse.SelectedValue is not int warehouseId)
            {
                MessageBox.Show(FindForm(), "Vui lòng chọn 1 kho trước.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            using var f = new ManualStockForm(_api, warehouseId);
            f.ShowDialog(FindForm());
            _ = LoadZonesAsync(); // vẽ lại sơ đồ ngay — có thể vừa nhập/xuất làm đổi % các ô
        }

        private void BtnManageZones_Click(object? sender, EventArgs e)
        {
            if (_api is null || cboWarehouse.SelectedValue is not int warehouseId)
            {
                MessageBox.Show(FindForm(), "Vui lòng chọn 1 kho trước.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            using var f = new ManageZonesForm(_api, warehouseId);
            f.ShowDialog(FindForm());
            _ = LoadZonesAsync(); // vẽ lại sơ đồ ngay khi đóng form — có thể vừa thêm khu/vị trí mới
        }

        private async void BtnEditWarehouse_Click(object? sender, EventArgs e)
        {
            if (_api is null || cboWarehouse.SelectedItem is not WarehouseListItem warehouse)
            {
                MessageBox.Show(FindForm(), "Vui lòng chọn 1 kho trước.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

            var confirm = MessageBox.Show(FindForm(),
                $"Xóa hẳn kho \"{warehouse.Name}\"? Toàn bộ khu vực/vị trí bên trong (nếu chưa có hàng) sẽ bị xóa theo. Không thể hoàn tác.",
                "Xác nhận xóa kho", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes) return;

            try
            {
                var result = await _api.Warehouses.DeleteWarehouseAsync(warehouseId);
                if (!result.Success)
                {
                    MessageBox.Show(FindForm(), result.Message ?? "Không thể xóa kho.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                await LoadWarehousesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(FindForm(), "Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private async void BtnSuggest_Click(object? sender, EventArgs e) => await RunSuggestAsync();

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

            lblStatus.Text = "Đang tải sơ đồ kho...";
            zonesPanel.Controls.Clear();
            try
            {
                // Kích thước canvas = đúng kích thước thật của kho (mét) x tỉ lệ vẽ.
                // Nếu kho chưa nhập kích thước (kho cũ, dữ liệu thiếu) thì dùng tạm
                // 1 kích thước mặc định hợp lý để vẫn hiển thị được, không vỡ layout.
                var lengthM = warehouse.LengthMeters ?? 30;
                var widthM = warehouse.WidthMeters ?? 20;
                var canvasWidth = (int)(lengthM * PixelsPerMeter);
                var canvasHeight = (int)(widthM * PixelsPerMeter);

                // AutoScrollMinSize thay vì đổi Size trực tiếp — giữ đúng cơ chế
                // cuộn của Panel khi nội dung lớn hơn khung nhìn thực tế.
                zonesPanel.AutoScrollMinSize = new Size(canvasWidth, canvasHeight);

                var zones = await _api.Warehouses.GetZonesAsync(warehouseId);
                foreach (var zone in zones)
                {
                    var bins = await _api.Warehouses.GetBinsAsync(zone.Id);
                    zonesPanel.Controls.Add(BuildZonePanel(zone, bins));
                }

                // Vẽ TƯỜNG kho (khung viền) và CỬA kho lên sơ đồ
                AddWarehouseWalls(canvasWidth, canvasHeight);
                _currentHasDoor = AddDoorMarker(warehouse, canvasWidth, canvasHeight);

                var missingSize = warehouse.LengthMeters is null || warehouse.WidthMeters is null;
                var warnings = new List<string>();
                if (missingSize) warnings.Add("⚠️ kho chưa khai báo kích thước thật, đang hiện tạm theo tỉ lệ mặc định");
                if (!_currentHasDoor) warnings.Add("⚠️ kho chưa khai báo vị trí cửa — chưa ưu tiên gần/xa cửa được (vào Sửa kho để khai báo)");
                lblStatus.Text = $"{zones.Count} khu vực" + string.Concat(warnings.Select(w => " — " + w));
                lblStatus.ForeColor = warnings.Count > 0 ? Theme.Warning : Theme.TextSecondary;
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Lỗi: " + ex.Message;
                lblStatus.ForeColor = Theme.Danger;
            }
        }

        /// <summary>Vẽ khung TƯỜNG bao quanh toàn bộ mặt sàn kho (nét đứt) — để thấy
        /// rõ ranh giới kho và vị trí cửa nằm trên bức tường nào.</summary>
        private void AddWarehouseWalls(int canvasWidth, int canvasHeight)
        {
            var floor = new Panel { Left = 0, Top = 0, Width = canvasWidth, Height = canvasHeight, BackColor = Color.Transparent };
            floor.Paint += (_, e) =>
            {
                using var pen = new Pen(Theme.TextSecondary, 2) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash };
                e.Graphics.DrawRectangle(pen, 1, 1, floor.Width - 3, floor.Height - 3);
            };
            zonesPanel.Controls.Add(floor);
            floor.SendToBack(); // nằm DƯỚI các khu vực, không che mất khu vực nào
        }

        /// <summary>Vẽ CỬA kho đúng vị trí trên tường (thanh màu cam dày, rộng ~3 m)
        /// kèm nhãn "🚪 CỬA" nằm phía trong kho, cạnh cửa. Trả về false nếu kho chưa
        /// khai báo cửa hoặc chưa có kích thước thật.</summary>
        private bool AddDoorMarker(WarehouseListItem warehouse, int canvasWidth, int canvasHeight)
        {
            if (warehouse.DoorSide is null || warehouse.DoorOffsetMeters is null
                || warehouse.LengthMeters is null || warehouse.WidthMeters is null)
                return false;

            const int thickness = 10;
            var doorColor = Color.FromArgb(234, 88, 12); // cam đậm — nổi bật trên nền sơ đồ
            var doorLength = Math.Max(40, (int)(3 * PixelsPerMeter));
            var offsetPx = (int)(warehouse.DoorOffsetMeters.Value * PixelsPerMeter);

            var rect = warehouse.DoorSide switch
            {
                "Top" => new Rectangle(offsetPx - doorLength / 2, 0, doorLength, thickness),
                "Bottom" => new Rectangle(offsetPx - doorLength / 2, canvasHeight - thickness, doorLength, thickness),
                "Left" => new Rectangle(0, offsetPx - doorLength / 2, thickness, doorLength),
                _ => new Rectangle(canvasWidth - thickness, offsetPx - doorLength / 2, thickness, doorLength), // Right
            };
            // Kẹp vào trong khung kho (cửa sát góc vẫn hiện trọn, không lòi ra ngoài)
            rect.X = Math.Clamp(rect.X, 0, Math.Max(0, canvasWidth - rect.Width));
            rect.Y = Math.Clamp(rect.Y, 0, Math.Max(0, canvasHeight - rect.Height));

            var tipText = $"🚪 Cửa kho — tường {WarehouseDoorHelper.WallName(warehouse.DoorSide)}, cách góc {warehouse.DoorOffsetMeters:0.#} m";
            var door = new Panel { Bounds = rect, BackColor = doorColor };
            new ToolTip().SetToolTip(door, tipText);
            zonesPanel.Controls.Add(door);
            door.BringToFront();

            var label = new Label
            {
                Text = "🚪 CỬA",
                AutoSize = true,
                Font = Theme.FontBold,
                ForeColor = doorColor,
                BackColor = Color.White,
            };
            zonesPanel.Controls.Add(label);
            var size = label.PreferredSize;
            int ClampX(int x) => Math.Clamp(x, 0, Math.Max(0, canvasWidth - size.Width));
            int ClampY(int y) => Math.Clamp(y, 0, Math.Max(0, canvasHeight - size.Height));
            label.Location = warehouse.DoorSide switch
            {
                "Top" => new Point(ClampX(rect.X + rect.Width / 2 - size.Width / 2), rect.Bottom + 2),
                "Bottom" => new Point(ClampX(rect.X + rect.Width / 2 - size.Width / 2), rect.Top - size.Height - 2),
                "Left" => new Point(rect.Right + 2, ClampY(rect.Y + rect.Height / 2 - size.Height / 2)),
                _ => new Point(rect.Left - size.Width - 2, ClampY(rect.Y + rect.Height / 2 - size.Height / 2)),
            };
            new ToolTip().SetToolTip(label, tipText);
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
            const int headerHeight = 26;
            const int padding = 6;

            var zoneColor = zone.ZoneType switch { "Fragile" => Theme.Warning, "Hazmat" => Theme.Danger, _ => Theme.Primary };

            var zonePanel = new Panel
            {
                Left = (int)(zone.PosX * PixelsPerMeter),
                Top = (int)(zone.PosY * PixelsPerMeter),
                Width = Math.Max(60, (int)(zone.WidthMeters * PixelsPerMeter)),
                Height = Math.Max(60, (int)(zone.HeightMeters * PixelsPerMeter)),
                BackColor = Theme.CardBg,
                Cursor = Cursors.Hand,
            };
            zonePanel.Paint += (_, e) =>
            {
                using var pen = new Pen(zoneColor, 2);
                e.Graphics.DrawRectangle(pen, 1, 1, zonePanel.Width - 3, zonePanel.Height - 3);
            };
            // Click vào NỀN khu vực (không phải vào 1 ô vị trí cụ thể) -> mở Sửa khu vực
            zonePanel.Click += async (_, _) => await OpenEditZoneAsync(zone);

            var lblZoneName = new Label
            {
                Text = $"{ZoneIcon(zone.ZoneType)} {zone.Name}",
                Font = Theme.FontSmall,
                ForeColor = Theme.TextPrimary,
                Top = 4,
                Left = 6,
                AutoSize = true,
                Cursor = Cursors.Hand,
            };
            lblZoneName.Click += async (_, _) => await OpenEditZoneAsync(zone);
            zonePanel.Controls.Add(lblZoneName);

            if (bins.Count == 0) return zonePanel;

            foreach (var bin in bins)
            {
                var pct = bin.CapacityM3 == 0 ? 0.0 : Math.Min(1.0, (double)(bin.CurrentUsedM3 / bin.CapacityM3));

                // Vẽ ĐÚNG tọa độ + kích thước thật (mét) của vị trí, TƯƠNG ĐỐI so
                // với góc trên-trái của khu vực (cùng cơ chế PixelsPerMeter như
                // cách Khu vực định vị trong Kho) — khác bản trước tự chia đều ô
                // theo lưới Row/Column, không phản ánh kích thước thật.
                var cell = new Panel
                {
                    Left = padding + (int)(bin.PosX * PixelsPerMeter),
                    Top = headerHeight + (int)(bin.PosY * PixelsPerMeter),
                    Width = Math.Max(4, (int)(bin.WidthMeters * PixelsPerMeter)),
                    Height = Math.Max(4, (int)(bin.HeightMeters * PixelsPerMeter)),
                    BackColor = Theme.Success, // nền XANH = phần còn TRỐNG
                    Cursor = Cursors.Hand,
                };

                // Vẽ phần ĐÃ DÙNG dâng từ DƯỚI LÊN, đúng tỉ lệ % — không phải cả
                // ô 1 màu như trước. VD: 60% đã dùng -> 60% chiều cao ô (tính từ
                // đáy) tô đỏ, 40% phía trên vẫn giữ màu xanh nền.
                cell.Paint += (_, e) =>
                {
                    if (pct <= 0) return;
                    var usedColor = pct >= 0.85 ? Theme.Danger : Theme.Warning; // gần đầy -> đỏ đậm hơn, còn lại -> vàng cam
                    var usedHeight = (int)(cell.Height * pct);
                    var usedRect = new Rectangle(0, cell.Height - usedHeight, cell.Width, usedHeight);
                    using var brush = new SolidBrush(usedColor);
                    e.Graphics.FillRectangle(brush, usedRect);
                };

                // Chỉ hiện chữ nếu ô đủ lớn để đọc được — ô quá nhỏ (kho nhiều vị
                // trí) chỉ cần thấy MÀU là đủ, nhồi chữ vào sẽ chỉ rối mắt.
                if (cell.Width >= 36 && cell.Height >= 28)
                {
                    var lblCode = new Label
                    {
                        Text = bin.Code,
                        Font = Theme.FontSmall,
                        ForeColor = Color.White,
                        BackColor = Color.Transparent,
                        Dock = DockStyle.Fill,
                        TextAlign = ContentAlignment.MiddleCenter,
                        AutoEllipsis = true,
                        Cursor = Cursors.Hand,
                    };
                    lblCode.Click += async (_, _) => await OpenEditBinAsync(bin, zone);
                    cell.Controls.Add(lblCode);
                }

                // Click vào Ô (kể cả phần chưa có Label che lên) -> mở Sửa vị trí ngay
                cell.Click += async (_, _) => await OpenEditBinAsync(bin, zone);

                var tooltip = new ToolTip { AutoPopDelay = 15000, InitialDelay = 300 }; // để đủ lâu đọc hết danh sách hàng bên trong
                var tooltipText = $"{bin.Code}\nĐã dùng: {bin.CurrentUsedM3:0.##}/{bin.CapacityM3} m³ ({pct:P0})\nTải tối đa: {bin.MaxWeightKg} kg\n" +
                    (_currentHasDoor ? $"🚪 Cách cửa: {bin.DistanceFromDoorMeters:0.#} m (tự tính)\n" : "") + "\n" +
                    (bin.Contents.Count == 0
                        ? "(Trống — chưa có hàng)"
                        : "📦 Hàng bên trong:\n" + string.Join("\n", bin.Contents.Select(c =>
                            $"• Đơn {c.OrderCode} — {c.CustomerName}\n   {c.CargoType}: {c.QtyStored:0.##} đơn vị (nhập {c.ReceivedAt.ToLocalTime():dd/MM HH:mm})"))) +
                    "\n\n(Click để sửa)";
                tooltip.SetToolTip(cell, tooltipText);
                foreach (Control child in cell.Controls) tooltip.SetToolTip(child, tooltipText);

                zonePanel.Controls.Add(cell);
            }
            return zonePanel;
        }

        /// <summary>Mở form Sửa khu vực ngay từ sơ đồ — vẽ lại sơ đồ ngay sau khi lưu.</summary>
        private async Task OpenEditZoneAsync(WarehouseZoneItem zone)
        {
            if (_api is null) return;
            using var f = new EditZoneForm(_api, zone);
            if (f.ShowDialog(FindForm()) == DialogResult.OK) await LoadZonesAsync();
        }

        /// <summary>Mở form Sửa vị trí ngay từ sơ đồ — vẽ lại sơ đồ ngay sau khi lưu
        /// (thấy ngay màu/tỉ lệ mới nếu vừa đổi sức chứa).</summary>
        private async Task OpenEditBinAsync(StorageBinItem bin, WarehouseZoneItem zone)
        {
            if (_api is null) return;
            using var f = new EditBinForm(_api, bin, zone.Name, zone.WidthMeters, zone.HeightMeters);
            if (f.ShowDialog(FindForm()) == DialogResult.OK) await LoadZonesAsync();
        }

        private static string ZoneIcon(string zoneType) => zoneType switch { "Fragile" => "🔷", "Hazmat" => "☣️", _ => "📦" };

        private async Task LoadOrdersForSuggestAsync()
        {
            if (_api is null) return;
            try
            {
                var orders = await _api.TransportOrders.GetTransportOrdersAsync();
                cboOrderSuggest.DataSource = orders;
                cboOrderSuggest.DisplayMember = nameof(TransportOrderListItem.DisplayText);
                cboOrderSuggest.ValueMember = nameof(TransportOrderListItem.Id);
            }
            catch (Exception ex)
            {
                lblResult.Text = "Lỗi tải danh sách đơn: " + ex.Message;
            }
        }

        /// <summary>Đổ dòng hàng ĐÚNG theo đơn đã chọn — hiện rõ LOẠI HÀNG (không
        /// chỉ mã số ID trơ trọi như trước), để nhân viên kho biết đang thử
        /// xếp vị trí cho loại hàng gì. Tự động điền sẵn số lượng CÒN LẠI của
        /// dòng hàng đó — đỡ phải gõ tay/tính nhẩm mỗi lần.</summary>
        private async Task LoadCargoLinesForSuggestAsync()
        {
            if (_api is null || cboOrderSuggest.SelectedValue is not int orderId || cboWarehouse.SelectedValue is not int warehouseId) return;
            try
            {
                var lines = await _api.Warehouses.GetCargoLinesForOrderAsync(orderId, warehouseId);
                cboCargoLineSuggest.DataSource = lines;
                cboCargoLineSuggest.DisplayMember = nameof(CargoLineOptionItem.CargoType);
                cboCargoLineSuggest.ValueMember = nameof(CargoLineOptionItem.Id);
            }
            catch (Exception ex)
            {
                lblResult.Text = "Lỗi tải dòng hàng: " + ex.Message;
            }
        }

        private void CboCargoLineSuggest_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cboCargoLineSuggest.SelectedItem is CargoLineOptionItem line && line.QtyRemaining > 0)
                numQty.Value = Math.Min(numQty.Maximum, line.QtyRemaining);
        }

        private async Task RunSuggestAsync()
        {
            if (_api is null) return;
            if (cboWarehouse.SelectedValue is not int warehouseId)
            {
                lblResult.Text = "Vui lòng chọn kho trước.";
                lblResult.ForeColor = Theme.Danger;
                return;
            }
            if (cboCargoLineSuggest.SelectedValue is not int cargoLineId)
            {
                lblResult.Text = "Vui lòng chọn đơn hàng và dòng hàng trước.";
                lblResult.ForeColor = Theme.Danger;
                return;
            }
            lblResult.Text = "Đang tính toán...";
            lblResult.ForeColor = Theme.TextSecondary;
            try
            {
                var result = await _api.Warehouses.SuggestBinAsync(warehouseId, cargoLineId, numQty.Value);
                if (result.Found)
                {
                    // Hiện ĐẦY ĐỦ tất cả vị trí được đề xuất — có thể nhiều hơn 1
                    // nếu số lượng phải chia ra nhiều vị trí/khu vực mới đủ chỗ.
                    var lines = result.Allocations.Select(a =>
                        $"• {a.BinCode} ({a.ZoneName}): xếp {a.QtyToPlace:0.##} đơn vị — ô còn trống {a.RemainingAfterM3:0.00} m³");
                    var header = result.Allocations.Count > 1
                        ? $"✅ Phải chia ra {result.Allocations.Count} vị trí:"
                        : "✅ Đề xuất vị trí:";
                    lblResult.Text = header + "\n" + string.Join("\n", lines) + (result.Message is null ? "" : "\n" + result.Message);
                    lblResult.ForeColor = Theme.Success;
                    await LoadZonesAsync(); // vẽ lại sơ đồ ngay để thấy % thay đổi sau khi test
                }
                else
                {
                    lblResult.Text = "⚠️ " + (result.Message ?? "Không tìm được vị trí phù hợp.");
                    lblResult.ForeColor = Theme.Warning;
                }
            }
            catch (Exception ex)
            {
                lblResult.Text = "Lỗi: " + ex.Message;
                lblResult.ForeColor = Theme.Danger;
            }
        }
    }
}