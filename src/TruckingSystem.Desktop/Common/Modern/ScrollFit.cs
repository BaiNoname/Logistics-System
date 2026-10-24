namespace TruckingSystem.Desktop.Common
{
    /// <summary>Cho màn hình danh sách CUỘN ĐƯỢC khi cửa sổ thấp: thanh thẻ số liệu + thanh nút + bộ lọc
    /// chiếm gần hết chiều cao thì bảng (hàng Percent 100% của TableLayoutPanel) bị ép còn 0 px — trước
    /// đây không kéo xuống xem được. Giờ màn hình giữ cho bảng tối thiểu ~N dòng; thiếu chỗ thì cả màn
    /// hiện thanh cuộn dọc (AutoScrollMinSize) để kéo xuống xem bảng.</summary>
    public static class ScrollFit
    {
        /// <param name="host">UserControl chứa root (Dock = Fill).</param>
        /// <param name="root">TableLayoutPanel bố cục chính.</param>
        /// <param name="main">Phần chính chiếm hàng 100% (bảng / sơ đồ kho).</param>
        /// <param name="minMainHeight">Chiều cao tối thiểu của phần chính (px); null = 8 dòng của bảng.</param>
        public static void Enable(ScrollableControl host, TableLayoutPanel root, Control main, int? minMainHeight = null)
        {
            host.AutoScroll = true;
            var busy = false;

            void Update()
            {
                if (busy || host.IsDisposed) return;
                busy = true;
                try
                {
                    var mainRow = root.GetRow(main);
                    var heights = root.GetRowHeights();
                    var others = 0;
                    for (var i = 0; i < heights.Length; i++)
                        if (i != mainRow) others += heights[i];
                    var minMain = minMainHeight ?? (main is DataGridView g
                        ? g.ColumnHeadersHeight + g.RowTemplate.Height * 8 + SystemInformation.HorizontalScrollBarHeight
                        : 320);
                    var needed = others + minMain + main.Margin.Vertical + root.Padding.Vertical + root.Margin.Vertical;
                    // Chỉ cần cuộn khi khung nhìn thấp hơn mức tối thiểu; không cuộn ngang.
                    var want = new Size(0, needed);
                    if (host.AutoScrollMinSize != want) host.AutoScrollMinSize = want;
                }
                finally { busy = false; }
            }

            root.Layout += (_, _) => Update();
            host.Resize += (_, _) => Update();
            OnReady(host, Update);
        }

        private static void OnReady(ScrollableControl host, Action action)
        {
            if (host is UserControl uc) uc.Load += (_, _) => action();
            else host.HandleCreated += (_, _) => action();
        }
    }
}
