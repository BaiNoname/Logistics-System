using System.Drawing;
using System.Windows.Forms;
using TruckingSystem.Desktop.DTO_LocationManagement;

namespace TruckingSystem.Desktop.Common
{
    /// <summary>Gợi ý địa chỉ VietMap cho ô Địa chỉ tự do (khách hàng, nhân viên).
    /// Khác PlaceSearchBinder (bắt buộc chọn để có tọa độ): ở đây người dùng vẫn được gõ tay,
    /// gợi ý chỉ giúp điền nhanh. Gõ ≥ 3 ký tự → chờ ~0,4 giây → danh sách nổi ngay dưới ô;
    /// bấm 1 dòng hoặc ↓ + Enter để điền, Esc / rời ô để đóng. Lỗi mạng/API chỉ ẩn danh sách,
    /// không chặn việc nhập hay lưu. Danh sách đặt trên Form nên không bị khung cuộn cắt.</summary>
    public sealed class AddressSuggestBinder
    {
        private const int MinLength = 3;
        private const int MaxRows = 6;

        private readonly TextBox _txt;
        private readonly Func<ApiServices?> _api;
        private readonly ListBox _lst = new()
        {
            Visible = false,
            IntegralHeight = false,
            BorderStyle = BorderStyle.FixedSingle,
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI", 9.5F),
            TabStop = false,
            DrawMode = DrawMode.OwnerDrawFixed,
            ItemHeight = 26,
        };
        private readonly System.Windows.Forms.Timer _debounce = new() { Interval = 400 };
        private readonly List<Control> _watched = new();
        private Form? _form;
        private bool _suppress;
        private int _version;

        public AddressSuggestBinder(TextBox txt, Func<ApiServices?> api)
        {
            _txt = txt;
            _api = api;
            if (string.IsNullOrEmpty(_txt.PlaceholderText))
                _txt.PlaceholderText = "Gõ số nhà, tên đường, phường... để chọn gợi ý";

            _txt.TextChanged += (_, _) =>
            {
                if (_suppress) { _suppress = false; return; }
                _version++; // hủy kết quả đang chờ
                _debounce.Stop();
                if (IsEditing && _txt.Text.Trim().Length >= MinLength) _debounce.Start();
                else Hide();
            };
            _txt.PreviewKeyDown += (_, e) =>
            {
                // Khi danh sách đang mở, Esc/Enter thuộc về ô này (không đóng form / không bấm Lưu).
                if (_lst.Visible && e.KeyCode is Keys.Escape or Keys.Enter or Keys.Down or Keys.Up)
                    e.IsInputKey = true;
            };
            _txt.KeyDown += (_, e) =>
            {
                if (!_lst.Visible || _lst.Items.Count == 0) return;
                switch (e.KeyCode)
                {
                    case Keys.Down:
                        _lst.SelectedIndex = Math.Min(_lst.SelectedIndex + 1, _lst.Items.Count - 1);
                        break;
                    case Keys.Up:
                        _lst.SelectedIndex = Math.Max(_lst.SelectedIndex - 1, 0);
                        break;
                    case Keys.Enter:
                        if (_lst.SelectedIndex < 0) return; // chưa chọn dòng -> giữ hành vi Enter mặc định
                        Pick();
                        break;
                    case Keys.Escape:
                        Hide();
                        break;
                    default:
                        return;
                }
                e.Handled = e.SuppressKeyPress = true;
            };
            _txt.Leave += (_, _) => _txt.BeginInvoke(() => { if (!_lst.Focused) Hide(); });
            _lst.MouseDown += (_, e) =>
            {
                var i = _lst.IndexFromPoint(e.Location);
                if (i < 0) return;
                _lst.SelectedIndex = i;
                Pick();
            };
            _lst.Leave += (_, _) => Hide();
            _lst.DrawItem += DrawItem;
            _debounce.Tick += async (_, _) => { _debounce.Stop(); await SearchAsync(); };
            _txt.HandleCreated += (_, _) => Attach();
            if (_txt.IsHandleCreated) Attach();
            _txt.Disposed += (_, _) => { _debounce.Dispose(); _lst.Dispose(); };
        }

        /// <summary>Văn bản điền vào ô khi chọn gợi ý: tên + phần địa chỉ còn lại, bỏ trùng lặp.</summary>
        public static string Compose(PlaceSuggestionItem item)
        {
            var name = item.Name?.Trim() ?? "";
            var address = item.Address?.Trim() ?? "";
            if (name.Length == 0) return address;
            if (address.Length == 0) return name;
            if (address.Contains(name, StringComparison.OrdinalIgnoreCase)) return address;
            if (name.Contains(address, StringComparison.OrdinalIgnoreCase)) return name;
            return $"{name}, {address}";
        }

        private void Attach()
        {
            var form = _txt.FindForm();
            if (form is null || ReferenceEquals(form, _form)) return;
            _form = form;
            form.Controls.Add(_lst);
            // Bất kỳ khung cha nào cuộn / đổi cỡ thì vị trí cũ sai -> đóng danh sách.
            foreach (var c in _watched) { c.LocationChanged -= OnLayoutMoved; c.SizeChanged -= OnLayoutMoved; }
            _watched.Clear();
            for (Control? c = _txt.Parent; c is not null && c != form; c = c.Parent)
            {
                c.LocationChanged += OnLayoutMoved;
                c.SizeChanged += OnLayoutMoved;
                _watched.Add(c);
            }
            form.Deactivate += (_, _) => Hide();
        }

        private void OnLayoutMoved(object? sender, EventArgs e) => Hide();

        private void DrawItem(object? sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            var selected = (e.State & DrawItemState.Selected) != 0;
            using var back = new SolidBrush(selected ? Color.FromArgb(219, 234, 254) : Theme.CardBg);
            e.Graphics.FillRectangle(back, e.Bounds);
            var text = new Rectangle(e.Bounds.X + 28, e.Bounds.Y, e.Bounds.Width - 32, e.Bounds.Height);
            TextRenderer.DrawText(e.Graphics, "📍", e.Font, new Rectangle(e.Bounds.X + 6, e.Bounds.Y, 22, e.Bounds.Height),
                Theme.TextSecondary, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
            TextRenderer.DrawText(e.Graphics, _lst.Items[e.Index]?.ToString(), e.Font, text, Theme.TextPrimary,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        }

        /// <summary>Ô địa chỉ đang là ô được chọn của form (kể cả khi app tạm mất focus hệ điều hành).</summary>
        private bool IsEditing => _txt.Focused || ReferenceEquals(_txt.FindForm()?.ActiveControl, _txt);

        private async Task SearchAsync()
        {
            var api = _api();
            var text = _txt.Text.Trim();
            if (api is null || text.Length < MinLength || !IsEditing) return;

            var version = ++_version;
            List<PlaceSuggestionItem> results;
            try
            {
                results = await api.Locations.SearchPlacesAsync(text) ?? new();
            }
            catch
            {
                results = new(); // mất mạng / hết quyền / chưa cấu hình VietMap -> vẫn nhập tay được
            }
            if (version != _version || _txt.IsDisposed || !IsEditing) return;

            var texts = results.Select(Compose)
                .Where(s => s.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            if (texts.Count == 0) { Hide(); return; }

            _lst.BeginUpdate();
            _lst.Items.Clear();
            foreach (var s in texts) _lst.Items.Add(s);
            _lst.EndUpdate();
            Show();
        }

        private void Show()
        {
            if (_form is null) Attach();
            if (_form is null) return;

            var rows = Math.Min(_lst.Items.Count, MaxRows);
            var height = rows * _lst.ItemHeight + 4;
            var top = _form.PointToClient(_txt.PointToScreen(Point.Empty));
            var below = top.Y + _txt.Height;
            // Không đủ chỗ phía dưới -> mở lên trên ô nhập.
            var y = below + height <= _form.ClientSize.Height || top.Y - height < 0 ? below : top.Y - height;
            _lst.SetBounds(top.X, y, _txt.Width, height);
            _lst.Visible = true;
            _lst.BringToFront();
        }

        private void Hide()
        {
            if (_lst.Visible) _lst.Visible = false;
        }

        private void Pick()
        {
            if (_lst.SelectedItem is not string text) return;
            Hide();
            _version++;
            _suppress = _txt.Text != text; // cùng chữ thì TextChanged không chạy -> không được bật cờ
            _txt.Text = text;
            _txt.Focus();
            _txt.SelectionStart = _txt.TextLength;
        }
    }
}
