namespace TruckingSystem.Desktop.GUI_Shared
{
    /// <summary>KHUNG CHÍNH CO GIÃN THEO BỀ NGANG CỬA SỔ (kéo nhỏ / laptop màn nhỏ):
    ///  • Hẹp (&lt; ~1280 px ở 100%): 4 nút chế độ màu chỉ còn biểu tượng (🌅 ☀️ 🌇 🌙 — rê chuột xem tên), ô tìm nhanh thu
    ///    còn ~220 px -> không còn nút bị che mất ở thanh trên.
    ///  • Rất hẹp (&lt; ~1000 px): ô tìm nhanh thu tiếp ~160 px.
    /// Rộng lại thì trở về như cũ. Bảng trong các màn tự co cột (GridLayout) — chỉ cuộn ngang khi thật sự hết chỗ.</summary>
    public partial class MainForm
    {
        private bool? _compactTop;
        private int _compactLevel = -1;

        private void SetupResponsive()
        {
            Resize += (_, _) => ApplyResponsive();
            Shown += (_, _) => ApplyResponsive();
        }

        private void ApplyResponsive()
        {
            if (WindowState == FormWindowState.Minimized || IsDisposed) return;
            int S(int v) => LogicalToDeviceUnits(v);
            var w = ClientSize.Width;
            var level = w < S(1000) ? 2 : w < S(1280) ? 1 : 0;
            if (level == _compactLevel) return;
            _compactLevel = level;
            SuspendLayout();
            var compact = level > 0;
            if (_compactTop != compact)
            {
                _compactTop = compact;
                foreach (var (b, m) in _modeButtons)
                {
                    var space = m.Name.IndexOf(' ');
                    b.Text = compact && space > 0 ? m.Name[..space] : m.Name;
                }
            }
            _searchHost.Width = S(level switch { 2 => 160, 1 => 220, _ => 340 });
            ResumeLayout(true);
        }
    }
}
