using TruckingSystem.Desktop.Common;

namespace TruckingSystem.Desktop.GUI_ResourceManagement
{
    /// <summary>CHI TIẾT 1 xe / 1 tài xế (nhấp đúp 1 dòng ở màn Nguồn lực theo kho / màn Tài xế) — bảng ngoài chỉ giữ
    /// vài cột chính, ở đây hiện ĐỦ thông tin, chia NHÓM có tiêu đề ("Thông tin", "Giấy tờ", "Hôm nay"...), và là NƠI
    /// SỬA luôn (giấy tờ / nhiên liệu của xe; hồ sơ / ngày làm của tài xế) — không cần mục sửa riêng ở chuột phải nữa.
    ///  • Bấm 1 nút thao tác: form sửa mở NGAY TRÊN cửa sổ chi tiết; lưu / đóng form sửa xong thì cửa sổ chi tiết
    ///    VẪN MỞ và tự cập nhật số liệu mới (trước đây cửa sổ chi tiết bị đóng, phải mở lại).
    ///  • Dòng có Label bắt đầu bằng "#" là TIÊU ĐỀ NHÓM (VD ("# Giấy tờ", "")).
    ///  • Giá trị bắt đầu bằng ✅ / 🟡 / ⛔ / ❌ tô xanh / cam / đỏ; chữ dài tự xuống dòng.
    ///  • Nội dung nằm trong khung CÓ THANH CUỘN: form không bao giờ cao / rộng quá ~85% màn hình.
    /// Form dựng bằng code (không có Designer) — không mở bằng trình thiết kế của Visual Studio.</summary>
    public sealed class ResourceDetailForm : BaseForm
    {
        private readonly Func<IEnumerable<(string Label, string Value)>> _rows;
        private readonly Func<IEnumerable<(string Text, bool Enabled, Func<IWin32Window, Task> Run)>> _actions;
        private readonly TableLayoutPanel _table;
        private readonly FlowLayoutPanel _bar;
        private readonly Panel _scroll;
        private readonly Button _close;
        private bool _busy;

        /// <param name="rows">Hàm dựng các dòng thông tin — được gọi LẠI sau mỗi lần sửa để hiện số liệu mới.</param>
        /// <param name="actions">Hàm dựng các nút sửa; Run nhận cửa sổ chi tiết làm chủ (owner) để form sửa nổi lên trên nó,
        /// và chỉ xong (Task hoàn tất) khi đã đóng form sửa VÀ tải lại dữ liệu.</param>
        public ResourceDetailForm(string title, Func<IEnumerable<(string Label, string Value)>> rows,
            Func<IEnumerable<(string Text, bool Enabled, Func<IWin32Window, Task> Run)>> actions)
        {
            _rows = rows;
            _actions = actions;
            Text = title;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = true;
            MinimizeBox = false;
            BackColor = Color.White;
            MinimumSize = new Size(LogicalToDeviceUnits(460), LogicalToDeviceUnits(320));

            // ---- nội dung (cuộn được)
            _table = new TableLayoutPanel
            {
                ColumnCount = 2,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Location = new Point(LogicalToDeviceUnits(20), LogicalToDeviceUnits(12)),
                Margin = new Padding(0),
            };
            _table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            _table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            _scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.White };
            _scroll.Controls.Add(_table);

            // ---- tiêu đề (trên) + hàng nút (dưới) cố định, không cuộn
            var titleLabel = new Label
            {
                Text = title,
                Dock = DockStyle.Top,
                AutoSize = false,
                Height = LogicalToDeviceUnits(46),
                Font = Theme.FontSectionTitle,
                ForeColor = Theme.TextPrimary,
                Padding = new Padding(LogicalToDeviceUnits(18), LogicalToDeviceUnits(10), 0, 0),
            };
            // Chân cửa sổ (quy tắc nút): các nút sửa (nút phụ) bên trái · nút Đóng tách sang góc phải.
            _bar = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                BackColor = Theme.GridAltRow,
                Margin = Padding.Empty,
            };
            _close = Theme.MakeSecondaryButton("Đóng", 100);
            _close.Click += (_, _) => Close();
            CancelButton = _close;

            FillRows();
            FillActions();

            var footer = new TableLayoutPanel
            {
                Dock = DockStyle.Bottom,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 2,
                BackColor = Theme.GridAltRow,
                Padding = new Padding(LogicalToDeviceUnits(14), LogicalToDeviceUnits(8), LogicalToDeviceUnits(14), LogicalToDeviceUnits(8)),
            };
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            footer.Controls.Add(_bar, 0, 0);
            _close.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            footer.Controls.Add(_close, 1, 0);

            Controls.Add(_scroll);
            Controls.Add(footer);
            Controls.Add(titleLabel);

            // Kích thước: vừa nội dung nhưng KHÔNG quá ~85% màn hình — dư thì khung giữa hiện thanh cuộn.
            Load += (_, _) =>
            {
                var area = Screen.FromControl((Control?)Owner ?? this).WorkingArea;
                var pref = _table.GetPreferredSize(Size.Empty);
                var w = Math.Min(pref.Width + LogicalToDeviceUnits(60), (int)(area.Width * 0.85));
                var h = Math.Min(titleLabel.Height + pref.Height + footer.GetPreferredSize(new Size(w, 0)).Height + LogicalToDeviceUnits(70),
                                 (int)(area.Height * 0.85));
                ClientSize = new Size(Math.Max(w, MinimumSize.Width), Math.Max(h, MinimumSize.Height));
                CenterToParent();
            };
        }

        private void FillRows()
        {
            _table.SuspendLayout();
            foreach (Control c in _table.Controls.Cast<Control>().ToList()) c.Dispose();
            _table.Controls.Clear();
            _table.RowStyles.Clear();
            _table.RowCount = 0;
            var valueWidth = LogicalToDeviceUnits(460);
            foreach (var (label, value) in _rows())
            {
                var r = _table.RowCount++;
                _table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                if (label.StartsWith('#'))
                {
                    var header = new Label
                    {
                        Text = label.TrimStart('#', ' ').ToUpperInvariant(),
                        AutoSize = true,
                        Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                        ForeColor = Theme.Primary,
                        Margin = new Padding(0, r == 0 ? 0 : LogicalToDeviceUnits(12), 0, LogicalToDeviceUnits(4)),
                    };
                    _table.Controls.Add(header, 0, r);
                    _table.SetColumnSpan(header, 2);
                    continue;
                }
                _table.Controls.Add(new Label
                {
                    Text = label,
                    AutoSize = true,
                    ForeColor = Theme.TextSecondary,
                    Font = Theme.FontNormal,
                    Margin = new Padding(0, LogicalToDeviceUnits(3), LogicalToDeviceUnits(16), LogicalToDeviceUnits(3)),
                }, 0, r);
                _table.Controls.Add(new Label
                {
                    Text = value,
                    AutoSize = true,
                    Font = Theme.FontBold,
                    ForeColor = ValueColor(value),
                    MaximumSize = new Size(valueWidth, 0),
                    Margin = new Padding(0, LogicalToDeviceUnits(3), 0, LogicalToDeviceUnits(3)),
                }, 1, r);
            }
            _table.ResumeLayout(true);
        }

        private void FillActions()
        {
            _bar.SuspendLayout();
            foreach (Control c in _bar.Controls.Cast<Control>().ToList()) c.Dispose();
            _bar.Controls.Clear();
            foreach (var (text, enabled, run) in _actions())
            {
                var b = Theme.MakeSecondaryButton(text, 160);
                b.Enabled = enabled;
                b.Click += async (_, _) => await RunActionAsync(run);
                _bar.Controls.Add(b);
            }
            _bar.ResumeLayout(true);
        }

        /// <summary>Mở form sửa ngay trên cửa sổ này; xong thì dựng lại thông tin + nút (cửa sổ chi tiết vẫn mở).</summary>
        private async Task RunActionAsync(Func<IWin32Window, Task> run)
        {
            if (_busy) return;
            _busy = true;
            try
            {
                await run(this);
                if (IsDisposed) return;
                var scroll = _scroll.AutoScrollPosition;
                FillRows();
                FillActions();
                _scroll.AutoScrollPosition = new Point(-scroll.X, -scroll.Y);
            }
            catch (Exception ex)
            {
                if (!IsDisposed) Ask.Fail(this, "Lỗi: " + ex.Message);
            }
            finally { _busy = false; }
        }

        private static Color ValueColor(string v)
            => v.StartsWith("✅") ? Theme.Success
             : v.StartsWith("🟡") || v.StartsWith("⚠️") ? Theme.Warning
             : v.StartsWith("⛔") || v.StartsWith("❌") ? Theme.Danger
             : Theme.TextPrimary;
    }
}
