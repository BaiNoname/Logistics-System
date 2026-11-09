namespace TruckingSystem.Desktop.Common
{
    /// <summary>DẢI NHẮC VIỆC đầu màn (nền vàng nhạt): "⚠️ Cần xử lý: 2 đơn chờ xác nhận · 1 đơn chờ chia xe" + 2 link
    /// [Chỉ xem các việc này] / [Xem tất cả]. Giải thích đúng con số đỏ trên menu trái; không có việc chờ thì tự ẩn.
    /// Dùng: banner.Update("2 đơn chờ xác nhận · 1 đơn chờ chia xe", count, filtering); banner.FilterToggled += ...</summary>
    public sealed class PendingBanner : FlowLayoutPanel
    {
        private readonly Label _text = new() { AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.FromArgb(146, 64, 14) };
        private readonly LinkLabel _toggle = new() { AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };

        /// <summary>true = người dùng muốn chỉ xem các việc chờ; false = xem tất cả.</summary>
        public event EventHandler<bool>? FilterToggled;
        private bool _filtering;

        public PendingBanner()
        {
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            WrapContents = true;
            Dock = DockStyle.Fill;
            BackColor = Color.FromArgb(254, 243, 199);
            Padding = new Padding(12, 8, 12, 8);
            Margin = new Padding(0, 0, 0, 8);
            Visible = false;
            _text.Margin = new Padding(0, 2, 16, 2);
            _toggle.Margin = new Padding(0, 2, 0, 2);
            _toggle.LinkColor = _toggle.ActiveLinkColor = Theme.Primary;
            _toggle.LinkClicked += (_, _) => FilterToggled?.Invoke(this, !_filtering);
            Controls.Add(_text);
            Controls.Add(_toggle);
        }

        /// <param name="detail">Mô tả các việc chờ, VD "2 đơn chờ xác nhận · 1 đơn chờ chia xe".</param>
        /// <param name="count">Tổng số việc chờ (0 = ẩn dải, trừ khi đang lọc).</param>
        /// <param name="filtering">Bảng đang chỉ hiện các việc chờ.</param>
        public void Update(string detail, int count, bool filtering)
        {
            _filtering = filtering;
            Visible = count > 0 || filtering;
            _text.Text = count > 0 ? $"⚠️ Cần xử lý: {detail}" : "✅ Đã xử lý hết các việc chờ";
            _toggle.Text = filtering ? "Xem tất cả" : "Chỉ xem các việc này";
        }
    }
}
