using TruckingSystem.Desktop.Common;

namespace TruckingSystem.Desktop.GUI_WarehouseManagement
{
    /// <summary>MÀN KHO — 3 Ô "VIỆC HÔM NAY" thay cho hàng nút ngang hàng (mẫu UX ④ đã duyệt):
    ///  📥 Hàng về kho, chờ NHẬP — số chuyến in to + nút "Nhập hàng ngay →"
    ///  📤 Chờ XUẤT lên xe giao — số chuyến in to + nút "Xuất kho ngay →"
    ///  🧍 Khách tự mang hàng tới — nút "Nhận hàng →"
    /// Có việc chờ thì nút xanh (nút chính), không có thì nút trắng. "📋 Tồn kho và lịch sử" và "⚙️ Quản lý kho ▾" (ít dùng)
    /// ở lại thanh trên. Các nút gốc ở Designer vẫn giữ để dùng lại xử lý, chỉ gỡ khỏi thanh nút.</summary>
    public partial class WarehousesControl
    {
        private readonly FlowLayoutPanel _tiles = new()
        {
            AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Dock = DockStyle.Fill, WrapContents = true,
            Margin = new Padding(0, 0, 0, 8),
        };
        private Label? _inCount, _outCount;
        private Button? _inButton, _outButton;

        private void SetupTaskTiles()
        {
            foreach (var b in new[] { btnPendingReceipts, btnTripIssues, btnManualStock }) toolbar.Controls.Remove(b);
            btnIssueStock.Text = "📋 Tồn kho và lịch sử";
            ButtonStyles.Secondary(btnIssueStock);

            (_inCount, _inButton) = AddTile(Theme.Primary, "📥 Hàng về kho, chờ NHẬP", "Nhập hàng ngay →",
                () => BtnPendingReceipts_Click(this, EventArgs.Empty), "Chuyến lấy hàng đã về kho (đã duyệt) nhưng hàng chưa lên kệ");
            (_outCount, _outButton) = AddTile(Theme.Warning, "📤 Chờ XUẤT lên xe giao", "Xuất kho ngay →",
                () => BtnTripIssues_Click(this, EventArgs.Empty), "Chuyến giao đã chia xe — kho soạn hàng, bốc lên xe rồi xác nhận xuất");
            // Dòng đỏ "⏰ n chuyến phải xuất hôm nay" (mốc xuất phát muộn nhất = hôm nay hoặc đã qua) ngay dưới số.
            if (_outCount.Parent is TableLayoutPanel outTile) KpiStrip.InsertRow(outTile, _outUrgent, outTile.GetRow(_outCount) + 1);
            var (manualLabel, manualButton) = AddTile(Theme.TextSecondary, "🧍 Khách tự mang hàng tới", "Nhận hàng →",
                () => BtnManualStock_Click(this, EventArgs.Empty), "Đơn \"khách tự mang tới kho\": nhận hàng tại quầy");
            manualLabel.Text = "Khi khách tới quầy";
            manualLabel.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            ButtonStyles.Secondary(manualButton);
            UpdateTiles(0, 0);

            KpiStrip.InsertRow(root, _tiles, root.GetRow(toolbar) + 1);
        }

        private (Label Count, Button Action) AddTile(Color accent, string caption, string action, Action run, string tip)
        {
            int S(int v) => LogicalToDeviceUnits(v);
            var tile = new TableLayoutPanel
            {
                ColumnCount = 1, RowCount = 3, AutoSize = false, Size = new Size(S(250), S(138)),
                BackColor = Color.White, Padding = new Padding(S(12), S(10), S(12), S(8)), Margin = new Padding(0, 0, S(12), S(4)),
            };
            tile.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tile.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tile.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            tile.Paint += (_, e) =>
            {
                using var pen = new Pen(Theme.BorderColor);
                e.Graphics.DrawRectangle(pen, 0, 0, tile.Width - 1, tile.Height - 1);
                using var bar = new SolidBrush(accent);
                e.Graphics.FillRectangle(bar, 0, 0, tile.Width, S(4));
            };
            var cap = new Label { Text = caption, AutoSize = true, Font = Theme.FontNormal, ForeColor = Theme.TextSecondary, Margin = Padding.Empty };
            var count = new Label { AutoSize = true, Font = new Font("Segoe UI", 18F, FontStyle.Bold), ForeColor = Theme.TextPrimary, Margin = new Padding(0, S(2), 0, S(4)) };
            var btn = new Button
            {
                Text = action, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Font = Theme.FontBold,
                MinimumSize = new Size(0, S(32)), Padding = new Padding(S(6), 0, S(6), 0), Anchor = AnchorStyles.Left | AnchorStyles.Bottom,
            };
            btn.Click += (_, _) => run();
            new ToolTip().SetToolTip(tile, tip);
            tile.Controls.Add(cap, 0, 0);
            tile.Controls.Add(count, 0, 1);
            tile.Controls.Add(btn, 0, 2);
            _tiles.Controls.Add(tile);
            return (count, btn);
        }

        private readonly Label _outUrgent = new()
        {
            AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(220, 38, 38), Margin = new Padding(0, 0, 0, 4), Visible = false,
        };

        /// <summary>Cập nhật số chuyến chờ nhập / chờ xuất của kho đang chọn (gọi từ UpdatePendingCountAsync).
        /// urgentOut: số chuyến chờ xuất phải xuất phát hôm nay / đã quá mốc.</summary>
        private void UpdateTiles(int inTrips, int outTrips, int urgentOut = 0)
        {
            _outUrgent.Text = $"⏰ {urgentOut} chuyến phải xuất hôm nay";
            _outUrgent.Visible = urgentOut > 0;
            if (_inCount is null || _outCount is null || _inButton is null || _outButton is null) return;
            _inCount.Text = inTrips > 0 ? $"{inTrips} chuyến" : "Không có";
            _outCount.Text = outTrips > 0 ? $"{outTrips} chuyến" : "Không có";
            _inCount.ForeColor = inTrips > 0 ? Theme.TextPrimary : Theme.TextSecondary;
            _outCount.ForeColor = outTrips > 0 ? Theme.TextPrimary : Theme.TextSecondary;
            if (inTrips > 0) ButtonStyles.Primary(_inButton); else ButtonStyles.Secondary(_inButton);
            if (outTrips > 0) ButtonStyles.Primary(_outButton); else ButtonStyles.Secondary(_outButton);
            _inButton.Text = inTrips > 0 ? "Nhập hàng ngay →" : "Mở danh sách →";
            _outButton.Text = outTrips > 0 ? "Xuất kho ngay →" : "Mở danh sách →";
        }
    }
}
