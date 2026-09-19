using System.Drawing;
using System.Windows.Forms;
using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_WarehouseManagement;

namespace TruckingSystem.Desktop.GUI_WarehouseManagement;

/// <summary>Logic — phần khung tĩnh (tiêu đề, combo chọn kho, khung test Best-Fit)
/// nằm ở WarehousesControl.Designer.cs. Riêng sơ đồ Zone/Bin (_zonesPanel bên
/// trong) vẫn tạo động bằng code vì số lượng khu vực/vị trí thay đổi tùy kho,
/// không cố định được — đây là phần hợp lý PHẢI viết code, Designer không giúp
/// được (giống việc DataGridView.DataSource cũng luôn phải gán bằng code).</summary>
public partial class WarehousesControl : BaseUserControl
{
    private ApiServices? _api;

    public WarehousesControl()
    {
        InitializeComponent();
        cboWarehouse.SelectedIndexChanged += async (_, _) => await LoadZonesAsync();

        Resize += (_, _) =>
        {
            zonesPanel.Width = Width;
            testPanel.Width = Width;
            testPanel.Top = Height - 160;
            zonesPanel.Height = testPanel.Top - zonesPanel.Top - 10;
        };

        Load += async (_, _) => await LoadWarehousesAsync();
    }

    public WarehousesControl(ApiServices api) : this()
    {
        _api = api;
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
        lblStatus.Text = "Đang tải sơ đồ kho...";
        zonesPanel.Controls.Clear();
        try
        {
            var zones = await _api.Warehouses.GetZonesAsync(warehouseId);
            foreach (var zone in zones)
            {
                var bins = await _api.Warehouses.GetBinsAsync(zone.Id);
                zonesPanel.Controls.Add(BuildZoneCard(zone, bins));
            }
            lblStatus.Text = $"{zones.Count} khu vực";
        }
        catch (Exception ex)
        {
            lblStatus.Text = "Lỗi: " + ex.Message;
            lblStatus.ForeColor = Theme.Danger;
        }
    }

    /// <summary>Vẽ 1 khu vực (Zone) thành SƠ ĐỒ LƯỚI — mỗi StorageBin là 1 ô vuông
    /// đặt ĐÚNG vị trí thật (Row, Column) trong kho, tô màu theo % sức chứa đã
    /// dùng (xanh=còn trống nhiều, vàng=vừa, đỏ=gần đầy) — giúp nhìn 1 phát biết
    /// ngay còn chỗ trống ở đâu, thay vì phải đọc từng dòng danh sách.</summary>
    private static Panel BuildZoneCard(WarehouseZoneItem zone, List<StorageBinItem> bins)
    {
        const int cellSize = 90;
        const int cellGap = 10;
        const int headerHeight = 44;

        var zoneColor = zone.ZoneType switch { "Fragile" => Theme.Warning, "Hazmat" => Theme.Danger, _ => Theme.Primary };

        int maxRow = bins.Count > 0 ? bins.Max(b => b.Row) : 0;
        int maxCol = bins.Count > 0 ? bins.Max(b => b.Column) : 0;
        int gridWidth = (maxCol + 1) * (cellSize + cellGap) + cellGap;
        int gridHeight = (maxRow + 1) * (cellSize + cellGap) + cellGap;

        var card = new Panel
        {
            Width = Math.Max(280, gridWidth),
            Height = headerHeight + gridHeight,
            Margin = new Padding(0, 0, 20, 20),
            BackColor = Theme.CardBg,
        };
        card.Paint += (_, e) =>
        {
            e.Graphics.DrawRectangle(new Pen(Theme.BorderColor), 0, 0, card.Width - 1, card.Height - 1);
            e.Graphics.FillRectangle(new SolidBrush(zoneColor), 0, 0, card.Width, 4);
        };

        var lblZoneName = new Label
        {
            Text = $"{ZoneIcon(zone.ZoneType)}  {zone.Name}",
            Font = Theme.FontBold,
            ForeColor = Theme.TextPrimary,
            Top = 14,
            Left = 16,
            AutoSize = true,
        };
        card.Controls.Add(lblZoneName);

        foreach (var bin in bins)
        {
            var pct = bin.CapacityM3 == 0 ? 0 : (double)(bin.CurrentUsedM3 / bin.CapacityM3);
            var cellColor = pct switch
            {
                >= 0.85 => Theme.Danger,
                >= 0.5 => Theme.Warning,
                _ => Theme.Success,
            };

            var cell = new Panel
            {
                Left = cellGap + bin.Column * (cellSize + cellGap),
                Top = headerHeight + bin.Row * (cellSize + cellGap),
                Width = cellSize,
                Height = cellSize,
                BackColor = cellColor,
                Cursor = Cursors.Hand,
            };

            var lblCode = new Label
            {
                Text = bin.Code,
                Font = Theme.FontBold,
                ForeColor = Color.White,
                Dock = DockStyle.Top,
                Height = cellSize / 2,
                TextAlign = ContentAlignment.MiddleCenter,
            };
            var lblPct = new Label
            {
                Text = $"{pct:P0}",
                Font = Theme.FontSmall,
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
            };
            cell.Controls.Add(lblPct);
            cell.Controls.Add(lblCode);

            var tooltip = new ToolTip();
            var tooltipText = $"{bin.Code}\nĐã dùng: {bin.CurrentUsedM3}/{bin.CapacityM3} m³\nTải tối đa: {bin.MaxWeightKg} kg";
            tooltip.SetToolTip(cell, tooltipText);
            tooltip.SetToolTip(lblCode, tooltipText);
            tooltip.SetToolTip(lblPct, tooltipText);

            card.Controls.Add(cell);
        }
        return card;
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