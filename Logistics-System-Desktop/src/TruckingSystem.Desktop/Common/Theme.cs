using System.Drawing;
using System.Windows.Forms;

namespace TruckingSystem.Desktop.Common
{
    /// <summary>Bảng màu + font dùng chung cho toàn bộ ứng dụng.</summary>
    public static class Theme
    {
        public static readonly Color SidebarBg = ColorTranslator.FromHtml("#1E293B");
        public static readonly Color SidebarBgActive = ColorTranslator.FromHtml("#2563EB");
        public static readonly Color SidebarText = ColorTranslator.FromHtml("#CBD5E1");
        public static readonly Color SidebarTextActive = Color.White;

        public static readonly Color ContentBg = ColorTranslator.FromHtml("#F1F5F9");
        public static readonly Color CardBg = Color.White;
        public static readonly Color Primary = ColorTranslator.FromHtml("#2563EB");
        public static readonly Color PrimaryDark = ColorTranslator.FromHtml("#1D4ED8");
        public static readonly Color Success = ColorTranslator.FromHtml("#16A34A");
        public static readonly Color Danger = ColorTranslator.FromHtml("#DC2626");
        public static readonly Color Warning = ColorTranslator.FromHtml("#D97706");
        public static readonly Color TextPrimary = ColorTranslator.FromHtml("#0F172A");
        public static readonly Color TextSecondary = ColorTranslator.FromHtml("#64748B");
        public static readonly Color BorderColor = ColorTranslator.FromHtml("#E2E8F0");
        public static readonly Color GridHeaderBg = ColorTranslator.FromHtml("#1E293B");
        public static readonly Color GridAltRow = ColorTranslator.FromHtml("#F8FAFC");

        public static readonly Font FontTitle = new("Segoe UI", 18, FontStyle.Bold);
        public static readonly Font FontSectionTitle = new("Segoe UI", 13, FontStyle.Bold);
        public static readonly Font FontNormal = new("Segoe UI", 10);
        public static readonly Font FontBold = new("Segoe UI", 10, FontStyle.Bold);
        public static readonly Font FontSmall = new("Segoe UI", 9);
        public static readonly Font FontKpiNumber = new("Segoe UI", 26, FontStyle.Bold);
        public static readonly Font FontSidebar = new("Segoe UI", 10.5f);

        public static Button MakePrimaryButton(string text, int width = 140, int height = 38)
        {
            var btn = new Button
            {
                Text = text, Width = width, Height = height,
                BackColor = Primary, ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Font = FontBold, Cursor = Cursors.Hand,
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = PrimaryDark;
            return btn;
        }

        public static Button MakeSecondaryButton(string text, int width = 140, int height = 38)
        {
            var btn = new Button
            {
                Text = text, Width = width, Height = height,
                BackColor = Color.White, ForeColor = TextPrimary,
                FlatStyle = FlatStyle.Flat, Font = FontBold, Cursor = Cursors.Hand,
            };
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.BorderColor = BorderColor;
            btn.FlatAppearance.MouseOverBackColor = GridAltRow;
            return btn;
        }

        /// <summary>Định dạng ĐẦY ĐỦ cho bảng danh sách (chỉ xem): phần nhìn + chỉ đọc, chọn
        /// nguyên dòng, 1 dòng mỗi lần, không có dòng trống cuối bảng.</summary>
        /// <summary>Các nút trên thanh công cụ CÙNG BỀ NGANG, CÙNG CHIỀU CAO, chữ căn giữa — khi màn
        /// hẹp phải xuống dòng thì các nút vẫn thẳng cột như 1 lưới. Gọi trong sự kiện Load (sau khi
        /// WinForms đã co giãn theo DPI, tránh nhân đôi kích thước); truyền nhiều thanh để các thanh
        /// cùng màn có nút rộng bằng nhau.</summary>
        public static void UniformButtons(params Control[] bars)
        {
            var buttons = bars.SelectMany(b => b.Controls.OfType<Button>()).ToList();
            if (buttons.Count < 2) return;
            var width = buttons.Max(b => b.GetPreferredSize(Size.Empty).Width);
            var height = buttons.Max(b => Math.Max(b.MinimumSize.Height, b.GetPreferredSize(Size.Empty).Height));
            foreach (var b in buttons)
            {
                b.MinimumSize = new Size(width, height);
                b.TextAlign = ContentAlignment.MiddleCenter;
            }
        }

        public static void StyleGrid(DataGridView grid)
        {
            StyleGridLook(grid);
            grid.AllowUserToAddRows = false;
            grid.AllowUserToResizeRows = false;
            grid.ReadOnly = true;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false;
        }

        /// <summary>Chỉ PHẦN NHÌN của bảng (dùng chung, tự áp cho mọi bảng khi form mở — xem
        /// UiStyle): cột giãn kín bề ngang dù ít cột (không còn mảng xám trống bên phải), nền
        /// trắng, tiêu đề đậm, tiêu đề cột "_" -> " ". Gọi nhiều lần cũng chỉ áp 1 lần.</summary>
        public static void StyleGridLook(DataGridView grid)
        {
            if (!UiStyle.MarkStyled(grid)) return;

            grid.BorderStyle = BorderStyle.None;
            grid.BackgroundColor = Color.White;
            grid.GridColor = BorderColor;
            grid.RowHeadersVisible = false;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            grid.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.True;
            grid.RowTemplate.Height = 36;
            grid.EnableHeadersVisualStyles = false;

            grid.ColumnHeadersDefaultCellStyle.BackColor = GridHeaderBg;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = GridHeaderBg;
            grid.ColumnHeadersDefaultCellStyle.Font = FontBold;
            grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 6, 4, 6);

            grid.DefaultCellStyle.Font = FontNormal;
            grid.DefaultCellStyle.ForeColor = TextPrimary;
            grid.DefaultCellStyle.SelectionBackColor = ColorTranslator.FromHtml("#DBEAFE");
            grid.DefaultCellStyle.SelectionForeColor = TextPrimary;
            grid.DefaultCellStyle.Padding = new Padding(8, 0, 4, 0);
            grid.AlternatingRowsDefaultCellStyle.BackColor = GridAltRow;

            // Tiêu đề cột lấy từ tên thuộc tính C# ("Mã_đơn") -> đổi "_" thành khoảng trắng mỗi
            // khi bảng nhận dữ liệu mới.
            FixHeaders(grid);
            grid.DataBindingComplete += (_, _) => FixHeaders(grid);
        }

        private static void FixHeaders(DataGridView grid)
        {
            foreach (DataGridViewColumn col in grid.Columns)
                col.HeaderText = col.HeaderText.Replace('_', ' ');
        }

        public static Panel MakeLabeledInput(string label, Control input, int width = 320)
        {
            var panel = new Panel { Width = width, Height = 56, Margin = new Padding(0, 0, 0, 8) };
            var lbl = new Label { Text = label, Font = FontSmall, ForeColor = TextSecondary, Top = 0, Left = 0, AutoSize = true };
            input.Top = 20; input.Left = 0; input.Width = width;
            panel.Controls.Add(lbl);
            panel.Controls.Add(input);
            return panel;
        }

        public static Color StatusColor(string status) => status switch
        {
            "Done" or "Paid" or "Available" or "Active" or "Completed" => Success,
            "Cancelled" or "Overdue" or "OutOfService" or "Terminated" => Danger,
            "InProgress" or "PartiallyPaid" or "OnTrip" => Warning,
            _ => TextSecondary,
        };
    }
}
