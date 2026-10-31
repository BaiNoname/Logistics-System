using System.Runtime.CompilerServices;

namespace TruckingSystem.Desktop.Common
{
    /// <summary>Bố cục BẢNG dùng chung (gọi 1 lần trong constructor, sau Theme.StyleGrid):
    ///
    /// • <see cref="FitColumns"/> — cột rộng VỪA ĐỦ nội dung (không còn "51C-123...." bị cắt), tiêu đề
    ///   1 dòng; ít cột thì giãn đều cho kín bề ngang, nhiều cột thì hiện thanh cuộn ngang thay vì
    ///   bóp nhỏ. Cột quá dài (VD lý do chặn) bị giới hạn bề ngang — rê chuột vào ô để xem đủ.
    ///
    /// • <see cref="GrowWithRows"/> — bảng CAO THEO SỐ DÒNG (không có thanh cuộn dọc riêng): dùng
    ///   cho màn có nhiều bảng xếp chồng, cả màn hình cuộn xuống như 1 trang. Lăn chuột trên bảng
    ///   sẽ cuộn cả trang.</summary>
    public static class GridLayout
    {
        private sealed class FitState
        {
            public int[] Natural = Array.Empty<int>();
            public int LastWidth = -1;
            public bool Busy;
        }

        private static readonly ConditionalWeakTable<DataGridView, FitState> Fits = new();
        private static readonly ConditionalWeakTable<DataGridView, object> Grows = new();

        // ------------------------------------------------------------------ cột

        public static void FitColumns(DataGridView grid)
        {
            if (Fits.TryGetValue(grid, out _)) return;
            var state = new FitState();
            Fits.Add(grid, state);
            grid.AllowUserToResizeColumns = true;
            grid.DataBindingComplete += (_, _) => Measure(grid, state);
            grid.ColumnStateChanged += (_, e) => { if (e.StateChanged == DataGridViewElementStates.Visible) Measure(grid, state); };
            grid.SizeChanged += (_, _) =>
            {
                if (grid.ClientSize.Width == state.LastWidth) return;
                Distribute(grid, state);
            };
        }

        /// <summary>Đo lại bề ngang tự nhiên của từng cột (sau khi nạp dữ liệu mới).</summary>
        public static void Refit(DataGridView grid)
        {
            if (Fits.TryGetValue(grid, out var state)) Measure(grid, state);
        }

        private static DataGridViewColumn[] VisibleColumns(DataGridView grid)
            => grid.Columns.Cast<DataGridViewColumn>().Where(c => c.Visible).OrderBy(c => c.DisplayIndex).ToArray();

        private static void Measure(DataGridView grid, FitState state)
        {
            if (state.Busy) return;
            state.Busy = true;
            try
            {
                grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                grid.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;
                var cols = VisibleColumns(grid);
                var max = grid.Font.Height * 22; // ~ 400 px ở 100% — đo theo font nên đúng mọi mức phóng to
                var min = grid.Font.Height * 3;
                // Bảng NHIỀU dòng (nhật ký, lịch sử...): chỉ đo các dòng đang hiện + tiêu đề — đo hết vài nghìn dòng mỗi lần
                // gõ lọc làm màn giật. Bảng ít dòng vẫn đo toàn bộ cho chính xác.
                var mode = grid.RowCount > 300 ? DataGridViewAutoSizeColumnMode.DisplayedCells : DataGridViewAutoSizeColumnMode.AllCells;
                state.Natural = cols
                    .Select(c => Math.Max(c.MinimumWidth, Math.Clamp(Math.Max(c.GetPreferredWidth(mode, true),
                        c.GetPreferredWidth(DataGridViewAutoSizeColumnMode.ColumnHeader, true)) + 4, min, max)))
                    .ToArray();
            }
            finally { state.Busy = false; }
            Distribute(grid, state);
        }

        /// <summary>Đặt bề ngang: đủ chỗ thì chia phần dư theo tỉ lệ cho kín bảng; thiếu chỗ thì giữ
        /// bề ngang tự nhiên (bảng tự hiện thanh cuộn ngang).</summary>
        private static void Distribute(DataGridView grid, FitState state)
        {
            if (state.Busy) return;
            var cols = VisibleColumns(grid);
            if (cols.Length == 0 || cols.Length != state.Natural.Length) return;
            state.Busy = true;
            try
            {
                state.LastWidth = grid.ClientSize.Width;
                var vScroll = grid.Controls.OfType<VScrollBar>().FirstOrDefault()?.Visible == true ? SystemInformation.VerticalScrollBarWidth : 0;
                var avail = grid.ClientSize.Width - vScroll - 2;
                var total = state.Natural.Sum();
                var extra = Math.Max(0, avail - total);
                var given = 0;
                for (var i = 0; i < cols.Length; i++)
                {
                    var add = i == cols.Length - 1 ? extra - given : (int)((long)extra * state.Natural[i] / Math.Max(1, total));
                    given += add;
                    cols[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                    cols[i].Width = state.Natural[i] + add;
                }
            }
            finally { state.Busy = false; }
        }

        // ------------------------------------------------------------------ chiều cao

        public static void GrowWithRows(DataGridView grid)
        {
            if (Grows.TryGetValue(grid, out _)) return;
            Grows.Add(grid, new object());
            grid.ScrollBars = ScrollBars.Horizontal;
            grid.Dock = DockStyle.None;
            grid.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;

            void Update() => UpdateHeight(grid);
            grid.DataBindingComplete += (_, _) => Update();
            grid.RowsAdded += (_, _) => Update();
            grid.RowsRemoved += (_, _) => Update();
            grid.ColumnWidthChanged += (_, _) => Update();
            grid.ColumnHeadersHeightChanged += (_, _) => Update();
            grid.RowHeightChanged += (_, _) => Update();
            grid.SizeChanged += (_, _) => Update();

            // Bảng không có thanh cuộn dọc -> lăn chuột cuộn cả trang (khung cuộn gần nhất).
            grid.MouseWheel += (_, e) =>
            {
                if (e is HandledMouseEventArgs h) h.Handled = true;
                for (Control? p = grid.Parent; p is not null; p = p.Parent)
                {
                    if (p is ScrollableControl sc && sc.AutoScroll && sc.VerticalScroll.Visible)
                    {
                        var pos = sc.AutoScrollPosition; // số âm
                        sc.AutoScrollPosition = new Point(-pos.X, Math.Max(0, -pos.Y - e.Delta));
                        return;
                    }
                }
            };
            Update();
        }

        private static void UpdateHeight(DataGridView grid)
        {
            var rows = grid.Rows.GetRowCount(DataGridViewElementStates.Visible);
            var h = (grid.ColumnHeadersVisible ? grid.ColumnHeadersHeight : 0)
                    + (rows == 0 ? grid.RowTemplate.Height : grid.Rows.GetRowsHeight(DataGridViewElementStates.Visible))
                    + 2;
            if (grid.Columns.GetColumnsWidth(DataGridViewElementStates.Visible) > grid.ClientSize.Width)
                h += SystemInformation.HorizontalScrollBarHeight;
            if (grid.Height != h) grid.Height = h;
        }
    }
}
