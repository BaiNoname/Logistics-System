using System.Drawing;
using System.Windows.Forms;
using TruckingSystem.Desktop.DTO_LocationManagement;

namespace TruckingSystem.Desktop.Common
{
    /// <summary>Ô tìm địa chỉ VietMap dùng CHUNG cho Thêm địa điểm / Thêm kho / Sửa kho.
    /// Cách dùng (người dùng): gõ tên đường / phường / quận → chờ ~0,3 giây → danh sách gợi ý hiện
    /// ngay dưới ô → BẤM 1 dòng (hoặc ↓ rồi Enter) → hệ thống lấy địa chỉ + TỌA ĐỘ của dòng đó,
    /// hiện "✅ địa chỉ (Lat, Lng)". Không chọn dòng nào thì KHÔNG có tọa độ (không lưu được).
    /// - Đang tìm / không tìm thấy / lỗi đều hiện ngay trong danh sách (trước đây danh sách
    ///   có thể hiện ra mà trống trơn, người dùng không biết đang xảy ra gì).
    /// - Gợi ý thiếu tọa độ -> tự gọi API chi tiết để lấy tọa độ thật.
    /// - Dùng phím ↑/↓ không còn tự chọn nhầm dòng (chỉ chọn khi bấm chuột hoặc Enter).</summary>
    public sealed class PlaceSearchBinder
    {
        private const string HintText = "Gõ tên đường / phường / quận rồi BẤM chọn 1 dòng gợi ý — tọa độ sẽ tự lấy theo dòng đã chọn.";

        private readonly TextBox _txt;
        private readonly ListBox _lst;
        private readonly Label _lblSelected;
        private readonly Action<PlaceSuggestionItem?> _onChanged;
        private readonly System.Windows.Forms.Timer _debounce = new() { Interval = 350 };
        private bool _suppress;
        private int _searchVersion;

        public ApiServices? Api { get; set; }
        public PlaceSuggestionItem? Selected { get; private set; }

        public PlaceSearchBinder(TextBox txt, ListBox lst, Label lblSelected, Action<PlaceSuggestionItem?> onChanged)
        {
            _txt = txt;
            _lst = lst;
            _lblSelected = lblSelected;
            _onChanged = onChanged;

            if (string.IsNullOrEmpty(_txt.PlaceholderText)) _txt.PlaceholderText = "VD: 12 Nguyễn Văn Cừ, Long Biên, Hà Nội";
            _lst.Visible = false;
            // Danh sách gợi ý trước đây Dock=Fill + MaximumSize(0, 150) trong ô TableLayoutPanel -> bị co
            // bề ngang về ~0 px (chỉ còn 1 vạch dọc: "Có 8 gợi ý" mà không thấy dòng nào).
            // Giờ: neo trái-phải, bề ngang = ô nhập địa chỉ, cao vừa số dòng (2..8 dòng).
            _lst.Dock = DockStyle.None;
            _lst.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            _lst.MaximumSize = Size.Empty;
            _lst.MinimumSize = Size.Empty;
            _lst.IntegralHeight = false;
            _lst.HorizontalScrollbar = true;
            _lst.BorderStyle = BorderStyle.FixedSingle;
            _lst.Cursor = Cursors.Hand;
            _txt.SizeChanged += (_, _) => FitList();

            _txt.TextChanged += (_, _) =>
            {
                if (_suppress) { _suppress = false; return; }
                if (Selected is not null) { Selected = null; _onChanged(null); }
                ShowHint(HintText, Theme.TextSecondary);
                _debounce.Stop();
                _debounce.Start();
            };
            _txt.KeyDown += (_, e) =>
            {
                if (e.KeyCode == Keys.Down && _lst.Visible && _lst.Items.Count > 0)
                {
                    _lst.Focus();
                    if (_lst.SelectedIndex < 0) _lst.SelectedIndex = 0;
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.Enter && _lst.Visible && _lst.Items.Count > 0 && _lst.Items[0] is PlaceSuggestionItem)
                {
                    _lst.SelectedIndex = 0;
                    _ = PickAsync();
                    e.Handled = e.SuppressKeyPress = true;
                }
            };
            _debounce.Tick += async (_, _) => { _debounce.Stop(); await SearchAsync(); };
            // Đóng form khi đang chờ gõ / đang tìm -> dừng hẳn, không đụng vào ô đã hủy (trước: hiện hộp "lỗi ngoài dự kiến").
            _txt.Disposed += (_, _) => { _searchVersion++; _debounce.Stop(); _debounce.Dispose(); };
            _lst.Click += async (_, _) => await PickAsync();
            _lst.KeyDown += async (_, e) =>
            {
                if (e.KeyCode != Keys.Enter) return;
                e.Handled = e.SuppressKeyPress = true;
                await PickAsync();
            };
        }

        /// <summary>Điền chữ vào ô tìm mà KHÔNG tìm lại (VD mở form Sửa với địa chỉ cũ).</summary>
        public void SetTextSilently(string text)
        {
            if (_txt.Text == text) return;
            _suppress = true;
            _txt.Text = text;
        }

        private void ShowHint(string text, Color color)
        {
            _lblSelected.Text = text;
            _lblSelected.ForeColor = color;
        }

        private void ShowMessage(string message)
        {
            _lst.Items.Clear();
            _lst.Items.Add(message); // dòng chữ thường (không phải gợi ý) -> bấm vào không làm gì
            ShowList();
        }

        private void ShowList()
        {
            _lst.Visible = true;
            FitList();
        }

        private void FitList()
        {
            if (!_lst.Visible) return;
            if (_txt.Width > 0) _lst.Width = _txt.Width;
            _lst.Height = Math.Clamp(_lst.Items.Count, 2, 8) * _lst.ItemHeight + 6;
        }

        private async Task SearchAsync()
        {
            if (Api is null || Gone) return;
            var text = _txt.Text.Trim();
            if (text.Length < 2) { _lst.Items.Clear(); _lst.Visible = false; return; }

            var version = ++_searchVersion;
            ShowMessage("⏳ Đang tìm địa chỉ...");
            try
            {
                var results = (await Api.Locations.SearchPlacesAsync(text) ?? new())
                    .Where(r => !string.IsNullOrWhiteSpace(r.Name) || !string.IsNullOrWhiteSpace(r.Address))
                    .ToList();
                if (version != _searchVersion || Gone) return; // đã gõ tiếp (hoặc đã đóng form) -> bỏ kết quả cũ

                if (results.Count == 0)
                {
                    ShowMessage("Không tìm thấy địa chỉ nào — gõ thêm phường / quận / tỉnh (VD \"Long Biên, Hà Nội\").");
                    return;
                }
                _lst.Items.Clear();
                foreach (var r in results) _lst.Items.Add(r);
                ShowList();
                ShowHint($"Có {results.Count} gợi ý — BẤM chọn 1 dòng để lấy tọa độ.", Theme.Primary);
            }
            catch (Exception ex)
            {
                if (version != _searchVersion || Gone) return;
                ShowMessage("⚠️ Không gọi được dịch vụ bản đồ: " + ex.Message);
            }
        }

        private bool Gone => _txt.IsDisposed || _lst.IsDisposed || _lblSelected.IsDisposed;

        private async Task PickAsync()
        {
            if (_lst.SelectedItem is not PlaceSuggestionItem item) return;

            // Gợi ý chưa kèm tọa độ -> hỏi API chi tiết theo RefId.
            if (item.Lat == 0 && item.Lng == 0 && Api is not null && !string.IsNullOrWhiteSpace(item.RefId))
            {
                ShowHint("⏳ Đang lấy tọa độ...", Theme.TextSecondary);
                try
                {
                    var detail = await Api.Locations.GetPlaceDetailAsync(item.RefId);
                    if (detail is not null && !(detail.Lat == 0 && detail.Lng == 0))
                    {
                        item.Lat = detail.Lat;
                        item.Lng = detail.Lng;
                        if (string.IsNullOrWhiteSpace(item.Address)) item.Address = detail.Address;
                    }
                }
                catch { /* báo ở dưới */ }
                if (Gone) return; // form đã đóng trong lúc chờ
            }
            if (item.Lat == 0 && item.Lng == 0)
            {
                ShowHint("⚠️ Gợi ý này không có tọa độ — chọn dòng khác hoặc gõ địa chỉ cụ thể hơn.", Theme.Danger);
                return;
            }
            if (string.IsNullOrWhiteSpace(item.Address)) item.Address = item.Name;

            Selected = item;
            _lst.Visible = false;
            SetTextSilently(item.Address);
            ShowHint($"✅ {item.Address}   (Lat {item.Lat:0.000000}, Lng {item.Lng:0.000000})", Theme.Success);
            _onChanged(item);
        }
    }
}
