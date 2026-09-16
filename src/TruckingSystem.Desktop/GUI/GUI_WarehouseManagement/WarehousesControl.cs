using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_WarehouseManagement;

namespace TruckingSystem.Desktop.GUI_WarehouseManagement;

public class WarehousesControl : BaseUserControl
{
    private readonly ApiServices _api;
    private readonly ComboBox _cboWarehouse = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 260 };
    private readonly FlowLayoutPanel _zonesPanel = new()
    {
        Top = 120, Left = 0, Height = 430, AutoScroll = true, FlowDirection = FlowDirection.LeftToRight, WrapContents = true,
    };
    private readonly Label _lblStatus = new() { Font = Theme.FontSmall, ForeColor = Theme.TextSecondary, Top = 90, Left = 0, AutoSize = true };

    private readonly Panel _testPanel = new() { Top = 570, Left = 0, Height = 150, BackColor = Theme.CardBg };
    private readonly NumericUpDown _numCargoLineId = new() { Minimum = 1, Maximum = 999999, Value = 1, Width = 100 };
    private readonly NumericUpDown _numQty = new() { Minimum = 0.1m, Maximum = 999999, DecimalPlaces = 2, Value = 2, Width = 100 };
    private readonly Label _lblResult = new() { Font = Theme.FontBold, AutoSize = true, Top = 0, Left = 360 };

    public WarehousesControl(ApiServices api)
    {
        _api = api;
        BackColor = Theme.ContentBg;

        var title = new Label { Text = "Kho trung chuyển", Font = Theme.FontTitle, ForeColor = Theme.TextPrimary, Top = 0, Left = 0, AutoSize = true };
        var lblChoose = new Label { Text = "Chọn kho:", Font = Theme.FontNormal, Top = 46, Left = 0, AutoSize = true };
        _cboWarehouse.Top = 44; _cboWarehouse.Left = 80;
        _cboWarehouse.SelectedIndexChanged += async (_, _) => await LoadZonesAsync();

        BuildTestPanel();

        Resize += (_, _) =>
        {
            _zonesPanel.Width = Width;
            _testPanel.Width = Width;
            _testPanel.Top = Height - 160;
            _zonesPanel.Height = _testPanel.Top - _zonesPanel.Top - 10;
        };

        Controls.Add(_zonesPanel);
        Controls.Add(_testPanel);
        Controls.Add(_lblStatus);
        Controls.Add(_cboWarehouse);
        Controls.Add(lblChoose);
        Controls.Add(title);

        Load += async (_, _) => await LoadWarehousesAsync();
    }

    private void BuildTestPanel()
    {
        _testPanel.Paint += (_, e) => e.Graphics.DrawRectangle(new Pen(Theme.BorderColor), 0, 0, _testPanel.Width - 1, _testPanel.Height - 1);

        var lblTitle = new Label { Text = "🧪  Kiểm tra thuật toán Best-Fit — đề xuất vị trí lưu trữ", Font = Theme.FontSectionTitle, ForeColor = Theme.TextPrimary, Top = 14, Left = 20, AutoSize = true };
        var lbl1 = new Label { Text = "Mã dòng hàng (CargoLineId):", Font = Theme.FontSmall, ForeColor = Theme.TextSecondary, Top = 56, Left = 20, AutoSize = true };
        _numCargoLineId.Top = 76; _numCargoLineId.Left = 20;
        var lbl2 = new Label { Text = "Số lượng cần xếp (m³):", Font = Theme.FontSmall, ForeColor = Theme.TextSecondary, Top = 56, Left = 140, AutoSize = true };
        _numQty.Top = 76; _numQty.Left = 140;

        var btnSuggest = Theme.MakePrimaryButton("🔍  Gợi ý vị trí", 160, 36);
        btnSuggest.Top = 75; btnSuggest.Left = 260;
        btnSuggest.Click += async (_, _) => await RunSuggestAsync();

        _lblResult.Top = 60; _lblResult.Left = 460; _lblResult.MaximumSize = new Size(600, 80);

        _testPanel.Controls.Add(lblTitle);
        _testPanel.Controls.Add(lbl1);
        _testPanel.Controls.Add(_numCargoLineId);
        _testPanel.Controls.Add(lbl2);
        _testPanel.Controls.Add(_numQty);
        _testPanel.Controls.Add(btnSuggest);
        _testPanel.Controls.Add(_lblResult);
    }

    private async Task LoadWarehousesAsync()
    {
        try
        {
            var warehouses = await _api.Warehouses.GetWarehousesAsync();
            _cboWarehouse.DataSource = warehouses;
            _cboWarehouse.DisplayMember = nameof(WarehouseListItem.Name);
            _cboWarehouse.ValueMember = nameof(WarehouseListItem.Id);
        }
        catch (Exception ex)
        {
            _lblStatus.Text = "Lỗi tải danh sách kho: " + ex.Message;
            _lblStatus.ForeColor = Theme.Danger;
        }
    }

    private async Task LoadZonesAsync()
    {
        if (_cboWarehouse.SelectedValue is not int warehouseId) return;
        _lblStatus.Text = "Đang tải sơ đồ kho...";
        _zonesPanel.Controls.Clear();
        try
        {
            var zones = await _api.Warehouses.GetZonesAsync(warehouseId);
            foreach (var zone in zones)
            {
                var bins = await _api.Warehouses.GetBinsAsync(zone.Id);
                _zonesPanel.Controls.Add(BuildZoneCard(zone, bins));
            }
            _lblStatus.Text = $"{zones.Count} khu vực";
        }
        catch (Exception ex)
        {
            _lblStatus.Text = "Lỗi: " + ex.Message;
            _lblStatus.ForeColor = Theme.Danger;
        }
    }

    private static Panel BuildZoneCard(WarehouseZoneItem zone, List<StorageBinItem> bins)
    {
        var zoneColor = zone.ZoneType switch { "Fragile" => Theme.Warning, "Hazmat" => Theme.Danger, _ => Theme.Primary };

        var card = new Panel { Width = 340, Height = 60 + bins.Count * 34, Margin = new Padding(0, 0, 16, 16), BackColor = Theme.CardBg };
        card.Paint += (_, e) =>
        {
            e.Graphics.DrawRectangle(new Pen(Theme.BorderColor), 0, 0, card.Width - 1, card.Height - 1);
            e.Graphics.FillRectangle(new SolidBrush(zoneColor), 0, 0, card.Width, 4);
        };

        var lblZoneName = new Label { Text = $"{ZoneIcon(zone.ZoneType)}  {zone.Name}", Font = Theme.FontBold, ForeColor = Theme.TextPrimary, Top = 14, Left = 16, AutoSize = true };
        card.Controls.Add(lblZoneName);

        int y = 44;
        foreach (var bin in bins)
        {
            var pct = bin.CapacityM3 == 0 ? 0 : (double)(bin.CurrentUsedM3 / bin.CapacityM3);
            var lblCode = new Label { Text = bin.Code, Font = Theme.FontSmall, ForeColor = Theme.TextPrimary, Top = y, Left = 16, Width = 60 };
            var barBg = new Panel { Top = y + 2, Left = 80, Width = 180, Height = 14, BackColor = Theme.BorderColor };
            var barFill = new Panel { Top = 0, Left = 0, Width = (int)(180 * Math.Min(pct, 1.0)), Height = 14, BackColor = pct > 0.85 ? Theme.Danger : Theme.Success };
            barBg.Controls.Add(barFill);
            var lblPct = new Label { Text = $"{bin.CurrentUsedM3}/{bin.CapacityM3} m³", Font = Theme.FontSmall, ForeColor = Theme.TextSecondary, Top = y, Left = 268, AutoSize = true };

            card.Controls.Add(lblCode);
            card.Controls.Add(barBg);
            card.Controls.Add(lblPct);
            y += 34;
        }
        return card;
    }

    private static string ZoneIcon(string zoneType) => zoneType switch { "Fragile" => "🔷", "Hazmat" => "☣️", _ => "📦" };

    private async Task RunSuggestAsync()
    {
        if (_cboWarehouse.SelectedValue is not int warehouseId)
        {
            _lblResult.Text = "Vui lòng chọn kho trước.";
            _lblResult.ForeColor = Theme.Danger;
            return;
        }
        _lblResult.Text = "Đang tính toán...";
        _lblResult.ForeColor = Theme.TextSecondary;
        try
        {
            var result = await _api.Warehouses.SuggestBinAsync(warehouseId, (int)_numCargoLineId.Value, _numQty.Value);
            if (result.Found)
            {
                _lblResult.Text = $"✅ Đề xuất vị trí: {result.BinCode} ({result.ZoneName})\nDư lại sau khi xếp: {result.RemainingAfterM3:0.00} m³";
                _lblResult.ForeColor = Theme.Success;
            }
            else
            {
                _lblResult.Text = "⚠️ " + (result.Message ?? "Không tìm được vị trí phù hợp.");
                _lblResult.ForeColor = Theme.Warning;
            }
        }
        catch (Exception ex)
        {
            _lblResult.Text = "Lỗi: " + ex.Message;
            _lblResult.ForeColor = Theme.Danger;
        }
    }
}
