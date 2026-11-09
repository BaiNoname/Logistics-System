namespace TruckingSystem.Desktop.GUI_VehicleManagement
{
    /// <summary>Danh mục "Loại xe" cho form Thêm/Sửa xe — chọn từ dropdown thay vì
    /// gõ tay (tránh mỗi người gõ 1 kiểu: "xe tai 5T", "Xe tải 5 tấn"...). Chọn loại
    /// xe sẽ tự điền TẢI TRỌNG (kg) và THỂ TÍCH thùng (m³) gợi ý — vẫn sửa lại được
    /// theo thông số thực tế của từng xe.</summary>
    public static class VehicleTypeOptions
    {
        public sealed record Preset(string Name, decimal CapacityKg, decimal CapacityM3);

        // Thông số tham khảo phổ biến tại VN (tải trọng cho phép chở hàng / thể tích thùng).
        public static readonly Preset[] Presets =
        {
            new("Xe van 1 tấn", 1000, 5),
            new("Xe tải 1,5 tấn", 1500, 8),
            new("Xe tải 2,5 tấn", 2500, 12),
            new("Xe tải 5 tấn", 5000, 25),
            new("Xe tải 8 tấn", 8000, 35),
            new("Xe tải 15 tấn", 15000, 50),
            new("Đầu kéo container 20 feet", 21000, 33),
            new("Đầu kéo container 40 feet", 26000, 67),
        };

        /// <summary>Đổ dropdown. Xe cũ đang có loại KHÔNG nằm trong danh mục (gõ tay
        /// trước đây) -> vẫn thêm vào để không mất dữ liệu.</summary>
        public static void Fill(ComboBox combo, string? current)
        {
            combo.Items.Clear();
            foreach (var p in Presets) combo.Items.Add(p.Name);

            var cur = (current ?? "").Trim();
            if (cur.Length == 0)
            {
                combo.SelectedItem = "Xe tải 5 tấn";
                return;
            }
            var match = Presets.FirstOrDefault(p => string.Equals(p.Name, cur, StringComparison.OrdinalIgnoreCase))?.Name;
            if (match is null)
            {
                combo.Items.Add(cur);
                match = cur;
            }
            combo.SelectedItem = match;
        }

        public static Preset? Find(string? name)
            => Presets.FirstOrDefault(p => string.Equals(p.Name, (name ?? "").Trim(), StringComparison.OrdinalIgnoreCase));
    }
}
