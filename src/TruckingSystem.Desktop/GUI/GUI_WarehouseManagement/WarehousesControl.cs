using System.Drawing;
using System.Windows.Forms;
using TruckingSystem.Desktop.Common;
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

            Resize += (_, _) =>
            {
                testPanel.Width = Width;
                testPanel.Top = Height - 160;
                zonesPanel.Width = Width;
                zonesPanel.Height = testPanel.Top - zonesPanel.Top - 10;
            };

            Load += async (_, _) => await LoadWarehousesAsync();
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

                var missingSize = warehouse.LengthMeters is null || warehouse.WidthMeters is null;
                lblStatus.Text = $"{zones.Count} khu vực" + (missingSize ? " — ⚠️ kho chưa khai báo kích thước thật, đang hiện tạm theo tỉ lệ mặc định" : "");
                lblStatus.ForeColor = missingSize ? Theme.Warning : Theme.TextSecondary;
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Lỗi: " + ex.Message;
                lblStatus.ForeColor = Theme.Danger;
            }
        }

        /// <summary>Vẽ 1 khu vực (Zone) ĐÚNG vị trí + kích thước thật (PosX, PosY,
        /// WidthMeters, HeightMeters, quy đổi ra pixel theo PixelsPerMeter) —
        /// khác bản trước chỉ hiện thẻ rời rạc không theo tỉ lệ nào cả. Bên trong
        /// vẫn vẽ lưới StorageBin theo Row/Column như cũ, nhưng giờ CO GIÃN vừa
        /// đúng khung của Zone thay vì cỡ ô cố định 90px.</summary>
        private static Panel BuildZonePanel(WarehouseZoneItem zone, List<StorageBinItem> bins)
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
            };
            zonePanel.Paint += (_, e) =>
            {
                using var pen = new Pen(zoneColor, 2);
                e.Graphics.DrawRectangle(pen, 1, 1, zonePanel.Width - 3, zonePanel.Height - 3);
            };

            var lblZoneName = new Label
            {
                Text = $"{ZoneIcon(zone.ZoneType)} {zone.Name}",
                Font = Theme.FontSmall,
                ForeColor = Theme.TextPrimary,
                Top = 4,
                Left = 6,
                AutoSize = true,
            };
            zonePanel.Controls.Add(lblZoneName);

            if (bins.Count == 0) return zonePanel;

            int maxRow = bins.Max(b => b.Row);
            int maxCol = bins.Max(b => b.Column);
            var gridAreaWidth = zonePanel.Width - padding * 2;
            var gridAreaHeight = zonePanel.Height - headerHeight - padding;
            var cellWidth = gridAreaWidth / (maxCol + 1);
            var cellHeight = gridAreaHeight / (maxRow + 1);
            var cellGap = Math.Max(2, Math.Min(cellWidth, cellHeight) / 12);

            foreach (var bin in bins)
            {
                var pct = bin.CapacityM3 == 0 ? 0 : (double)(bin.CurrentUsedM3 / bin.CapacityM3);
                var cellColor = pct switch { >= 0.85 => Theme.Danger, >= 0.5 => Theme.Warning, _ => Theme.Success };

                var cell = new Panel
                {
                    Left = padding + bin.Column * cellWidth + cellGap / 2,
                    Top = headerHeight + bin.Row * cellHeight + cellGap / 2,
                    Width = Math.Max(4, cellWidth - cellGap),
                    Height = Math.Max(4, cellHeight - cellGap),
                    BackColor = cellColor,
                    Cursor = Cursors.Hand,
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
                        Dock = DockStyle.Fill,
                        TextAlign = ContentAlignment.MiddleCenter,
                        AutoEllipsis = true,
                    };
                    cell.Controls.Add(lblCode);
                }

                var tooltip = new ToolTip();
                var tooltipText = $"{bin.Code}\nĐã dùng: {bin.CurrentUsedM3}/{bin.CapacityM3} m³ ({pct:P0})\nTải tối đa: {bin.MaxWeightKg} kg";
                tooltip.SetToolTip(cell, tooltipText);
                foreach (Control child in cell.Controls) tooltip.SetToolTip(child, tooltipText);

                zonePanel.Controls.Add(cell);
            }
            return zonePanel;
        }

        private static string ZoneIcon(string zoneType) => zoneType switch { "Fragile" => "🔷", "Hazmat" => "☣️", _ => "📦" };

        private async Task RunSuggestAsync()
        {
            if (_api is null) return;
            if (cboWarehouse.SelectedValue is not int warehouseId)
            {
                lblResult.Text = "Vui lòng chọn kho trước.";
                lblResult.ForeColor = Theme.Danger;
                return;
            }
            lblResult.Text = "Đang tính toán...";
            lblResult.ForeColor = Theme.TextSecondary;
            try
            {
                var result = await _api.Warehouses.SuggestBinAsync(warehouseId, (int)numCargoLineId.Value, numQty.Value);
                if (result.Found)
                {
                    lblResult.Text = $"✅ Đề xuất vị trí: {result.BinCode} ({result.ZoneName})\nDư lại sau khi xếp: {result.RemainingAfterM3:0.00} m³";
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
