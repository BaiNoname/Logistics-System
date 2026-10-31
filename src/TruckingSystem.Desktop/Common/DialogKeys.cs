namespace TruckingSystem.Desktop.Common
{
    /// <summary>PHÍM TẮT cho cửa sổ phụ: Esc = đóng cửa sổ (như bấm ✕), Enter = bấm nút Lưu (chỉ bật cho form nhập ngắn,
    /// không có ô nhiều dòng). Trước đây phần lớn cửa sổ phụ phải cầm chuột bấm ✕ / Lưu.
    /// Dùng: DialogKeys.Enable(this) hoặc DialogKeys.Enable(this, btnSave) — gọi ngay sau InitializeComponent().</summary>
    public static class DialogKeys
    {
        public static void Enable(Form form, Button? accept = null)
        {
            if (accept is not null && form.AcceptButton is null) form.AcceptButton = accept;
            if (form.CancelButton is not null) return; // đã có nút Hủy / Đóng gắn Esc sẵn
            form.KeyPreview = true;
            form.KeyDown += (_, e) =>
            {
                if (e.KeyCode != Keys.Escape) return;
                // Esc đang dùng để đóng ô xổ xuống thì để ô đó xử lý, không đóng cả cửa sổ.
                if (form.ActiveControl is ComboBox { DroppedDown: true }) return;
                e.Handled = e.SuppressKeyPress = true;
                form.DialogResult = DialogResult.Cancel;
                form.Close();
            };
        }
    }
}
