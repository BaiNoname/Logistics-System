using TruckingSystem.Desktop.Common;
using TruckingSystem.Desktop.DTO_TripManagement;

namespace TruckingSystem.Desktop.GUI_TripManagement
{
    /// <summary>THẺ "CHUYẾN ĐANG CHỌN" (UserControl) nằm ngay trên bảng chuyến: nhìn 1 chỗ là biết chuyến nào
    /// đang chọn, xe / tài xế / tuyến / ngày xuất phát, và VIỆC CẦN LÀM TIẾP THEO (vạch màu theo trạng thái).
    /// Thay cho dòng chữ dài trước đây — nhân viên không phải dò cả hàng nút để biết bấm gì.
    /// Giao diện nằm ở SelectedTripBar.Designer.cs.</summary>
    public partial class SelectedTripBar : UserControl
    {
        public SelectedTripBar()
        {
            InitializeComponent();
            Dock = DockStyle.Top;
        }

        /// <summary>Không có chuyến nào được chọn (bảng trống / đang tải).</summary>
        public void ShowEmpty(string? message = null)
        {
            accent.BackColor = Theme.BorderColor;
            lblHeadline.Text = "Chưa chọn chuyến";
            lblDetail.Text = message ?? "Bấm 1 dòng trong bảng bên dưới để xem thông tin và việc cần làm của chuyến.";
            lblNext.Text = "";
        }

        /// <param name="statusText">Trạng thái đang hiện trên bảng (gồm các trạng thái chờ của chặng Giao).</param>
        /// <param name="legText">Tên chặng có biểu tượng ("🔽 Lấy hàng"...).</param>
        /// <param name="nextStep">Việc cần làm tiếp theo (đã tính sẵn ở màn Chuyến).</param>
        /// <param name="extra">Thông tin thêm (số lượng, tiến độ kho, mốc xuất phát, tình trạng hạn) — các cột đã bỏ khỏi bảng.</param>
        /// <param name="blocked">true = chuyến đang bị chặn / cần xử lý ở màn khác (tô cam).</param>
        public void ShowTrip(TripListItem trip, string statusText, string legText, string nextStep, IEnumerable<string> extra, bool blocked)
        {
            accent.BackColor = trip.Status switch
            {
                "InProgress" => Theme.Primary,
                "PendingConfirmation" => Theme.Warning,
                "Completed" => Theme.Success,
                "Cancelled" => Theme.Danger,
                _ => blocked ? Theme.Warning : Theme.TextSecondary,
            };
            lblHeadline.Text = $"Chuyến #{trip.Id}  ·  {legText}  ·  Đơn {trip.OrderCode}  ·  {statusText}";

            var parts = new List<string>
            {
                $"🚚 {(string.IsNullOrWhiteSpace(trip.VehiclePlate) ? "—" : trip.VehiclePlate)}",
                $"👤 {(string.IsNullOrWhiteSpace(trip.DriverName) ? "—" : trip.DriverName)}",
            };
            if (!string.IsNullOrWhiteSpace(trip.FromName) || !string.IsNullOrWhiteSpace(trip.ToName))
                parts.Add($"📍 {trip.FromName ?? "?"} → {trip.ToName ?? "?"}");
            if (trip.PlannedStartDate is DateTime start) parts.Add($"🗓 Xuất phát {start:dd/MM/yyyy}");
            if (trip.CommittedDeliveryDate is DateTime due) parts.Add($"⏰ Hạn giao {due:dd/MM/yyyy}");
            parts.AddRange(extra.Where(e => !string.IsNullOrWhiteSpace(e)));
            // Mỗi cụm dùng khoảng trắng KHÔNG NGẮT DÒNG -> màn hẹp chỉ xuống dòng GIỮA các cụm (không tách "Xuất phát" khỏi ngày).
            lblDetail.Text = string.Join("   ·   ", parts.Select(p => p.Replace(' ', '\u00A0')));

            lblNext.Text = string.IsNullOrWhiteSpace(nextStep) ? "" : "👉 " + nextStep;
            lblNext.ForeColor = blocked ? Theme.Warning : Theme.Primary;
        }
    }
}
