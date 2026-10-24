using System.ComponentModel;
using System.Globalization;
using System.Text;

namespace TruckingSystem.Desktop.Common
{
    /// <summary>THANH LỌC dùng chung cho các bảng danh sách: ô tìm nhanh (gõ không dấu cũng tìm
    /// được; nhiều từ thì phải khớp đủ các từ) + các ô chọn do từng màn tự thêm (AddChoice) + nút
    /// "Xóa lọc" + dòng "Đang hiện x/y". Lọc ngay trên dữ liệu đã tải, không gọi lại API.
    ///
    /// Là 1 FlowLayoutPanel (tự xuống dòng khi màn hình hẹp) — kéo thả được trong Designer như
    /// mọi control khác; đặt vào 1 hàng AutoSize của TableLayoutPanel, Dock = Fill.
    /// Mỗi cặp "nhãn + ô" nằm chung 1 khối không tách được — màn hẹp (laptop 14") thì cả khối
    /// xuống dòng, không còn cảnh chữ "Khách hàng:" ở cuối dòng trên còn ô chọn rơi xuống dòng dưới.
    /// Cách dùng ở màn danh sách:
    ///   filterBar.AddChoice("status", "Trạng thái", new[] { "Nháp", "Đã xác nhận" });
    ///   filterBar.FilterChanged += (_, _) => BindGrid();
    ///   rows.Where(r => filterBar.Is("status", r.StatusText) &amp;&amp; filterBar.MatchesSearch(r.Code, r.Name))
    ///   filterBar.SetCount(shown, total);</summary>
    [DesignerCategory("Code")]
    public class FilterBar : FlowLayoutPanel
    {
        public const string AllText = "Tất cả";

        private static readonly Color Muted = Color.FromArgb(100, 116, 139);
        private static readonly Color Accent = Color.FromArgb(37, 99, 235);

        private readonly TextBox _txtSearch;
        private readonly Button _btnClear;
        private readonly Label _lblCount;
        private readonly FlowLayoutPanel _tailGroup;
        private readonly Dictionary<string, ComboBox> _choices = new();
        private readonly System.Windows.Forms.Timer _searchTimer = new() { Interval = 250 };
        private bool _suppress;

        /// <summary>Bất kỳ điều kiện lọc nào đổi (gõ tìm, chọn ô, xóa lọc).</summary>
        public event EventHandler? FilterChanged;

        public FilterBar()
        {
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            WrapContents = true;
            Margin = new Padding(0, 0, 0, 6);

            var lblSearch = new Label { Text = "🔎", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 0, 4, 0) };
            _txtSearch = new TextBox
            {
                PlaceholderText = "Tìm nhanh...",
                Anchor = AnchorStyles.Left,
                Margin = new Padding(0),
                Width = 240,
            };
            _txtSearch.TextChanged += (_, _) =>
            {
                if (_suppress) return;
                _searchTimer.Stop();
                _searchTimer.Start(); // gõ xong mới lọc, đỡ giật khi gõ nhanh
            };
            _btnClear = new Button
            {
                Text = "✖ Xóa lọc",
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                Cursor = Cursors.Hand,
                Enabled = false,
                Anchor = AnchorStyles.Left,
                Margin = new Padding(0),
                MinimumSize = new Size(0, 30),
                Padding = new Padding(6, 0, 6, 0),
                UseVisualStyleBackColor = false,
            };
            _btnClear.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            _btnClear.Click += (_, _) => ClearFilters();
            _lblCount = new Label { AutoSize = true, ForeColor = Muted, Anchor = AnchorStyles.Left, Margin = new Padding(10, 0, 0, 0) };

            Controls.Add(Group(lblSearch, _txtSearch));
            _tailGroup = Group(_btnClear, _lblCount);
            Controls.Add(_tailGroup);

            _searchTimer.Tick += (_, _) => { _searchTimer.Stop(); RaiseChanged(); };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _searchTimer.Dispose();
            base.Dispose(disposing);
        }

        /// <summary>Gợi ý trong ô tìm (VD "Mã đơn, khách hàng, địa chỉ...").</summary>
        [Category("Behavior"), DefaultValue("Tìm nhanh...")]
        public string SearchPlaceholder
        {
            get => _txtSearch.PlaceholderText;
            set => _txtSearch.PlaceholderText = value;
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string SearchText => _txtSearch.Text.Trim();

        /// <summary>Thêm 1 ô chọn (đặt trước nút "Xóa lọc"). Dòng đầu luôn là "Tất cả" = không lọc.</summary>
        public void AddChoice(string key, string label, IEnumerable<string> options)
        {
            var lbl = new Label { Text = label + ":", AutoSize = true, ForeColor = Muted, Anchor = AnchorStyles.Left, Margin = new Padding(0, 0, 6, 0) };
            var cbo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Anchor = AnchorStyles.Left, Margin = new Padding(0) };
            cbo.Items.Add(AllText);
            cbo.Items.AddRange(options.Distinct().Cast<object>().ToArray());
            cbo.SelectedIndex = 0;
            cbo.SelectedIndexChanged += (_, _) => { if (!_suppress) RaiseChanged(); };
            var group = Group(lbl, cbo);
            Controls.Add(group);
            Controls.SetChildIndex(group, Controls.IndexOf(_tailGroup));
            _choices[key] = cbo;
            FitWidth(cbo);
        }

        /// <summary>Khối KHÔNG xuống dòng giữa chừng (nhãn luôn đi cùng ô của nó), các khối cách đều
        /// nhau, căn giữa theo chiều dọc.</summary>
        private static FlowLayoutPanel Group(params Control[] parts)
        {
            var g = new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                WrapContents = false,
                Anchor = AnchorStyles.Left,
                Margin = new Padding(0, 0, 18, 6),
                Padding = new Padding(0),
            };
            g.Controls.AddRange(parts);
            return g;
        }

        /// <summary>Đổi danh sách lựa chọn (VD khách hàng / kho lấy từ dữ liệu vừa tải) — GIỮ lựa
        /// chọn đang chọn nếu vẫn còn.</summary>
        public void SetChoiceOptions(string key, IEnumerable<string> options)
        {
            if (!_choices.TryGetValue(key, out var cbo)) return;
            var keep = cbo.SelectedItem as string;
            _suppress = true;
            try
            {
                cbo.Items.Clear();
                cbo.Items.Add(AllText);
                cbo.Items.AddRange(options.Where(o => !string.IsNullOrWhiteSpace(o)).Distinct().OrderBy(o => o).Cast<object>().ToArray());
                var idx = keep is null ? 0 : cbo.Items.IndexOf(keep);
                cbo.SelectedIndex = idx >= 0 ? idx : 0;
                FitWidth(cbo);
            }
            finally { _suppress = false; }
        }

        /// <summary>Giá trị đang chọn của ô; null = "Tất cả".</summary>
        public string? Choice(string key)
            => _choices.TryGetValue(key, out var cbo) && cbo.SelectedItem is string s && s != AllText ? s : null;

        /// <summary>true nếu ô <paramref name="key"/> đang "Tất cả" hoặc chọn đúng <paramref name="value"/>.</summary>
        public bool Is(string key, string? value) => Choice(key) is not string c || c == value;

        /// <summary>Ô tìm khớp các trường (không phân biệt hoa thường / dấu; nhiều từ thì phải khớp đủ).</summary>
        public bool MatchesSearch(params object?[] fields)
        {
            var q = Normalize(SearchText);
            if (q.Length == 0) return true;
            var hay = Normalize(string.Join(" | ", fields.Where(f => f is not null)
                .Select(f => f is DateTime d ? d.ToString("dd/MM/yyyy") : f!.ToString())));
            return q.Split(' ', StringSplitOptions.RemoveEmptyEntries).All(hay.Contains);
        }

        /// <summary>Dòng đếm: "🔎 Đang hiện 12/80" khi đang lọc, "80 dòng" khi không.</summary>
        public void SetCount(int shown, int total)
        {
            var filtering = SearchText.Length > 0 || _choices.Values.Any(c => c.SelectedIndex > 0);
            _lblCount.Text = filtering ? $"🔎 Đang hiện {shown}/{total}" : $"{total} dòng";
            _lblCount.ForeColor = filtering ? Accent : Muted;
            _btnClear.Enabled = filtering;
        }

        /// <summary>Chỉ lọc theo ĐÚNG 1 ô (các ô chọn khác về "Tất cả", giữ ô tìm nhanh) — dùng cho
        /// thẻ số liệu (KpiStrip) bấm vào là xem ngay nhóm đó. key = null -> bỏ mọi ô chọn.</summary>
        public void SetOnly(string? key, string? value)
        {
            _suppress = true;
            try
            {
                foreach (var (k, c) in _choices)
                {
                    if (c.Items.Count == 0) continue;
                    var idx = k == key && value is not null ? c.Items.IndexOf(value) : 0;
                    c.SelectedIndex = idx >= 0 ? idx : 0;
                }
            }
            finally { _suppress = false; }
            RaiseChanged();
        }

        /// <summary>Bỏ mọi ô chọn về "Tất cả" và điền sẵn ô tìm — dùng khi mở màn hình từ
        /// thông báo (nhấp đúp) để chỉ hiện đúng chuyến / đơn / xe / tài xế đó.</summary>
        public void ShowOnlySearch(string text)
        {
            _suppress = true;
            try
            {
                foreach (var c in _choices.Values) if (c.Items.Count > 0) c.SelectedIndex = 0;
                _txtSearch.Text = text ?? "";
            }
            finally { _suppress = false; }
            _searchTimer.Stop();
            RaiseChanged();
        }

        public void ClearFilters()
        {
            _suppress = true;
            try
            {
                _txtSearch.Text = "";
                foreach (var c in _choices.Values) if (c.Items.Count > 0) c.SelectedIndex = 0;
            }
            finally { _suppress = false; }
            _searchTimer.Stop();
            RaiseChanged();
        }

        private void RaiseChanged() => FilterChanged?.Invoke(this, EventArgs.Empty);

        /// <summary>Bề ngang ô chọn theo chữ dài nhất (đo theo font -> đúng ở mọi mức phóng to).</summary>
        private static void FitWidth(ComboBox cbo)
        {
            var text = cbo.Items.Cast<object>().Select(i => TextRenderer.MeasureText(i.ToString(), cbo.Font).Width).DefaultIfEmpty(0).Max();
            var pad = cbo.Font.Height * 2; // chỗ cho mũi tên thả xuống
            cbo.Width = Math.Max(cbo.Font.Height * 7, Math.Min(text + pad, cbo.Font.Height * 16));
            cbo.DropDownWidth = Math.Max(cbo.Width, text + pad);
        }

        /// <summary>"Đường Số 9, Thủ Đức" -> "duong so 9, thu duc".</summary>
        public static string Normalize(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "";
            var d = s.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(d.Length);
            foreach (var ch in d)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.NonSpacingMark) continue;
                sb.Append(ch == 'đ' ? 'd' : ch);
            }
            return sb.ToString();
        }
    }
}
