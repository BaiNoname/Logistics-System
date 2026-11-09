namespace TruckingSystem.Desktop.GUI_Shared
{
    /// <summary>CHẾ ĐỘ "🔍 CHỮ TO" (mẫu UX D1 đã duyệt): bật thì chữ + độ cao dòng của các BẢNG trong màn đang mở to
    /// thêm ~20% (dễ đọc cho người lớn tuổi / màn hình nhỏ). Chỉ phóng bảng — không phóng cả màn để bố cục không vỡ.
    /// Nhớ lựa chọn ở %AppData%/LogisticsSystem/big-text.txt; áp lại mỗi lần chuyển màn.</summary>
    public partial class MainForm
    {
        private const float BigScale = 1.2f;
        private static readonly string BigTextFile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LogisticsSystem", "big-text.txt");
        private bool _bigText;
        private Button? _bigTextButton;

        private void SetupBigText()
        {
            try { _bigText = File.Exists(BigTextFile) && File.ReadAllText(BigTextFile).Trim() == "1"; } catch { }
            _bigTextButton = new Button
            {
                Text = "🔍 Chữ to", AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Cursor = Cursors.Hand, UseVisualStyleBackColor = false,
                Margin = new Padding(LogicalToDeviceUnits(8), 3, 0, 3), Padding = new Padding(6, 2, 6, 2),
                MinimumSize = new Size(0, LogicalToDeviceUnits(32)),
            };
            _bigTextButton.FlatAppearance.BorderSize = 1;
            new ToolTip().SetToolTip(_bigTextButton, "Phóng to chữ trong các bảng (bấm lần nữa để trở lại cỡ thường)");
            _bigTextButton.Click += (_, _) =>
            {
                _bigText = !_bigText;
                try { Directory.CreateDirectory(Path.GetDirectoryName(BigTextFile)!); File.WriteAllText(BigTextFile, _bigText ? "1" : "0"); } catch { }
                StyleBigTextButton();
                // Áp ngay cho màn đang mở: mở lại màn hiện tại cho bảng vẽ lại đúng cỡ.
                if (_activeNavButton is { } b && _buildOf.TryGetValue(b, out var build)) NavigateTo(b, build);
            };
            _topLeft.Controls.Add(_bigTextButton);
            StyleBigTextButton();
        }

        private void StyleBigTextButton()
        {
            if (_bigTextButton is null) return;
            _bigTextButton.BackColor = _bigText ? Color.FromArgb(37, 99, 235) : Color.White;
            _bigTextButton.ForeColor = _bigText ? Color.White : Color.FromArgb(71, 85, 105);
            _bigTextButton.FlatAppearance.BorderColor = _bigText ? Color.FromArgb(37, 99, 235) : Color.FromArgb(203, 213, 225);
        }

        /// <summary>Áp chữ to SAU KHI màn đã có handle. (Bản trước gọi BeginInvoke ngay trong lúc dựng MainForm — lúc đó
        /// cửa sổ chưa có handle nên ném lỗi: bật "Chữ to" rồi đăng nhập lại thì app mất luôn.)</summary>
        private void ApplyBigTextWhenReady(Control screen)
        {
            void Run()
            {
                if (screen.IsDisposed || !screen.IsHandleCreated) return;
                screen.BeginInvoke(new Action(() => { if (!screen.IsDisposed) ApplyBigText(screen); }));
            }
            if (screen.IsHandleCreated) Run();
            else
            {
                void OnCreated(object? s, EventArgs e) { screen.HandleCreated -= OnCreated; Run(); }
                screen.HandleCreated += OnCreated;
            }
        }

        /// <summary>Phóng chữ + dòng của mọi bảng trong màn vừa mở (gọi sau khi thêm màn vào khung nội dung).</summary>
        private void ApplyBigText(Control screen)
        {
            if (!_bigText) return;
            foreach (var grid in Descendants(screen).OfType<DataGridView>())
            {
                Font Big(Font? f) => f is null ? new Font("Segoe UI", 9F * BigScale) : new Font(f.FontFamily, f.Size * BigScale, f.Style);
                grid.DefaultCellStyle.Font = Big(grid.DefaultCellStyle.Font ?? grid.Font);
                grid.ColumnHeadersDefaultCellStyle.Font = Big(grid.ColumnHeadersDefaultCellStyle.Font ?? grid.Font);
                grid.RowTemplate.Height = (int)(grid.RowTemplate.Height * BigScale);
                foreach (DataGridViewRow r in grid.Rows) r.Height = grid.RowTemplate.Height;
            }
        }

        private static IEnumerable<Control> Descendants(Control c)
        {
            foreach (Control child in c.Controls)
            {
                yield return child;
                foreach (var d in Descendants(child)) yield return d;
            }
        }
    }
}
