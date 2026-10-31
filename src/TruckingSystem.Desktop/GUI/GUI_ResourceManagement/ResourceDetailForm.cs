using TruckingSystem.Desktop.Common;

namespace TruckingSystem.Desktop.GUI_ResourceManagement
{
    /// <summary>CHI TIẾT 1 xe / 1 tài xế (nhấp đúp 1 dòng ở màn Nguồn lực theo kho / màn Tài xế) — bảng ngoài chỉ giữ
    /// vài cột chính, ở đây hiện ĐỦ thông tin, chia NHÓM có tiêu đề ("Thông tin", "Giấy tờ", "Hôm nay"...).
    ///  • Dòng có Label bắt đầu bằng "#" là TIÊU ĐỀ NHÓM (VD ("# Giấy tờ", "")).
    ///  • Giá trị bắt đầu bằng ✅ / 🟡 / ⛔ / ❌ tô xanh / cam / đỏ; chữ dài tự xuống dòng.
    ///  • Nội dung nằm trong khung CÓ THANH CUỘN dọc + ngang: form không bao giờ cao / rộng quá màn hình
    ///    (laptop nhỏ, phóng to 150%) — trước đây form tự giãn theo nội dung nên phần dưới bị khuất, không xem hết.
    ///  • Hàng nút thao tác luôn nằm dưới cùng (không bị cuộn mất).
    /// Form dựng bằng code (không có Designer) — không mở bằng trình thiết kế của Visual Studio.</summary>
    public sealed class ResourceDetailForm : BaseForm
    {
        /// <summary>Nút thao tác bấm xong (form đã đóng) — màn ngoài chạy việc này rồi tải lại dữ liệu.</summary>
        public Func<Task>? PendingAction { get; private set; }

        public ResourceDetailForm(string title, IEnumerable<(string Label, string Value)> rows,
            IEnumerable<(string Text, bool Enabled, Func<Task> Run)> actions)
        {
            Text = title;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = true;
            MinimizeBox = false;
            BackColor = Color.White;
            MinimumSize = new Size(LogicalToDeviceUnits(460), LogicalToDeviceUnits(320));

            // ---- nội dung (cuộn được)
            var table = new TableLayoutPanel
            {
                ColumnCount = 2,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Location = new Point(LogicalToDeviceUnits(20), LogicalToDeviceUnits(12)),
                Margin = new Padding(0),
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            var valueWidth = LogicalToDeviceUnits(460);
            foreach (var (label, value) in rows)
            {
                var r = table.RowCount++;
                table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
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
                    table.Controls.Add(header, 0, r);
                    table.SetColumnSpan(header, 2);
                    continue;
                }
                table.Controls.Add(new Label
                {
                    Text = label,
                    AutoSize = true,
                    ForeColor = Theme.TextSecondary,
                    Font = Theme.FontNormal,
                    Margin = new Padding(0, LogicalToDeviceUnits(3), LogicalToDeviceUnits(16), LogicalToDeviceUnits(3)),
                }, 0, r);
                table.Controls.Add(new Label
                {
                    Text = value,
                    AutoSize = true,
                    Font = Theme.FontBold,
                    ForeColor = ValueColor(value),
                    MaximumSize = new Size(valueWidth, 0),
                    Margin = new Padding(0, LogicalToDeviceUnits(3), 0, LogicalToDeviceUnits(3)),
                }, 1, r);
            }
            var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.White };
            scroll.Controls.Add(table);

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
            var bar = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                BackColor = Theme.GridAltRow,
                Padding = new Padding(LogicalToDeviceUnits(14), LogicalToDeviceUnits(8), LogicalToDeviceUnits(14), LogicalToDeviceUnits(8)),
            };
            foreach (var (text, enabled, run) in actions)
            {
                var b = Theme.MakeSecondaryButton(text, 160);
                b.Enabled = enabled;
                b.Click += (_, _) => { PendingAction = run; DialogResult = DialogResult.OK; Close(); };
                bar.Controls.Add(b);
            }
            var close = Theme.MakeSecondaryButton("Đóng", 100);
            close.Click += (_, _) => Close();
            bar.Controls.Add(close);
            CancelButton = close;

            Controls.Add(scroll);
            Controls.Add(bar);
            Controls.Add(titleLabel);

            // Kích thước: vừa nội dung nhưng KHÔNG quá ~85% màn hình — dư thì khung giữa hiện thanh cuộn.
            Load += (_, _) =>
            {
                var area = Screen.FromControl((Control?)Owner ?? this).WorkingArea;
                var pref = table.GetPreferredSize(Size.Empty);
                var w = Math.Min(pref.Width + LogicalToDeviceUnits(60), (int)(area.Width * 0.85));
                var h = Math.Min(titleLabel.Height + pref.Height + bar.GetPreferredSize(new Size(w, 0)).Height + LogicalToDeviceUnits(70),
                                 (int)(area.Height * 0.85));
                ClientSize = new Size(Math.Max(w, MinimumSize.Width), Math.Max(h, MinimumSize.Height));
                CenterToParent();
            };
        }

        private static Color ValueColor(string v)
            => v.StartsWith("✅") ? Theme.Success
             : v.StartsWith("🟡") || v.StartsWith("⚠️") ? Theme.Warning
             : v.StartsWith("⛔") || v.StartsWith("❌") ? Theme.Danger
             : Theme.TextPrimary;
    }
}
