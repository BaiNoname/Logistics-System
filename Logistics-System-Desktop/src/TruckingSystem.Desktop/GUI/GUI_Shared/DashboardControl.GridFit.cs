using System.Windows.Forms;

namespace TruckingSystem.Desktop.GUI_Shared
{
    /// <summary>Chiều cao các bảng ở màn Tổng quan CO GIÃN THEO MÀN HÌNH:
    /// • Ít dòng -> bảng thấp vừa đủ số dòng (không còn khoảng trắng lớn bên dưới).
    /// • Nhiều dòng -> bảng cao tới gần 1 màn hình rồi mới có thanh cuộn trong bảng;
    ///   màn hình lớn / phóng to cửa sổ thì bảng cao thêm, màn nhỏ thì thấp lại.
    /// (Bề ngang cột đã tự giãn theo màn bằng GridLayout.FitColumns.)</summary>
    public partial class DashboardControl
    {
        private bool _fitting;

        private void SetupGridFit()
        {
            foreach (var g in new[] { _grid, _gridVehicleFuel, _gridFuel })
                g.DataBindingComplete += (_, _) => { if (IsHandleCreated) BeginInvoke(new Action(FitGridHeights)); };
            _root.Resize += (_, _) => FitGridHeights();
            _pages.Changed += (_, _) => { if (IsHandleCreated) BeginInvoke(new Action(FitGridHeights)); };
        }

        private void FitGridHeights()
        {
            if (_fitting || IsDisposed) return;
            var view = _root.ClientSize.Height;
            if (view <= 0) return;
            _fitting = true;
            try
            {
                // Chừa ~ thanh tiêu đề + ô lọc phía trên bảng để vẫn thấy được tiêu đề khi cuộn tới bảng.
                var tall = Math.Max(280, view - 160);
                dashLayout.SuspendLayout();
                SetGridRowHeight(_grid, tall, 5);
                SetGridRowHeight(_gridVehicleFuel, Math.Max(200, view / 2), 3); // bảng tham khảo: tối đa nửa màn
                SetGridRowHeight(_gridFuel, tall, 5);
                dashLayout.ResumeLayout(true);
            }
            finally { _fitting = false; }
        }

        /// <summary>Cao = tiêu đề cột + các dòng (ít nhất <paramref name="minRows"/> dòng), không quá <paramref name="max"/>.</summary>
        private void SetGridRowHeight(DataGridView g, int max, int minRows)
        {
            var i = _rows.FindIndex(r => r.Control == g);
            if (i < 0) return;
            var rowsH = Math.Max(g.Rows.GetRowsHeight(DataGridViewElementStates.Visible), minRows * g.RowTemplate.Height);
            var hScroll = g.Controls.OfType<HScrollBar>().FirstOrDefault()?.Visible == true ? SystemInformation.HorizontalScrollBarHeight : 0;
            var content = (g.ColumnHeadersVisible ? g.ColumnHeadersHeight : 0) + rowsH + hScroll + 4;
            var h = Math.Min(content, max) + g.Margin.Vertical;
            if (_rows[i].Height == h) return;
            _rows[i] = (g, _rows[i].Page, h);
            // Chỉ đổi hàng đang hiện (hàng của trang khác đang để cao 0 — ApplyPage sẽ dùng chiều cao mới khi chuyển trang).
            if (i < dashLayout.RowStyles.Count && dashLayout.RowStyles[i].Height > 0)
                dashLayout.RowStyles[i] = new RowStyle(SizeType.Absolute, h);
        }
    }
}
