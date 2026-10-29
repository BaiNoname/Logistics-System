using System.Text.RegularExpressions;

namespace TruckingSystem.Desktop.DTO_NotificationManagement
{
    public class NotificationItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Message { get; set; } = "";

        /// <summary>Cột IsRead của server, từ bản này mang nghĩa "ĐÃ XỬ LÝ" (không còn là "đã đọc"):
        /// thông báo gắn với việc cụ thể chỉ chuyển true khi việc đó thật sự xong.</summary>
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }

        /// <summary>"Chuyến chờ duyệt" / "Chuyến bị chặn" / "Chờ nhập kho" / "Giấy tờ xe" / "Giấy tờ tài xế" / "Hạn giao" / "Lịch tài xế" / "Khác".
        /// Server cũ chưa trả trường này -> Desktop tự suy ra từ tiêu đề.</summary>
        public string? Category { get; set; }

        public bool IsDone => IsRead;

        public string CategoryText => !string.IsNullOrWhiteSpace(Category) ? Category! : GuessCategory(Title);

        /// <summary>Việc mà hệ thống TỰ kiểm tra được đã xong chưa -> không cho đánh dấu tay.
        /// Chỉ thông báo thông tin (loại "Khác") mới đánh dấu "Đã xử lý" bằng tay.</summary>
        public bool IsAutoTracked => CategoryText != "Khác";

        /// <summary>Màn hình cần mở khi nhấp đúp (null = không xác định được).</summary>
        public NotificationTarget? Target => NotificationTarget.Parse(Title);

        /// <summary>Giờ Việt Nam để hiển thị (server lưu UTC).</summary>
        public DateTime CreatedAtLocal => CreatedAt.Kind switch
        {
            DateTimeKind.Local => CreatedAt,
            _ => DateTime.SpecifyKind(CreatedAt, DateTimeKind.Utc).ToLocalTime(),
        };

        public static string GuessCategory(string title)
        {
            if (title.StartsWith("🟢")) return "Chuyến chờ duyệt";
            if (title.StartsWith("🔴") || title.StartsWith("🟡")) return "Giấy tờ xe";
            if (title.StartsWith("📅")) return "Lịch tài xế";
            if (title.StartsWith("🪪")) return "Giấy tờ tài xế";
            if (title.StartsWith("⏰")) return "Hạn giao";
            if (title.StartsWith("🚫")) return "Chuyến bị chặn";
            if (title.StartsWith("📦")) return "Chờ nhập kho";
            return "Khác";
        }
    }

    public enum NotificationTargetKind { Trip, Order, Vehicle, Driver, PendingReceipt, TripIssue }

    /// <summary>Việc mà 1 thông báo nói tới — suy ra từ tiêu đề (tiêu đề do hệ thống sinh theo
    /// mẫu cố định, xem NotificationService/TripService phía server).
    /// Key: số chuyến / mã đơn / biển số / mã tài xế / tên kho (có thể rỗng với thông báo cũ).</summary>
    public record NotificationTarget(NotificationTargetKind Kind, string Key, int? TripId = null)
    {
        private static readonly Regex TripNo = new(@"[Cc]huyến #(\d+)", RegexOptions.Compiled);
        private static readonly Regex OrderCode = new(@"Đơn (\S+)", RegexOptions.Compiled);
        private static readonly Regex Plate = new(@" xe (.+?) (ĐÃ HẾT HẠN|sắp hết hạn)\s*$", RegexOptions.Compiled);
        private static readonly Regex DriverCode = new(@"\(([^()]+)\)[^()]*$", RegexOptions.Compiled);
        private static readonly Regex IssueWarehouse = new(@"chờ kho ?(.*?) xuất hàng\s*$", RegexOptions.Compiled);
        private static readonly Regex ReceiptWarehouse = new(@"đã về kho (.+?) — chờ nhập kho", RegexOptions.Compiled);

        /// <summary>Mô tả ngắn cho tooltip / thanh trạng thái.</summary>
        public string Describe() => Kind switch
        {
            NotificationTargetKind.Trip => $"Quản lý Chuyến → chuyến #{Key}",
            NotificationTargetKind.Order => $"Đơn vận chuyển → đơn {Key}",
            NotificationTargetKind.Vehicle => $"Đội xe → xe {Key}",
            NotificationTargetKind.Driver => $"Tài xế → {Key}",
            NotificationTargetKind.PendingReceipt => "Kho trung chuyển → 📦 Nhập hàng từ chuyến" + (Key.Length > 0 ? $" (kho {Key})" : ""),
            NotificationTargetKind.TripIssue => "Kho trung chuyển → 📤 Xuất kho theo chuyến" + (Key.Length > 0 ? $" (kho {Key})" : ""),
            _ => "",
        };

        public static NotificationTarget? Parse(string title)
        {
            if (string.IsNullOrWhiteSpace(title)) return null;
            Match m;

            if (title.StartsWith("📦"))
            {
                var trip = TripNo.Match(title);
                int? tripId = trip.Success ? int.Parse(trip.Groups[1].Value) : null;
                m = ReceiptWarehouse.Match(title);
                return new NotificationTarget(NotificationTargetKind.PendingReceipt, m.Success ? m.Groups[1].Value.Trim() : "", tripId);
            }

            if (title.StartsWith("🔴") || title.StartsWith("🟡"))
            {
                m = Plate.Match(title);
                return m.Success ? new NotificationTarget(NotificationTargetKind.Vehicle, m.Groups[1].Value.Trim()) : null;
            }

            if (title.StartsWith("🪪") || title.StartsWith("📅"))
            {
                m = DriverCode.Match(title);
                return m.Success ? new NotificationTarget(NotificationTargetKind.Driver, m.Groups[1].Value.Trim()) : null;
            }

            // 🚫 chờ kho xuất hàng -> màn Kho (việc của kho, nhân viên kho không vào được màn Chuyến).
            if (title.StartsWith("🚫") && IssueWarehouse.Match(title) is { Success: true } issue)
            {
                var trip = TripNo.Match(title);
                return new NotificationTarget(NotificationTargetKind.TripIssue, issue.Groups[1].Value.Trim(),
                    trip.Success ? int.Parse(trip.Groups[1].Value) : null);
            }

            // 🟢 chờ duyệt, 🚫 bị chặn, ⏰ hạn giao theo chuyến -> màn Chuyến; ⏰ theo đơn -> màn Đơn.
            m = TripNo.Match(title);
            if (m.Success) return new NotificationTarget(NotificationTargetKind.Trip, m.Groups[1].Value, int.Parse(m.Groups[1].Value));

            m = OrderCode.Match(title);
            if (m.Success) return new NotificationTarget(NotificationTargetKind.Order, m.Groups[1].Value.Trim());

            return null;
        }
    }
}
