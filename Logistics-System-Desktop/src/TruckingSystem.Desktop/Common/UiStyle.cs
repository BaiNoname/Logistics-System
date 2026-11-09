using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace TruckingSystem.Desktop.Common
{
    /// <summary>Kiểu hiển thị CHUNG tự áp cho mọi màn hình (gọi từ BaseForm / BaseUserControl
    /// khi mở) — không phải nhớ gọi ở từng form:
    ///  - Bảng (DataGridView) chưa định dạng: giãn cột kín bề ngang (dù ít cột), nền trắng,
    ///    tiêu đề cột đậm, "Loại_nhiên_liệu" -> "Loại nhiên liệu". Chỉ đổi PHẦN NHÌN, không đổi
    ///    hành vi (sửa được / chọn nhiều dòng... giữ nguyên như form đã thiết lập).
    ///  - Ô chọn ngày (DateTimePicker) kiểu Ngắn/Dài: hiện "dd/MM/yyyy" (12/10/2026) thay vì
    ///    theo định dạng máy (12-Oct-26).</summary>
    public static class UiStyle
    {
        public const string DateFormat = "dd/MM/yyyy";

        public static void ApplyDefaults(Control root)
        {
            foreach (var c in Walk(root))
            {
                switch (c)
                {
                    case DataGridView grid:
                        Theme.StyleGridLook(grid);
                        break;
                    case DateTimePicker dtp when dtp.Format is DateTimePickerFormat.Short or DateTimePickerFormat.Long:
                        dtp.Format = DateTimePickerFormat.Custom;
                        dtp.CustomFormat = DateFormat;
                        break;
                }
            }
        }

        private static IEnumerable<Control> Walk(Control root)
        {
            foreach (Control c in root.Controls)
            {
                yield return c;
                foreach (var child in Walk(c)) yield return child;
            }
        }

        // Đánh dấu bảng đã định dạng — gọi lại không gắn thêm sự kiện lần 2.
        private static readonly ConditionalWeakTable<DataGridView, object> Styled = new();
        internal static bool MarkStyled(DataGridView grid)
        {
            if (Styled.TryGetValue(grid, out _)) return false;
            Styled.Add(grid, new object());
            return true;
        }
    }
}
