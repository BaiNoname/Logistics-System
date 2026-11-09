using System.Drawing;
using System.Windows.Forms;
using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_WarehouseManagement;

namespace TruckingSystem.Desktop.GUI_WarehouseManagement
{
    /// <summary>Gắn sơ đồ xem trước (PlacementHelper) vào 4 form Thêm / Sửa khu, kệ + đổi nhãn tọa độ dễ hiểu:
    /// "Vị trí X / Y" -> "Cách tường trái / trên (m)" (khu) hoặc "Cách mép trái / trên của khu (m)" (kệ).</summary>
    internal static class PlacementSetup
    {
        public static void Zone(Form form, TableLayoutPanel inputs, NumericUpDown x, NumericUpDown y, NumericUpDown w, NumericUpDown h,
            Label lblX, Label lblY, Label lblW, Label lblH, Label lblNote, Button save, Func<string> name,
            ApiServices? api, int warehouseId, int? excludeZoneId)
        {
            lblX.Text = "Cách tường trái (m)";
            lblY.Text = "Cách tường trên (m)";
            lblW.Text = "Rộng (m) * — theo chiều ngang sơ đồ";
            lblH.Text = "Dài (m) * — theo chiều dọc sơ đồ";
            lblNote.Text = "💡 Sơ đồ bên dưới vẽ lại ngay khi bạn gõ số — khung đỏ là đang đè khu khác hoặc tràn ra ngoài kho.";
            if (api is null || warehouseId <= 0) return;
            var helper = new PlacementHelper(form, inputs, x, y, w, h, save, "kho", () => string.IsNullOrWhiteSpace(name()) ? "Khu mới" : name());
            form.Load += async (_, _) =>
            {
                try
                {
                    var warehouse = (await api.Warehouses.GetWarehousesAsync()).FirstOrDefault(wh => wh.Id == warehouseId);
                    var zones = await api.Warehouses.GetZonesAsync(warehouseId);
                    if (form.IsDisposed) return;
                    // Quy ước sơ đồ: X theo chiều DÀI kho, Y theo chiều RỘNG kho. Kho chưa khai kích thước -> lấy theo khu xa nhất.
                    var areaW = warehouse?.LengthMeters ?? zones.Select(z => z.PosX + z.WidthMeters).DefaultIfEmpty(30).Max();
                    var areaH = warehouse?.WidthMeters ?? zones.Select(z => z.PosY + z.HeightMeters).DefaultIfEmpty(20).Max();
                    helper.SetData(areaW, areaH, $"{warehouse?.Name ?? "Kho"} — {areaW:0.##} × {areaH:0.##} m",
                        zones.Where(z => z.Id != excludeZoneId).Select(z => new LayoutPreview.Box(z.Name,
                            new RectangleF((float)z.PosX, (float)z.PosY, (float)z.WidthMeters, (float)z.HeightMeters), LayoutPreview.ZoneColor(z.ZoneType))));
                }
                catch { /* không tải được sơ đồ: form vẫn dùng bình thường, server vẫn kiểm tra khi lưu */ }
            };
        }

        public static void Bin(Form form, TableLayoutPanel inputs, NumericUpDown x, NumericUpDown y, NumericUpDown w, NumericUpDown h,
            Label lblX, Label lblY, Label lblW, Label lblH, Button save, Func<string> code,
            ApiServices? api, int zoneId, string zoneName, decimal zoneW, decimal zoneH, int? excludeBinId)
        {
            lblX.Text = "Cách mép trái của khu (m)";
            lblY.Text = "Cách mép trên của khu (m)";
            lblW.Text = "Rộng kệ (m) *";
            lblH.Text = "Dài kệ (m) *";
            if (api is null || zoneId <= 0) return;
            var helper = new PlacementHelper(form, inputs, x, y, w, h, save, $"khu \"{zoneName}\"",
                () => string.IsNullOrWhiteSpace(code()) ? "Kệ mới" : code());
            form.Load += async (_, _) =>
            {
                try
                {
                    var bins = await api.Warehouses.GetBinsAsync(zoneId) ?? new List<StorageBinItem>();
                    if (form.IsDisposed) return;
                    helper.SetData(zoneW, zoneH, $"{zoneName} — {zoneW:0.##} × {zoneH:0.##} m",
                        bins.Where(b => b.Id != excludeBinId).Select(b => new LayoutPreview.Box(b.Code,
                            new RectangleF((float)b.PosX, (float)b.PosY, (float)b.WidthMeters, (float)b.HeightMeters),
                            LayoutPreview.BinColor(b.CurrentUsedM3, b.CapacityM3))));
                }
                catch { }
            };
        }
    }
}
