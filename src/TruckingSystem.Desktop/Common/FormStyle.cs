namespace TruckingSystem.Desktop.Common
{
    /// <summary>Áp nhanh các quy tắc trong design.md cho cửa sổ nhập liệu — dùng chung để mọi form trông giống nhau:
    ///  • Section: tiêu đề khối 11 đậm, màu Primary.
    ///  • FieldLabels: nhãn ô nhập màu chữ chính (đủ tương phản).
    ///  • SaveButton / CloseButton: nút chính "💾 …" và nút "Đóng" (gắn Esc) theo ButtonStyles.
    ///  • Status: dòng trạng thái luôn đặt lại đúng màu theo loại thông báo (không bị kẹt màu cam / xám cũ).
    ///  • NoTab: ô chỉ để xem không nhận phím Tab.</summary>
    public static class FormStyle
    {
        public static readonly Font SectionFont = new("Segoe UI", 11F, FontStyle.Bold);

        public static void Section(params Label[] labels)
        {
            foreach (var l in labels)
            {
                l.Font = SectionFont;
                l.ForeColor = Theme.Primary;
            }
        }

        public static void FieldLabels(params Label[] labels)
        {
            foreach (var l in labels) l.ForeColor = Theme.TextPrimary;
        }

        /// <summary>Nút chính của cửa sổ: nền xanh + biểu tượng lưu ở đầu chữ.</summary>
        public static void SaveButton(Button b)
        {
            ButtonStyles.Primary(b);
            b.Font = Theme.FontBold;
            if (!b.Text.StartsWith("💾")) b.Text = "💾 " + b.Text;
        }

        /// <summary>Đổi nút Hủy / Đóng sẵn có thành nút phụ "Đóng" và gắn phím Esc.</summary>
        public static void CloseButton(Form form, Button b, string text = "Đóng")
        {
            ButtonStyles.Secondary(b);
            b.Font = Theme.FontBold;
            b.Text = text;
            b.DialogResult = DialogResult.Cancel;
            form.CancelButton = b;
        }

        /// <summary>Tạo nút "Đóng" mới (nút phụ, gắn Esc).</summary>
        public static Button NewCloseButton(Form form, string text = "Đóng")
        {
            var b = new Button
            {
                AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
                MinimumSize = new Size(96, 36), Padding = new Padding(10, 4, 10, 4),
            };
            CloseButton(form, b, text);
            return b;
        }

        public static void Status(Label status, string text, StatusKind kind = StatusKind.Error)
        {
            status.ForeColor = kind switch
            {
                StatusKind.Warning => Theme.Warning,
                StatusKind.Info => Theme.TextSecondary,
                StatusKind.Success => Theme.Success,
                _ => Theme.Danger,
            };
            status.Text = text;
        }

        public static void NoTab(params Control[] controls)
        {
            foreach (var c in controls) c.TabStop = false;
        }
    }

    public enum StatusKind { Error, Warning, Info, Success }
}
