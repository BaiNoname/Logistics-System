using TruckingSystem.Desktop.Common;

namespace TruckingSystem.Desktop.GUI_VehicleManagement
{
    /// <summary>Danh sách "Loại nhiên liệu" cho form Thêm/Sửa xe — lấy từ BẢNG GIÁ
    /// ở màn Nhiên liệu (các loại đã có giá), KHÔNG gán cứng. Nhờ vậy loại nhiên
    /// liệu của xe luôn khớp đúng tên trong bảng giá -> ghi đổ xăng / ước tính
    /// chi phí tự tìm được đơn giá.</summary>
    public static class FuelTypeOptions
    {
        // Chỉ dùng khi KHÔNG gọi được API (mất mạng) để form vẫn dùng được.
        private static readonly string[] Fallback = { "Dầu DO", "Xăng RON95", "Xăng RON92", "Xăng E5" };

        public static async Task FillAsync(ComboBox combo, ApiServices api, string? current, Label? statusLabel = null)
        {
            List<string> types;
            try
            {
                // Chỉ các loại ĐÃ CÓ GIÁ — chọn loại chưa có giá thì ghi xăng sẽ không
                // tìm được đơn giá. (Endpoint mở cho mọi tài khoản, kể cả Dispatcher.)
                types = await api.Connection.GetAsync<List<string>>("/api/fuel/types?pricedOnly=true") ?? new();
                if (types.Count == 0 && statusLabel != null)
                    statusLabel.Text = "Bảng giá nhiên liệu chưa có loại nào — vào màn Nhiên liệu để thêm giá trước.";
            }
            catch
            {
                types = Fallback.ToList();
            }

            combo.Items.Clear();
            combo.Items.Add("");
            foreach (var t in types) combo.Items.Add(t);

            // Xe đang dùng loại KHÔNG còn trong bảng giá -> vẫn hiện để không mất
            // dữ liệu, kèm cảnh báo (ghi xăng sẽ không tìm được đơn giá).
            var cur = (current ?? "").Trim();
            if (cur.Length > 0)
            {
                var match = types.FirstOrDefault(t => string.Equals(t, cur, StringComparison.OrdinalIgnoreCase));
                if (match is null)
                {
                    combo.Items.Add(cur);
                    match = cur;
                    if (statusLabel != null)
                        statusLabel.Text = $"Loại nhiên liệu \"{cur}\" của xe chưa có trong bảng giá — nên chọn lại cho khớp.";
                }
                combo.SelectedItem = match;
            }
            else
            {
                combo.SelectedIndex = 0;
            }
        }
    }
}
