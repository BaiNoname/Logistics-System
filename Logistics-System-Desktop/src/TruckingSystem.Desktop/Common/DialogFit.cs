namespace TruckingSystem.Desktop.Common
{
    /// <summary>CỬA SỔ VỪA MÀN HÌNH, KHÔNG MẤT CHỮ — dùng cho mọi form có bảng (Chi tiết đơn, Xuất kho, Tồn kho…):
    ///  • Lúc mở (và khi gọi Grow() sau khi tải dữ liệu / hiện thêm khung cảnh báo): nếu nội dung cần cao hơn cửa sổ thì
    ///    TỰ NỚI cửa sổ, nhưng không vượt vùng làm việc của màn hình (laptop 1366×768 vẫn thấy đủ nút dưới cùng).
    ///  • Cửa sổ thiết kế cao hơn màn hình (VD 760 px trên laptop 768) thì tự thu lại cho vừa và căn giữa.
    ///  • Vẫn không đủ chỗ: mỗi bảng giữ tối thiểu tiêu đề + 3 dòng, phần thiếu thì CUỘN cả cửa sổ — không còn bảng
    ///    bị ép chỉ còn dòng tiêu đề, chữ bị cắt.
    /// Hàng "giãn" (Percent) của bảng bố cục root được coi là phần chính; hàng đang ẩn (cao 0) bỏ qua.
    /// Dùng: DialogFit.Enable(this, root) ngay sau InitializeComponent(); sau khi tải dữ liệu có thể gọi lại Grow.</summary>
    public static class DialogFit
    {
        private const int MinGridRows = 3;

        /// <param name="scrollHost">Khung chứa root (Dock Fill) cần cuộn thay cho cả cửa sổ — để chân cửa sổ (Dock Bottom,
        /// nút Lưu…) đứng yên, luôn thấy. Bỏ trống = cuộn cả cửa sổ như cũ.</param>
        public static Action Enable(Form form, TableLayoutPanel root, ScrollableControl? scrollHost = null)
        {
            var busy = false;

            int MinFor(Control c)
            {
                var min = c is DataGridView g
                    ? g.ColumnHeadersHeight + g.RowTemplate.Height * MinGridRows + SystemInformation.HorizontalScrollBarHeight + 4
                    : form.LogicalToDeviceUnits(120);
                return Math.Max(min, c.MinimumSize.Height) + c.Margin.Vertical;
            }

            /// <summary>Chiều cao vùng khách cần để mọi hàng hiện đủ (phần chính ở mức tối thiểu).</summary>
            int Needed() => Inner() + Outside();
            /// <summary>Phần ngoài root (chân cửa sổ Dock Bottom, thanh trên…).</summary>
            int Outside()
            {
                Control top = root;
                while (top.Parent is not null && top.Parent != form) top = top.Parent;
                return form.Controls.Cast<Control>()
                    .Where(c => c != top && c.Visible && (c.Dock == DockStyle.Top || c.Dock == DockStyle.Bottom))
                    .Sum(c => c.Height);
            }
            /// <summary>Chiều cao riêng root cần.</summary>
            int Inner()
            {
                var heights = root.GetRowHeights();
                var mainRows = new HashSet<int>();
                var mainMin = 0;
                foreach (Control c in root.Controls)
                {
                    if (!c.Visible) continue;
                    var row = root.GetRow(c);
                    if (row < 0 || row >= root.RowStyles.Count || root.RowStyles[row].SizeType != SizeType.Percent) continue;
                    if (mainRows.Add(row)) mainMin += MinFor(c);
                }
                var others = 0;
                for (var i = 0; i < heights.Length; i++) if (!mainRows.Contains(i)) others += heights[i];
                return others + mainMin + root.Padding.Vertical + root.Margin.Vertical;
            }

            void EnsureScroll()
            {
                if (busy || form.IsDisposed) return;
                busy = true;
                try
                {
                    ScrollableControl sc = scrollHost ?? form;
                    var need = scrollHost is null ? Needed() : Inner();
                    var want = need > sc.ClientSize.Height + 1 ? new Size(0, need) : Size.Empty;
                    if (sc.AutoScrollMinSize != want)
                    {
                        sc.AutoScroll = want != Size.Empty || sc.AutoScroll;
                        sc.AutoScrollMinSize = want;
                    }
                }
                finally { busy = false; }
            }

            void Grow()
            {
                if (form.IsDisposed || form.WindowState != FormWindowState.Normal) return;
                var area = Screen.FromControl(form).WorkingArea;
                var gap = form.LogicalToDeviceUnits(8);
                var maxH = area.Height - 2 * gap;
                var maxW = area.Width - 2 * gap;
                var nonClient = form.Height - form.ClientSize.Height;
                var want = Math.Min(Needed() + nonClient, maxH);
                var moved = false;
                if (form.Height < want) { form.Height = want; moved = true; }
                if (form.Height > maxH) { form.Height = maxH; moved = true; }
                if (form.Width > maxW) { form.Width = maxW; moved = true; }
                if (moved || form.Bottom > area.Bottom || form.Top < area.Top)
                {
                    form.Top = Math.Max(area.Top + gap, area.Top + (area.Height - form.Height) / 2);
                    form.Left = Math.Max(area.Left + gap, Math.Min(form.Left, area.Right - form.Width - gap));
                }
                EnsureScroll();
            }

            form.Load += (_, _) => { if (form.IsHandleCreated) form.BeginInvoke(new Action(Grow)); };
            root.Layout += (_, _) => EnsureScroll();
            form.Resize += (_, _) => EnsureScroll();
            return () => { if (!form.IsDisposed && form.IsHandleCreated) form.BeginInvoke(new Action(Grow)); };
        }
    }
}
