namespace TruckingSystem.Desktop.Common
{
    /// <summary>THẺ "ĐANG CHỌN" dùng chung (UserControl) đặt ngay trên bảng danh sách: vạch màu theo trạng thái,
    /// dòng tiêu đề đậm, dòng chi tiết (các cụm cách nhau bằng " · ", không bị ngắt giữa cụm), dòng VIỆC CẦN LÀM
    /// TIẾP THEO và gợi ý cách dùng chuột ở bên phải. Màn Đơn vận chuyển dùng thẻ này.
    /// Giao diện nằm ở SelectionCard.Designer.cs.</summary>
    public partial class SelectionCard : UserControl
    {
        public SelectionCard()
        {
            InitializeComponent();
            Dock = DockStyle.Top;
        }

        /// <summary>Chữ gợi ý bên phải (VD "Chuột phải vào dòng: mọi thao tác / Nhấp đúp: xem chi tiết").</summary>
        public string Hint
        {
            get => lblHint.Text;
            set => lblHint.Text = value;
        }

        public void ShowEmpty(string title, string message)
        {
            accent.BackColor = Theme.BorderColor;
            lblHeadline.Text = title;
            lblDetail.Text = message;
            lblNext.Text = "";
        }

        /// <param name="accentColor">Màu vạch trái theo trạng thái.</param>
        /// <param name="details">Các cụm thông tin; khoảng trắng trong mỗi cụm không ngắt dòng.</param>
        /// <param name="nextStep">Việc cần làm tiếp theo ("" = không có).</param>
        /// <param name="warn">true = tô cam (đang bị chặn / cần xử lý ở màn khác).</param>
        public void ShowItem(Color accentColor, string headline, IEnumerable<string> details, string nextStep, bool warn)
        {
            accent.BackColor = accentColor;
            lblHeadline.Text = headline;
            lblDetail.Text = string.Join("   ·   ", details.Where(d => !string.IsNullOrWhiteSpace(d)).Select(d => d.Replace(' ', '\u00A0')));
            lblNext.Text = string.IsNullOrWhiteSpace(nextStep) ? "" : "👉 " + nextStep;
            lblNext.ForeColor = warn ? Theme.Warning : Theme.Primary;
        }
    }
}
