using System.Runtime.CompilerServices;

namespace TruckingSystem.Desktop.Common
{
    /// <summary>BÁO LỖI NGAY TẠI Ô NHẬP: bấm Lưu mà ô bắt buộc trống / sai -> ô đó nền đỏ nhạt + biểu tượng ⚠ bên cạnh
    /// (rê chuột xem lý do), con trỏ nhảy vào đúng ô, dòng trạng thái ghi lý do màu đỏ. Sửa ô là tự hết đỏ.
    /// Trước đây chỉ có 1 dòng chữ ở cuối form, nhân viên phải tự dò xem ô nào sai.
    /// Dùng: FieldHint.Show(txtPlateNumber, lblStatus, "Vui lòng nhập biển số xe."); return;</summary>
    public static class FieldHint
    {
        private static readonly Color ErrorBack = Color.FromArgb(254, 226, 226);
        private static readonly ConditionalWeakTable<Form, ErrorProvider> Providers = new();
        private static readonly ConditionalWeakTable<Control, object> Hooked = new();
        private static readonly ConditionalWeakTable<Control, object> OriginalBack = new();

        public static void Show(Control field, Label? status, string message)
        {
            if (status is not null)
            {
                status.Text = "⚠️ " + message;
                status.ForeColor = Theme.Danger;
            }
            var form = field.FindForm();
            if (form is not null)
            {
                var ep = Providers.GetValue(form, f =>
                {
                    var p = new ErrorProvider { BlinkStyle = ErrorBlinkStyle.NeverBlink };
                    f.Disposed += (_, _) => p.Dispose();
                    return p;
                });
                ep.SetIconAlignment(field, ErrorIconAlignment.MiddleRight);
                ep.SetIconPadding(field, 2);
                ep.SetError(field, message);
            }
            if (!OriginalBack.TryGetValue(field, out _)) OriginalBack.Add(field, field.BackColor);
            field.BackColor = ErrorBack;
            Hook(field);
            field.Focus();
            if (field is TextBox tb) tb.SelectAll();
        }

        /// <summary>Gỡ đánh dấu lỗi của 1 ô (tự gọi khi người dùng sửa ô).</summary>
        public static void Clear(Control field)
        {
            if (OriginalBack.TryGetValue(field, out var back) && back is Color c) field.BackColor = c;
            if (field.FindForm() is { } form && Providers.TryGetValue(form, out var ep)) ep.SetError(field, "");
        }

        private static void Hook(Control field)
        {
            if (Hooked.TryGetValue(field, out _)) return;
            Hooked.Add(field, new object());
            switch (field)
            {
                case ComboBox cb: cb.SelectedIndexChanged += (_, _) => Clear(cb); cb.TextChanged += (_, _) => Clear(cb); break;
                case NumericUpDown nu: nu.ValueChanged += (_, _) => Clear(nu); break;
                case DateTimePicker dp: dp.ValueChanged += (_, _) => Clear(dp); break;
                default: field.TextChanged += (_, _) => Clear(field); break;
            }
        }
    }
}
