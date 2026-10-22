namespace TruckingSystem.Desktop.Common
{
    /// <summary>Dropdown "Kho đóng quân" (tuỳ chọn) dùng chung cho Xe + Hồ sơ tài xế.
    /// Đổ bằng Items (ComboOption), KHÔNG dùng DataSource + ValueMember: dòng "Không cố định" có
    /// Id = null làm ComboBox ném "Value cannot be null (Parameter 'key')" — dropdown không nạp được,
    /// form lại lưu kho cũ (trống) nên chọn kho xong lưu vẫn ra "mọi kho".</summary>
    public static class WarehouseOptions
    {
        public const string NoneText = "(Không cố định — dùng được mọi kho)";

        /// <summary>Nạp danh sách kho và chọn sẵn <paramref name="selectedId"/>. Lỗi thì ghi vào
        /// <paramref name="status"/> và trả false (người gọi giữ nguyên kho cũ khi lưu).</summary>
        public static async Task<bool> FillAsync(ComboBox combo, ApiServices api, int? selectedId, Label? status)
        {
            try
            {
                var warehouses = await api.Warehouses.GetWarehousesAsync();
                combo.BeginUpdate();
                try
                {
                    combo.DataSource = null;
                    combo.Items.Clear();
                    combo.Items.Add(new ComboOption("", NoneText));
                    foreach (var w in warehouses.OrderBy(w => w.Name))
                        combo.Items.Add(new ComboOption(w.Id.ToString(), w.Name));
                }
                finally { combo.EndUpdate(); }
                Select(combo, selectedId);
                return true;
            }
            catch (Exception ex)
            {
                if (status is not null) status.Text = "Không tải được danh sách kho: " + ex.Message;
                return false;
            }
        }

        public static void Select(ComboBox combo, int? warehouseId)
        {
            var key = warehouseId?.ToString() ?? "";
            var match = combo.Items.OfType<ComboOption>().FirstOrDefault(o => o.Value == key);
            if (match is null && combo.Items.Count == 0) return;
            combo.SelectedItem = match ?? combo.Items[0];
        }

        /// <summary>Kho đang chọn; null = "Không cố định".</summary>
        public static int? Selected(ComboBox combo)
            => combo.SelectedItem is ComboOption o && int.TryParse(o.Value, out var id) ? id : null;
    }
}
